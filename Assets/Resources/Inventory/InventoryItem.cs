using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    //アイテムの名前
    [SerializeField] public string ItemName;
    //アイテムのアイコン
    [SerializeField] public Sprite ItemIcon;
    //アイテムの最大スタック数
    [SerializeField] public int MaxStackSize = 10;

    //即時使用するか/選択状態にするか
    [field:SerializeField] public bool IsImmediateUse { get; private set; }

    public InventoryItem(InventoryItem itemData)
    {
        ItemName         = itemData.ItemName;
        ItemIcon         = itemData.ItemIcon;
        MaxStackSize     = itemData.MaxStackSize;
        SelectDelegate   = itemData.SelectDelegate;
        DeselectDelegate = itemData.DeselectDelegate;
        UseDelegate      = itemData.UseDelegate;
        UsedupDelegate   = itemData.UsedupDelegate;
        IsImmediateUse   = itemData.IsImmediateUse;
    }

    public InventoryItem() { }

    //選択されたときに実行される処理
    [SerializeField] public Action<SlotScript> SelectDelegate;
    //選択解除されたときに実行される処理
    [SerializeField] public Action<SlotScript> DeselectDelegate;
    //使用されたときに実行される処理
    [SerializeField] public Func<SlotScript, bool> UseDelegate;
    //使い切った時に実行される処理
    [SerializeField] public Action<SlotScript> UsedupDelegate;
}
