using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemShortcutScript : MonoBehaviour
{
    //選択中のスロット
    public static int SelectIndex = UNSELECT_INDEX;
    public const int UNSELECT_INDEX = -1;

    void Update()
    {
        float scroll = -Input.GetAxis("Mouse ScrollWheel");
        //+なら1に、-なら-1に、0に近ければ0
        int indexShift = (Mathf.Abs(scroll) < 0.00001) ? 0 : (scroll > 0) ? 1 : -1;
        //マウスホイールまわしたとき
        if (indexShift != 0)
        {
            if (SelectIndex != -1)
            {
                //まず今のスロットの選択解除
                ItemShortcutSlotScript.itemShortcutSlots[ItemShortcutSlotScript.IndexList[SelectIndex]].ItemShortcutSlotScript?.SlotExit();
            }
            //-1~9までが入るように計算
            SelectIndex = (ItemShortcutSlotScript.IndexList.Length + 1 + SelectIndex + indexShift + 1) % (ItemShortcutSlotScript.IndexList.Length + 1) - 1;
            Debug.Log("Index : " + SelectIndex.ToString());
            if (SelectIndex != -1)
            {
                //スロットを選択
                ItemShortcutSlotScript.itemShortcutSlots[ItemShortcutSlotScript.IndexList[SelectIndex]].ItemShortcutSlotScript?.SlotEnter();
                //Debug.Log("Select : " + ItemShortcutSlotScript.IndexList[SelectIndex] + "   Index : " + SelectIndex.ToString());
            }
        }
    }

    /// <summary>
    /// 選択しているインデックスをキーに登録されているインデックスに直す
    /// </summary>
    /// <returns></returns>
    public static int GetSelectIndexForSlotIndex()
    {
        return (SelectIndex != -1) ? ItemShortcutSlotScript.IndexList[SelectIndex] : -1;
    }
}
