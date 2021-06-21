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
    public static InventoryScript Instance { get; private set; }

    //�X���b�g���X�g
    private List<SlotData> allSlots;
    //�C���x���g���[�̈ړ��R���|�[�l���g
    private RectTransform inventoryRect;
    //�C���x���g���[�̃T�C�Y
    private Vector2 inventorySize;
    //�X���b�g��
    [SerializeField] private int slots;
    //�s��
    [SerializeField] private int rows;
    //�X���b�g�Ԃ̃p�f�B���O
    [SerializeField] private Vector2 slotPadding;
    //�X���b�g��prefab
    [SerializeField] private GameObject slotPrefab;

    //�h���b�O�p�z�o�[�X���b�g�̊Ǘ��N���X
    private SlotScript hoverSlotScript;
    //�h���b�O�p�z�o�[�X���b�g�̃A�C�R��
    private Image hoverSlotIconImage;
    //�h���b�O���̃X���b�g
    private SlotData dragSlotData;
    //�h���b�O�p�z�o�[�X���b�g
    [SerializeField] private GameObject hoverSlot;

    //�I�𒆂̃A�C�R��
    [field:SerializeField] public Sprite SelectSprite { get; protected set; }

    //��I���̃A�C�R��
    [field:SerializeField] public Sprite UnselectSprite { get; protected set; }

    //�C���x���g���[�̗L���t���O
    public bool IsActiveInventory { get; private set; } = false;


    void Start()
    {
        Instance = this;

        //�C���x���g���[�̃��C�A�E�g�����
        CreateLayout();

        //�z�o�[�p�̃X���b�g�̏����ݒ�
        hoverSlotIconImage = hoverSlot.transform.Find("ItemIcon")?.GetComponent<Image>();
        hoverSlotScript = hoverSlot.GetComponent<SlotScript>();
        //��ԍŉ��ʂցi�őO�ʂɕ\�����邽�߁j
        hoverSlot.transform.SetAsLastSibling();
        //�񊈐���
        hoverSlot.SetActive(false);

        //�C���x���g���[�͕��Ă���
        HideInventory();
    }

    void Update()
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
            hoverSlot.SetActive(false);
            hoverSlotScript.ResetSlot();
        }

        //�E�N���b�N������I����Ԃ̃A�C�e�����g�p����
        if ((!IsActiveInventory) && Input.GetMouseButtonDown(1))
        {
            //�A�C�e���g�p
            UseItem(SlotScript.selectSlotData);
        }
    }

    /// <summary>
    ///�C���x���g���[���J��
    /// </summary>
    public void ShowInventory()
    {
        //�}�E�X�J�[�\���߂�
        Cursor.lockState = CursorLockMode.None;
        gameObject.transform.localScale = Vector3.one;
    }
    
    /// <summary>
    ///�C���x���g���[�����
    /// </summary>
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
        float SlotSize = slotPrefab.GetComponent<RectTransform>().rect.width;
        //�C���x���g���[�̃T�C�Y���v�Z
        inventorySize.x = (slots / rows) * (SlotSize + slotPadding.x) + slotPadding.x;
        inventorySize.y = rows * (SlotSize + slotPadding.y) + slotPadding.y;
        //�C���x���g���[�̃T�C�Y���Z�b�g
        inventoryRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, inventorySize.x);
        inventoryRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, inventorySize.y);

        int columns = slots / rows;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                SlotData slotData = SlotScript.Create(slotPrefab);
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
        if (slot == null)
        {
            return false;
        }
        else
        {
            //�A�C�e���̎g�p
            return slot.SlotScript.UseItem();
        }
    }

    /// <summary>
    /// �h���b�O�J�n�C�x���g�n���h���[
    /// </summary>
    /// <param name="data">�C�x���g���</param>
    /// <param name="slot">�X���b�g���</param>
    public void OnBeginDragDelegate(PointerEventData data, SlotData slot)
    {
        //���N���b�N�ȊO�󂯕t���Ȃ�
        if (!(data.button == PointerEventData.InputButton.Left)) return;
        //��̃X���b�g�̓h���b�O�ł��Ȃ�
        if (slot.SlotScript.IsEmpty()) return;

        //�z�o�[�X���b�g��L����
        hoverSlot.SetActive(true);
        //�z�o�[�X���b�g���̒u������
        hoverSlotScript.Replace(slot.SlotScript);
        //�h���b�O���̃X���b�g�f�[�^
        dragSlotData = slot;
    }

    /// <summary>
    /// �h���b�O���C�x���g�n���h���[
    /// </summary>
    /// <param name="data">�C�x���g���</param>
    /// <param name="slot">�X���b�g���</param>
    public void OnDragDelegate(PointerEventData data, SlotData slot)
    {
        Vector3 position = new Vector3(data.position.x, data.position.y, 0);
        //�A�C�R�����}�E�X�J�[�\���̈ʒu��
        hoverSlot.transform.position = position;
    }

    /// <summary>
    /// �h���b�v���C�x���g�n���h���[
    /// </summary>
    /// <param name="data">�C�x���g���</param>
    /// <param name="slot">�X���b�g���</param>
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
        hoverSlot.SetActive(false);
        hoverSlotScript.ResetSlot(false);
    }

    /// <summary>
    /// �N���b�N���C�x���g�n���h���[
    /// </summary>
    /// <param name="data">�C�x���g���</param>
    /// <param name="slot">�X���b�g���</param>
    public void OnPinterClickDelegate(PointerEventData data, SlotData slot)
    {
        //�E�N���b�N�ȊO�󂯕t���Ȃ�
        if (!(data.button == PointerEventData.InputButton.Right)) return;
        //��̃X���b�g�̓N���b�N�ł��Ȃ�
        if (slot.SlotScript.IsEmpty()) return;

        //�����g�p���I����
        if (slot.SlotScript.Item?.IsImmediateUse == true)
        {
            //�A�C�e���g�p
            bool isSucceeded = UseItem(slot);
            if (!isSucceeded)
            {
                Debug.Log("Item could not be used successfully");
            }
        }
        else
        {
            //�A�C�e���X���b�g�I��
            SlotScript.SelectSlot(slot);
        }
    }
}
