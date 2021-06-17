using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemShortcutScript : MonoBehaviour
{   
    //�L�[���蓖�ėp�C���f�b�N�X���X�g
    public static readonly int[] IndexList = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
    //���݂̊��蓖�ď�
    private int nowIndex = 0;

    //�I�𒆂̃X���b�g
    public static int SelectIndex = UNSELECT_INDEX;
    //��I�𒆂̃C���f�b�N�X
    public static readonly int UNSELECT_INDEX = -1;

    //�V���[�g�J�b�g�̈ړ��R���|�[�l���g
    private RectTransform itemShortcutRect;
    //�V���[�g�J�b�g�̃T�C�Y
    private Vector2 itemShortcutSize;
    //�A�C�e���V���[�g�J�b�g�p�X���b�g��prefab
    public GameObject ItemShortcutSlotPrefab;
    //�X���b�g�Ԃ̃p�f�B���O
    public Vector2 slotPadding;

    //�X���b�g
    public Dictionary<int, ItemShortcutSlotData> itemShortcutSlots { get; private set; } = new Dictionary<int, ItemShortcutSlotData>(IndexList.Length);

    void Start()
    {
        //�A�C�e���V���[�g�J�b�g�����
        CreateLayout();
    }

    void Update()
    {
        float scroll = -Input.GetAxis("Mouse ScrollWheel");
        //+�Ȃ�1�ɁA-�Ȃ�-1�ɁA0�ɋ߂����0
        int indexShift = (Mathf.Abs(scroll) < 0.00001) ? 0 : (scroll > 0) ? 1 : -1;
        //�}�E�X�z�C�[���܂킵���Ƃ�
        if (indexShift != 0)
        {
            //-1~9�܂ł�����悤�Ɍv�Z
            SelectIndex = (IndexList.Length + 1 + SelectIndex + indexShift + 1) % (IndexList.Length + 1) - 1;
            if (SelectIndex != -1)
            {
                //�X���b�g��I��
                SlotScript.SelectSlot(itemShortcutSlots[IndexList[SelectIndex]].ItemShortcutSlotScript.slotData);
            }
            else
            {
                //���ݑI������Ă���X���b�g�̑I�����������āA�I���X���b�g��Null�ɂ���
                SlotScript.SelectSlot(null);
            }
        }
    }

    /// <summary>
    /// �I�����Ă���C���f�b�N�X���L�[�ɓo�^����Ă���C���f�b�N�X�ɒ���
    /// </summary>
    /// <returns></returns>
    public static int GetSelectIndexForSlotIndex()
    {
        return (SelectIndex != -1) ? IndexList[SelectIndex] : -1;
    }

    /// <summary>
    /// �A�C�e���V���[�g�J�b�g�̃��C�A�E�g�𐮂���
    /// </summary>
    private void CreateLayout()
    {
        //�A�C�e���V���[�g�J�b�g�𐶐�

        //�����L�[�����
        int itemShortCutSlots = IndexList.Length;
        //�V���[�g�J�b�g�̃g�����X�t�H�[���R���|�[�l���g�擾
        itemShortcutRect = GetComponent<RectTransform>();
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
            ItemShortcutSlotData newSlot = ItemShortcutSlotScript.Create(ItemShortcutSlotPrefab, gameObject, IndexList, nowIndex);
            if (newSlot == null)
            {
                Debug.Log("Item Shortcut Initialize Failed");
                break;
            }
            //�X���b�g�̃g�����X�t�H�[���R���|�[�l���g���擾
            RectTransform slotRectTransform = newSlot.Slot.GetComponent<RectTransform>();
            //���O��ύX
            newSlot.Slot.name = "ShortcutSlot";
            //�e��ItemShortcut��
            newSlot.Slot.transform.SetParent(transform);
            //�T�C�Y�ƈʒu���Z�b�g
            Vector3 offset = new Vector3(itemShortcutRect.rect.width, itemShortcutRect.rect.height, 0);
            offset.Scale(new Vector3(itemShortcutRect.pivot.x, itemShortcutRect.pivot.y, 0));
            slotRectTransform.localPosition = new Vector3(slotPadding.x * (x + 1) + (itemShortcutSlotSize * x) - offset.x, -slotPadding.y + offset.y);
            slotRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, itemShortcutSlotSize);
            slotRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, itemShortcutSlotSize);

            //�C���f�b�N�X�J�E���g�A�b�v
            nowIndex++;

            //���X�g�Ɋi�[
            itemShortcutSlots.Add(newSlot.ItemShortcutSlotScript.Index, newSlot);
        }
    }
}
