using System;
using UnityEngine;

public partial class ChunkScript
{

    //チャンクの一辺のサイズ
    private const int chunkSize = 32;
    //Boxのサイズ
    private const float boxSize = 1;
    //自動生成距離
    private const int far = 0;
    //ノイズ解像度（平行）
    private const float mapScaleHorizontal = 0.007f;
    //ノイズ解像度（垂直）
    private const int mapResolutionVertical = 75;
    //ランダムシード
    private const int seed = 12345;

    //Playerのいるインデックス
    private static Index3D PlayerIndex;

    //現在地形生成中のチャンク
    private static ChunkScript nowGenerateTerrainChunk;

    /// <summary>
    /// Boxの方向
    /// </summary>
    public enum Direction
    {
        First = 0,
        TopLeftForward = First, //上左前
        TopForward,             //上前
        TopRightForward,        //上右前
        TopRigft,               //上右
        TopRightBehind,         //上右後
        TopBehind,              //上後
        TopLeftBehind,          //上左後
        TopLeft,                //上左
        RightForward,           //右前
        LeftForward,            //左前
        Top,                    //上
        Left,                   //左
        Forward,                //前
        Behind,                 //後
        Right,                  //右
        Bottom,                 //下
        RightBehind,            //右後
        LeftBehind,             //左後
        BottomRigft,            //下右
        BottomRightForward,     //下右前
        BottomForward,          //下前
        BottomLeftForward,      //下左前
        BottomLeft,             //下左
        BottomLeftBehind,       //下左後
        BottomBehind,           //下後
        BottomRightBehind,      //下右後
        Max = BottomRightBehind
    }

    /// <summary>
    /// Boxがワールドのどこにいるのか
    /// ブロック単位での座標
    /// </summary>
    [Serializable]
    public struct Index3D
    {
        public int x;
        public int y;
        public int z;

        /// <summary>
        /// レンジからはみ出ていないかチェック
        /// </summary>
        /// <param name="min">最小値（レンジ内で最小の値）</param>
        /// <param name="max">最大値（レンジ内で最大の値）</param>
        /// <returns>レンジ内ならTrue</returns>
        public bool IsFitIntoRange(int min, int max)
        {
            return (x >= min) && (x <= max) && (y >= min) && (y <= max) && (z >= min) && (z <= max);
        }

        public Index3D(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Index3D(int xyz)
        {
            this.x = xyz;
            this.y = xyz;
            this.z = xyz;
        }

        public override bool Equals(object obj)
        {
            return obj is Index3D d &&
                   x == d.x &&
                   y == d.y &&
                   z == d.z;
        }

        public override int GetHashCode()
        {

            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
        }

        public override string ToString()
        {
            return "Index{ " + x.ToString() + " , " + y.ToString() + " , " + z.ToString() + " }";
        }

        public static bool operator ==(Index3D left, Index3D rigft)
        {
            return (left.x == rigft.x) && (left.y == rigft.y) && (left.z == rigft.z);
        }

        public static bool operator !=(Index3D left, Index3D rigft)
        {
            return (left.x != rigft.x) || (left.y != rigft.y) || (left.z != rigft.z);
        }

        public static Index3D operator +(Index3D left, Index3D rigft)
        {
            return new Index3D(left.x + rigft.x, left.y + rigft.y, left.z + rigft.z);
        }

        public static Index3D operator -(Index3D left, Index3D rigft)
        {
            return new Index3D(left.x - rigft.x, left.y - rigft.y, left.z - rigft.z);
        }
    }

    /// <summary>
    /// BoxのGameObjectとスクリプトをまとめたクラス
    /// </summary>
    public class BoxData
    {
        public BoxBase Script { get; private set; }
        public GameObject Object { get; private set; }

        public BoxData(GameObject gameObject, BoxBase script)
        {
            Script = script;
            Object = gameObject;
        }
    }

    /// <summary>
    /// DirectionOffsetの添え字アクセス用
    /// </summary>
    public const int X = 0, Y = 1, Z = 2;

    /// <summary>
    /// ChunkScript.Direction番目の中身は
    /// その方向へのオフセットになっている
    /// <summary>
    public readonly int[][] DirectionOffset = new int[(int)Direction.Max + 1][]
    {
       new int[]{-1, 1, 1 },//上左前
       new int[]{ 0, 1, 1 },//上前
       new int[]{ 1, 1, 1 },//上右前
       new int[]{ 1, 1, 0 },//上右
       new int[]{ 1, 1,-1 },//上右後
       new int[]{ 0, 1,-1 },//上後
       new int[]{-1, 1,-1 },//上左後
       new int[]{-1, 1, 0 },//上左
       new int[]{ 1, 0, 1 },//右前
       new int[]{-1, 0, 1 },//左前
       new int[]{ 0, 1, 0 },//上
       new int[]{-1, 0, 0 },//左
       new int[]{ 0, 0, 1 },//前
       new int[]{ 0, 0,-1 },//後
       new int[]{ 1, 0, 0 },//右
       new int[]{ 0,-1, 0 },//下
       new int[]{ 1, 0,-1 },//右後
       new int[]{-1, 0,-1 },//左後
       new int[]{ 1,-1, 0 },//下右
       new int[]{-1,-1, 1 },//下右前
       new int[]{ 0,-1, 1 },//下前
       new int[]{ 1,-1, 1 },//下左前
       new int[]{-1,-1, 0 },//下左
       new int[]{ 1,-1,-1 },//下左後
       new int[]{ 0,-1,-1 },//下後
       new int[]{-1,-1,-1 } //下右後
    };

    /// <summary>
    /// affiliationChunksフィールド用
    /// 自分の位置
    /// </summary>
    public readonly int[] DirectionOffsetCenter = new int[] { 1, 1, 1 };

    /// <summary>
    /// チャンクを生成する（重複での生成をブロックする機能付き）
    /// </summary>
    /// <param name="position">生成位置</param>
    /// <returns>生成できていればGameObject、生成失敗でnull</returns>
    public static GameObject Create(Vector3 position)
    {
        //インデックスを計算
        Index3D chunkIndex = CalcWorldIndex(position);

        //TODO:Debug
        if (chunkIndex.y < 0)
        {
            return null;
        }
        if (chunkIndex.y > 1)
        {
            return null;
        }

        //インデックスが登録されていない（作られていない）か確認
        if (ChunkManagerScript.chunks.ContainsKey(chunkIndex))
        {
            return null;
        }

        //prefab
        var prefab = PrefabManager.Instance.GetPrefab("Chunk");
        //チャンク生成
        GameObject chunk = Instantiate(prefab, position, Quaternion.identity);
        //スクリプトの取得
        ChunkScript script = chunk.GetComponent<ChunkScript>();
        //インデックスを設定
        script.WorldIndex = chunkIndex;
        //名前を変更
        chunk.name = prefab.name + script.WorldIndex.ToString();

        //チャンクを返す
        return chunk;
    }

    /// <summary>
    /// 座標をワールドでのブロック単位の座標に変換する
    /// </summary>
    /// <param name="position">変換する座標</param>
    /// <returns>ワールドでのブロック単位の座標</returns>
    public static Index3D CalcWorldIndex(Vector3 position)
    {
        Index3D index = new Index3D
            (
                (int)(Mathf.Round(position.x / chunkSize)),
                (int)(Mathf.Round(position.y / chunkSize)),
                (int)(Mathf.Round(position.z / chunkSize))
            );
        return index;
    }

    /// <summary>
    /// Boxのインデックスからチャンクへのオフセットを計算
    /// チャンク内の場合（0,0,0）
    /// X+方向にはみ出している場合（1,0,0）
    /// 2個以上またいでいる場合は不具合のもとなので使用禁止
    /// </summary>
    /// <param name="index">Boxのインデックス</param>
    /// <returns>チャンクへのオフセット</returns>
    public static Index3D CalcChunkOffsetFromBoxIndex(Index3D index)
    {
        return new Index3D(
            Math.Sign((index.x >= 0 && chunkSize > index.x ? 0 : index.x) / (float)chunkSize),
            Math.Sign((index.y >= 0 && chunkSize > index.y ? 0 : index.y) / (float)chunkSize),
            Math.Sign((index.z >= 0 && chunkSize > index.z ? 0 : index.z) / (float)chunkSize)
            );
    }

    /// <summary>
    /// Boxを生成して自動でチャンクに所属させる
    /// </summary>
    /// <param name="prefab">prefab</param>
    /// <param name="position">生成座標</param>
    /// <param name="rotation">回転</param>
    /// <returns></returns>
    public static GameObject CreateBoxAndAutoBelongToChunk(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        //まずBoxの生成位置のチャンクを調べる
        Index3D index = CalcWorldIndex(position);
        //チャンクがないので作れない
        if (!ChunkManagerScript.chunks.ContainsKey(index)) return null;
        //チャンクを取得
        ChunkScript chunk = ChunkManagerScript.chunks[index];

        //生成可能なら生成
        GameObject box = chunk.CreateBoxAndBelongToChunk(prefab, position, rotation);
        
        return box;
    }
}
