using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxSpawnerScript : MonoBehaviour
{
    //�{�b�N�X��prefab
    public GameObject Box;

    //��ʒ����̍��W
    private Vector2 displayCenter;
    // �{�b�N�X��ݒu����ʒu
    private Vector3 boxSpawnPos;


    //�ݒu�\���pBOX�̃��b�V�������_���[
    private MeshRenderer predictionBoxMeshRenderer;
    //�ݒu�\���pBOX
    [SerializeField] private GameObject predictionBox;
    //�ݒu�\���pBOX�̐���X�N���v�g
    [SerializeField] private PredictionBoxScript predictionBoxScript;

    void Start()
    {
        //��ʒ����̍��W
        displayCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        //�ݒu�\��BOX�̃����_�[�擾
        predictionBoxMeshRenderer = predictionBox.GetComponent<MeshRenderer>();
        //�ݒu�\��BOX�̔񊈐���
        predictionBox.SetActive(false);

        //�ڒn�\��BOX���X�V
        PredictionBoxUpdate();
    }

    void Update()
    {
        bool isCreatable = false;
        if (predictionBoxScript.IsValid())
        {
            isCreatable = predictionBoxScript.IsCreatable;
        }
        else
        {
            PredictionBoxUpdate();
        }

        //���C���΂��x�N�g���̃Z�b�g
        Ray ray = Camera.main.ScreenPointToRay(displayCenter);
        //���C�����������I�u�W�F�N�g
        RaycastHit raycastHit;
        //���C���[�}�X�N
        LayerMask layerMask = LayerMask.GetMask("Box");

        //���C���΂�
        if (!(InventoryScript.Instance.IsActiveInventory) && Physics.Raycast(ray, out raycastHit, 10.0f, layerMask))
        {
            //���C�����������ʂ̖@�������Ƀ{�b�N�X�𐶐�����
            boxSpawnPos = raycastHit.normal + raycastHit.collider.transform.position;

            //BOX�̐ݒu�\��������
            predictionBox.SetActive(true);
            predictionBox.transform.position = boxSpawnPos;

            //BOX�̉�]
            if (Input.GetKeyDown(KeyCode.R))
            {
                //��]�ʒm
                predictionBoxScript.Rotate();
            }

            //BOX�̐����E�폜����
            if (Input.GetMouseButtonDown(1) && isCreatable && (Box != null))
            {
                if (Box != null)
                {
                    // �����ʒu�̕ϐ��̍��W�Ƀu���b�N�𐶐�
                    GameObject madeBox = Instantiate(Box, boxSpawnPos, predictionBoxScript.GetRotate());
                    madeBox.name = Box.name;
                    //�ݒu����
                    SlotScript.selectSlotData?.SlotScript?.UseItem();
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                //���C�����������I�u�W�F�N�g���폜
                bool isCreate = ItemScript.Create(raycastHit.collider.gameObject);
                if (isCreate)
                {
                    Destroy(raycastHit.collider.gameObject);
                }
            }
        }
        else
        {
            //BOX�̐ݒu�\���񊈐���
            predictionBox.SetActive(false);
        }
    }

    /// <summary>
    /// �ݒu�\��BOX���ŐV�̏�Ԃ֍X�V
    /// </summary>
    public void PredictionBoxUpdate()
    {
        predictionBoxScript.AttachPrefab(Box);
    }
}
