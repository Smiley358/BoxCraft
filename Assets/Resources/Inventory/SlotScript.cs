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
    public static SlotData Create(GameObject prefab, GameObject inventory, InventoryScript inventoryScript)
    {
        //�X���b�g�̐���
        GameObject newSlot = Instantiate(prefab);

        SlotScript slotScript = newSlot.GetComponent<SlotScript>();
        //�C���x���g���[�̓o�^
        slotScript.InventoryScript = inventoryScript;
        //�C�x���g�n���h���[�̐ݒ�
        slotScript.PrepareEventTrigger();

        return new SlotData(newSlot, slotScript);
    }

    [SerializeField]
    //�X�^�b�N�\��/�Ǘ��X�N���v�g
    private StackScript stackScript;
    //�A�C�e���f�[�^
    public InventoryItem Item { get; protected set; } = null;
    //�A�C�e���A�C�R���\���R���|�[�l���g
    public Image ItemIcon { get; protected set; }
    //�C���x���g���[�Ǘ��N���X
    public InventoryScript InventoryScript;
    //�X���b�g�̃A�C�R��
    protected Image SlotIcon;

    protected void Start()
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

    /// <summary>
    /// 
    /// </summary>
    public void PrepareEventTrigger()
    {
        //�C�x���g�g���K�[
        EventTrigger trigger = GetComponent<EventTrigger>();

        //�h���b�O���C�x���g
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Drag;
        entry.callback.AddListener((data) => { InventoryScript.OnDragDelegate((PointerEventData)data, (new SlotData(gameObject, this))); });
        trigger.triggers.Add(entry);

        //�h���b�v�C�x���g
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Drop;
        entry.callback.AddListener((data) => { InventoryScript.OnDropDelegate((PointerEventData)data, (new SlotData(gameObject, this))); });
        trigger.triggers.Add(entry);

        //�h���b�O�J�n�C�x���g
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.BeginDrag;
        entry.callback.AddListener((data) => { InventoryScript.OnBeginDragDelegate((PointerEventData)data, (new SlotData(gameObject, this))); });
        trigger.triggers.Add(entry);

        //�N���b�N�C�x���g
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { InventoryScript.OnPinterClickDelegate((PointerEventData)data, (new SlotData(gameObject, this))); });
        trigger.triggers.Add(entry);
    }

    /// <summary>
    /// �I����Ԃ�
    /// </summary>
    public virtual void Select()
    {
        //�A�C�R����I�𒆂�
        SlotIcon.sprite = InventoryScript.SelectSprite;
        //�I�����̃f���Q�[�g�����s
        Item?.SelectDelegate?.Invoke(this);
    }

    /// <summary>
    /// �I����ԉ���
    /// </summary>
    public virtual void Deselect()
    {
        //�A�C�R�����I�𒆂�
        SlotIcon.sprite = InventoryScript.UnselectSprite;
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
            stackScript.itemCount--;
            
            //�g�p�ł��鐔���Ȃ�
            if (stackScript.itemCount <= 0)
            {
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
        if(Item.MaxStackSize <= stackScript.itemCount)
        {
            return 0;
        }
        return Item.MaxStackSize - stackScript.itemCount;
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
        stackScript.itemCount += count;
        int addedItemOverFlowCount = Item.MaxStackSize - stackScript.itemCount;
        //�X�^�b�N���I�[�o�[�t���[���Ă�����
        if(addedItemOverFlowCount < 0)
        {
            //�ő吔�ɂ���
            stackScript.itemCount = Item.MaxStackSize;
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
        ItemIcon.sprite = Item.ItemIcon;
        ItemIcon.enabled = true;
    }

    /// <summary>
    /// �A�C�e�����擾
    /// </summary>
    /// <returns></returns>
    public int GetStackCount()
    {
        return stackScript.itemCount;
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
            //�Z���N�g�����f���Q�[�g�����s
            Deselect();
        }
        //�A�C�e�����폜
        Item = null;
        //�A�C�e���A�C�R��������
        if (ItemIcon != null)
        {
            ItemIcon.enabled = false;
            ItemIcon.sprite = null;
        }
        //�X�^�b�N��񏉊���
        if (stackScript != null)
        {
            stackScript.itemCount = 0;
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
