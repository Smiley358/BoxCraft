using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BOX�̊��N���X
/// ���̃N���X���p�������BOX�Ƃ��Ă̍Œ���̋@�\��ۏ؂���
/// </summary>
public abstract class BoxBase : MonoBehaviour
{
    //�A�C�e���f�[�^
    public InventoryItem ItemData { get; protected set; } = new InventoryItem();
    //��]�\���ǂ���
    protected bool canRotate;
    //�A�C�e���A�C�R��
    [SerializeField] private Sprite itemIcon;

    void Awake()
    {
        ItemData.ItemName = gameObject.name.Replace("(Clone)", "");
        ItemData.ItemIcon = itemIcon;

        ItemData.SelectDelegate = (SlotScript slot) =>
        {
            BoxSpawnerScript boxSpawnerScript = GameObject.Find("BoxSpawner")?.GetComponent<BoxSpawnerScript>();
            string path = "Box/" + ItemData.ItemName + "/" + ItemData.ItemName;
            boxSpawnerScript.Box = Resources.Load(path) as GameObject;
            boxSpawnerScript.PredictionBoxUpdate();
        };

        ItemData.DeselectDelegate = (SlotScript slot) =>
        {
            BoxSpawnerScript boxSpawnerScript = GameObject.Find("BoxSpawner")?.GetComponent<BoxSpawnerScript>();
            boxSpawnerScript.Box = null;
            boxSpawnerScript.PredictionBoxUpdate();
        };

        ItemData.UseDelegate = () => true;

        ItemData.UsedupDelegate = (SlotScript slot) => 
        {
            BoxSpawnerScript boxSpawnerScript = GameObject.Find("BoxSpawner")?.GetComponent<BoxSpawnerScript>();
            boxSpawnerScript.Box = null;
            boxSpawnerScript.PredictionBoxUpdate();
        };
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