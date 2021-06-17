using System;
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

public class ItemShortcutSlotScript : SlotScript
{
    //�V���[�g�J�b�g�p�X���b�g�̍쐬�Ɗ��蓖��
    public static ItemShortcutSlotData Create(GameObject prefab, GameObject inventory, int[] IndexList, int index)
    {
        //�L�[���蓖�Ăł��Ȃ��̂ō��Ȃ�
        if (index > IndexList.Length - 1) return null;

        //�V���[�g�J�b�g�X���b�g����
        GameObject slot = Instantiate(prefab);
        ItemShortcutSlotScript itemShortcutSlotScript = slot.GetComponent<ItemShortcutSlotScript>();
        itemShortcutSlotScript.slotData = new SlotData(slot, itemShortcutSlotScript);
        //�C�x���g�n���h���[�����ݒ�
        itemShortcutSlotScript.slotData.SlotScript.PrepareEventTrigger();


        //�C���f�b�N�X���蓖��
        itemShortcutSlotScript.Index = IndexList[index];
        //�C���f�b�N�X�\��
        Text text = slot.transform.Find("Index")?.GetComponent<Text>();
        text.text = itemShortcutSlotScript.Index.ToString();

        return new ItemShortcutSlotData(slot, itemShortcutSlotScript.slotData.SlotScript, itemShortcutSlotScript);
    }



    //�L�[���蓖�ėp�C���f�b�N�X
    public int Index { get; private set; } = -1;

    void Awake()
    {
        //�X�^�b�N�Ǘ��N���X�̎擾
        stackScript = transform.Find("Stacks")?.GetComponent<StackScript>();
        if (stackScript == null)
        {
            Debug.Log("�X���b�g�̏������Ɏ��s");
        }
        //�A�C�R���\���R���|�[�l���g���擾
        ItemIcon = transform.Find("ItemIcon")?.GetComponent<Image>();
        if (ItemIcon == null)
        {
            Debug.Log("�X���b�g�̏������Ɏ��s");
        }
        SlotIcon = GetComponent<Image>();
        if (SlotIcon == null)
        {
            Debug.Log("�X���b�g�̏������Ɏ��s");
        }
    }

    void Update()
    {
        if (SlotIcon == null)
        {
            Debug.Log("�X���b�g�̏������Ɏ��s");
        }
        //�����̃C���f�b�N�X�Ɠ����L�[�������ꂽ���ǂ���
        if (Input.GetKeyDown(KeyCode.Alpha0 + Index))
        {
            SlotEnter();
        }
    }

    //�I��
    public void SlotEnter()
    {
        if (selectSlotData != slotData)
        {
            //�����g�p���I����
            if (slotData.SlotScript.Item?.IsImmediateUse == true)
            {
                //�A�C�e���g�p
                InventoryScript.InventoryScriptInstance.UseItem(slotData);
            }
            else
            {
                //�A�C�e���I��
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
    //�I���I���
    public void SlotExit()
    {
        //�A�C�e���I������
        DeselectSlot(slotData);
    }

    /// <summary>
    /// �I����Ԃ�
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
