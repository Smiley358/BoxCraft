using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Threading;
public class InventoryScript : MonoBehaviour
{
    //�C���x���g���[�̃C���X�^���X
    public static InventoryScript InventoryScriptInstance { get; private set; }
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

    //�X���b�g��prefab
    public GameObject SlotPrefab;
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

    //�I��
    public Sprite SelectSprite;
    //��I��
    public Sprite UnselectSprite;

    //�C���x���g���[�̗L���t���O
    public bool IsActiveInventory { get; private set; } = false;


    void Start()
    {
        InventoryScriptInstance = this;

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
                SlotData slotData = SlotScript.Create(SlotPrefab, gameObject);
                //�X���b�g�̃g�����X�t�H�[���R���|�[�l���g���擾
                RectTransform slotRectTransform = slotData.Slot.GetComponent<RectTransform>();
                //���O��ύX
                slotData.Slot.name = "Slot";
                //�e���C���x���g����
                slotData.Slot.transform.SetParent(transform);
                //�T�C�Y�ƈʒu���Z�b�g
                slotRectTransform.localPosition = new Vector3(slotPadding.x * (x + 1) + (SlotSize * x), -slotPadding.y * (y + 1) - (SlotSize * y));
                slotRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, SlotSize);
                slotRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, SlotSize);

                //���X�g�Ɋi�[
                allSlots.Add(slotData);
            }
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
        //�z�o�[�X���b�g���̒u������
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
            //�h���b�v�����I�𒆂Ȃ�I������
            SlotScript.DeselectSlot(dragSlotData);
            //�h���b�v��̃X���b�g�Ƀh���b�O��(�z�o�[�X���b�g)�X���b�g�̃f�[�^���ڂ�
            slot.SlotScript.Replace(hoverSlotScript);
            //�h���b�v�悪�I�𒆂Ȃ�
            if(SlotScript.selectSlotData == slot)
            {
                SlotScript.SelectSlot(slot);
            }
            //�h���b�O���f�[�^�̕ێ���j��
            dragSlotData = null;
        }

        //�z�o�[�X���b�g�𖳌���
        HoverSlot.SetActive(false);
        hoverSlotScript.ResetSlot(false);
    }

    public void OnPinterClickDelegate(PointerEventData data, SlotData slot)
    {
        //�E�N���b�N�ȊO�󂯕t���Ȃ�
        if (!(data.button == PointerEventData.InputButton.Right)) return;
        //��̃X���b�g�̓N���b�N�ł��Ȃ�
        if (slot.SlotScript.IsEmpty()) return;

        //�E�N���b�N�Ŏg�p���I����
        if (slot.SlotScript.Item?.IsImmediateUse == true)
        {
            //�A�C�e���g�p
            UseItem(slot);
        }
        else
        {
            //�����X���b�g�Ȃ�I������
            SlotScript.DeselectSlot(slot);
            //�A�C�e���X���b�g�I��
            SlotScript.SelectSlot(slot);
        }
    }
}
