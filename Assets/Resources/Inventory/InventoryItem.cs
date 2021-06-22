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
    
    //�����g�p���邩/�I����Ԃɂ��邩
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
    
    //�I�����ꂽ�Ƃ��Ɏ��s����鏈��
    public Action<SlotScript> SelectDelegate;
    //�I���������ꂽ�Ƃ��Ɏ��s����鏈��
    public Action<SlotScript> DeselectDelegate;
    //�g�p���ꂽ�Ƃ��Ɏ��s����鏈��
    public Func<SlotScript, bool> UseDelegate;
    //�g���؂������Ɏ��s����鏈��
    public Action<SlotScript> UsedupDelegate;
}
