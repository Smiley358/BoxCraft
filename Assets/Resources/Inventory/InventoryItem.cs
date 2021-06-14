using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem
{
    //�A�C�e���̖��O
    public string ItemName;
    //�A�C�e���̃A�C�R��
    public Sprite ItemIcon;
    //�A�C�e���̍ő�X�^�b�N��
    public int MaxStackSize = 10;
    //�E�N���b�N�Ŏg�p���邩/�I����Ԏ��ɂ��邩
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
