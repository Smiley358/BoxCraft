using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// BOX�̊��N���X
/// ���̃N���X���p�������BOX�Ƃ��Ă̍Œ���̋@�\��ۏ؂���
/// </summary>
public abstract class BoxBase : MonoBehaviour, IAttackableObject, IItemizeObject
{
    public static GameObject Create(GameObject box, Vector3 position, Quaternion rotation)
    {
        // �����ʒu�̕ϐ��̍��W�Ƀu���b�N�𐶐�
        GameObject madeBox = Instantiate(box, position, rotation);
        if (madeBox is null)
        {
            return null;
        }
        return madeBox;
    }

    public static GameObject Create(ChunkScript chunk, GameObject box, Vector3 position, Quaternion rotation)
    {
        // �����ʒu�̕ϐ��̍��W�Ƀu���b�N�𐶐�
        GameObject madeBox = Instantiate(box, position, rotation);
        if (madeBox is null)
        {
            return null;
        }
        madeBox.GetComponent<BoxBase>().affiliationChunk = chunk;
        return madeBox;
    }

    //�A�C�e���f�[�^
    public InventoryItem ItemData { get; protected set; } = new InventoryItem();
    //��]�\���ǂ���
    protected bool canRotate;
    //���G���Ԃ��ǂ���
    protected TimeSpanFlag isGod;
    //�R���C�_�[
    protected Collider boxCollider;
    //���b�V�������_���[
    protected MeshRenderer meshRenderer;
    //�����`�����N
    protected ChunkScript affiliationChunk;
    //HP
    [field: SerializeField] public int HP { get; protected set; }
    //�ő�HP
    [SerializeField] private int MaxHP = 5;
    //�������g��prefab���ABoxSpawner�ɃZ�b�g����悤
    [SerializeField] protected string prefabName;
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

        //���G���Ԃ̐ݒ�
        isGod = new TimeSpanFlag(300);

        //HP�ݒ�
        HP = MaxHP;

        //Collider��ێ�
        boxCollider = GetComponent<Collider>();

        //���b�V�������_���[��ێ�
        meshRenderer = GetMeshRenderer();
    }

    private void Start()
    {
        InitializeBox();
        DisableIfNeeded();
    }

    private void Update()
    {
        FrustumCulling();
    }

    private void OnDestroy()
    {
        //�אڂ���6������Box�ɑ΂��Ēʒm
        for (int i = (int)ChunkScript.Direction.Top; i <= (int)ChunkScript.Direction.Bottom; i++)
        {
            //�אڂ���Box�̎擾
            ChunkScript.BoxData affiliationBox = affiliationChunk?.GetAdjacentBox(gameObject, (ChunkScript.Direction)i);
            //�L�����ʒm
            affiliationBox?.Script?.OnDestroyNotification(ChunkScript.Direction.Bottom - i);
        }
    }

    private void OnEnable()
    {
        //�אڂ���6������Box�ɑ΂��Ēʒm
        for (int i = (int)ChunkScript.Direction.Top; i <= (int)ChunkScript.Direction.Bottom; i++)
        {
            //�אڂ���Box�̎擾
            ChunkScript.BoxData affiliationBox = affiliationChunk?.GetAdjacentBox(gameObject, (ChunkScript.Direction)i);
            //�L�����ʒm
            affiliationBox?.Script?.OnEnableNotification(ChunkScript.Direction.Bottom - i);
        }
    }

    private void OnDisable()
    {
        //�אڂ���6������Box�ɑ΂��Ēʒm
        for (int i = (int)ChunkScript.Direction.Top; i <= (int)ChunkScript.Direction.Bottom; i++)
        {
            //�אڂ���Box�̎擾
            ChunkScript.BoxData affiliationBox = affiliationChunk?.GetAdjacentBox(gameObject, (ChunkScript.Direction)i);
            //�L�����ʒm
            affiliationBox?.Script?.OnDisableNotification(ChunkScript.Direction.Bottom - i);
        }
    }

    /// <summary>
    /// ����direction������Box���L�������ꂽ�Ƃ��ɌĂ΂��
    /// </summary>
    /// <param name="direction">�������猩���ʒm�҂̕���</param>
    private void OnEnableNotification(ChunkScript.Direction direction)
    {
        DisableIfNeeded();
    }

    /// <summary>
    /// ����direction������Box�����������ꂽ�Ƃ��ɌĂ΂��
    /// </summary>
    /// <param name="direction">�������猩���ʒm�҂̕���</param>
    private void OnDisableNotification(ChunkScript.Direction direction)
    {

    }

    /// <summary>
    /// ����direction������Box���j�󂳂ꂽ�Ƃ��ɌĂ΂��
    /// </summary>
    /// <param name="direction">�������猩���ʒm�҂̕���</param>
    private void OnDestroyNotification(ChunkScript.Direction direction)
    {
        //����������������Ă����玩���͖���������
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// �K�v�ɉ�����GameObject�𖳌�������
    /// </summary>
    private void DisableIfNeeded()
    {
        bool isAllAdjacetBoxActive = true;
        //�אڂ���6������Box�S��
        for (int i = (int)ChunkScript.Direction.Top; i <= (int)ChunkScript.Direction.Bottom; i++)
        {
            //�אڂ���Box�̎擾
            ChunkScript.BoxData affiliationBox = affiliationChunk.GetAdjacentBox(gameObject, (ChunkScript.Direction)i);
            //�����Ȃ����Box�����݂��Ă��Ȃ��Ƃ�������
            if (affiliationBox is null)
            {
                //�������͂��Ȃ�
                isAllAdjacetBoxActive = false;
                break;
            }
        }

        //�אڂ���U�ʑS�Ă��������玩���͖���������
        if (isAllAdjacetBoxActive)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// EditorMode�̍ۂɎ�����J�����O���������d���̂ŉ�����邽�߂̃f�o�b�O�p�֐�
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    protected void FrustumCulling()
    {
        if (GeometryUtility.TestPlanesAABB(PlayerCamera.FrustumPlanes, boxCollider.bounds))
        {
            meshRenderer.enabled = true;
        }
        else
        {
            meshRenderer.enabled = false;
        }
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
        foreach (Collider colliderFlagment in colliders)
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
    public virtual void OnAttack(in GameObject attacker)
    {
        if (isGod) return;
        isGod.Begin();
        HP--;

        if (HP > 0) return;

        bool isCreate = ItemScript.Create(gameObject);
        if (isCreate)
        {
            affiliationChunk.DestroyBox(gameObject);
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
        return meshRenderer ?? GetComponent<MeshRenderer>();
    }
}