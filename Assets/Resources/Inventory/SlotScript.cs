using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// �X���b�g���X�g�ɕۑ�����f�[�^
/// </summary>
public class SlotData
{

    public static bool operator ==(SlotData slotData1, SlotData slotData2) => slotData1?.Slot == slotData2?.Slot;
    public static bool operator !=(SlotData slotData1, SlotData slotData2) => slotData1?.Slot != slotData2?.Slot;

    public GameObject Slot;
    public SlotScript SlotScript;

    public SlotData(GameObject slot, SlotScript slotScript)
    {
        Slot = slot;
        SlotScript = slotScript;
    }

    public override bool Equals(object obj)
    {
        return obj is SlotData data &&
               EqualityComparer<GameObject>.Default.Equals(Slot, data.Slot) &&
               EqualityComparer<SlotScript>.Default.Equals(SlotScript, data.SlotScript);
    }

    public override int GetHashCode()
    {
        int hashCode = 1553962839;
        hashCode = hashCode * -1521134295 + EqualityComparer<GameObject>.Default.GetHashCode(Slot);
        hashCode = hashCode * -1521134295 + EqualityComparer<SlotScript>.Default.GetHashCode(SlotScript);
        return hashCode;
    }
}

public class SlotScript : MonoBehaviour
{
    //�I�𒆂̃X���b�g
    public static SlotData selectSlotData { get; private set; }

    /// <summary>
    /// �X���b�g�̍쐬�Ə����ݒ�̊ȗ���
    /// </summary>
    /// <param name="prefab">�쐬����prefab</param>
    /// <param name="inventory"></param>
    /// <returns></returns>
    public static SlotData Create(GameObject prefab)
    {
        //�X���b�g�̐���
        GameObject newSlot = Instantiate(prefab);

        SlotScript slotScript = newSlot.GetComponent<SlotScript>();
        //�C�x���g�n���h���[�̐ݒ�
        slotScript.PrepareEventTrigger();
        slotScript.SlotData = new SlotData(newSlot, slotScript);
        return slotScript.SlotData;
    }

    /// <summary>
    /// �X���b�g��I������
    /// ���ɑI������Ă�����̂�����ꍇ�I�������f���Q�[�g�����s����
    /// </summary>
    /// <param name="slotData">���ɑI������X���b�g�f�[�^�B
    /// null���w�肵���ꍇ�A���ݑI������Ă���X���b�g����������B</param>
    public static void SelectSlot(SlotData slotData)
    {
        //���ɑI�𒆂�������
        if (slotData == selectSlotData)
        {
            //�I������
            selectSlotData?.SlotScript?.Deselect();
            selectSlotData = null;
            return;
        }
        //�I�𒆂̃X���b�g������ΑI������
        selectSlotData?.SlotScript?.Deselect();
        //�A�C�e���I��
        slotData?.SlotScript?.Select();
        //�I�𒆃X���b�g���Z�b�g
        selectSlotData = slotData;
    }

    /// <summary>
    /// �w�肳�ꂽ�X���b�g���I������Ă����ꍇ�X���b�g�̑I����Ԃ�����
    /// </summary>
    /// <param name="slotData">�I�������������X���b�g�B
    /// ���ݑI������Ă�����̂łȂ��ꍇ�����s��Ȃ�</param>
    public static void DeselectSlot(SlotData slotData)
    {
        if (slotData != selectSlotData) return;
        //�A�C�e���I������
        slotData?.SlotScript.Deselect();
        //�I�𒆃X���b�g���Ȃ���
        selectSlotData = null;
    }

    //�A�C�e���f�[�^
    public InventoryItem Item { get; set; } = null;
    //�C���x���g���[�ŊǗ����Ă���X���b�g�f�[�^�N���X
    public SlotData SlotData { get; protected set; }
    //�A�C�e���A�C�R���\���R���|�[�l���g
    protected Image itemIcon;
    //�X���b�g�̃A�C�R��
    protected Image slotIcon;
    //�X�^�b�N�\��/�Ǘ��X�N���v�g
    [SerializeField] protected StackScript stackScript;

    protected void Start()
    {
        //�A�C�R���\���R���|�[�l���g���擾
        itemIcon = transform.Find("ItemIcon")?.GetComponent<Image>();
        if (itemIcon == null)
        {
            Debug.Log("�X���b�g�̏������Ɏ��s");
        }
        slotIcon = GetComponent<Image>();
        if (slotIcon == null)
        {
            Debug.Log("�X���b�g�̏������Ɏ��s");
        }
    }

    /// <summary>
    /// �C�x���g�n���h���[�̐ݒ�
    /// </summary>
    public void PrepareEventTrigger()
    {
        //�C�x���g�g���K�[
        EventTrigger trigger = GetComponent<EventTrigger>();

        //�h���b�O���C�x���g
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Drag;
        entry.callback.AddListener((data) => { InventoryScript.Instance.OnDragDelegate((PointerEventData)data, (new SlotData(gameObject, this))); });
        trigger.triggers.Add(entry);

        //�h���b�v�C�x���g
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Drop;
        entry.callback.AddListener((data) => { InventoryScript.Instance.OnDropDelegate((PointerEventData)data, (new SlotData(gameObject, this))); });
        trigger.triggers.Add(entry);

        //�h���b�O�J�n�C�x���g
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.BeginDrag;
        entry.callback.AddListener((data) => { InventoryScript.Instance.OnBeginDragDelegate((PointerEventData)data, (new SlotData(gameObject, this))); });
        trigger.triggers.Add(entry);

        //�N���b�N�C�x���g
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { InventoryScript.Instance.OnPinterClickDelegate((PointerEventData)data, (new SlotData(gameObject, this))); });
        trigger.triggers.Add(entry);
    }

    /// <summary>
    /// �I����Ԃ�
    /// </summary>
    protected virtual void Select()
    {
        //�A�C�R����I�𒆂�
        slotIcon.sprite = InventoryScript.Instance.SelectSprite;
        //�I�����̃f���Q�[�g�����s
        Item?.SelectDelegate?.Invoke(this);
    }

    /// <summary>
    /// �I����ԉ���
    /// </summary>
    protected virtual void Deselect()
    {
        //�A�C�R�����I�𒆂�
        slotIcon.sprite = InventoryScript.Instance.UnselectSprite;
        //�I���������̃f���Q�[�g�����s
        Item?.DeselectDelegate?.Invoke(this);
    }

    /// <summary>
    /// �A�C�e���g�p
    /// </summary>
    /// <returns>�A�C�e�����g�p�ł�����</returns>
    public bool UseItem()
    {
        //�A�C�e���f�[�^�Ȃ������玸�s
        if (Item == null) return false;

        //�A�C�e���g�p
        bool isUsed = Item.UseDelegate?.Invoke() ?? false;

        //�A�C�e���̎g�p������I�����Ă�����
        if (isUsed)
        {
            //�A�C�e���������炷
            stackScript.ItemCount--;

            //�g�p�ł��鐔���Ȃ�
            if (stackScript.ItemCount <= 0)
            {
                //�S�A�C�e���g�p�f���Q�[�g���s
                Item.UsedupDelegate?.Invoke(this);
                //�X���b�g�����Z�b�g
                ResetSlot();
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// �A�C�e�����ǉ��\��
    /// </summary>
    /// <returns>�ǉ��\��</returns>
    public int IsItemAddable(InventoryItem item)
    {
        //�A�C�e���f�[�^����̎��̓X���b�g�͋�Ȃ̂ł��̃A�C�e���̍ő吔��Ԃ�
        if (Item == null)
        {
            return item.MaxStackSize;
        }

        //�A�C�e���������ς��Ȃ�
        if (Item.MaxStackSize <= stackScript.ItemCount)
        {
            return 0;
        }
        return Item.MaxStackSize - stackScript.ItemCount;
    }

    /// <summary>
    /// �A�C�e����ǉ�
    /// </summary>
    /// <param name="count">�ǉ���</param>
    /// <returns>�ǉ��ł��Ȃ�������</returns>
    public int AddItem(int count, InventoryItem addItemData)
    {
        //�A�C�e���f�[�^�����ĂȂ�������ǉ����Ȃ�
        if (addItemData == null) return 0;
        //�����A�C�e���������Ă��Ȃ�������
        if (Item == null)
        {
            ItemSet(addItemData);
        }
        //�A�C�e�����������ǉ����Ȃ�
        if (!CanAddItem(addItemData.ItemName)) return 0;

        //�ǉ�
        stackScript.ItemCount += count;
        int addedItemOverFlowCount = Item.MaxStackSize - stackScript.ItemCount;
        //�X�^�b�N���I�[�o�[�t���[���Ă�����
        if (addedItemOverFlowCount < 0)
        {
            //�ő吔�ɂ���
            stackScript.ItemCount = Item.MaxStackSize;
            //�ǉ��łȂ��������̐���Ԃ�
            return System.Math.Abs(addedItemOverFlowCount);
        }

        //������Ȃ������A�C�e���͂Ȃ�
        return 0;
    }

    /// <summary>
    /// �A�C�e�����ǉ��ł��邩
    /// </summary>
    /// <param name="name">�ǉ��������A�C�e���Ǘ��f�[�^</param>
    /// <returns>�ǉ��ł���ꍇTrue</returns>
    public bool CanAddItem(string name)
    {
        //�A�C�e������Ȃ�ǉ��\
        if (Item == null) return true;
        //�������O�Ȃ�i�[�ł���
        if (name == Item.ItemName) return true;
        //�i�[�ł��Ȃ�
        return false;
    }

    /// <summary>
    /// �A�C�e�����Z�b�g����
    /// </summary>
    /// <param name="item">�A�C�e���Ǘ��f�[�^</param>
    void ItemSet(InventoryItem item)
    {
        Item = item;
        itemIcon.sprite = Item.ItemIcon;
        itemIcon.enabled = true;
    }

    /// <summary>
    /// �A�C�e�����擾
    /// </summary>
    /// <returns></returns>
    public int GetStackCount()
    {
        return stackScript.ItemCount;
    }

    /// <summary>
    /// �A�C�e���u������
    /// </summary>
    /// <param name="source"></param>
    public void Replace(SlotScript source)
    {
        //�A�C�e����������
        ResetSlot();
        //�A�C�e����ǉ�
        AddItem(source.GetStackCount(), source.Item);
    }

    /// <summary>
    /// �X���b�g�����Z�b�g
    /// </summary>
    /// <param name="isDelegateExecute">�f���Q�[�g�����s���邩</param>
    public void ResetSlot(bool isDelegateExecute = true)
    {
        if ((Item != null) && isDelegateExecute)
        {
            //�I�𒆂Ȃ����
            DeselectSlot(SlotData);
        }
        //�A�C�e�����폜
        Item = null;
        //�A�C�e���A�C�R��������
        if (itemIcon != null)
        {
            itemIcon.enabled = false;
            itemIcon.sprite = null;
        }
        //�X�^�b�N��񏉊���
        if (stackScript != null)
        {
            stackScript.ItemCount = 0;
        }
    }

    /// <summary>
    /// �X���b�g���󂩊m���߂�
    /// </summary>
    /// <returns>��Ȃ�True</returns>
    public bool IsEmpty()
    {
        return (Item == null);
    }
}
