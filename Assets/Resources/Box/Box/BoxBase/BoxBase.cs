using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// BOXの基底クラス
/// このクラスを継承すればBOXとしての最低限の機能を保証する
/// </summary>
public abstract class BoxBase : MonoBehaviour, IAttackableObject, IItemizeObject
{
    public static GameObject Create(GameObject box, Vector3 position, Quaternion rotation)
    {
        // 生成位置の変数の座標にブロックを生成
        GameObject madeBox = Instantiate(box, position, rotation);
        if (madeBox is null)
        {
            return null;
        }
        return madeBox;
    }

    public static GameObject Create(ChunkScript chunk, GameObject box, Vector3 position, Quaternion rotation)
    {
        // 生成位置の変数の座標にブロックを生成
        GameObject madeBox = Instantiate(box, position, rotation);
        if (madeBox is null)
        {
            return null;
        }
        madeBox.GetComponent<BoxBase>().affiliationChunk = chunk;
        return madeBox;
    }

    //アイテムデータ
    public InventoryItem ItemData { get; protected set; } = new InventoryItem();
    //回転可能かどうか
    protected bool canRotate;
    //初期化済みかどうか
    protected bool isInitialized;
    //無敵時間かどうか
    protected TimeSpanFlag isGod;
    //処理をするかどうか
    protected TimeSpanFlag isUpdate;
    //コライダー
    protected Collider boxCollider;
    //メッシュレンダラー
    protected MeshRenderer meshRenderer;
    //所属チャンク
    protected ChunkScript affiliationChunk;
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
        //戦闘の(Clone)を消して、Editorから追加したときに出る後ろの数字を消す
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
            return BoxSpawnerScript.ScriptInstance.ReservationSpawnBox(PrefabManager.Instance.GetPrefab(slot.Item.ItemName));
        };

        ItemData.UsedupDelegate = (SlotScript slot) =>
        {
            BoxSpawnerScript.ScriptInstance.NextBox = null;
        };

        //無敵時間の設定
        isGod = new TimeSpanFlag(300);

        //Updateのタイミングを設定
        isUpdate = new TimeSpanFlag(500);

        //HP設定
        HP = MaxHP;

        //Colliderを保持
        boxCollider = GetComponent<Collider>();

        //メッシュレンダラーを保持
        meshRenderer = GetMeshRenderer();

        //初期化終了フラグを立てる
        isInitialized = true;
    }

    private void Start()
    {
        InitializeBox();
        DisableIfNeeded();
        OnEnable();
    }

    private void Update()
    {
        //FrustumCulling();
        //Updateフラグが入っていたら
        if (isUpdate)
        {
            //Updateフラグのタイマースタート
            isUpdate.Begin();
            //無効化するか処理
            DisableIfNeeded();
        }
    }

    private void OnDestroy()
    {
        //隣接する6方向のBoxに対して通知
        for (int i = (int)ChunkScript.Direction.Top; i <= (int)ChunkScript.Direction.Bottom; i++)
        {
            //隣接するBoxの取得
            ChunkScript.BoxData affiliationBox = affiliationChunk?.GetAdjacentBox(gameObject, (ChunkScript.Direction)i);
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
            ChunkScript.BoxData affiliationBox = affiliationChunk?.GetAdjacentBox(gameObject, (ChunkScript.Direction)i);
            //有効化通知
            affiliationBox?.Script?.OnEnableNotification(ChunkScript.Direction.Bottom - i);
        }
    }

    private void OnDisable()
    {
        //隣接する6方向のBoxに対して通知
        //for (int i = (int)ChunkScript.Direction.Top; i <= (int)ChunkScript.Direction.Bottom; i++)
        //{
        //    //隣接するBoxの取得
        //    ChunkScript.BoxData affiliationBox = affiliationChunk?.GetAdjacentBox(gameObject, (ChunkScript.Direction)i);
        //    //有効化通知
        //    affiliationBox?.Script?.OnDisableNotification(ChunkScript.Direction.Bottom - i);
        //}
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
    /// 引数direction方向のBoxが無効化されたときに呼ばれる
    /// </summary>
    /// <param name="direction">自分から見た通知者の方向</param>
    private void OnDisableNotification(ChunkScript.Direction direction)
    {

    }

    /// <summary>
    /// 引数direction方向のBoxが破壊されたときに呼ばれる
    /// </summary>
    /// <param name="direction">自分から見た通知者の方向</param>
    private void OnDestroyNotification(ChunkScript.Direction direction)
    {
        //自分が無効化されていたら自分は無効化する
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 必要に応じてGameObjectを無効化する
    /// </summary>
    private void DisableIfNeeded()
    {
        bool isAllAdjacetBoxActive = true;
        //隣接する6方向のBox全て
        for (int i = (int)ChunkScript.Direction.Top; i <= (int)ChunkScript.Direction.Bottom; i++)
        {
            //隣接するBoxの取得
            bool isExistAffiliationBox = affiliationChunk.IsAdjacetBoxExist(gameObject, (ChunkScript.Direction)i);
            //何もなければBoxが存在していないということ
            if (isExistAffiliationBox == false)
            {
                //無効化はしない
                isAllAdjacetBoxActive = false;
                break;
            }
        }

        //隣接する６面全てがあったら自分は無効化する
        if (isAllAdjacetBoxActive)
        {
            gameObject.SetActive(false);
        }
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


    //Implementation from Interface:IAttackableObject
    public virtual void OnAttack(in GameObject attacker)
    {
        if (isGod) return;
        isGod.Begin();
        HP--;

        if (HP > 0) return;

        bool isCreate = ItemScript.Create(gameObject);
        if (isCreate)
        {
            affiliationChunk.DestroyBox(gameObject);
        }
    }

    //Implementation from Interface:IItemizeObject
    public virtual InventoryItem GetItemData()
    {
        return ItemData;
    }

    //Implementation from Interface:IItemizeObject
    public virtual MeshFilter GetMeshFilter()
    {
        return GetComponent<MeshFilter>();
    }

    //Implementation from Interface:IItemizeObject
    public virtual MeshRenderer GetMeshRenderer()
    {
        return meshRenderer ?? GetComponent<MeshRenderer>();
    }
}