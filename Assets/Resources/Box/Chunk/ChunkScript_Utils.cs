using System;
using System.Collections;
using System.Collections.Generic;
using JsonConverter.ChunkJsonConverter;
using UnityEngine;

public partial class ChunkScript
{
    /// <summary>
    /// Player����̍ő勗�����ɂ��邩�ǂ���
    /// </summary>
    /// <param name="index">�m�F�������C���f�b�N�X</param>
    /// <returns>�͈͓��Ȃ�True</returns>
    private bool IsFitIntoFar(Index3D index)
    {
        //index����player�܂ł�XYZ�̊e����
        Index3D distance = PlayerIndex - index;
        //�S��far���߂���΂���
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
    /// Box���w�肳�ꂽ�R�����C���f�b�N�X�̏ꏊ�ɐ����ł��邩�ǂ���
    /// </summary>
    /// <param name="index">Box�𐶐��������ꏊ</param>
    /// <returns></returns>
    public bool CanSpawnBox(Index3D index)
    {
        return index.IsFitIntoRange(0, chunkSize - 1) && (boxDatas[index.x, index.y, index.z] == null);
    }

    /// <summary>
    /// Box�̃`�����N���ł̃C���f�b�N�X��
    /// ���[���h���W�ɕϊ�
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Vector3 CalcWorldPositionFromBoxLocalIndex(Index3D index)
    {
        //���W
        Vector3 position = new Vector3(index.x, index.y, index.z);
        //�C���f�b�N�X���W���烍�[�J�����W��
        position -= new Vector3(chunkSize / 2f, chunkSize / 2f, chunkSize / 2f);
        //���W��Box�̒��S��
        position += new Vector3(boxSize / 2f, boxSize / 2f, boxSize / 2f);
        //���[�J�����W���烏�[���h���W��
        position += center;

        return position;
    }

    /// <summary>
    /// Box�̍��W���`�����N���ł̃��[�J���C���f�b�N�X�ɕϊ�
    /// </summary>
    /// <param name="position">�ϊ�������W</param>
    /// <param name="chunkCenter">�`�����N�̒��S���W</param>
    /// <returns>�`�����N���ł̃��[�J���C���f�b�N�X</returns>
    public Index3D CalcLocalIndexFromBoxWorldPosition(Vector3 position)
    {
        //���[�J�����W�̌v�Z
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
    /// �w�肵���C���f�b�N�X�̃{�b�N�X�̃v���n�u���擾
    /// </summary>
    /// <param name="x">���W</param>
    /// <param name="y">���W</param>
    /// <param name="z">���W</param>
    /// <returns></returns>
    public GameObject GetGenetrateBox(int x, int y, int z)
    {
        int mountainThreshold = 45;

        int biome = Noise.GetNoiseInt(x, 0, z, 80, 20, 1.5f);

        //�΂̍ő卂�����擾
        int stone = Noise.GetNoiseInt(x, y, z, 75, 5, 1.2f);
        //�y�̍ő卂�����擾
        int dirt = Noise.GetNoiseInt(x, y, z, 100, 15, 0);
        if (biome >= mountainThreshold)
        {
            //�΂̍ő卂�����擾
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
    /// �m�C�Y�����N���X
    /// </summary>
    public class Noise
    {
        //�O���f�B�G���g
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

        //����
        private static short[] permutation;
        private static short[] p = new short[512];
        private static short[] pMod12 = new short[512];

        //�X�L���[�C���O�W��
        private static double F3 = 1.0 / 3.0;
        private static double G3 = 1.0 / 6.0;

        static Noise()
        {
            UnityEngine.Random.InitState(seed);
            permutation = new short[256];

            //0-256�܂œ����
            for (short i = 0; i < 256; i++)
            {
                permutation[i] = i;
            }
            //�����_���ɂ���������
            for (short i = 0; i < 256; i++)
            {
                var tmp = permutation[i];
                var random = UnityEngine.Random.Range(0, 256);
                permutation[i] = permutation[random];
                permutation[random] = tmp;
            }

            //������
            for (int i = 0; i < 512; i++)
            {
                p[i] = permutation[i & 255];
                pMod12[i] = (short)(p[i] % 12);
            }
        }

        /// <summary>
        /// Double��Floor�֐����Ȃ������̂ō����
        /// Math.Floor�Ɗ�{����
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static int Floor(double value)
        {
            int xi = (int)value;
            return value < xi ? xi - 1 : xi;
        }

        /// <summary>
        /// �h�b�g��
        /// </summary>
        /// <param name="g">�x�N�g��A</param>
        /// <param name="x">�x�N�g��B��X</param>
        /// <param name="y">�x�N�g��B��Y</param>
        /// <param name="z">�x�N�g��B��Z</param>
        /// <returns></returns>
        private static double dot(Grad g, double x, double y, double z)
        {
            return g.x * x + g.y * y + g.z * z;
        }

        /// <summary>
        /// �p�[�����m�C�Y�𐶐�����
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static float GetNoise(double x, double y, double z)
        {
            //�l��
            double n0, n1, n2, n3;

            //�X�L���[�C���O�W�����g�p���Č��݂̃Z���̊���o��
            double s = (x + y + z) * F3;
            int i = Floor(x + s);
            int j = Floor(y + s);
            int k = Floor(z + s);
            double t = (i + j + k) * G3;

            //���_��X.Y.Z���W�n�ɖ߂�
            double X0 = i - t;
            double Y0 = j - t;
            double Z0 = k - t;

            //���_����̃I�t�Z�b�g�i1�Ԗځj
            double x0 = x - X0;
            double y0 = y - Y0;
            double z0 = z - Z0;

            //�V���v���b�N�X�̓���
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

            //���_����̃I�t�Z�b�g�i2�Ԗځj
            double x1 = x0 - i1 + G3;
            double y1 = y0 - j1 + G3;
            double z1 = z0 - k1 + G3;
            //���_����̃I�t�Z�b�g�i3�Ԗځj
            double x2 = x0 - i2 + 2.0 * G3;
            double y2 = y0 - j2 + 2.0 * G3;
            double z2 = z0 - k2 + 2.0 * G3;
            //���_����̃I�t�Z�b�g�i4�Ԗځj
            double x3 = x0 - 1.0 + 3.0 * G3;
            double y3 = y0 - 1.0 + 3.0 * G3;
            double z3 = z0 - 1.0 + 3.0 * G3;

            //���z�v�Z
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
        /// �I�N�^�[�u�m�C�Y�̎擾
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
        /// int�^�̃m�C�Y�̎擾
        /// </summary>
        /// <param name="x">���W</param>
        /// <param name="y">���W</param>
        /// <param name="z">���W</param>
        /// <param name="scale">�m�C�Y�̑e��</param>
        /// <param name="height">�ő卂��</param>
        /// <param name="power">���፷�w���i�����ꏊ�͂�荂���A�Ⴂ�ꏊ�͂��Ⴍ�j</param>
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
