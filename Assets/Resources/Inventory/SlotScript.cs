using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotScript : MonoBehaviour
{
    [SerializeField]
    //�X�^�b�N�\��/�Ǘ��X�N���v�g
    private StackScript stackScript;
    //�A�C�e���f�[�^
    public InventoryItem Item { get; private set; } = null;
    //�A�C�R���\���R���|�[�l���g
    public Image ItemIcon { get; private set; }
    //�C���x���g���[�Ǘ��N���X
    public InventoryScript Inventory;

    private void Start()
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
        entry.callback.AddListener((data) => { Inventory.OnDragDelegate((PointerEventData)data, (new InventoryScript.SlotData(gameObject, this))); });
        trigger.triggers.Add(entry);

        //�h���b�v�C�x���g
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Drop;
        entry.callback.AddListener((data) => { Inventory.OnDropDelegate((PointerEventData)data, (new InventoryScript.SlotData(gameObject, this))); });
        trigger.triggers.Add(entry);

        //�h���b�O�J�n�C�x���g
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.BeginDrag;
        entry.callback.AddListener((data) => { Inventory.OnBeginDragDelegate((PointerEventData)data, (new InventoryScript.SlotData(gameObject, this))); });
        trigger.triggers.Add(entry);

        //�N���b�N�C�x���g
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { Inventory.OnPinterClickDelegate((PointerEventData)data, (new InventoryScript.SlotData(gameObject, this))); });
        trigger.triggers.Add(entry);
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
    /// �X���b�g��������
    /// </summary>
    public void ResetSlot()
    {
        if (Item != null)
        {
            //�Z���N�g�����f���Q�[�g�����s
            Item?.DeselectDelegate?.Invoke(null);
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
