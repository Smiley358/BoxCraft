using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemShortcutScript : MonoBehaviour
{
    //�I�𒆂̃X���b�g
    public static int SelectIndex = UNSELECT_INDEX;
    public const int UNSELECT_INDEX = -1;

    void Update()
    {
        float scroll = -Input.GetAxis("Mouse ScrollWheel");
        //+�Ȃ�1�ɁA-�Ȃ�-1�ɁA0�ɋ߂����0
        int indexShift = (Mathf.Abs(scroll) < 0.00001) ? 0 : (scroll > 0) ? 1 : -1;
        //�}�E�X�z�C�[���܂킵���Ƃ�
        if (indexShift != 0)
        {
            if (SelectIndex != -1)
            {
                //�܂����̃X���b�g�̑I������
                ItemShortcutSlotScript.itemShortcutSlots[ItemShortcutSlotScript.IndexList[SelectIndex]].ItemShortcutSlotScript?.SlotExit();
            }
            //-1~9�܂ł�����悤�Ɍv�Z
            SelectIndex = (ItemShortcutSlotScript.IndexList.Length + 1 + SelectIndex + indexShift + 1) % (ItemShortcutSlotScript.IndexList.Length + 1) - 1;
            Debug.Log("Index : " + SelectIndex.ToString());
            if (SelectIndex != -1)
            {
                //�X���b�g��I��
                ItemShortcutSlotScript.itemShortcutSlots[ItemShortcutSlotScript.IndexList[SelectIndex]].ItemShortcutSlotScript?.SlotEnter();
                //Debug.Log("Select : " + ItemShortcutSlotScript.IndexList[SelectIndex] + "   Index : " + SelectIndex.ToString());
            }
        }
    }

    /// <summary>
    /// �I�����Ă���C���f�b�N�X���L�[�ɓo�^����Ă���C���f�b�N�X�ɒ���
    /// </summary>
    /// <returns></returns>
    public static int GetSelectIndexForSlotIndex()
    {
        return (SelectIndex != -1) ? ItemShortcutSlotScript.IndexList[SelectIndex] : -1;
    }
}
