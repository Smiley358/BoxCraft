using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BOX�̊��N���X
/// ���̃N���X���p�������BOX�Ƃ��Ă̍Œ���̋@�\��ۏ؂���
/// </summary>
public abstract class BoxBase : MonoBehaviour,IAttackableObject,IItemizeObject
{
    public static GameObject Create(GameObject box, Vector3 position, Quaternion rotation)
    {
        // �����ʒu�̕ϐ��̍��W�Ƀu���b�N�𐶐�
        GameObject madeBox = Instantiate(box, position, rotation);
        if(madeBox is null)
        {
            return null;
        }
        return madeBox;
    }

    //�������g��prefab���ABoxSpawner�ɃZ�b�g����悤
    [SerializeField] protected string prefabName;
    //�A�C�e���f�[�^
    public InventoryItem ItemData { get; protected set; } = new InventoryItem();
    //��]�\���ǂ���
    protected bool canRotate;
    //�A�C�e���A�C�R��
    [SerializeField] private Sprite itemIcon;

    protected void InitializeBox()
    {
        //�퓬��(Clone)�������āAEditor����ǉ������Ƃ��ɏo����̐���������
        gameObject.name = gameObject.name.Replace("(Clone)", "").Split(' ')[0];
        ItemData.ItemName = gameObject.name;
        ItemData.ItemIcon = itemIcon;

        ItemData.SelectDelegate = (SlotScript slot) =>
        {
            BoxSpawnerScript.ScriptInstance.NextBox = PrefabManager.Instance.GetPrefab(slot.Item.ItemName);
        };

        ItemData.DeselectDelegate = (SlotScript slot) =>
        {
            BoxSpawnerScript.ScriptInstance.NextBox = null;
        };

        ItemData.UseDelegate = (SlotScript slot) =>
        {
            return BoxSpawnerScript.ScriptInstance.ReservationSpawnBox(PrefabManager.Instance.GetPrefab(slot.Item.ItemName));
        };

        ItemData.UsedupDelegate = (SlotScript slot) => 
        {
            BoxSpawnerScript.ScriptInstance.NextBox = null;
        };
    }

    private void Start()
    {
        InitializeBox();
    }

    /// <summary>
    /// 90Deg��]
    /// </summary>
    public void Rotate()
    {
        if (!canRotate) return;

        transform.Rotate(new Vector3(0, 90, 0));
    }

    /// <summary>
    /// �S�Ă̓����蔻��̖�����
    /// </summary>
    public void DisableCollition()
    {
        Collider[] colliders = GetComponents<Collider>();
        foreach(Collider colliderFlagment in colliders)
        {
            colliderFlagment.enabled = false;
        }
    }

    /// <summary>
    /// �S�Ă̓����蔻��̗L����
    /// </summary>
    public void EnableCollition()
    {
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider colliderFlagment in colliders)
        {
            colliderFlagment.enabled = true;
        }
    }

    /// <summary>
    /// �S�Ă̓����蔻��̃g���K�[������
    /// </summary>
    public void DisableCollitionIsTrigger()
    {
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider colliderFlagment in colliders)
        {
            colliderFlagment.isTrigger = false;
        }
    }

    /// <summary>
    /// �S�Ă̓����蔻��̃g���K�[��
    /// </summary>
    public void EnableCollitionIsTrigger()
    {
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider colliderFlagment in colliders)
        {
            colliderFlagment.isTrigger = true;
        }
    }


    //Implementation from Interface:IAttackableObject
    public  virtual void OnAttack(in GameObject attacker)
    {
        bool isCreate = ItemScript.Create(gameObject);
        if (isCreate)
        {
            Destroy(gameObject);
        }
    }

    //Implementation from Interface:IItemizeObject
    public virtual InventoryItem GetItemData()
    {
        return ItemData;
    }

    //Implementation from Interface:IItemizeObject
    public virtual MeshFilter GetMeshFilter()
    {
        return GetComponent<MeshFilter>();
    }

    //Implementation from Interface:IItemizeObject
    public virtual MeshRenderer GetMeshRenderer()
    {
        return GetComponent<MeshRenderer>();
    }
}