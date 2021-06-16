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

public class ItemShortcutSlotScript : MonoBehaviour
{
    //ショートカット用スロットの作成と割り当て
    public static ItemShortcutSlotData Create(GameObject prefab, GameObject inventory, InventoryScript inventoryScript,int[] IndexList,int index)
    {
        //キー割り当てできないので作らない
        if (index > IndexList.Length - 1) return null;

        //ショートカットスロット生成
        GameObject slot = Instantiate(prefab);
        ItemShortcutSlotScript itemShortcutSlotScript = slot.GetComponent<ItemShortcutSlotScript>();
        itemShortcutSlotScript.slotData = new SlotData(slot, slot.GetComponent<SlotScript>());
        //イベントハンドラー初期設定
        itemShortcutSlotScript.slotData.SlotScript.PrepareEventTrigger();
        itemShortcutSlotScript.slotData.SlotScript.InventoryScript = inventoryScript;


        //インデックス割り当て
        itemShortcutSlotScript.Index = IndexList[index];
        //インデックス表示
        Text text = slot.transform.Find("Index")?.GetComponent<Text>();
        text.text = itemShortcutSlotScript.Index.ToString();

        return new ItemShortcutSlotData(slot, itemShortcutSlotScript.slotData.SlotScript, itemShortcutSlotScript);
    }



    //キー割り当て用インデックス
    public int Index { get; private set; } = -1;
    //インベントリーで管理しているスロットデータクラス
    public SlotData slotData;

    void Update()
    {
        //自分のインデックスと同じキーが押されたかどうか
        if (Input.GetKeyDown(KeyCode.Alpha0 + Index))
        {
            //選択中にもう一回選択されたら選択解除
            if (slotData.SlotScript.InventoryScript.selectSlotData == slotData)
            {
                ItemShortcutScript.SelectIndex = ItemShortcutScript.UNSELECT_INDEX;
                SlotExit();
            }
            else
            {
                SlotEnter();
            }
        }
    }

    //選択
    public void SlotEnter()
    {
        {
            //即時使用か選択か
            if (slotData.SlotScript.Item?.IsImmediateUse == true)
            {
                //アイテム使用
                slotData.SlotScript.InventoryScript.UseItem(slotData);
            }
            else
            {
                //アイテム選択
                slotData.SlotScript.InventoryScript.SelectSlot(slotData);
            }
        }
    }
    //選択終わり
    public void SlotExit()
    {
        //アイテム選択解除
        slotData.SlotScript.InventoryScript.DeselectSlot(slotData);
    }

}
