using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//アイテムショートカットのスロット保持用
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
    //ショートカット用スロットの作成と割り当て
    public static ItemShortcutSlotData Create(GameObject prefab, GameObject inventory, int[] IndexList, int index)
    {
        //キー割り当てできないので作らない
        if (index > IndexList.Length - 1) return null;

        //ショートカットスロット生成
        GameObject slot = Instantiate(prefab);
        ItemShortcutSlotScript itemShortcutSlotScript = slot.GetComponent<ItemShortcutSlotScript>();
        itemShortcutSlotScript.slotData = new SlotData(slot, itemShortcutSlotScript);
        //イベントハンドラー初期設定
        itemShortcutSlotScript.slotData.SlotScript.PrepareEventTrigger();


        //インデックス割り当て
        itemShortcutSlotScript.Index = IndexList[index];
        //インデックス表示
        Text text = slot.transform.Find("Index")?.GetComponent<Text>();
        text.text = itemShortcutSlotScript.Index.ToString();

        return new ItemShortcutSlotData(slot, itemShortcutSlotScript.slotData.SlotScript, itemShortcutSlotScript);
    }



    //キー割り当て用インデックス
    public int Index { get; private set; } = -1;

    void Awake()
    {
        //スタック管理クラスの取得
        stackScript = transform.Find("Stacks")?.GetComponent<StackScript>();
        if (stackScript == null)
        {
            Debug.Log("スロットの初期化に失敗");
        }
        //アイコン表示コンポーネントを取得
        ItemIcon = transform.Find("ItemIcon")?.GetComponent<Image>();
        if (ItemIcon == null)
        {
            Debug.Log("スロットの初期化に失敗");
        }
        SlotIcon = GetComponent<Image>();
        if (SlotIcon == null)
        {
            Debug.Log("スロットの初期化に失敗");
        }
    }

    void Update()
    {
        if (SlotIcon == null)
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
        if (selectSlotData != slotData)
        {
            //即時使用か選択か
            if (slotData.SlotScript.Item?.IsImmediateUse == true)
            {
                //アイテム使用
                InventoryScript.InventoryScriptInstance.UseItem(slotData);
            }
            else
            {
                //アイテム選択
                SelectSlot(slotData);
                ItemShortcutScript.SelectIndex = Array.IndexOf(ItemShortcutScript.IndexList, Index);
            }
        }
        else
        {
            SlotExit();
            ItemShortcutScript.SelectIndex = ItemShortcutScript.UNSELECT_INDEX;
        }
    }
    //選択終わり
    public void SlotExit()
    {
        //アイテム選択解除
        DeselectSlot(slotData);
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
