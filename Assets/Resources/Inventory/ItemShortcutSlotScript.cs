using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemShortcutSlotScript : MonoBehaviour
{
    //�A�C�e���V���[�g�J�b�g�̃X���b�g�ێ��p
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

    //�L�[���蓖�ėp�C���f�b�N�X���X�g
    public static readonly int[] IndexList = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
    //���݂̊��蓖�ď�
    private static int nowIndex = 0;
    //�X���b�g
    public static Dictionary<int, ItemShortcutSlotData> itemShortcutSlots { get; private set; } = new Dictionary<int, ItemShortcutSlotData>(IndexList.Length);

    //�V���[�g�J�b�g�p�X���b�g�̍쐬�Ɗ��蓖��
    public static GameObject Create(GameObject prefab)
    {
        //�L�[���蓖�Ăł��Ȃ��̂ō��Ȃ�
        if (nowIndex > IndexList.Length - 1) return null;

        //�V���[�g�J�b�g�X���b�g����
        GameObject slot = Instantiate(prefab);
        ItemShortcutSlotScript itemShortcutSlotScript = slot.GetComponent<ItemShortcutSlotScript>();
        itemShortcutSlotScript.slotScript = slot.GetComponent<SlotScript>();
        //�C���f�b�N�X���蓖��
        itemShortcutSlotScript.Index = IndexList[nowIndex];
        nowIndex++;
        //�C���f�b�N�X�\��
        Text text = slot.transform.Find("Index")?.GetComponent<Text>();
        text.text = itemShortcutSlotScript.Index.ToString();

        //�f�B�N�V���i���[�֒ǉ�
        itemShortcutSlots.Add(itemShortcutSlotScript.Index, new ItemShortcutSlotData(slot, itemShortcutSlotScript.slotScript, itemShortcutSlotScript));

        return slot;
    }




    //�L�[���蓖�ėp�C���f�b�N�X
    public int Index { get; private set; } = -1;
    //�X���b�g�{�̂̊Ǘ��p�N���X
    private SlotScript slotScript;
    //�C���x���g���[
    public GameObject inventory;

    void Update()
    {
        //�����̃C���f�b�N�X�Ɠ����L�[�������ꂽ���ǂ���
        if (Input.GetKeyDown(KeyCode.Alpha0 + Index))
        {
            //�I�𒆂ɂ������I�����ꂽ��I������
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
            //�I����Ԃ�
            GetComponent<Button>().Select();
        }
        else
        {

        }

    }

    //�I��
    public void SlotEnter()
    {
        //��̃X���b�g�͑I���ł��Ȃ�
        if (slotScript.IsEmpty()) return;

        //�����g�p���I����
        if (slotScript.Item.IsImmediateUse)
        {
            //�A�C�e���g�p
            inventory.SendMessage("UseItem", new InventoryScript.SlotData(gameObject, slotScript));
        }
        else
        {
            //�A�C�e���I��
            slotScript.Item?.SelectDelegate?.Invoke(slotScript);
        }
    }
    //�I���I���
    public void SlotExit()
    {
        //�A�C�e���I������
        slotScript.Item?.DeselectDelegate?.Invoke(slotScript);
    }

}
