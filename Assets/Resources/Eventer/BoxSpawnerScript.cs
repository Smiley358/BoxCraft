using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxSpawnerScript : MonoBehaviour
{
    static public BoxSpawnerScript ScriptInstance;


    //�O����ύX����p��Box
    public GameObject NextBox;
    //�{�b�N�X��prefab
    private GameObject box;
    // �{�b�N�X��ݒu����ʒu
    private Vector3 boxSpawnPos;
    //BOX��ݒu���邽�߂̃f���Q�[�g
    private Action<GameObject, Vector3, Quaternion> boxSpawnDelegate;

    //�ݒu�\���pBOX
    [SerializeField] private GameObject predictionBox;
    //�ݒu�\���pBOX�̐���X�N���v�g
    [SerializeField] private PredictionBoxScript predictionBoxScript;


    private void Awake()
    {
        ScriptInstance = this;
    }

    void Start()
    {
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

        //���C�L���X�g�����擾
        RaycastHit raycastHit = RaycastManager.RaycastHitObject;

        //�C���x���g�����J���Ă��炸�A���C�������ɓ������Ă���
        if (!(InventoryScript.Instance.IsActiveInventory) && (raycastHit.collider != null))
        {
            //�{�b�N�X�łȂ���Ή������Ȃ�
            if(raycastHit.collider.tag == "Box")
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
                ChunkScript.CreateBoxAndAutomBelongToChunk(box, position, rotation);
            }
            boxSpawnDelegate = null;
        };

        return true;
    }
}
