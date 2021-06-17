using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///�A�C�e���V���[�g�J�b�g�̃X���b�g�ێ��p
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
    ///�V���[�g�J�b�g�p�X���b�g�̍쐬�Ɗ��蓖��
    /// </summary>
    /// <param name="prefab">�쐬����prefab</param>
    /// <param name="IndexList">�쐬���邽�߂̃C���f�b�N�X���X�g</param>
    /// <param name="index">���݂̃C���f�b�N�X</param>
    /// <returns></returns>
    public static ItemShortcutSlotData Create(GameObject prefab, int[] IndexList, int index)
    {
        //�L�[���蓖�Ăł��Ȃ��̂ō��Ȃ�
        if (index > IndexList.Length - 1) return null;

        //�V���[�g�J�b�g�X���b�g����
        GameObject slot = Instantiate(prefab);
        ItemShortcutSlotScript itemShortcutSlotScript = slot.GetComponent<ItemShortcutSlotScript>();
        itemShortcutSlotScript.SlotData = new SlotData(slot, itemShortcutSlotScript);
        //�C�x���g�n���h���[�����ݒ�
        itemShortcutSlotScript.SlotData.SlotScript.PrepareEventTrigger();


        //�C���f�b�N�X���蓖��
        itemShortcutSlotScript.Index = IndexList[index];
        //�C���f�b�N�X�\��
        Text text = slot.transform.Find("Index")?.GetComponent<Text>();
        text.text = itemShortcutSlotScript.Index.ToString();

        return new ItemShortcutSlotData(slot, itemShortcutSlotScript.SlotData.SlotScript, itemShortcutSlotScript);
    }



    //�L�[���蓖�ėp�C���f�b�N�X
    public int Index { get; private set; } = -1;

    void Update()
    {
        if (slotIcon == null)
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
        if (selectSlotData != SlotData)
        {
            //�����g�p���I����
            if (SlotData.SlotScript.Item?.IsImmediateUse == true)
            {
                //�A�C�e���g�p
                InventoryScript.Instance.UseItem(SlotData);
            }
            else
            {
                //�A�C�e���I��
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
    //�I���I���
    public void SlotExit()
    {
        //�A�C�e���I������
        DeselectSlot(SlotData);
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
