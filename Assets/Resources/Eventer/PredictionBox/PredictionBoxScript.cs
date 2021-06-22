using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredictionBoxScript : MonoBehaviour
{
    //�ݒu�\���}�e���A��
    [SerializeField] private Material canCreatableMaterial;
    //�ݒu�s�\���}�e���A��
    [SerializeField] private Material canNotCreatableMaterial;

    //�\���pBOX
    private GameObject renderBox;
    //�\���pBOX
    private BoxBase renderBoxScript;
    //�\���pBOX�}�e���A��
    private Material renderBoxMaterial;

    //��O�ɓ����Ă���BOX
    private GameObject previousBox;

    //�����\�t���O
    public bool IsCreatable { get; private set; }

    void Start()
    {
        IsCreatable = true;
    }

    void Update()
    {
        if (!IsValid()) return;

        if (renderBoxMaterial == null)
        {
            renderBoxMaterial = renderBoxScript?.GetMeshRenderer()?.material;
            if (renderBoxMaterial == null) return;
        }

        //BOX�̐ݒu�\���ە\��
        if (IsCreatable)
        {
            renderBoxMaterial.color = canCreatableMaterial.color;
        }
        else
        {
            renderBoxMaterial.color = canNotCreatableMaterial.color;
        }
    }

    public void OnTriggerEnter(UnityEngine.Collider other)
    {
        //Player�ȊO�͖���
        if (other.name != "Player") return;
        //�����s��
        IsCreatable = false;
    }

    public void OnTriggerExit(UnityEngine.Collider other)
    {
        //Player�ȊO�͖���
        if (other.name != "Player") return;
        //�����\
        IsCreatable = true;
    }

    public void OnTriggerStay(UnityEngine.Collider other)
    {
        //Player�ȊO�͖���
        if (other.name != "Player") return;
        IsCreatable = false;
    }

    /// <summary>
    /// �g�p�\���ǂ���
    /// </summary>
    /// <returns></returns>
    public bool IsValid()
    {
        return (renderBox != null) && (renderBoxScript != null);
    }


    /// <summary>
    /// BOX��90�x��]������
    /// </summary>
    public void Rotate()
    {
        if (renderBox == null) return;
        if (renderBoxScript == null) return;
        //��]������i��]���邩���Ȃ�����BoxBase�ɂ䂾�˂�j
        renderBoxScript.Rotate();
    }

    /// <summary>
    /// �ݒu���̉�]�p���擾
    /// </summary>
    public Quaternion GetRotate()
    {
        return renderBox!=null ? renderBox.transform.rotation : Quaternion.identity;
    }

    /// <summary>
    /// �\���p��BOX��Prefab���A�^�b�`����
    /// </summary>
    /// <param name="attach">�\���p��BOX��Prefab</param>
    public void AttachPrefab(GameObject attach)
    {
        //�������̂��A�^�b�`���悤�Ƃ��Ă����牽�����Ȃ�
        if (attach == previousBox)
        {
            return;
        }
        else
        {
            //attach�����Box��ۑ�
            previousBox = attach;
        }
        //���݂̕\���pBOX���f�^�b�`
        DetachPrefab();
        //null�Ȃ�쐬���Ȃ�
        if (attach == null) return;


        //�\���pBOX�̐���
        GameObject madeObject = Instantiate(attach, gameObject.transform);
        renderBox = madeObject;
        //���C���[�������Ɠ����ɁiIgnore raycast�j
        madeObject.layer = gameObject.layer;
        //�_�C�i�~�b�N�I�u�W�F�N�g�ɕύX
        madeObject.isStatic = false;
        //�X�N���v�g���擾
        renderBoxScript = renderBox.GetComponent<BoxBase>();
        //�����蔻��𖳌���
        renderBoxScript?.DisableCollition();
        //�V�F�[�_�[��Fade��
        renderBoxMaterial = renderBoxScript?.GetMeshRenderer()?.material;
        if (renderBoxMaterial != null)
        {
            renderBoxMaterial.SetOverrideTag("RenderType", "Transparent");
            renderBoxMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            renderBoxMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            renderBoxMaterial.SetInt("_ZWrite", 0);
            renderBoxMaterial.DisableKeyword("_ALPHATEST_ON");
            renderBoxMaterial.EnableKeyword("_ALPHABLEND_ON");
            renderBoxMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            renderBoxMaterial.renderQueue = 3000;
        }
    }

    /// <summary>
    /// �\���p��BOX��Prefab���f�^�b�`����
    /// </summary>
    public void DetachPrefab()
    {
        Destroy(renderBox);
        renderBox = null;
        renderBoxScript = null;
    }
}
