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
    
    //即時使用するか/選択状態にするか
    public bool IsImmediateUse { get; private set; }

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
    public Action<SlotScript> SelectDelegate;
    //選択解除されたときに実行される処理
    public Action<SlotScript> DeselectDelegate;
    //使用されたときに実行される処理
    public Func<SlotScript, bool> UseDelegate;
    //使い切った時に実行される処理
    public Action<SlotScript> UsedupDelegate;
}
