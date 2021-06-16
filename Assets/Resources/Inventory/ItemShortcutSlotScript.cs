using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemShortcutSlotScript : MonoBehaviour
{
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

        public ItemShortcutSlotData(GameObject slot,SlotScript slotScript,ItemShortcutSlotScript itemShortcutSlotScript)
        {
            Slot = slot;
            SlotScript = slotScript;
            ItemShortcutSlotScript = itemShortcutSlotScript;
        }
    }

    //キー割り当て用インデックスリスト
    public static readonly int[] IndexList = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
    //現在の割り当て状況
    private static int nowIndex = 0;
    //スロット
    public static Dictionary<int, ItemShortcutSlotData> itemShortcutSlots { get; private set; } = new Dictionary<int, ItemShortcutSlotData>(IndexList.Length);

    //ショートカット用スロットの作成と割り当て
    public static GameObject Create(GameObject prefab)
    {
        //キー割り当てできないので作らない
        if (nowIndex > IndexList.Length - 1) return null;

        //ショートカットスロット生成
        GameObject slot = Instantiate(prefab);
        ItemShortcutSlotScript itemShortcutSlotScript = slot.GetComponent<ItemShortcutSlotScript>();
        itemShortcutSlotScript.slotScript = slot.GetComponent<SlotScript>();
        //インデックス割り当て
        itemShortcutSlotScript.Index = IndexList[nowIndex];
        nowIndex++;
        //インデックス表示
        Text text = slot.transform.Find("Index")?.GetComponent<Text>();
        text.text = itemShortcutSlotScript.Index.ToString();

        //ディクショナリーへ追加
        itemShortcutSlots.Add(itemShortcutSlotScript.Index, new ItemShortcutSlotData(slot, itemShortcutSlotScript.slotScript, itemShortcutSlotScript));

        return slot;
    }




    //キー割り当て用インデックス
    public int Index { get; private set; } = -1;
    //スロット本体の管理用クラス
    private SlotScript slotScript;
    //インベントリー
    public GameObject inventory;

    void Update()
    {
        //自分のインデックスと同じキーが押されたかどうか
        if (Input.GetKeyDown(KeyCode.Alpha0 + Index))
        {
            //選択中にもう一回選択されたら選択解除
            if (ItemShortcutScript.GetSelectIndexForSlotIndex() == Index)
            {
                ItemShortcutScript.SelectIndex = ItemShortcutScript.UNSELECT_INDEX;
                SlotExit();
            }
            else
            {
                SlotEnter();
            }
        }

        if (ItemShortcutScript.GetSelectIndexForSlotIndex() == Index)
        {
            //選択状態に
            GetComponent<Button>().Select();
        }
        else
        {

        }

    }

    //選択
    public void SlotEnter()
    {
        //空のスロットは選択できない
        if (slotScript.IsEmpty()) return;

        //即時使用か選択か
        if (slotScript.Item.IsImmediateUse)
        {
            //アイテム使用
            inventory.SendMessage("UseItem", new InventoryScript.SlotData(gameObject, slotScript));
        }
        else
        {
            //アイテム選択
            slotScript.Item?.SelectDelegate?.Invoke(slotScript);
        }
    }
    //選択終わり
    public void SlotExit()
    {
        //アイテム選択解除
        slotScript.Item?.DeselectDelegate?.Invoke(slotScript);
    }

}
