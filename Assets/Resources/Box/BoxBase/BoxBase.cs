using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BOXの基底クラス
/// このクラスを継承すればBOXとしての最低限の機能を保証する
/// </summary>
public abstract class BoxBase : MonoBehaviour
{
    //アイテムデータ
    public InventoryItem ItemData { get; protected set; } = new InventoryItem();
    //回転可能かどうか
    protected bool canRotate;
    //アイテムアイコン
    [SerializeField] private Sprite itemIcon;

    void Awake()
    {
        ItemData.ItemName = gameObject.name.Replace("(Clone)", "");
        ItemData.ItemIcon = itemIcon;

        ItemData.SelectDelegate = (SlotScript slot) =>
        {
            BoxSpawnerScript boxSpawnerScript = GameObject.Find("BoxSpawner")?.GetComponent<BoxSpawnerScript>();
            string path = "Box/" + ItemData.ItemName + "/" + ItemData.ItemName;
            boxSpawnerScript.Box = Resources.Load(path) as GameObject;
            boxSpawnerScript.PredictionBoxUpdate();
        };

        ItemData.DeselectDelegate = (SlotScript slot) =>
        {
            BoxSpawnerScript boxSpawnerScript = GameObject.Find("BoxSpawner")?.GetComponent<BoxSpawnerScript>();
            boxSpawnerScript.Box = null;
            boxSpawnerScript.PredictionBoxUpdate();
        };

        ItemData.UseDelegate = () => true;

        ItemData.UsedupDelegate = (SlotScript slot) => 
        {
            BoxSpawnerScript boxSpawnerScript = GameObject.Find("BoxSpawner")?.GetComponent<BoxSpawnerScript>();
            boxSpawnerScript.Box = null;
            boxSpawnerScript.PredictionBoxUpdate();
        };
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