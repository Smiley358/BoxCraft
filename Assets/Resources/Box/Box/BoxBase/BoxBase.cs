using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// BOX�̊��N���X
/// ���̃N���X���p�������BOX�Ƃ��Ă̍Œ���̋@�\��ۏ؂���
/// </summary>
public abstract class BoxBase : MonoBehaviour, IAttackableObject, IItemizeObject, IMeshAccessor
{
    /// <summary>
    /// Box�𐶐����ď����`�����N���Z�b�g����
    /// </summary>
    /// <param name="chunk">�����`�����N</param>
    /// <param name="box">prefab</param>
    /// <param name="position">�������W</param>
    /// <param name="rotation">��]</param>
    /// <returns>��������Box</returns>
    public static GameObject Create(ChunkScript chunk, GameObject box, Vector3 position, Quaternion rotation)
    {
        //�`�����N�ɏ������Ȃ�Box�͋����Ȃ�
        if (chunk is null) return null;
        // �����ʒu�̕ϐ��̍��W�Ƀu���b�N�𐶐�
        GameObject madeBox = Instantiate(box, position, rotation);
        if (madeBox == null)
        {
            return null;
        }
        madeBox.GetComponent<BoxBase>().parentChunk = chunk;
        return madeBox;
    }

    //�A�C�e���f�[�^
    public InventoryItem ItemData { get; protected set; } = new InventoryItem();
    //�����`�����N
    public ChunkScript parentChunk { get; protected set; }
    //��]�\���ǂ���
    protected bool canRotate;
    //�������ς݂��ǂ���
    protected bool isInitialized;
    //���G���Ԃ��ǂ���
    protected TimeSpanFlag isGod;
    //�R���C�_�[
    protected Collider boxCollider;
    //���b�V�������_���[
    protected MeshRenderer meshRenderer;
    //���b�V���t�B���^�[
    protected MeshFilter meshFilter;
    //HP
    [field: SerializeField] public int HP { get; protected set; } = 0;
    //�ő�HP
    [SerializeField] private int MaxHP = 5;
    //�������g��prefab���ABoxSpawner�ɃZ�b�g����悤
    [SerializeField] protected string prefabName;
    //�A�C�e���A�C�R��
    [SerializeField] private Sprite itemIcon;

    protected void InitializeBox()
    {
        //�擪��(Clone)�������āAEditor����ǉ������Ƃ��ɏo����̐���������
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
            bool isUsed = BoxSpawnerScript.ScriptInstance.ReservationSpawnBox(PrefabManager.Instance.GetPrefab(slot.Item.ItemName));
            if (isUsed)
            {
                SoundEmitter.FindClip("set_box").Play();
            }
            return isUsed;
        };

        ItemData.UsedupDelegate = (SlotScript slot) =>
        {
            BoxSpawnerScript.ScriptInstance.NextBox = null;
        };
    }

    protected void InitializeField()
    {
        //���G���Ԃ̐ݒ�
        isGod = new TimeSpanFlag(300);

        //HP�ݒ�
        HP = MaxHP;

        //Collider��ێ�
        boxCollider = GetComponent<Collider>();

        //���b�V�������_���[��ێ�
        meshRenderer = GetComponent<MeshRenderer>();

        //���b�V���t�B���^�[��ێ�
        meshFilter = GetComponent<MeshFilter>();

        //�������I���t���O�𗧂Ă�
        isInitialized = true;
    }

    private void Awake()
    {
        InitializeField();
    }

    private void Start()
    {
        InitializeBox();
    }

    private void OnDestroy()
    {
        //�אڂ���6������Box�ɑ΂��Ēʒm
        for (int i = (int)ChunkScript.Direction.Top; i <= (int)ChunkScript.Direction.Bottom; i++)
        {
            //�אڂ���Box�̎擾
            ChunkScript.BoxData affiliationBox = parentChunk?.GetAdjacentBox(transform.position, (ChunkScript.Direction)i);
            //�L�����ʒm
            affiliationBox?.Script?.OnDestroyNotification(ChunkScript.Direction.Bottom - i);
        }
    }

    private void OnEnable()
    {
        if (!isInitialized) return;
        //�אڂ���6������Box�ɑ΂��Ēʒm
        for (int i = (int)ChunkScript.Direction.Top; i <= (int)ChunkScript.Direction.Bottom; i++)
        {
            //�אڂ���Box�̎擾
            ChunkScript.BoxData affiliationBox = parentChunk?.GetAdjacentBox(transform.position, (ChunkScript.Direction)i);
            //�L�����ʒm
            affiliationBox?.Script?.OnEnableNotification(ChunkScript.Direction.Bottom - i);
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
    /// ����direction������Box���j�󂳂ꂽ�Ƃ��ɌĂ΂��
    /// </summary>
    /// <param name="direction">�������猩���ʒm�҂̕���</param>
    private void OnDestroyNotification(ChunkScript.Direction direction)
    {
        //����������������Ă����玩����L��������
        if (!(gameObject?.activeSelf ?? false))
        {
            gameObject.SetActive(true);
        }
        MeshUpdate(null);
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
    /// �K�v�ɉ�����GameObject�𖳌�������
    /// </summary>
    public void DisableIfNeeded()
    {
        //����Box��ʂ��Ă��Ȃ��ʂ̕���
        List<ChunkScript.Direction> notAdjoining = new List<ChunkScript.Direction>();
        //�ڂ��Ă��Ȃ��ʂ����������ǂ���
        bool isAllAdjacetBoxActive = true;
        //�אڂ���6������Box�S��
        for (int i = (int)ChunkScript.Direction.Top; i <= (int)ChunkScript.Direction.Bottom; i++)
        {
            //�אڂ���Box�̎擾
            bool isExistAffiliationBox = parentChunk?.IsAdjacetBoxExist(gameObject, (ChunkScript.Direction)i) ?? false;
            //�����Ȃ����Box�����݂��Ă��Ȃ��Ƃ�������
            if (isExistAffiliationBox == false)
            {
                //�������͂��Ȃ�
                isAllAdjacetBoxActive = false;
                notAdjoining.Add((ChunkScript.Direction)i);
            }
        }

        //�אڂ���U�ʑS�Ă��������玩���͖���������
        if (isAllAdjacetBoxActive)
        {
            gameObject.SetActive(false);
        }
        //�ڂ��Ă��Ȃ��ʂ̂݃��b�V���𒣂�
        else
        {
            MeshUpdate(notAdjoining);
        }
    }

    /// <summary>
    /// ���b�V���`����X�V����
    /// </summary>
    /// <param name="notAdjoining">����Box�ɐڂ��Ă��Ȃ���</param>
    private void MeshUpdate(List<ChunkScript.Direction> notAdjoining)
    {
        if(notAdjoining == null)
        {
            //����Box��ʂ��Ă��Ȃ��ʂ̕���
            notAdjoining = new List<ChunkScript.Direction>();
            //�אڂ���6������Box�S��
            for (int i = (int)ChunkScript.Direction.Top; i <= (int)ChunkScript.Direction.Bottom; i++)
            {
                //�אڂ���Box�̎擾
                bool isExistAffiliationBox = parentChunk?.IsAdjacetBoxExist(gameObject, (ChunkScript.Direction)i) ?? false;
                //�����Ȃ����Box�����݂��Ă��Ȃ��Ƃ�������
                if (isExistAffiliationBox == false)
                {
                    notAdjoining.Add((ChunkScript.Direction)i);
                }
            }
        }

        //���b�V�������p
        CombineInstance[] combine = new CombineInstance[notAdjoining.Count];
        for (int i = 0; i < notAdjoining.Count; i++)
        {
            //���b�V�������f�[�^�����
            var direction = notAdjoining[i];
            combine[i].mesh = PrefabManager.Instance.GetMesh(direction.ToString()).sharedMesh;
            combine[i].transform = PrefabManager.Instance.GetMesh(direction.ToString()).transform.localToWorldMatrix;
        }
        //���b�V���̐���
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        meshFilter.mesh.CombineMeshes(combine);
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

    /// <summary>
    /// �󂳂ꂽ�Ƃ��̌��ʉ����Đ�
    /// </summary>
    protected virtual void PlaySEBreak()
    {
        SoundEmitter.FindClip("break_stone")?.Play();
    }

    /// <summary>
    /// �U�����󂯂����̌��ʉ����Đ�
    /// </summary>
    protected virtual void PlaySEOnAttack()
    {
        SoundEmitter.FindClip("dig_stone")?.Play();
    }


    //Implementation from Interface:IAttackableObject
    public virtual void OnAttack(in GameObject attacker)
    {
        //���G���Ԓ��͉������Ȃ�
        if (isGod) return;
        //���G�^�C�}�[�J�n
        isGod.Begin();
        //HP�J�E���g�_�E��
        HP--;

        //HP���Ȃ��Ȃ����H
        if (HP > 0)
        {
            //�U�����̉����o��
            PlaySEOnAttack();
            //���ĂȂ��̂ŏ����͂����܂�
            return;
        }
        else
        {
            //�j�󎞂̉����o��
            PlaySEBreak();
        }
        //���b�V����Box�֖߂�
        meshFilter.mesh = PrefabManager.Instance.GetMesh("Box").sharedMesh;
        //�ȉ��j�󏈗�
        bool isCreate = ItemScript.Create(gameObject);
        if (isCreate)
        {
            parentChunk.DestroyBox(gameObject);
        }
    }

    //Implementation from Interface:IItemizeObject,IMeshAccessor
    public virtual Mesh GetMesh()
    {
        return meshFilter.mesh;
    }

    //Implementation from Interface:IItemizeObject,IMeshAccessor
    public virtual Material GetMaterial()
    {
        return GetComponent<MeshRenderer>().material;
    }

    //Implementation from Interface:IItemizeObject
    public virtual InventoryItem GetItemData()
    {
        return ItemData;
    }

    //Implementation from Interface:IItemizeObject
    public virtual Vector3 GetMeshScale()
    {
        return Vector3.one;
    }

    //Implementation from Interface:IMeshAccessor
    public virtual MeshFilter GetMeshFilter()
    {
        return meshFilter;
    }

    //Implementation from Interface:IMeshAccessor
    public virtual MeshRenderer GetMeshRenderer()
    {
        return GetComponent<MeshRenderer>();
    }
}