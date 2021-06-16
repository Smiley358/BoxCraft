using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public ItemShortcutSlotData(GameObject slot, SlotScript slotScript, ItemShortcutSlotScript itemShortcutSlotScript)
    {
        Slot = slot;
        SlotScript = slotScript;
        ItemShortcutSlotScript = itemShortcutSlotScript;
    }
}

public class ItemShortcutSlotScript : MonoBehaviour
{
    //�V���[�g�J�b�g�p�X���b�g�̍쐬�Ɗ��蓖��
    public static ItemShortcutSlotData Create(GameObject prefab, GameObject inventory, InventoryScript inventoryScript,int[] IndexList,int index)
    {
        //�L�[���蓖�Ăł��Ȃ��̂ō��Ȃ�
        if (index > IndexList.Length - 1) return null;

        //�V���[�g�J�b�g�X���b�g����
        GameObject slot = Instantiate(prefab);
        ItemShortcutSlotScript itemShortcutSlotScript = slot.GetComponent<ItemShortcutSlotScript>();
        itemShortcutSlotScript.slotData = new SlotData(slot, slot.GetComponent<SlotScript>());
        //�C�x���g�n���h���[�����ݒ�
        itemShortcutSlotScript.slotData.SlotScript.PrepareEventTrigger();
        itemShortcutSlotScript.slotData.SlotScript.InventoryScript = inventoryScript;


        //�C���f�b�N�X���蓖��
        itemShortcutSlotScript.Index = IndexList[index];
        //�C���f�b�N�X�\��
        Text text = slot.transform.Find("Index")?.GetComponent<Text>();
        text.text = itemShortcutSlotScript.Index.ToString();

        return new ItemShortcutSlotData(slot, itemShortcutSlotScript.slotData.SlotScript, itemShortcutSlotScript);
    }



    //�L�[���蓖�ėp�C���f�b�N�X
    public int Index { get; private set; } = -1;
    //�C���x���g���[�ŊǗ����Ă���X���b�g�f�[�^�N���X
    public SlotData slotData;

    void Update()
    {
        //�����̃C���f�b�N�X�Ɠ����L�[�������ꂽ���ǂ���
        if (Input.GetKeyDown(KeyCode.Alpha0 + Index))
        {
            //�I�𒆂ɂ������I�����ꂽ��I������
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

    //�I��
    public void SlotEnter()
    {
        {
            //�����g�p���I����
            if (slotData.SlotScript.Item?.IsImmediateUse == true)
            {
                //�A�C�e���g�p
                slotData.SlotScript.InventoryScript.UseItem(slotData);
            }
            else
            {
                //�A�C�e���I��
                slotData.SlotScript.InventoryScript.SelectSlot(slotData);
            }
        }
    }
    //�I���I���
    public void SlotExit()
    {
        //�A�C�e���I������
        slotData.SlotScript.InventoryScript.DeselectSlot(slotData);
    }

}
