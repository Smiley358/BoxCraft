using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JsonConverter.ChunkJsonConverter;
using UnityEngine;

public partial class ChunkScript : MonoBehaviour
{
    //初期化完了フラグ
    public bool IsTerrainGenerateCompleted { get; private set; }
    //キルタイマーが入っているかどうか
    public bool IsKillTimerSet { get; private set; }
    //チャンクの配列インデックス
    public Index3D worldIndex { get; private set; }

    //隣接チャンク
    private ChunkScript[,,] adjacentChunks;
    //チャンク内のBoxデータ
    private BoxData[,,] boxDatas;
    //Boxの生成データ
    private string[,,] boxGenerateData;
    //メッシュ結合フラグ
    private bool isCombineMesh;

    //Boxに対する変更点
    [SerializeField] private List<BoxSaveData> changes;
    //チャンクのX,Y,Zサイズ
    [SerializeField] private Vector3 size;
    //チャンクの中心座標
    [SerializeField] private Vector3 center;
    //生成するオブジェクト
    [SerializeField] private GameObject prefab;

    private void Awake()
    {
        //チャンク内のBoxデータ配列
        boxDatas = new BoxData[chunkSize, chunkSize, chunkSize];

        //チャンク内に生成したBoxのデータ
        boxGenerateData = new string[chunkSize, chunkSize, chunkSize];

        //隣接チャンク
        adjacentChunks ??= new ChunkScript[3, 3, 3];

        //チャンクサイズ
        size = new Vector3(chunkSize, chunkSize, chunkSize);

        //中心座標
        center = transform.position;
    }

    private void Start()
    {
        //チャンクの変更点データをロード
        ChunkSaveData load = Converter.LoadSaveData(worldIndex);
        //セーブデータがあれば保持
        if ((load != null) && load.Changes != null)
        {
            changes.AddRange(load?.Changes);
        }

        //コライダーのサイズをセット
        GetComponent<BoxCollider>().size = size;

        //地形生成
        StartCoroutine(GenerateTerrain());

        //近くのチャンクを生成
        StartCoroutine(GenerateChunkIfNeeded());

        //１０秒おきにPlayerからの距離を見て離れすぎていたら削除する
        if (worldIndex.y != 0)
        {
            InvokeRepeating(nameof(KillTimerSetIfNeeded), 0, 10);
        }
        else
        {
            InvokeRepeating(nameof(KillTimerSetIfNeeded), 0, 10);
        }
    }

    private void LateUpdate()
    {
        //メッシュ結合フラグ入ってたら
        if (isCombineMesh)
        {
            //メッシュの更新（再結合）
            CombineMesh();
            //フラグを下げる
            isCombineMesh = false;
        }
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

            adjacentChunks[x, y, z]?.DestroyNotification(Direction.Max - direction);
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
            if (adjacentChunks[x, y, z] == null)
            {
                //参照が残っているスクリプト
                if (!(adjacentChunks[x, y, z] is null))
                {
                    //参照を消す
                    adjacentChunks[x, y, z] = null;
                }

                Vector3 offset = new Vector3(DirectionOffset[direction][X], DirectionOffset[direction][Y], DirectionOffset[direction][Z]);
                offset *= chunkSize;
                Index3D index3D = CalcWorldIndex(center + offset);
                //全チャンクデータから探す
                if (ChunkManagerScript.chunks.ContainsKey(index3D))
                {
                    adjacentChunks[x, y, z] = ChunkManagerScript.chunks[index3D];
                }
            }
            //移動通知
            adjacentChunks[x, y, z]?.PlayerMoveNotification();
        }
    }

    private void OnDrawGizmos()
    {
        //境目が分からないのでデバッグ表示
        if (worldIndex.y == 0)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.gray;
        }
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

        //地表のBoxと追加されたBoxを生成
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                //ノイズ生成
                float noise = Mathf.PerlinNoise(
                    (transform.position.x + x) / mapResolutionHorizontal,
                    (transform.position.z + z) / mapResolutionHorizontal);
                //地形の一番上の位置
                int top = (int)Mathf.Round(mapResolutionVertical * noise);
                for (int y = 0; y < chunkSize; y++)
                {
                    //生成予定の場所に変更点があるか取得
                    BoxSaveData change = changes.Find(data => data.Index == new Index3D(x, y, z));
                    GameObject createPrefab = null;
                    //yがtopより小さい時
                    if (y <= top)
                    {
                        //地表のBoxのみ
                        if (y == top)
                        {
                            //通常生成のprefabを入れておく
                            createPrefab = prefab;
                        }
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
                        GameObject box = CreateBoxAndBelongToChunk(
                            createPrefab,
                            CalcWorldPositionFromBoxLocalIndex(new Index3D(x, y, z)),
                            Quaternion.identity);
                    }
                }
            }
        }

        //地形生成完了フラグを立てる
        IsTerrainGenerateCompleted = true;
        Debug.Log("Generate Terrain : " + worldIndex.ToString());

        //全チャンクの生成待ち
        while (true)
        {
            if (ChunkManagerScript.IsCompleted) break;
            yield return null;
        }

        //削除されたBox周辺のBoxの生成
        var destroys = changes.FindAll(data => data.Name == "");
        foreach (var destroyBox in destroys)
        {
            //座標
            Vector3 position = CalcWorldPositionFromBoxLocalIndex(destroyBox.Index);

            //周辺に必要であればBoxを生成
            CreateAdjacentBoxIfNeeded(position);
        }

        //チャンク内の全Box
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    //必要に応じて無効化させる
                    var script = boxDatas[x, y, z]?.Script;
                    if (script != null)
                    {
                        script.DisableIfNeeded();
                    }
                }
            }
            yield return null;
        }

        //メッシュを結合
        CombineMesh();
    }

    /// <summary>
    /// 必要に応じて周辺にチャンクを生成して保持する
    /// 同時にPlayerの移動通知を出す
    /// </summary>
    private IEnumerator GenerateChunkIfNeeded()
    {
        //生成予約に出したIndexリスト
        List<Index3D> createList = new List<Index3D>();

        //direction方向へのオフセット計算
        Func<int, Index3D> calcDirectionOffsetDelegate = (direction) =>
        {
            return new Index3D(
                DirectionOffset[direction][X] + DirectionOffsetCenter[X],
                DirectionOffset[direction][Y] + DirectionOffsetCenter[Y],
                DirectionOffset[direction][Z] + DirectionOffsetCenter[Z]
                );
        };
        //生成座標を計算する
        Func<int, Vector3> calcCreatePositionDelegate = (direction) =>
        {
            return new Vector3(
                    DirectionOffset[direction][X],
                    DirectionOffset[direction][Y],
                    DirectionOffset[direction][Z]) * chunkSize + center;
        };

        //全方位走査
        for (int direction = (int)Direction.First; direction <= (int)Direction.Max; direction++)
        {
            Index3D arrayIndex = calcDirectionOffsetDelegate(direction);

            //チャンクがなかったら
            if (adjacentChunks[arrayIndex.x, arrayIndex.y, arrayIndex.z] == null)
            {
                //参照が残っているスクリプト
                if (!(adjacentChunks[arrayIndex.x, arrayIndex.y, arrayIndex.z] is null))
                {
                    //参照を消す
                    adjacentChunks[arrayIndex.x, arrayIndex.y, arrayIndex.z] = null;
                }

                //生成する座標
                Vector3 position = calcCreatePositionDelegate(direction);
                //生成するインデックス
                Index3D index3D = CalcWorldIndex(position);
                //全チャンクデータから探す
                if (ChunkManagerScript.chunks.ContainsKey(index3D))
                {
                    //見つかったら保持
                    adjacentChunks[arrayIndex.x, arrayIndex.y, arrayIndex.z] = ChunkManagerScript.chunks[index3D];
                }
                else
                {
                    //Playerから離れすぎていなければ
                    if (IsFitIntoFar(index3D))
                    {
                        //生成キューへ追加
                        ChunkManagerScript.CreateOrder(position);
                        //生成キューへ追加したので保存
                        createList.Add(index3D);
                    }
                }
            }
        }

        //全チャンクの生成待ち
        while (true)
        {
            if (ChunkManagerScript.IsCompleted) break;
            yield return null;
        }
        Debug.Log("Create Chunk : " + worldIndex.ToString());

        for (int direction = (int)Direction.First; direction <= (int)Direction.Max; direction++)
        {
            Index3D arrayIndex = calcDirectionOffsetDelegate(direction);

            //チャンクがなかったら
            if (adjacentChunks[arrayIndex.x, arrayIndex.y, arrayIndex.z] == null)
            {
                //参照が残っているスクリプト
                if (!(adjacentChunks[arrayIndex.x, arrayIndex.y, arrayIndex.z] is null))
                {
                    //参照を消す
                    adjacentChunks[arrayIndex.x, arrayIndex.y, arrayIndex.z] = null;
                }

                //生成するインデックス
                Index3D index3D = CalcWorldIndex(calcCreatePositionDelegate(direction));
                //生成予約していたら
                if (createList.Contains(index3D))
                {
                    //全チャンクデータから探す
                    while (true)
                    {
                        //生成されていたとき
                        if (ChunkManagerScript.chunks.ContainsKey(index3D))
                        {
                            adjacentChunks[arrayIndex.x, arrayIndex.y, arrayIndex.z] = ChunkManagerScript.chunks[index3D];
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

    /// <summary>
    /// メッシュを結合
    /// </summary>
    private void CombineMesh()
    {
        //Boxからメッシュへのアクセサーを取得
        IMeshAccessor[] meshAccessors = transform.GetComponentsInChildren<IMeshAccessor>();

        //マテリアル
        Dictionary<string, Material> materials = new Dictionary<string, Material>();
        //同マテリアルのMeshFilterリスト
        Dictionary<string, List<MeshFilter>> meshFilters = new Dictionary<string, List<MeshFilter>>();

        //全IMeshAccessorに対して行う
        for (int i = 0; i < meshAccessors.Length; i++)
        {
            //マテリアル
            Material material = meshAccessors[i].GetMaterial();
            //マテリアル名、マテリアルがなかったらマテリアル無グループ"NonMaterial"
            string materialName = material == null ? "NonMaterial" : material.name;

            //マテリアルが登録されていない場合
            if (!meshFilters.ContainsKey(materialName))
            {
                //マテリアル登録
                List<MeshFilter> filterList = new List<MeshFilter>();
                meshFilters.Add(materialName, filterList);
                materials.Add(materialName, material);
            }

            //メッシュフィルターをリストに追加
            meshFilters[materialName].Add(meshAccessors[i].GetMeshFilter());
            //レンダリングを無効化
            var renderer = meshAccessors[i].GetMeshRenderer();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }

        //マテリアルごとにメッシュを結合
        foreach (var meshFilter in meshFilters)
        {
            //結合したメッシュを表示するゲームオブジェクト
            GameObject combineObject = null;
            //ゲームオブジェクト名
            string gameObjectName = "Combined:" + meshFilter.Key;
            //既にあれば取得
            combineObject = transform.Find(gameObjectName)?.gameObject;

            //無かったら作る
            if (combineObject == null)
            {
                combineObject = new GameObject();
                combineObject.name = gameObjectName;
                combineObject.transform.SetParent(transform);
                combineObject.transform.SetAsFirstSibling();
            }

            //MeshFilterを取得、なければアタッチ
            MeshFilter combinedMeshFilter = combineObject.GetComponent<MeshFilter>();
            if (combinedMeshFilter == null)
            {
                combinedMeshFilter = combineObject.AddComponent<MeshFilter>();
            }
            //MeshRendererを取得、なければアタッチ
            MeshRenderer combinedMeshRenderer = combineObject.GetComponent<MeshRenderer>();
            if (combinedMeshRenderer == null)
            {
                combinedMeshRenderer = combineObject.AddComponent<MeshRenderer>();
            }

            //結合するメッシュの配列
            List<MeshFilter> combineMeshFilters = meshFilter.Value;
            //結合用構造体
            CombineInstance[] combine = new CombineInstance[combineMeshFilters.Count];

            //結合するメッシュの情報をCombineInstanceに追加
            for (int i = 0; i < combineMeshFilters.Count; i++)
            {
                combine[i].mesh = combineMeshFilters[i].sharedMesh;
                combine[i].transform = combineMeshFilters[i].transform.localToWorldMatrix;
            }

            //結合したメッシュを作成したゲームオブジェクトアタッチ
            combinedMeshFilter.mesh = new Mesh();
            combinedMeshFilter.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            combinedMeshFilter.mesh.CombineMeshes(combine);

            //結合したメッシュにマテリアルをアタッチ
            combinedMeshRenderer.material = materials[meshFilter.Key];
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

        adjacentChunks[x, y, z] = null;
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
    /// 必要に応じて周辺にBoxを生成する
    /// </summary>
    /// <param name="position">中心座標</param>
    /// <returns>範囲外チャンク</returns>
    private List<ChunkScript> CreateAdjacentBoxIfNeeded(Vector3 position)
    {
        //はみ出たBoxの所属するチャンク(立方体の特性上３個以上にはならない)
        List<ChunkScript> chunks = new List<ChunkScript>();
        //隣接する６方向のBox
        BoxData[] createDatas = new BoxData[6];
        //隣接する6方向のBoxを必要に応じて自動生成
        for (int direction = (int)Direction.Top, i = 0; direction <= (int)Direction.Bottom; direction++, i++)
        {
            //隣接するBoxの取得(自動生成True)
            BoxData affiliationBox = GetAdjacentBox(position, (Direction)direction, true, true);
            //一時保存
            createDatas[i] = affiliationBox;
            //所属が自分じゃないとき
            if ((affiliationBox != null) && (affiliationBox.Script.parentChunk != this))
            {
                chunks.Add(affiliationBox.Script.parentChunk);
            }
        }
        //生成したBoxに対して自動無効化計算
        foreach (var boxData in createDatas)
        {
            boxData?.Script?.DisableIfNeeded();
        }

        return chunks;
    }

    /// <summary>
    /// Boxを生成してデータを保存する
    /// </summary>
    /// <param name="prefab">生成するBoxのprefab</param>
    /// <param name="position">生成位置</param>
    /// <param name="rotation">回転</param>
    public GameObject CreateBoxAndBelongToChunk(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        //生成インデックスの計算
        Index3D index = CalcLocalIndexFromBoxWorldPosition(position);
        //生成可能でなければ何もしない
        if (!CanSpawnBox(index)) return null;

        //生成
        GameObject box = BoxBase.Create(this, prefab, position, rotation);
        //親を設定
        box.transform.SetParent(transform);
        //データを保存
        boxDatas[index.x, index.y, index.z] = new BoxData(box, box.GetComponent<BoxBase>());
        //メッシュ再結合フラグを立てる
        isCombineMesh = true;

        //チャンクですでに変更が行われているか取得
        BoxSaveData change = changes.Find(data => data.Index == index);
        //変更があれば削除
        changes.Remove(change);
        //生成するBoxが自動生成と同じものでなければ
        if (boxGenerateData[index.x, index.y, index.z] != prefab.name)
        {
            //変更情報を追加
            changes.Add(new BoxSaveData(prefab.name, index, box.transform.rotation));
        }

        return box;
    }

    /// <summary>
    /// 近隣のBoxを取得する
    /// </summary>
    /// <param name="baseBox">基準Box</param>
    /// <param name="direction">基準Boxからどの方向のBoxか</param>
    /// <param name="autoCreate">無かったら作るかどうか</param>
    /// <param name="acrossChunk">チャンクをまたいで処理するかどうか</param>
    /// <returns>direction方向のBox</returns>
    public BoxData GetAdjacentBox(Vector3 position, Direction direction, bool autoCreate = false, bool acrossChunk = false)
    {
        //ローカル座標
        Vector3 localPosition = new Vector3(
            position.x + (chunkSize / 2),
            position.y + (chunkSize / 2),
            position.z + (chunkSize / 2)) - center;

        //direction方向へのオフセット
        Vector3 offset = new Vector3(
            DirectionOffset[(int)direction][X] * boxSize,
            DirectionOffset[(int)direction][Y] * boxSize,
            DirectionOffset[(int)direction][Z] * boxSize);

        //オフセットを加算
        localPosition += offset;

        //インデックス
        Index3D index = new Index3D(
            (int)Mathf.Floor(localPosition.x), 
            (int)Mathf.Floor(localPosition.y), 
            (int)Mathf.Floor(localPosition.z));

        //自チャンク外なら
        if (!index.IsFitIntoRange(0, chunkSize - 1))
        {
            //チャンク間をまたいで処理するとき
            if (acrossChunk)
            {
                //当該チャンク内のインデックスになおす
                Index3D convertIndex = new Index3D(
                (index.x + chunkSize) % chunkSize,
                (index.y + chunkSize) % chunkSize,
                (index.z + chunkSize) % chunkSize);
                //方向を割り出す
                Index3D directionOffset = new Index3D(
                    //1,0,-1に直す
                    Math.Sign((int)(index.x / (float)chunkSize)) + DirectionOffsetCenter[X],
                    Math.Sign((int)(index.y / (float)chunkSize)) + DirectionOffsetCenter[Y],
                    Math.Sign((int)(index.z / (float)chunkSize)) + DirectionOffsetCenter[Z]);

                ChunkScript acrossChunkScript = adjacentChunks[directionOffset.x, directionOffset.y, directionOffset.z];
                //近隣のチャンクデータがない場合
                if (acrossChunkScript == null)
                {
                    //保存されていないだけかもしれないのでマネージャーから探す
                    ChunkManagerScript.chunks.TryGetValue(worldIndex + directionOffset, out acrossChunkScript);
                    //保存
                    adjacentChunks[directionOffset.x, directionOffset.y, directionOffset.z] = acrossChunkScript;
                }

                //当該チャンクのBoxデータを取得
                return acrossChunkScript?.GetBox(convertIndex, autoCreate);
            }
            else
            {
                //チャンクをまたいで処理しないので無し判定
                return null;
            }
        }

        //Boxを返す
        return GetBox(index, autoCreate);
    }

    /// <summary>
    /// Boxを取得する
    /// </summary>
    /// <param name="index">取得したいBoxのインデックス</param>
    /// <param name="autoCreate">生成待機の場合生成するのかどうか</param>
    /// <returns>Boxデータ</returns>
    public BoxData GetBox(Index3D index, bool autoCreate = false)
    {
        //インデックス外であればチャンク外なのでBoxが存在しない判定
        if (!index.IsFitIntoRange(0, chunkSize - 1)) return null;

        //当該インデックスのボックスデータを取得
        BoxData boxData = boxDatas?[index.x, index.y, index.z];
        //Boxが存在せず、自動生成（autoCreate）がTrueなら
        if ((boxData == null) && autoCreate)
        {
            //変更データ
            BoxSaveData change = changes.Find(data => data.Index == index);
            //prefab名
            string prefabName = null;
            //回転
            Quaternion rotation = Quaternion.identity;

            //変更点がある
            if (change != null)
            {
                //削除じゃないとき
                prefabName = change.Name;
                rotation = change.Rotation;
            }
            //変更点がない
            else
            {
                //自動生成状況を取得
                prefabName = boxGenerateData?[index.x, index.y, index.z];
            }

            //prefabNameが空じゃなかったら
            if (!string.IsNullOrEmpty(prefabName))
            {
                //ワールド座標
                Vector3 worldPosition = CalcWorldPositionFromBoxLocalIndex(index);
                //生成されていないだけなので作る
                var box = CreateBoxAndBelongToChunk(PrefabManager.Instance.GetPrefab(prefabName), worldPosition, rotation);
                boxData = new BoxData(box, box.GetComponent<BoxBase>());
            }
        }

        //Boxを返す
        return boxData;
    }


    /// <summary>
    /// 隣接するBoxがあるか確認する
    /// チャンク外なら当該チャンク内を確認
    /// </summary>
    /// <param name="baseBox">基準Box</param>
    /// <param name="direction">基準Boxからどの方向のBoxか</param>
    /// <returns>direction方向のBoxが存在するか</returns>
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

        //インデックスへ
        Index3D index = new Index3D(x, y, z);

        //自チャンク外なら
        if (!index.IsFitIntoRange(0, chunkSize - 1))
        {
            //当該チャンク内のインデックスになおす
            Index3D convertIndex = new Index3D(
            (index.x + chunkSize) % chunkSize,
            (index.y + chunkSize) % chunkSize,
            (index.z + chunkSize) % chunkSize);
            //方向を割り出す
            Index3D directionOffset = new Index3D(
                //1,0,-1に直す
                Math.Sign((int)(index.x / (float)chunkSize)) + DirectionOffsetCenter[X],
                Math.Sign((int)(index.y / (float)chunkSize)) + DirectionOffsetCenter[Y],
                Math.Sign((int)(index.z / (float)chunkSize)) + DirectionOffsetCenter[Z]);

            //当該チャンクのBoxデータを取得
            return adjacentChunks[directionOffset.x, directionOffset.y, directionOffset.z]?.IsBoxExist(convertIndex) ?? false;
        }

        //Boxが存在しているかを返す
        return IsBoxExist(index);
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

        //範囲外は何もしない
        if (!index.IsFitIntoRange(0, chunkSize - 1)) return;

        //周辺に自動生成
        CreateAdjacentBoxIfNeeded(box.transform.position);


        //保持データを消す
        boxDatas[index.x, index.y, index.z] = null;
        //削除
        Destroy(box);
        //メッシュ再結合フラグを立てる
        isCombineMesh = true;

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
