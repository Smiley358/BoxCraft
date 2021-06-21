using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxSpawnerScript : MonoBehaviour
{
    static public BoxSpawnerScript ScriptInstance;

    //�{�b�N�X��prefab
    private GameObject box;
    //�O����ύX����p��Box
    public GameObject NextBox;

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

    //BOX��ݒu���邽�߂̃f���Q�[�g
    private Action<GameObject, Vector3, Quaternion> boxSpawnDelegate;

    private void Awake()
    {
        ScriptInstance = this;
    }

    void Start()
    {
        //��ʒ����̍��W
        displayCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        //�ݒu�\��BOX�̃����_�[�擾
        predictionBoxMeshRenderer = predictionBox.GetComponent<MeshRenderer>();
        //�ݒu�\��BOX�̔񊈐���
        predictionBox.SetActive(false);

        //�ڒn�\��BOX���X�V
        predictionBoxScript.AttachPrefab(box);
    }

    void LateUpdate()
    {
        bool isCreatable = false;
        if (predictionBoxScript.IsValid())
        {
            isCreatable = predictionBoxScript.IsCreatable;
        }
        else
        {
            predictionBoxScript.AttachPrefab(box);
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

            //BOX�̐ݒu�\��������
            predictionBox.SetActive(true);

            //BOX�̉�]
            if (Input.GetKeyDown(KeyCode.R))
            {
                //��]�ʒm
                predictionBoxScript.Rotate();
            }

            //BOX�̐����E�폜����
            if (isCreatable)
            {
                //���C�����������ʂ̖@�������Ƀ{�b�N�X�𐶐����邽�ߌv�Z
                boxSpawnPos = raycastHit.normal + raycastHit.collider.transform.position;
                //�\��BOX�̍��W���X�V
                predictionBox.transform.position = boxSpawnPos;
                //Box�̐������\�񂳂�Ă���ꍇ��Box�𐶐�
                boxSpawnDelegate?.Invoke(box, boxSpawnPos, predictionBoxScript.GetRotate());
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

        //Box�̍X�V
        if(NextBox != box)
        {
            box = NextBox;
        }
        //�ݒu�\��BOX�̍X�V
        predictionBoxScript.AttachPrefab(box);
    }


    /// <summary>
    /// Box�̐�����\�񂷂�
    /// </summary>
    /// <param name="box">��������Box��prefab</param>
    /// <returns>�����\��o������True</returns>
    public bool ReservationSpawnBox(GameObject box)
    {
        //�ݒu�\�����ׂ�
        bool isCreatable = false;
        if (predictionBoxScript.IsValid())
        {
            isCreatable = predictionBoxScript.IsCreatable;
        }

        //���ɗ\��ς݂Ȃ�False��Ԃ�
        if (boxSpawnDelegate != null) return false;
        //���ݐ����s��ԂȂ�False��Ԃ�
        if (!isCreatable) return false;

        //Box�̐����f���Q�[�g�쐬
        boxSpawnDelegate = (GameObject box, Vector3 position, Quaternion rotation) =>
        {
            if (this.box != null)
            {
                // �����ʒu�̕ϐ��̍��W�Ƀu���b�N�𐶐�
                GameObject madeBox = Instantiate(box, position, rotation);
                madeBox.name = this.box.name;
            }
            boxSpawnDelegate = null;
        };

        return true;
    }
}
