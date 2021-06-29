using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkScript : MonoBehaviour
{
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
    public struct Index3D
    {
        public int x;
        public int y;
        public int z;

        public Index3D(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
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

        public static bool operator ==(Index3D left, Index3D rigft)
        {
            return (left.x == rigft.x) && (left.y == rigft.y) && (left.z == rigft.z);
        }

        public static bool operator !=(Index3D left, Index3D rigft)
        {
            return (left.x != rigft.x) || (left.y != rigft.y) || (left.z != rigft.z);
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

        //インデックスが登録されていない（作られていない）か確認
        if (ChunkManagerScript.chunks.ContainsKey(chunkIndex))
        {
            return null;
        }

        //チャンク生成
        GameObject chunk = Instantiate(PrefabManager.Instance.GetPrefab("Chunk"), position, Quaternion.identity);
        //スクリプトの取得
        ChunkScript script = chunk.GetComponent<ChunkScript>();
        //インデックスを設定
        script.worldIndex = chunkIndex;

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

    //チャンクの一辺のサイズ
    private const int chunkSize = 64;
    //ノイズ解像度（平行）
    private const float mapResolutionHorizontal = 50.0f;
    //ノイズ解像度（垂直）
    private const float mapResolutionVertical = 45.0f;
    //Boxのサイズ
    private const float boxSize = 1;
    //自動生成距離
    private const float far = 1.5f;

    //初期化完了フラグ
    public bool IsTerrainGenerateCompleted { get; private set; }
    //キルタイマーが入っているかどうか
    public bool IsKillTimerSet { get; private set; }
    //チャンクの配列インデックス
    public Index3D worldIndex { get; private set; }

    //チャンク内のBoxデータ
    private BoxData[,,] chunkData;
    //隣接チャンク
    private ChunkScript[,,] affiliationChunks;

    //チャンクのX,Y,Zサイズ
    [SerializeField] private Vector3 size;
    //チャンクの中心座標
    [SerializeField] private Vector3 center;
    //生成するオブジェクト
    [SerializeField] private GameObject prefab;

    private void Start()
    {
        //チャンク内のBoxデータ配列
        chunkData = new BoxData[chunkSize, chunkSize, chunkSize];

        //隣接チャンク
        affiliationChunks ??= new ChunkScript[3, 3, 3];

        //チャンクサイズ
        size = new Vector3(chunkSize, chunkSize, chunkSize);
        //中心座標
        center = transform.position;

        //地表チャンクであれば地形を生成
        if (worldIndex.y == 0)
        {
            StartCoroutine(GenerateTerrain());
        }
        else
        {
            //地表チャンクでない場合地形生成完了フラグを入れておく
            IsTerrainGenerateCompleted = true;
        }

        //近くのチャンクを生成
        StartCoroutine(GenerateChunkIfNeeded());
    }

    private void Update()
    {
        //１０秒おきにPlayerからの距離を見て離れすぎていたら削除する
        InvokeRepeating(nameof(KillTimerSetIfNeeded), 0, 10);
    }

    private void OnDestroy()
    {
        //チャンクデータの初期化
        for (int x = 0; x < chunkData.GetLength(0); x++)
        {
            for (int y = 0; y < chunkData.GetLength(1); y++)
            {
                for (int z = 0; z < chunkData.GetLength(2); z++)
                {
                    chunkData[x, y, z] = null;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Player空しか受け付けない
        if (other.name != "Player") return;
        //自分自身にPlayerの移動通知
        PlayerMoveNotification();

        //全方位分ループ
        for (int direction = (int)Direction.First; direction <= (int)Direction.Max; direction++)
        {
            //隣接チャンクの座標を求める
            int x = DirectionOffset[direction][0] + DirectionOffsetCenter[0];
            int y = DirectionOffset[direction][1] + DirectionOffsetCenter[1];
            int z = DirectionOffset[direction][2] + DirectionOffsetCenter[2];

            //チャンクがなかったら
            if (affiliationChunks?[x, y, z] is null)
            {
                Vector3 offset = new Vector3(DirectionOffset[direction][0], DirectionOffset[direction][1], DirectionOffset[direction][2]);
                offset *= chunkSize;
                Index3D index3D = CalcWorldIndex(center + offset);
                //全チャンクデータから探す
                if (ChunkManagerScript.chunks.ContainsKey(index3D))
                {
                    affiliationChunks[x, y, z] = ChunkManagerScript.chunks[index3D];
                }
            }
            //移動通知
            affiliationChunks[x, y, z]?.PlayerMoveNotification();
        }
    }

    /// <summary>
    /// Boxを削除
    /// </summary>
    /// <param name="box"></param>
    public void DestroyBox(GameObject box)
    {
        int x = (int)Mathf.Floor(box.transform.localPosition.x + (chunkSize / 2 - boxSize / 2));
        int y = (int)Mathf.Floor(box.transform.localPosition.y + (chunkSize / 2 - boxSize / 2));
        int z = (int)Mathf.Floor(box.transform.localPosition.z + (chunkSize / 2 - boxSize / 2));
        chunkData[x, y, z] = null;
        Destroy(box);
    }

    /// <summary>
    /// 地形を生成する
    /// </summary>
    private IEnumerator GenerateTerrain()
    {
        //バウンディングボックスのX,Y,Z起点位置までのオフセット
        Vector3 offset = size / 2 - center;

        //chunkSize回
        for (int x = 0; x < chunkSize; x++)
        {
            //chunkSize回
            for (int z = 0; z < chunkSize; z++)
            {
                //ノイズ生成
                float noise = Mathf.PerlinNoise(
                    (transform.position.x + x) / mapResolutionHorizontal,
                    (transform.position.z + z) / mapResolutionHorizontal);
                //一番上の位置
                int top = (int)Mathf.Round(mapResolutionVertical * noise);
                //最低面から一番上のBoxまでを埋める
                for (int y = 0; y < top; y++)
                {
                    //生成
                    GameObject box = BoxBase.Create(
                        this,
                        prefab,
                        new Vector3(
                        boxSize * x + boxSize / 2.0f - offset.x,
                        boxSize * y + boxSize / 2.0f - offset.y,
                        boxSize * z + boxSize / 2.0f - offset.z),
                        Quaternion.identity);
                    //親に設定
                    box.transform.parent = gameObject.transform;

                    //データを保存
                    chunkData[x, y, z] = new BoxData(box, box.GetComponent<BoxBase>());
                }
            }
            yield return null;
        }
        //地形生成完了フラグを立てる
        IsTerrainGenerateCompleted = true;
    }

    /// <summary>
    /// 必要に応じて周辺にチャンクを生成して保持する
    /// 同時にPlayerの移動通知を出す
    /// </summary>
    private IEnumerator GenerateChunkIfNeeded()
    {
        //Playerとの距離を確かめる
        float playerDistanceMax = chunkSize * far;

        for (int direction = (int)Direction.First; direction <= (int)Direction.Max; direction++)
        {
            int x = DirectionOffset[direction][0] + DirectionOffsetCenter[0];
            int y = DirectionOffset[direction][1] + DirectionOffsetCenter[1];
            int z = DirectionOffset[direction][2] + DirectionOffsetCenter[2];

            //チャンクがなかったら
            if (affiliationChunks[x, y, z] is null)
            {
                Vector3 offset = new Vector3(DirectionOffset[direction][0], DirectionOffset[direction][1], DirectionOffset[direction][2]);
                offset *= chunkSize;
                Index3D index3D = CalcWorldIndex(center + offset);
                //全チャンクデータから探す
                if (ChunkManagerScript.chunks.ContainsKey(index3D))
                {
                    //見つかったら保持
                    affiliationChunks[x, y, z] = ChunkManagerScript.chunks[index3D];
                }
                else
                {
                    //生成する座標を計算
                    Vector3 position = new Vector3(
                        DirectionOffset[direction][0] * chunkSize,
                        DirectionOffset[direction][1] * chunkSize,
                        DirectionOffset[direction][2] * chunkSize);
                    //生成一のずれを補正
                    position += center;
                    //Playerから離れ過ぎていたら生成しない
                    if (Vector3.Distance(position, Camera.main.transform.position) <= playerDistanceMax)
                    {
                        //生成キューへ追加
                        ChunkManagerScript.CreateOrder(position);
                        //生成が完了するまで待つ
                        while (true)
                        {
                            //生成されていたとき
                            if (ChunkManagerScript.chunks.ContainsKey(index3D))
                            {
                                affiliationChunks[x, y, z] = ChunkManagerScript.chunks[index3D];
                                break;
                            }
                            //生成失敗していたとき
                            else if (ChunkManagerScript.IsCreateFailed(index3D))
                            {
                                break;
                            }
                            yield return null;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Playerがチャンクを移動したときに呼ばれる
    /// </summary>
    private void PlayerMoveNotification()
    {
        //キルタイマーが入っていたら
        if (IsKillTimerSet)
        {
            //削除タイマーをキャンセル
            CancelInvoke(nameof(DestroyThis));
            IsKillTimerSet = false;
        }
        //必要に応じてチャンクの生成と近隣チャンクへ移動通知
        StartCoroutine(GenerateChunkIfNeeded());
    }

    /// <summary>
    /// チャンクの中心座標とPlayerが
    /// chunkSize * far離れていた場合
    /// キルタイマーを入れる
    /// </summary>
    private void KillTimerSetIfNeeded()
    {
        //Playerとの距離を確かめる
        float playerDistanceMax = chunkSize * far;

        //Playerと離れすぎていたら
        if (Vector3.Distance(Camera.main.transform.position, transform.position) >= playerDistanceMax)
        {
            //削除予約
            Debug.Log("Kill Timer : " + transform.position.ToString());
            Invoke(nameof(DestroyThis), 10);
            IsKillTimerSet = true;
        }
    }

    /// <summary>
    /// 自分を削除
    /// 削除と一緒にデータベースからも消す
    /// 参照エラー対策用
    /// </summary>
    private void DestroyThis()
    {
        ChunkManagerScript.ForceDestroyChunk(gameObject);
    }

    /// <summary>
    /// 近隣のBoxを取得する
    /// </summary>
    /// <param name="baseBox">基準Box</param>
    /// <param name="direction">基準Boxからどの方向のBoxか</param>
    /// <returns>direction方向のBox</returns>
    public BoxData GetAdjacentBox(GameObject baseBox, Direction direction)
    {
        //baseBoxが格納されている３次元配列のインデックスを計算
        int x = (int)Mathf.Floor(baseBox.transform.localPosition.x + (chunkSize / 2 - boxSize / 2));
        int y = (int)Mathf.Floor(baseBox.transform.localPosition.y + (chunkSize / 2 - boxSize / 2));
        int z = (int)Mathf.Floor(baseBox.transform.localPosition.z + (chunkSize / 2 - boxSize / 2));

        //baseBoxからdirection方向のBoxのインデックスを計算
        x += DirectionOffset[(int)direction][0];
        y += DirectionOffset[(int)direction][1];
        z += DirectionOffset[(int)direction][2];

        try
        {
            //インデックス外であればチャンク外なのでBoxが存在する判定
            if (x >= chunkSize || x < 0) return null;
            if (y >= chunkSize || y < 0) return null;
            if (z >= chunkSize || z < 0) return null;

            //Boxが存在しているかを返す
            return chunkData?[x, y, z];
        }
        catch
        {
            Debug.Log(baseBox.ToString() + " : " + x.ToString() + " , " + y.ToString() + " , " + z.ToString());
        }


        return null;
    }

    /// <summary>
    /// 隣接するBoxがあるか確認する
    /// チャンク端の場合はTrue
    /// </summary>
    /// <param name="baseBox">基準Box</param>
    /// <param name="direction">基準Boxからどの方向のBoxか</param>
    /// <returns>direction方向のBoxが存在するか、チャンク端ならTrue</returns>
    public bool IsAdjacetBoxExist(GameObject baseBox, Direction direction)
    {
        //baseBoxが格納されている３次元配列のインデックスを計算
        int x = (int)Mathf.Floor(baseBox.transform.localPosition.x + (chunkSize / 2 - boxSize / 2));
        int y = (int)Mathf.Floor(baseBox.transform.localPosition.y + (chunkSize / 2 - boxSize / 2));
        int z = (int)Mathf.Floor(baseBox.transform.localPosition.z + (chunkSize / 2 - boxSize / 2));

        //baseBoxからdirection方向のBoxのインデックスを計算
        x += DirectionOffset[(int)direction][0];
        y += DirectionOffset[(int)direction][1];
        z += DirectionOffset[(int)direction][2];

        //インデックス外であればチャンク外なのでBoxが存在する判定
        if (x >= chunkSize || x < 0) return true;
        if (y >= chunkSize || y < 0) return true;
        if (z >= chunkSize || z < 0) return true;

        //Boxが存在しているかを返す
        return chunkData?[x, y, z] != null;
    }
}
