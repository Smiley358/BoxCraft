using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BOX�̊��N���X
/// ���̃N���X���p�������BOX�Ƃ��Ă̍Œ���̋@�\��ۏ؂���
/// </summary>
public abstract class BoxBase : MonoBehaviour
{
    //�������g��prefab�ABoxSpawner�ɃZ�b�g����悤
    [SerializeField] protected GameObject prefab;
    //�A�C�e���f�[�^
    public InventoryItem ItemData { get; protected set; } = new InventoryItem();
    //��]�\���ǂ���
    protected bool canRotate;
    //�A�C�e���A�C�R��
    [SerializeField] private Sprite itemIcon;

    protected void InitializeBox()
    {
        ItemData.ItemName = gameObject.name.Replace("(Clone)", "").Split(' ')[0];
        ItemData.ItemIcon = itemIcon;

        ItemData.SelectDelegate = (SlotScript slot) =>
        {
            string path = "Box/" + ItemData.ItemName + "/" + ItemData.ItemName;
            BoxSpawnerScript.ScriptInstance.NextBox = Resources.Load(path) as GameObject;
        };

        ItemData.DeselectDelegate = (SlotScript slot) =>
        {
            BoxSpawnerScript.ScriptInstance.NextBox = null;
        };

        ItemData.UseDelegate = () =>
        {
            string path = "Box/" + ItemData.ItemName + "/" + ItemData.ItemName;
            return BoxSpawnerScript.ScriptInstance.ReservationSpawnBox(Resources.Load(path) as GameObject);
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
}