using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// BOXの基底クラス
/// このクラスを継承すればBOXとしての最低限の機能を保証する
/// </summary>
public abstract class BoxBase : MonoBehaviour, IAttackableObject, IItemizeObject, IMeshAccessor
{
    /// <summary>
    /// Boxを生成して所属チャンクをセットする
    /// </summary>
    /// <param name="chunk">所属チャンク</param>
    /// <param name="box">prefab</param>
    /// <param name="position">生成座標</param>
    /// <param name="rotation">回転</param>
    /// <returns>生成したBox</returns>
    public static GameObject Create(ChunkScript chunk, GameObject box, Vector3 position, Quaternion rotation)
    {
        //チャンクに所属しないBoxは許可しない
        if (chunk is null) return null;
        // 生成位置の変数の座標にブロックを生成
        GameObject madeBox = Instantiate(box, position, rotation);
        if (madeBox == null)
        {
            return null;
        }
        madeBox.GetComponent<BoxBase>().parentChunk = chunk;
        return madeBox;
    }

    //アイテムデータ
    public InventoryItem ItemData { get; protected set; } = new InventoryItem();
    //所属チャンク
    public ChunkScript parentChunk { get; protected set; }
    //回転可能かどうか
    protected bool canRotate;
    //初期化済みかどうか
    protected bool isInitialized;
    //無敵時間かどうか
    protected TimeSpanFlag isGod;
    //コライダー
    protected Collider boxCollider;
    //メッシュレンダラー
    protected MeshRenderer meshRenderer;
    //メッシュフィルター
    protected MeshFilter meshFilter;
    //HP
    [field: SerializeField] public int HP { get; protected set; } = 0;
    //最大HP
    [SerializeField] private int MaxHP = 5;
    //自分自身のprefab名、BoxSpawnerにセットするよう
    [SerializeField] protected string prefabName;
    //アイテムアイコン
    [SerializeField] private Sprite itemIcon;

    protected void InitializeBox()
    {
        //先頭の(Clone)を消して、Editorから追加したときに出る後ろの数字を消す
        gameObject.name = gameObject.name.Replace("(Clone)", "").Split(' ')[0];
        ItemData.ItemName = gameObject.name;
        ItemData.ItemIcon = itemIcon;

        ItemData.SelectDelegate = (SlotScript slot) =>
        {
            BoxSpawnerScript.ScriptInstance.NextBox = PrefabManager.Instance.GetPrefab(slot.Item.ItemName);
        };

        ItemData.DeselectDelegate = (SlotScript slot) =>
        {
            BoxSpawnerScript.ScriptInstance.NextBox = null;
        };

        ItemData.UseDelegate = (SlotScript slot) =>
        {
            bool isUsed = BoxSpawnerScript.ScriptInstance.ReservationSpawnBox(PrefabManager.Instance.GetPrefab(slot.Item.ItemName));
            if (isUsed)
            {
                SoundEmitter.FindClip("set_box").Play();
            }
            return isUsed;
        };

        ItemData.UsedupDelegate = (SlotScript slot) =>
        {
            BoxSpawnerScript.ScriptInstance.NextBox = null;
        };
    }

    protected void InitializeField()
    {
        //無敵時間の設定
        isGod = new TimeSpanFlag(300);

        //HP設定
        HP = MaxHP;

        //Colliderを保持
        boxCollider = GetComponent<Collider>();

        //メッシュレンダラーを保持
        meshRenderer = GetComponent<MeshRenderer>();

        //メッシュフィルターを保持
        meshFilter = GetComponent<MeshFilter>();

        //初期化終了フラグを立てる
        isInitialized = true;
    }

    private void Awake()
    {
        InitializeField();
    }

    private void Start()
    {
        InitializeBox();
    }

    private void OnDestroy()
    {
        //隣接する6方向のBoxに対して通知
        for (int i = (int)ChunkScript.Direction.Top; i <= (int)ChunkScript.Direction.Bottom; i++)
        {
            //隣接するBoxの取得
            ChunkScript.BoxData affiliationBox = parentChunk?.GetAdjacentBox(transform.position, (ChunkScript.Direction)i);
            //有効化通知
            affiliationBox?.Script?.OnDestroyNotification(ChunkScript.Direction.Bottom - i);
        }
    }

    private void OnEnable()
    {
        if (!isInitialized) return;
        //隣接する6方向のBoxに対して通知
        for (int i = (int)ChunkScript.Direction.Top; i <= (int)ChunkScript.Direction.Bottom; i++)
        {
            //隣接するBoxの取得
            ChunkScript.BoxData affiliationBox = parentChunk?.GetAdjacentBox(transform.position, (ChunkScript.Direction)i);
            //有効化通知
            affiliationBox?.Script?.OnEnableNotification(ChunkScript.Direction.Bottom - i);
        }
    }

    /// <summary>
    /// 引数direction方向のBoxが有効化されたときに呼ばれる
    /// </summary>
    /// <param name="direction">自分から見た通知者の方向</param>
    private void OnEnableNotification(ChunkScript.Direction direction)
    {
        DisableIfNeeded();
    }

    /// <summary>
    /// 引数direction方向のBoxが破壊されたときに呼ばれる
    /// </summary>
    /// <param name="direction">自分から見た通知者の方向</param>
    private void OnDestroyNotification(ChunkScript.Direction direction)
    {
        //自分が無効化されていたら自分を有効化する
        if (!(gameObject?.activeSelf ?? false))
        {
            gameObject.SetActive(true);
        }
        MeshUpdate(null);
    }

    /// <summary>
    /// EditorModeの際に視錐台カリングがきかず重いので回避するためのデバッグ用関数
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    protected void FrustumCulling()
    {
        if (GeometryUtility.TestPlanesAABB(PlayerCamera.FrustumPlanes, boxCollider.bounds))
        {
            meshRenderer.enabled = true;
        }
        else
        {
            meshRenderer.enabled = false;
        }
    }

    /// <summary>
    /// 必要に応じてGameObjectを無効化する
    /// </summary>
    public void DisableIfNeeded()
    {
        //他のBoxを面していない面の方向
        List<ChunkScript.Direction> notAdjoining = new List<ChunkScript.Direction>();
        //接していない面があったかどうか
        bool isAllAdjacetBoxActive = true;
        //隣接する6方向のBox全て
        for (int i = (int)ChunkScript.Direction.Top; i <= (int)ChunkScript.Direction.Bottom; i++)
        {
            //隣接するBoxの取得
            bool isExistAffiliationBox = parentChunk?.IsAdjacetBoxExist(gameObject, (ChunkScript.Direction)i) ?? false;
            //何もなければBoxが存在していないということ
            if (isExistAffiliationBox == false)
            {
                //無効化はしない
                isAllAdjacetBoxActive = false;
                notAdjoining.Add((ChunkScript.Direction)i);
            }
        }

        //隣接する６面全てがあったら自分は無効化する
        if (isAllAdjacetBoxActive)
        {
            gameObject.SetActive(false);
        }
        //接していない面のみメッシュを張る
        else
        {
            MeshUpdate(notAdjoining);
        }
    }

    /// <summary>
    /// メッシュ形状を更新する
    /// </summary>
    /// <param name="notAdjoining">他のBoxに接していない面</param>
    private void MeshUpdate(List<ChunkScript.Direction> notAdjoining)
    {
        if(notAdjoining == null)
        {
            //他のBoxを面していない面の方向
            notAdjoining = new List<ChunkScript.Direction>();
            //隣接する6方向のBox全て
            for (int i = (int)ChunkScript.Direction.Top; i <= (int)ChunkScript.Direction.Bottom; i++)
            {
                //隣接するBoxの取得
                bool isExistAffiliationBox = parentChunk?.IsAdjacetBoxExist(gameObject, (ChunkScript.Direction)i) ?? false;
                //何もなければBoxが存在していないということ
                if (isExistAffiliationBox == false)
                {
                    notAdjoining.Add((ChunkScript.Direction)i);
                }
            }
        }

        //メッシュ結合用
        CombineInstance[] combine = new CombineInstance[notAdjoining.Count];
        for (int i = 0; i < notAdjoining.Count; i++)
        {
            //メッシュ結合データ入れる
            var direction = notAdjoining[i];
            combine[i].mesh = PrefabManager.Instance.GetMesh(direction.ToString()).sharedMesh;
            combine[i].transform = PrefabManager.Instance.GetMesh(direction.ToString()).transform.localToWorldMatrix;
        }
        //メッシュの生成
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        meshFilter.mesh.CombineMeshes(combine);
    }

    /// <summary>
    /// 90Deg回転
    /// </summary>
    public void Rotate()
    {
        if (!canRotate) return;

        transform.Rotate(new Vector3(0, 90, 0));
    }

    /// <summary>
    /// 全ての当たり判定の無効化
    /// </summary>
    public void DisableCollition()
    {
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider colliderFlagment in colliders)
        {
            colliderFlagment.enabled = false;
        }
    }

    /// <summary>
    /// 全ての当たり判定の有効化
    /// </summary>
    public void EnableCollition()
    {
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider colliderFlagment in colliders)
        {
            colliderFlagment.enabled = true;
        }
    }

    /// <summary>
    /// 全ての当たり判定のトリガー化解除
    /// </summary>
    public void DisableCollitionIsTrigger()
    {
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider colliderFlagment in colliders)
        {
            colliderFlagment.isTrigger = false;
        }
    }

    /// <summary>
    /// 全ての当たり判定のトリガー化
    /// </summary>
    public void EnableCollitionIsTrigger()
    {
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider colliderFlagment in colliders)
        {
            colliderFlagment.isTrigger = true;
        }
    }

    /// <summary>
    /// 壊されたときの効果音を再生
    /// </summary>
    protected virtual void PlaySEBreak()
    {
        SoundEmitter.FindClip("break_stone")?.Play();
    }

    /// <summary>
    /// 攻撃を受けた時の効果音を再生
    /// </summary>
    protected virtual void PlaySEOnAttack()
    {
        SoundEmitter.FindClip("dig_stone")?.Play();
    }


    //Implementation from Interface:IAttackableObject
    public virtual void OnAttack(in GameObject attacker)
    {
        //無敵時間中は何もしない
        if (isGod) return;
        //無敵タイマー開始
        isGod.Begin();
        //HPカウントダウン
        HP--;

        //HPがなくなった？
        if (HP > 0)
        {
            //攻撃時の音を出す
            PlaySEOnAttack();
            //壊れてないので処理はここまで
            return;
        }
        else
        {
            //破壊時の音を出す
            PlaySEBreak();
        }
        //メッシュをBoxへ戻す
        meshFilter.mesh = PrefabManager.Instance.GetMesh("Box").sharedMesh;
        //以下破壊処理
        bool isCreate = ItemScript.Create(gameObject);
        if (isCreate)
        {
            parentChunk.DestroyBox(gameObject);
        }
    }

    //Implementation from Interface:IItemizeObject,IMeshAccessor
    public virtual Mesh GetMesh()
    {
        return meshFilter.mesh;
    }

    //Implementation from Interface:IItemizeObject,IMeshAccessor
    public virtual Material GetMaterial()
    {
        return GetComponent<MeshRenderer>().material;
    }

    //Implementation from Interface:IItemizeObject
    public virtual InventoryItem GetItemData()
    {
        return ItemData;
    }

    //Implementation from Interface:IItemizeObject
    public virtual Vector3 GetMeshScale()
    {
        return Vector3.one;
    }

    //Implementation from Interface:IMeshAccessor
    public virtual MeshFilter GetMeshFilter()
    {
        return meshFilter;
    }

    //Implementation from Interface:IMeshAccessor
    public virtual MeshRenderer GetMeshRenderer()
    {
        return GetComponent<MeshRenderer>();
    }
}