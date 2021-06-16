using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Threading;
public class InventoryScript : MonoBehaviour
{
    /// <summary>
    /// �X���b�g���X�g�ɕۑ�����f�[�^
    /// </summary>
    public class SlotData
    {
        public GameObject Slot;
        public SlotScript SlotScript;

        public SlotData(GameObject slot, SlotScript slotScript)
        {
            Slot = slot;
            SlotScript = slotScript;
        }
    }

    //�C���x���g���[�̈ړ��R���|�[�l���g
    private RectTransform inventoryRect;
    //�C���x���g���[�̃T�C�Y
    private Vector2 inventorySize;
    //�X���b�g��
    public int Slots;
    //�s��
    public int Rows;
    //�X���b�g�Ԃ̃p�f�B���O
    public Vector2 slotPadding;

    //�V���[�g�J�b�g�̈ړ��R���|�[�l���g
    private RectTransform itemShortcutRect;
    //�V���[�g�J�b�g�̃T�C�Y
    private Vector2 itemShortcutSize;
    //�A�C�e���V���[�g�J�b�g
    public GameObject ItemShortcut;

    //�X���b�g��prefab
    public GameObject SlotPrefab;
    //�A�C�e���V���[�g�J�b�g�p�X���b�g��prefab
    public GameObject ItemShortcutSlotPrefab;
    //�X���b�g���X�g
    private List<SlotData> allSlots;

    //�h���b�O�p�z�o�[�X���b�g
    public GameObject HoverSlot;
    //�h���b�O�p�z�o�[�X���b�g�̊Ǘ��N���X
    private SlotScript hoverSlotScript;
    //�h���b�O�p�z�o�[�X���b�g�̃A�C�R��
    private Image hoverSlotIconImage;
    //�h���b�O���̃X���b�g
    private SlotData dragSlotData;

    //�C���x���g���[�̗L���t���O
    public bool IsActiveInventory { get; private set; } = false;


    void Start()
    {
        //�C���x���g���[�̃��C�A�E�g�����
        CreateLayout();

        //�z�o�[�p�̃X���b�g�̏����ݒ�
        hoverSlotIconImage = HoverSlot.transform.Find("ItemIcon")?.GetComponent<Image>();
        hoverSlotScript = HoverSlot.GetComponent<SlotScript>();
        //��ԍŉ��ʂցi�őO�ʂɕ\�����邽�߁j
        HoverSlot.transform.SetAsLastSibling();
        //�񊈐���
        HoverSlot.SetActive(false);

        //�C���x���g���[�͕��Ă���
        HideInventory();
    }

    private void Update()
    {
        //Tab�ŃC���x���g���[�J��
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            IsActiveInventory = !IsActiveInventory;

            if (IsActiveInventory)
            {
                ShowInventory();
            }
            else
            {
                HideInventory();
            }
        }

        //�}�E�X�{�^����������z�o�[�X���b�g�͏���
        if (Input.GetMouseButtonUp(0))
        {
            //�h���b�O���f�[�^�̕ێ���j��
            dragSlotData = null;
            //�z�o�[�X���b�g�𖳌���
            HoverSlot.SetActive(false);
            hoverSlotScript.ResetSlot();
        }
    }

    //�C���x���g���[���J��
    public void ShowInventory()
    {
        //�}�E�X�J�[�\���߂�
        Cursor.lockState = CursorLockMode.None;
        gameObject.transform.localScale = Vector3.one;
    }

    //�C���x���g���[�����
    public void HideInventory()
    {
        //�}�E�X�J�[�����O�ɏo�Ȃ��悤��
        Cursor.lockState = CursorLockMode.Locked;
        gameObject.transform.localScale = Vector3.zero;
    }

    /// <summary>
    /// �A�C�e���̒ǉ������݂�
    /// </summary>
    /// <param name="item">�t�B�[���h�A�C�e���I�u�W�F�N�g�p�N���X</param>
    /// <returns>���ׂĒǉ��ł�������True</returns>
    public bool TryAddItem(ItemScript item)
    {
        while (true)
        {
            //�܂��͋󂢂Ă���X���b�g��T��
            foreach (var slot in allSlots)
            {
                //�A�C�e���I�u�W�F�N�g�ɃX�^�b�N���ꂽ�A�C�e���������Ȃ�
                if (item.contentCount <= 0)
                {
                    break;
                }
                //�A�C�e���̎�ނ��Ⴄ�Ȃ牽�����Ȃ�
                if (!slot.SlotScript.CanAddItem(item.ItemData.ItemName)) continue;

                //�X���b�g�ɓ���邱�Ƃ��ł���A�C�e���̐�
                int canStorageableCount = slot.SlotScript.IsItemAddable(item.ItemData);
                //�X���b�g�ɒǉ��ł��Ȃ��Ȃ牽�����Ȃ�
                if (canStorageableCount <= 0) continue;

                //�A�C�e���I�u�W�F�N�g����X���b�g����镪�̃J�E���g�����炷
                int storageCount = item.StorageItem(canStorageableCount);
                //�X���b�g�ɃA�C�e��������
                int couldNotAddedCount = slot.SlotScript.AddItem(storageCount, item.ItemData);
                //�]�蕪���A�C�e���I�u�W�F�N�g�ɖ߂�
                item.JoinItem(couldNotAddedCount);
            }

            //�ǉ��ł��Ă��Ȃ����̂�����
            if (item.contentCount > 0)
            {
                Debug.LogWarning("�A�C�e���ǉ����s");
                return false;
            }

            break;
        }
        return true;
    }


    /// <summary>
    /// �C���x���g���[�̃��C�A�E�g�𐮂���
    /// </summary>
    private void CreateLayout()
    {
        //�܂��C���x���g���[�{�̂𐶐�

        //�X���b�g���X�g
        allSlots = new List<SlotData>();
        //�C���x���g���[�̃g�����X�t�H�[���R���|�[�l���g�擾
        inventoryRect = GetComponent<RectTransform>();
        //�X���b�g�̑傫����Unity�G�f�B�^�[�̕����g�p����
        float SlotSize = SlotPrefab.GetComponent<RectTransform>().rect.width;
        //�C���x���g���[�̃T�C�Y���v�Z
        inventorySize.x = (Slots / Rows) * (SlotSize + slotPadding.x) + slotPadding.x;
        inventorySize.y = Rows * (SlotSize + slotPadding.y) + slotPadding.y;
        //�C���x���g���[�̃T�C�Y���Z�b�g
        inventoryRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, inventorySize.x);
        inventoryRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, inventorySize.y);

        int columns = Slots / Rows;

        for (int y = 0; y < Rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                //�X���b�g�̐���
                GameObject newSlot = Instantiate(SlotPrefab);
                //�X���b�g�̃g�����X�t�H�[���R���|�[�l���g���擾
                RectTransform slotRectTransform = newSlot.GetComponent<RectTransform>();
                //���O��ύX
                newSlot.name = "Slot";
                //�e���C���x���g����
                newSlot.transform.SetParent(transform);
                //�T�C�Y�ƈʒu���Z�b�g
                slotRectTransform.localPosition = new Vector3(slotPadding.x * (x + 1) + (SlotSize * x), -slotPadding.y * (y + 1) - (SlotSize * y));
                slotRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, SlotSize);
                slotRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, SlotSize);

                //���X�g�Ɋi�[
                SlotScript slotScript = newSlot.GetComponent<SlotScript>();
                slotScript.Inventory = this;
                slotScript.PrepareEventTrigger();
                SlotData slotData = new SlotData(newSlot, slotScript);
                allSlots.Add(slotData);
            }
        }


        //�A�C�e���V���[�g�J�b�g�𐶐�

        //�����L�[�����
        int itemShortCutSlots = ItemShortcutSlotScript.IndexList.Length;
        //�V���[�g�J�b�g�̃g�����X�t�H�[���R���|�[�l���g�擾
        itemShortcutRect = ItemShortcut.GetComponent<RectTransform>();
        //�X���b�g�̑傫����Unity�G�f�B�^�[�̕����g�p����
        float itemShortcutSlotSize = ItemShortcutSlotPrefab.GetComponent<RectTransform>().rect.width;
        //�V���[�g�J�b�g�̃T�C�Y���v�Z
        itemShortcutSize.x = itemShortCutSlots * (itemShortcutSlotSize + slotPadding.x) + slotPadding.x;
        itemShortcutSize.y = itemShortcutSlotSize + slotPadding.y * 2;
        //�V���[�g�J�b�g�̃T�C�Y���Z�b�g
        itemShortcutRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, itemShortcutSize.x);
        itemShortcutRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, itemShortcutSize.y);

        for (int x = 0; x < itemShortCutSlots; x++)
        {
            //�X���b�g�̐���
            GameObject newSlot = ItemShortcutSlotScript.Create(ItemShortcutSlotPrefab);
            if (newSlot == null)
            {
                Debug.Log("Item Shortcut Initialize Failed");
                break;
            }
            //�X���b�g�̃g�����X�t�H�[���R���|�[�l���g���擾
            RectTransform slotRectTransform = newSlot.GetComponent<RectTransform>();
            //���O��ύX
            newSlot.name = "ShortcutSlot";
            //�e��ItemShortcut��
            newSlot.transform.SetParent(ItemShortcut.transform);
            //�T�C�Y�ƈʒu���Z�b�g
            Vector3 offset = new Vector3(itemShortcutRect.rect.width, itemShortcutRect.rect.height, 0);
            offset.Scale(new Vector3(itemShortcutRect.pivot.x, itemShortcutRect.pivot.y, 0));
            slotRectTransform.localPosition = new Vector3(slotPadding.x * (x + 1) + (itemShortcutSlotSize * x) - offset.x, -slotPadding.y + offset.y);
            slotRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, itemShortcutSlotSize);
            slotRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, itemShortcutSlotSize);

            //���X�g�Ɋi�[
            SlotScript slotScript = newSlot.GetComponent<SlotScript>();
            slotScript.Inventory = this;
            slotScript.PrepareEventTrigger();
            SlotData slotData = new SlotData(newSlot, slotScript);
            allSlots.Add(slotData);
        }
    }


    /// <summary>
    /// �A�C�e���̎g�p
    /// </summary>
    /// <param name="slot">�g�p����A�C�e���̃X���b�g</param>
    /// <returns>����Ɏg�p�ł�����True</returns>
    public bool UseItem(SlotData slot)
    {
        //�X���b�g�f�[�^�Ȃ������玸�s
        if (slot == null) return false;

        //�A�C�e���̎g�p
        return slot.SlotScript.UseItem();
    }


    public void OnBeginDragDelegate(PointerEventData data, SlotData slot)
    {
        //���N���b�N�ȊO�󂯕t���Ȃ�
        if (!(data.button == PointerEventData.InputButton.Left)) return;
        //��̃X���b�g�̓h���b�O�ł��Ȃ�
        if (slot.SlotScript.IsEmpty()) return;

        //�z�o�[�X���b�g��L����
        HoverSlot.SetActive(true);
        //�X���b�g���̒u������
        hoverSlotScript.Replace(slot.SlotScript);
        //�h���b�O���̃X���b�g�f�[�^
        dragSlotData = slot;
    }

    public void OnDragDelegate(PointerEventData data, SlotData slot)
    {
        Vector3 position = new Vector3(data.position.x, data.position.y, 0);
        //�A�C�R�����}�E�X�J�[�\���̈ʒu��
        HoverSlot.transform.position = position;
    }

    public void OnDropDelegate(PointerEventData data, SlotData slot)
    {
        //�h���b�O���̃f�[�^������ꍇ
        if (dragSlotData != null)
        {
            //�h���b�v���̃X���b�g�Ƀh���b�v��X���b�g�̃f�[�^���ڂ�
            dragSlotData.SlotScript.Replace(slot.SlotScript);
            //�h���b�v��̃X���b�g�Ƀh���b�O��(�z�o�[�X���b�g)�X���b�g�̃f�[�^���ڂ�
            slot.SlotScript.Replace(hoverSlotScript);
            //�I����Ԃɂ���
            slot.Slot.GetComponent<Button>().Select();
            //�h���b�O���f�[�^�̕ێ���j��
            dragSlotData = null;
        }

        //�z�o�[�X���b�g�𖳌���
        HoverSlot.SetActive(false);
        hoverSlotScript.ResetSlot();
    }

    public void OnPinterClickDelegate(PointerEventData data, SlotData slot)
    {
        //�E�N���b�N�ȊO�󂯕t���Ȃ�
        if (!(data.button == PointerEventData.InputButton.Right)) return;
        //��̃X���b�g�̓N���b�N�ł��Ȃ�
        if (slot.SlotScript.IsEmpty()) return;

        //�E�N���b�N�Ŏg�p���I����
        if (slot.SlotScript.Item.IsImmediateUse)
        {
            //�A�C�e���g�p
            UseItem(slot);
        }
        else
        {
            //�A�C�e���I��
            slot.SlotScript.Item?.SelectDelegate?.Invoke(slot.SlotScript);
        }
    }
}
