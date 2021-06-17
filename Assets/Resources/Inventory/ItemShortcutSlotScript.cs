using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///アイテムショートカットのスロット保持用
/// </summary>
public class ItemShortcutSlotData
{
    public GameObject Slot;
    public SlotScript SlotScript;
    public ItemShortcutSlotScript ItemShortcutSlotScript;

    public ItemShortcutSlotData(ItemShortcutSlotData data)
    {
        Slot = data.Slot;
        SlotScript = data.SlotScript;
        ItemShortcutSlotScript = data.ItemShortcutSlotScript;
    }

    public ItemShortcutSlotData(GameObject slot, SlotScript slotScript, ItemShortcutSlotScript itemShortcutSlotScript)
    {
        Slot = slot;
        SlotScript = slotScript;
        ItemShortcutSlotScript = itemShortcutSlotScript;
    }
}

public class ItemShortcutSlotScript : SlotScript
{
    /// <summary>
    ///ショートカット用スロットの作成と割り当て
    /// </summary>
    /// <param name="prefab">作成するprefab</param>
    /// <param name="IndexList">作成するためのインデックスリスト</param>
    /// <param name="index">現在のインデックス</param>
    /// <returns></returns>
    public static ItemShortcutSlotData Create(GameObject prefab, int[] IndexList, int index)
    {
        //キー割り当てできないので作らない
        if (index > IndexList.Length - 1) return null;

        //ショートカットスロット生成
        GameObject slot = Instantiate(prefab);
        ItemShortcutSlotScript itemShortcutSlotScript = slot.GetComponent<ItemShortcutSlotScript>();
        itemShortcutSlotScript.SlotData = new SlotData(slot, itemShortcutSlotScript);
        //イベントハンドラー初期設定
        itemShortcutSlotScript.SlotData.SlotScript.PrepareEventTrigger();


        //インデックス割り当て
        itemShortcutSlotScript.Index = IndexList[index];
        //インデックス表示
        Text text = slot.transform.Find("Index")?.GetComponent<Text>();
        text.text = itemShortcutSlotScript.Index.ToString();

        return new ItemShortcutSlotData(slot, itemShortcutSlotScript.SlotData.SlotScript, itemShortcutSlotScript);
    }



    //キー割り当て用インデックス
    public int Index { get; private set; } = -1;

    void Update()
    {
        if (slotIcon == null)
        {
            Debug.Log("スロットの初期化に失敗");
        }
        //自分のインデックスと同じキーが押されたかどうか
        if (Input.GetKeyDown(KeyCode.Alpha0 + Index))
        {
            SlotEnter();
        }
    }

    //選択
    public void SlotEnter()
    {
        if (selectSlotData != SlotData)
        {
            //即時使用か選択か
            if (SlotData.SlotScript.Item?.IsImmediateUse == true)
            {
                //アイテム使用
                InventoryScript.Instance.UseItem(SlotData);
            }
            else
            {
                //アイテム選択
                SelectSlot(SlotData);
                ItemShortcutScript.SelectIndex = Array.IndexOf(ItemShortcutScript.IndexList, Index);
            }
        }
        else
        {
            SlotExit();
            ItemShortcutScript.SelectIndex = ItemShortcutScript.UnselectIndex;
        }
    }
    //選択終わり
    public void SlotExit()
    {
        //アイテム選択解除
        DeselectSlot(SlotData);
    }

    /// <summary>
    /// 選択状態に
    /// </summary>
    protected override void Select()
    {
        if (Index != ItemShortcutScript.GetSelectIndexForSlotIndex())
        {
            ItemShortcutScript.SelectIndex = Array.IndexOf(ItemShortcutScript.IndexList, Index);
        }
        base.Select();
    }
}
