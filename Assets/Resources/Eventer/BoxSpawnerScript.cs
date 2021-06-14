using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxSpawnerScript : MonoBehaviour
{
    //�A�C�e���̃��C���[
    private const int ItemLayer = 6;

    //��ʒ����̍��W
    private Vector2 displayCenter;

    // �{�b�N�X��ݒu����ʒu
    private Vector3 boxSpawnPos;

    //�{�b�N�X��prefab
    public GameObject Box;

    //�ݒu�\���pBOX
    public GameObject PredictionBox;
    //�ݒu�\���pBOX�̃��b�V�������_���[
    private MeshRenderer predictionBoxMeshRenderer;
    //�ݒu�\���pBOX�̐���X�N���v�g
    private PredictionBoxScript predictionBoxScript;

    //�C���x���g��
    public GameObject Inventory;
    //�C���x���g���Ǘ��N���X
    public InventoryScript inventoryScript;
    //�I�𒆂̃X���b�g
    public SlotScript selectSlot;


    void Start()
    {
        //��ʒ����̍��W
        displayCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        //�ݒu�\��BOX�̔񊈐���
        PredictionBox.SetActive(false);
        //�ݒu�\��BOX�̃����_�[�擾
        predictionBoxMeshRenderer = PredictionBox.GetComponent<MeshRenderer>();
        //�ݒu�\��BOX�̐���X�N���v�g�擾
        predictionBoxScript = PredictionBox.GetComponent<PredictionBoxScript>();

        //�C���x���g���Z�b�g
        inventoryScript = GameObject.Find("Inventory")?.GetComponent<InventoryScript>();

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
        if (!(inventoryScript.IsActiveInventory) && Physics.Raycast(ray, out raycastHit, 10.0f, layerMask))
        {
            //���C�����������ʂ̖@�������Ƀ{�b�N�X�𐶐�����
            boxSpawnPos = raycastHit.normal + raycastHit.collider.transform.position;

            //BOX�̐ݒu�\��������
            PredictionBox.SetActive(true);
            PredictionBox.transform.position = boxSpawnPos;

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
                    selectSlot?.UseItem();
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
            PredictionBox.SetActive(false);
        }
    }

    public void PredictionBoxUpdate()
    {
        predictionBoxScript.AttachPrefab(Box);
    }
}
