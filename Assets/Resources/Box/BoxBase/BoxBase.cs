using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BOXの基底クラス
/// このクラスを継承すればBOXとしての最低限の機能を保証する
/// </summary>
public abstract class BoxBase : MonoBehaviour,IAttackableObject,IItemizeObject
{
    public static GameObject Create(GameObject box, Vector3 position, Quaternion rotation)
    {
        // 生成位置の変数の座標にブロックを生成
        GameObject madeBox = Instantiate(box, position, rotation);
        if(madeBox is null)
        {
            return null;
        }
        return madeBox;
    }

    //自分自身のprefab名、BoxSpawnerにセットするよう
    [SerializeField] protected string prefabName;
    //アイテムデータ
    public InventoryItem ItemData { get; protected set; } = new InventoryItem();
    //回転可能かどうか
    protected bool canRotate;
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
    }

    private void Start()
    {
        InitializeBox();
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
        foreach(Collider colliderFlagment in colliders)
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
    public  virtual void OnAttack(in GameObject attacker)
    {
        bool isCreate = ItemScript.Create(gameObject);
        if (isCreate)
        {
            Destroy(gameObject);
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
        return GetComponent<MeshRenderer>();
    }
}