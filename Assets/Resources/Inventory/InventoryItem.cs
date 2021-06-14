using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem
{
    //アイテムの名前
    public string ItemName;
    //アイテムのアイコン
    public Sprite ItemIcon;
    //アイテムの最大スタック数
    public int MaxStackSize = 10;
    //右クリックで使用するか/選択状態死にするか
    public bool IsRMBUse { get; private set; }

    public InventoryItem(InventoryItem itemData)
    {
        ItemName = itemData.ItemName;
        ItemIcon = itemData.ItemIcon;
        MaxStackSize = itemData.MaxStackSize;
        SelectDelegate = itemData.SelectDelegate;
        DeselectDelegate = itemData.DeselectDelegate;
        UseDelegate = itemData.UseDelegate;
        UsedupDelegate = itemData.UsedupDelegate;
        IsRMBUse = itemData.IsRMBUse;
    }

    public InventoryItem() { }
    
    public Action<SlotScript> SelectDelegate;
    public Action<SlotScript> DeselectDelegate;
    public Func<bool> UseDelegate;
    public Action<SlotScript> UsedupDelegate;
}
