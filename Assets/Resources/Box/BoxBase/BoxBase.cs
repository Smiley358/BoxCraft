using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BOXの基底クラス
/// このクラスを継承すればBOXとしての最低限の機能を保証する
/// </summary>
public abstract class BoxBase : MonoBehaviour
{
    //自分自身のprefab、BoxSpawnerにセットするよう
    [SerializeField] protected GameObject prefab;
    //アイテムデータ
    public InventoryItem ItemData { get; protected set; } = new InventoryItem();
    //回転可能かどうか
    protected bool canRotate;
    //アイテムアイコン
    [SerializeField] private Sprite itemIcon;

    protected void InitializeBox()
    {
        ItemData.ItemName = gameObject.name.Replace("(Clone)", "").Split(' ')[0];
        ItemData.ItemIcon = itemIcon;

        ItemData.SelectDelegate = (SlotScript slot) =>
        {
            string path = "Box/" + ItemData.ItemName + "/" + ItemData.ItemName;
            BoxSpawnerScript.ScriptInstance.NextBox = Resources.Load(path) as GameObject;
        };

        ItemData.DeselectDelegate = (SlotScript slot) =>
        {
            BoxSpawnerScript.ScriptInstance.NextBox = null;
        };

        ItemData.UseDelegate = () =>
        {
            string path = "Box/" + ItemData.ItemName + "/" + ItemData.ItemName;
            return BoxSpawnerScript.ScriptInstance.ReservationSpawnBox(Resources.Load(path) as GameObject);
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
}