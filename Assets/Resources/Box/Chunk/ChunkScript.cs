using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JsonConverter.ChunkJsonConverter;
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
    [Serializable] public struct Index3D
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

    /// <summary>
    /// Boxを生成して自動でチャンクに所属させる
    /// </summary>
    /// <param name="prefab">prefab</param>
    /// <param name="position">生成座標</param>
    /// <param name="rotation">回転</param>
    /// <returns></returns>
    public static GameObject CreateBoxAndAutomBelongToChunk(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        //まずBoxの生成位置のチャンクを調べる
        Index3D index = CalcWorldIndex(position);
        //チャンクがないので作れない
        if (!ChunkManagerScript.chunks.ContainsKey(index)) return null;
        //チャンクを取得
        ChunkScript chunk = ChunkManagerScript.chunks[index];
        //ローカル座標を計算
        Vector3 localPosition = (position - chunk.transform.position);
        //ローカルインデックスを計算
        Index3D localIndex = new Index3D
            (
                (int)(Mathf.Floor(localPosition.x + (chunkSize / 2 - boxSize / 2))),
                (int)(Mathf.Floor(localPosition.y + (chunkSize / 2 - boxSize / 2))),
                (int)(Mathf.Floor(localPosition.z + (chunkSize / 2 - boxSize / 2)))
            );
        //範囲外なら作らない
        if (!localIndex.IsFitIntoRange(0, chunkSize - 1)) return null;
        //チャンク内に生成不可なら生成しない
        if (!chunk.CanSpawnBox(localIndex)) return null;

        //ここまで来たらつくる
        GameObject box = BoxBase.Create(chunk, prefab, position, rotation);
        //チャンクを親に
        box.transform.parent = chunk.transform;
        //チャンクに追加
        chunk.boxDatas[localIndex.x, localIndex.y, localIndex.z] = new BoxData(box, box.GetComponent<BoxBase>());

        //チャンクですでに変更が行われているか取得
        BoxSaveData change = chunk.changes.Find(data => data.Index == localIndex);
        //変更があれば削除
        chunk.changes.Remove(change);
        //生成するBoxが自動生成と同じものであれば保持しない
        if (chunk.boxGenerateData[localIndex.x, localIndex.y, localIndex.z] != prefab.name)
        {
            //追加
            chunk.changes.Add(new BoxSaveData(prefab.name, localIndex, box.transform.rotation));
        }

        return box;
    }

    //チャンクの一辺のサイズ
    private const int chunkSize = 32;
    //Boxのサイズ
    private const float boxSize = 1;
    //自動生成距離
    private const int far = 1;
    //ノイズ解像度（平行）
    private const float mapResolutionHorizontal = 50.0f;
    //ノイズ解像度（垂直）
    private const float mapResolutionVertical = 25.0f;

    //Playerのいるインデックス
    private static Index3D PlayerIndex;

    //初期化完了フラグ
    public bool IsTerrainGenerateCompleted { get; private set; }
    //キルタイマーが入っているかどうか
    public bool IsKillTimerSet { get; private set; }
    //チャンクの配列インデックス
    public Index3D worldIndex { get; private set; }

    //隣接チャンク
    private ChunkScript[,,] affiliationChunks;
    //チャンク内のBoxデータ
    private BoxData[,,] boxDatas;
    //Boxの生成データ
    private string[,,] boxGenerateData;
    //Boxに対する変更点
    [SerializeField] private List<BoxSaveData> changes;

    //チャンクのX,Y,Zサイズ
    [SerializeField] private Vector3 size;
    //チャンクの中心座標
    [SerializeField] private Vector3 center;
    //生成するオブジェクト
    [SerializeField] private GameObject prefab;

    private void Start()
    {
        //チャンク内のBoxデータ配列
        boxDatas = new BoxData[chunkSize, chunkSize, chunkSize];
        //チャンク内に生成したBoxのデータ
        boxGenerateData = new string[chunkSize, chunkSize, chunkSize];
        //チャンクの変更点データをロード
        ChunkSaveData load = Converter.LoadSaveData(worldIndex);
        //セーブデータがあれば保持
        if ((load != null) && load.Changes != null)
        {
            changes.AddRange(load?.Changes);
        }

        //隣接チャンク
        affiliationChunks ??= new ChunkScript[3, 3, 3];

        //チャンクサイズ
        size = new Vector3(chunkSize, chunkSize, chunkSize);
        //中心座標
        center = transform.position;

        //コライダーのサイズをセット
        GetComponent<BoxCollider>().size = size;

        //地形生成
        StartCoroutine(GenerateTerrain());

        //近くのチャンクを生成
        StartCoroutine(GenerateChunkIfNeeded());

        //１０秒おきにPlayerからの距離を見て離れすぎていたら削除する
        InvokeRepeating(nameof(KillTimerSetIfNeeded), 0, 10);
    }

    private void OnDestroy()
    {
        //チャンクデータの初期化
        for (int x = 0; x < boxDatas.GetLength(0); x++)
        {
            for (int y = 0; y < boxDatas.GetLength(1); y++)
            {
                for (int z = 0; z < boxDatas.GetLength(2); z++)
                {
                    boxDatas[x, y, z] = null;
                }
            }
        }

        //チャンクデータの保存
        Converter.UpdateSavedata(worldIndex, changes);

        //隣接チャンクに破壊通知
        for (int direction = (int)Direction.First; direction <= (int)Direction.Max; direction++)
        {
            int x = DirectionOffset[direction][X] + DirectionOffsetCenter[X];
            int y = DirectionOffset[direction][Y] + DirectionOffsetCenter[Y];
            int z = DirectionOffset[direction][Z] + DirectionOffsetCenter[Z];

            affiliationChunks?[x, y, z]?.DestroyNotification(Direction.Max - direction);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Player空しか受け付けない
        if (other.name != "Player") return;
        //自分自身にPlayerの移動通知
        PlayerMoveNotification();
        //Playerのインデックスを自分に
        PlayerIndex = worldIndex;

        //全方位分ループ
        for (int direction = (int)Direction.First; direction <= (int)Direction.Max; direction++)
        {
            //隣接チャンクの座標を求める
            int x = DirectionOffset[direction][X] + DirectionOffsetCenter[X];
            int y = DirectionOffset[direction][Y] + DirectionOffsetCenter[Y];
            int z = DirectionOffset[direction][Z] + DirectionOffsetCenter[Z];

            //チャンクがなかったら
            if (affiliationChunks?[x, y, z] is null)
            {
                Vector3 offset = new Vector3(DirectionOffset[direction][X], DirectionOffset[direction][Y], DirectionOffset[direction][Z]);
                offset *= chunkSize;
                Index3D index3D = CalcWorldIndex(center + offset);
                //全チャンクデータから探す
                if (ChunkManagerScript.chunks.ContainsKey(index3D))
                {
                    affiliationChunks[x, y, z] = ChunkManagerScript.chunks[index3D];
                }
            }
            //移動通知
            affiliationChunks?[x, y, z]?.PlayerMoveNotification();
        }
    }

    private void OnDrawGizmos()
    {
        //境目が分からないのでデバッグ表示
        if (worldIndex.y != 0) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, size);
    }

    /// <summary>
    /// 地形を生成する
    /// </summary>
    private IEnumerator GenerateTerrain()
    {
        //地表でなければ
        if (worldIndex.y != 0)
        {
            //地形生成完了フラグを立てる
            IsTerrainGenerateCompleted = true;
            yield break;
        }

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
                //地形の一番上の位置
                int top = (int)Mathf.Round(mapResolutionVertical * noise);
                //最低面から一番上のBoxまでを埋める
                for (int y = 0; y < chunkSize; y++)
                {
                    //生成予定の場所に変更点があるか取得
                    BoxSaveData change = changes.Find(data => data.Index == new Index3D(x, y, z));
                    GameObject createPrefab = null;
                    //yがtopより小さい時
                    if (y < top)
                    {
                        //通常生成のprefabを入れておく
                        createPrefab = prefab;
                        //地形自動生成のデータのみ保持
                        boxGenerateData[x, y, z] = prefab.name;
                    }

                    //変更点があったら
                    if (change != null)
                    {
                        //Nameがあれば何か置かれている
                        if (change.Name.Length > 0)
                        {
                            //変更されたBoxのprefabをロード
                            createPrefab = PrefabManager.Instance.GetPrefab(change.Name);
                        }
                        else
                        {
                            //Boxの削除データだったときなので何も生成しない
                            createPrefab = null;
                        }
                    }

                    //createPrefabがnullでなければ自動生成かセーブデータからの生成
                    if (createPrefab != null)
                    {
                        //生成
                        GameObject box = BoxBase.Create(
                            this,
                            createPrefab,
                            new Vector3(
                            boxSize * x + boxSize / 2.0f - offset.x,
                            boxSize * y + boxSize / 2.0f - offset.y,
                            boxSize * z + boxSize / 2.0f - offset.z),
                            Quaternion.identity);
                        //親に設定
                        box.transform.parent = gameObject.transform;

                        //データを保存
                        boxDatas[x, y, z] = new BoxData(box, box.GetComponent<BoxBase>());
                    }
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
        for (int direction = (int)Direction.First; direction <= (int)Direction.Max; direction++)
        {
            int x = DirectionOffset[direction][X] + DirectionOffsetCenter[X];
            int y = DirectionOffset[direction][Y] + DirectionOffsetCenter[Y];
            int z = DirectionOffset[direction][Z] + DirectionOffsetCenter[Z];

            //チャンクがなかったら
            if (affiliationChunks?[x, y, z] is null)
            {
                //自座標から生成座標へのオフセット
                Vector3 offset = new Vector3(
                    DirectionOffset[direction][X], 
                    DirectionOffset[direction][Y], 
                    DirectionOffset[direction][Z]) * chunkSize;
                //生成する座標
                Vector3 position = center + offset;
                //生成するインデックス
                Index3D index3D = CalcWorldIndex(position);
                //全チャンクデータから探す
                if (ChunkManagerScript.chunks.ContainsKey(index3D))
                {
                    //見つかったら保持
                    affiliationChunks[x, y, z] = ChunkManagerScript.chunks[index3D];
                }
                else
                {
                    //Playerから離れすぎていなければ
                    if (IsFitIntoFar(index3D))
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
            Debug.Log("Kill Timer Cancel: " + worldIndex.ToString());
            CancelInvoke(nameof(DestroyThis));
            IsKillTimerSet = false;
        }
        //必要に応じてチャンクの生成と近隣チャンクへ移動通知
        StartCoroutine(GenerateChunkIfNeeded());
    }

    /// <summary>
    /// 周辺チャンクが削除された際に呼ばれる
    /// </summary>
    /// <param name="direction"></param>
    private void DestroyNotification(Direction direction)
    {
        int x = DirectionOffset[(int)direction][X] + DirectionOffsetCenter[X];
        int y = DirectionOffset[(int)direction][Y] + DirectionOffsetCenter[Y];
        int z = DirectionOffset[(int)direction][Z] + DirectionOffsetCenter[Z];

        affiliationChunks[x, y, z] = null;
    }

    /// <summary>
    /// チャンクの中心座標とPlayerが
    /// chunkSize * far離れていた場合
    /// キルタイマーを入れる
    /// </summary>
    private void KillTimerSetIfNeeded()
    {
        //キルタイマーが入っているときは処理しない
        if (IsKillTimerSet) return;

        //Playerと離れすぎていれば
        if (!IsFitIntoFar(worldIndex))
        {
            //削除予約
            Debug.Log("Kill Timer Begin: " + worldIndex.ToString());
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
            (Math.Abs(distance.y) <= far) &&
            (Math.Abs(distance.z) <= far))
        {
            return true;
        }
        return false;
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
        x += DirectionOffset[(int)direction][X];
        y += DirectionOffset[(int)direction][Y];
        z += DirectionOffset[(int)direction][Z];

        try
        {
            //インデックス外であればチャンク外なのでBoxが存在する判定
            if (x >= chunkSize || x < 0) return null;
            if (y >= chunkSize || y < 0) return null;
            if (z >= chunkSize || z < 0) return null;

            //Boxが存在しているかを返す
            return boxDatas?[x, y, z];
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
        x += DirectionOffset[(int)direction][X];
        y += DirectionOffset[(int)direction][Y];
        z += DirectionOffset[(int)direction][Z];

        //インデックス外であればチャンク外なのでBoxが存在する判定
        if (x >= chunkSize || x < 0) return true;
        if (y >= chunkSize || y < 0) return true;
        if (z >= chunkSize || z < 0) return true;

        //Boxが存在しているかを返す
        return boxDatas?[x, y, z] != null;
    }

    /// <summary>
    /// Boxを指定された３次元インデックスの場所に生成できるかどうか
    /// </summary>
    /// <param name="index">Boxを生成したい場所</param>
    /// <returns></returns>
    public bool CanSpawnBox(Index3D index)
    {
        return index.IsFitIntoRange(0, chunkSize) && (boxDatas[index.x, index.y, index.z] == null);
    }

    /// <summary>
    /// Boxを削除
    /// </summary>
    /// <param name="box"></param>
    public void DestroyBox(GameObject box)
    {
        //インデックスの計算
        Index3D index = new Index3D(
                                (int)Mathf.Floor(box.transform.localPosition.x + (chunkSize / 2 - boxSize / 2)),
                                (int)Mathf.Floor(box.transform.localPosition.y + (chunkSize / 2 - boxSize / 2)),
                                (int)Mathf.Floor(box.transform.localPosition.z + (chunkSize / 2 - boxSize / 2))
                            );
        //保持データを消す
        boxDatas[index.x, index.y, index.z] = null;
        //削除
        Destroy(box);


        //チャンクですでに変更が行われているか取得
        BoxSaveData change = changes.Find(data => data.Index == index);
        //変更があれば削除
        changes.Remove(change);
        //生成時に何もなかったところであれば変更を保持しない
        if (!string.IsNullOrEmpty(boxGenerateData[index.x, index.y, index.z]))
        {
            //追加
            changes.Add(new BoxSaveData("", index, box.transform.rotation));
        }
    }
}
