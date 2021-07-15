using System;
using System.Collections;
using System.Collections.Generic;
using JsonConverter.ChunkJsonConverter;
using UnityEngine;

public partial class ChunkScript
{
    /// <summary>
    /// Playerからの最大距離内にいるかどうか
    /// </summary>
    /// <param name="index">確認したいインデックス</param>
    /// <returns>範囲内ならTrue</returns>
    private bool IsFitIntoFar(Index3D index)
    {
        //indexからplayerまでのXYZの各距離
        Index3D distance = PlayerIndex - index;
        //全てfarより近ければいい
        if ((Math.Abs(distance.x) <= far) &&
            //((distance.y <= (0 - far / 2)) && (Math.Abs(distance.y) <= far + far / 2)) &&
            (Math.Abs(distance.y) <= far) &&
            (Math.Abs(distance.z) <= far))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Boxを指定された３次元インデックスの場所に生成できるかどうか
    /// </summary>
    /// <param name="index">Boxを生成したい場所</param>
    /// <returns></returns>
    public bool CanSpawnBox(Index3D index)
    {
        return index.IsFitIntoRange(0, chunkSize - 1) && (boxDatas[index.x, index.y, index.z] == null);
    }

    /// <summary>
    /// Boxのチャンク内でのインデックスを
    /// ワールド座標に変換
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Vector3 CalcWorldPositionFromBoxLocalIndex(Index3D index)
    {
        //座標
        Vector3 position = new Vector3(index.x, index.y, index.z);
        //インデックス座標からローカル座標へ
        position -= new Vector3(chunkSize / 2f, chunkSize / 2f, chunkSize / 2f);
        //座標をBoxの中心へ
        position += new Vector3(boxSize / 2f, boxSize / 2f, boxSize / 2f);
        //ローカル座標からワールド座標へ
        position += center;

        return position;
    }

    /// <summary>
    /// Boxの座標をチャンク内でのローカルインデックスに変換
    /// </summary>
    /// <param name="position">変換する座標</param>
    /// <param name="chunkCenter">チャンクの中心座標</param>
    /// <returns>チャンク内でのローカルインデックス</returns>
    public Index3D CalcLocalIndexFromBoxWorldPosition(Vector3 position)
    {
        //ローカル座標の計算
        Vector3 localPosition = new Vector3(
            position.x + (chunkSize / 2),
            position.y + (chunkSize / 2),
            position.z + (chunkSize / 2)) - center;

        return new Index3D(
            (int)Mathf.Floor(localPosition.x),
            (int)Mathf.Floor(localPosition.y),
            (int)Mathf.Floor(localPosition.z));
    }

    /// <summary>
    /// 指定したインデックスのボックスのプレハブを取得
    /// </summary>
    /// <param name="x">座標</param>
    /// <param name="y">座標</param>
    /// <param name="z">座標</param>
    /// <returns></returns>
    public GameObject GetGenetrateBox(int x, int y, int z)
    {
        int mountainThreshold = 45;

        int biome = Noise.GetNoiseInt(x, 0, z, 80, 20, 1.5f);

        //石の最大高さを取得
        int stone = Noise.GetNoiseInt(x, y, z, 75, 5, 1.2f);
        //土の最大高さを取得
        int dirt = Noise.GetNoiseInt(x, y, z, 100, 15, 0);
        if (biome >= mountainThreshold)
        {
            //石の最大高さを取得
            stone += biome - mountainThreshold;
        }

        if (y <= stone)
        {
            return PrefabManager.Instance.GetPrefab("BoxVariant_Tile");
        }
        else if (y <= dirt + stone)
        {
            return PrefabManager.Instance.GetPrefab("BoxVariant_WoodBox");
        }
        return null;
    }

    /// <summary>
    /// ノイズ生成クラス
    /// </summary>
    public class Noise
    {
        //グラディエント
        private struct Grad
        {
            public double x, y, z, w;

            public Grad(double x, double y, double z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.w = 0;
            }
        }

        private static Grad[] grad3 = new Grad[] {
            new Grad(1,1,0), new Grad(-1,1,0), new Grad(1,-1,0), new Grad(-1,-1,0),
            new Grad(1,0,1), new Grad(-1,0,1), new Grad(1,0,-1), new Grad(-1,0,-1),
            new Grad(0,1,1), new Grad(0,-1,1), new Grad(0,1,-1), new Grad(0,-1,-1)
        };

        //順列
        private static short[] permutation;
        private static short[] p = new short[512];
        private static short[] pMod12 = new short[512];

        //スキューイング係数
        private static double F3 = 1.0 / 3.0;
        private static double G3 = 1.0 / 6.0;

        static Noise()
        {
            UnityEngine.Random.InitState(seed);
            permutation = new short[256];

            //0-256まで入れる
            for (short i = 0; i < 256; i++)
            {
                permutation[i] = i;
            }
            //ランダムにかき混ぜる
            for (short i = 0; i < 256; i++)
            {
                var tmp = permutation[i];
                var random = UnityEngine.Random.Range(0, 256);
                permutation[i] = permutation[random];
                permutation[random] = tmp;
            }

            //初期化
            for (int i = 0; i < 512; i++)
            {
                p[i] = permutation[i & 255];
                pMod12[i] = (short)(p[i] % 12);
            }
        }

        /// <summary>
        /// DoubleのFloor関数がなかったので作った
        /// Math.Floorと基本同じ
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static int Floor(double value)
        {
            int xi = (int)value;
            return value < xi ? xi - 1 : xi;
        }

        /// <summary>
        /// ドット積
        /// </summary>
        /// <param name="g">ベクトルA</param>
        /// <param name="x">ベクトルBのX</param>
        /// <param name="y">ベクトルBのY</param>
        /// <param name="z">ベクトルBのZ</param>
        /// <returns></returns>
        private static double dot(Grad g, double x, double y, double z)
        {
            return g.x * x + g.y * y + g.z * z;
        }

        /// <summary>
        /// パーリンノイズを生成する
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static float GetNoise(double x, double y, double z)
        {
            //四隅
            double n0, n1, n2, n3;

            //スキューイング係数を使用して現在のセルの割り出し
            double s = (x + y + z) * F3;
            int i = Floor(x + s);
            int j = Floor(y + s);
            int k = Floor(z + s);
            double t = (i + j + k) * G3;

            //原点をX.Y.Z座標系に戻す
            double X0 = i - t;
            double Y0 = j - t;
            double Z0 = k - t;

            //原点からのオフセット（1番目）
            double x0 = x - X0;
            double y0 = y - Y0;
            double z0 = z - Z0;

            //シンプレックスの特定
            int i1, j1, k1;
            int i2, j2, k2;
            if (x0 >= y0)
            {
                if (y0 >= z0)
                { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } // X Y Z
                else if (x0 >= z0)
                { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; } // X Z Y
                else
                { i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; } // Z X Y
            }
            else
            {
                if (y0 < z0)
                { i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; } // Z Y X
                else if (x0 < z0)
                { i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; } // Y Z X
                else
                { i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } // Y X Z
            }

            //原点からのオフセット（2番目）
            double x1 = x0 - i1 + G3;
            double y1 = y0 - j1 + G3;
            double z1 = z0 - k1 + G3;
            //原点からのオフセット（3番目）
            double x2 = x0 - i2 + 2.0 * G3;
            double y2 = y0 - j2 + 2.0 * G3;
            double z2 = z0 - k2 + 2.0 * G3;
            //原点からのオフセット（4番目）
            double x3 = x0 - 1.0 + 3.0 * G3;
            double y3 = y0 - 1.0 + 3.0 * G3;
            double z3 = z0 - 1.0 + 3.0 * G3;

            //勾配計算
            int ii = i & 255;
            int jj = j & 255;
            int kk = k & 255;
            int gi0 = pMod12[ii + p[jj + p[kk]]];
            int gi1 = pMod12[ii + i1 + p[jj + j1 + p[kk + k1]]];
            int gi2 = pMod12[ii + i2 + p[jj + j2 + p[kk + k2]]];
            int gi3 = pMod12[ii + 1 + p[jj + 1 + p[kk + 1]]];
            double t0 = 0.6 - x0 * x0 - y0 * y0 - z0 * z0;
            if (t0 < 0) n0 = 0.0;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * dot(grad3[gi0], x0, y0, z0);
            }
            double t1 = 0.6 - x1 * x1 - y1 * y1 - z1 * z1;
            if (t1 < 0) n1 = 0.0;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * dot(grad3[gi1], x1, y1, z1);
            }
            double t2 = 0.6 - x2 * x2 - y2 * y2 - z2 * z2;
            if (t2 < 0) n2 = 0.0;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * dot(grad3[gi2], x2, y2, z2);
            }
            double t3 = 0.6 - x3 * x3 - y3 * y3 - z3 * z3;
            if (t3 < 0) n3 = 0.0;
            else
            {
                t3 *= t3;
                n3 = t3 * t3 * dot(grad3[gi3], x3, y3, z3);
            }

            return (float)(32.0 * (n0 + n1 + n2 + n3) + 1) * 0.5f;
        }

        /// <summary>
        /// オクターブノイズの取得
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="octaves"></param>
        /// <returns></returns>
        public static float GetOctaveNoise(double x, double y, double z, int octaves)
        {
            float value = 0;
            float divisor = 0;
            float currentHalf = 0;
            float currentDouble = 0;

            for (int i = 0; i < octaves; i++)
            {
                currentHalf = (float)Math.Pow(0.5f, i);
                currentDouble = (float)Math.Pow(2, i);
                value += GetNoise(x * currentDouble, y * currentDouble, z) * currentHalf;
                divisor += currentHalf;
            }

            return value / divisor;
        }

        /// <summary>
        /// int型のノイズの取得
        /// </summary>
        /// <param name="x">座標</param>
        /// <param name="y">座標</param>
        /// <param name="z">座標</param>
        /// <param name="scale">ノイズの粗さ</param>
        /// <param name="height">最大高さ</param>
        /// <param name="power">高低差指数（高い場所はより高く、低い場所はより低く）</param>
        /// <returns></returns>
        public static int GetNoiseInt(int x, int y, int z, float scale, float height, float power)
        {
            float rValue;
            rValue = GetOctaveNoise(((double)x) / scale, ((double)y) / scale, ((double)z) / scale, 3);
            rValue *= height;

            if (power != 0)
            {
                rValue = Mathf.Pow(rValue, power);
            }

            return (int)rValue;
        }
    }
}
