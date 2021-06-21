using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
class PlayerInitializeFailException : Exception
{
    private string paramName;

    public PlayerInitializeFailException(string paramName,string message) : base(message)
    {
        this.paramName = paramName;
    }
    public override string ToString()
    {
        return "Initialize failed : " + paramName + "\n\n" + Message;
    }
}

public class PlayerScript : MonoBehaviour, IGroundCheck
{
    //�ʏ펞�̃R���X�g���C���g�iX�EY��]���Œ�AY���ړ����Œ�j
    private const RigidbodyConstraints NormalConstraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
    //���V���̃R���X�g���C���g�iX�EY��]���Œ�j
    private const RigidbodyConstraints FloatingConstraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    //�ڒn����
    private bool IsGround;
    //���_�x�N�g��
    private Vector3 ViewVector;
    //Player��Rigidbody
    private Rigidbody playerRigidbody;
    //�ړ����x
    [SerializeField] private float speed;
    //�_�b�V�����x
    [SerializeField] private float sprintSpeed;
    //�}�E�X���x
    [SerializeField] private float mouseSensitivity;
    //�W�����v��
    [SerializeField] private float jumpPower;
    //�v���C���[�̎��_�J����
    [SerializeField] private GameObject playerCamera;

    void Start()
    {
        //�������_
        ViewVector = new Vector3(1.0f, 0.0f, 0.0f);
        IsGround = false;
        //Rigidbody�擾
        playerRigidbody = GetComponent<Rigidbody>() ?? throw new PlayerInitializeFailException(paramName: nameof(playerRigidbody), message: "playerRigidbody cannot be null");
    }

    void Update()
    {
        //�C���x���g���[���J���Ă���Ƃ��̓J�����������Ȃ�
        if (!InventoryScript.Instance.IsActiveInventory)
        {
            //�}�E�X�x�N�g���̎擾
            Vector2 mouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            mouse *= mouseSensitivity;

            //���_�ړ�
            Vector3 nowVec = new Vector3(ViewVector.z, ViewVector.y, -ViewVector.x);
            ViewVector += nowVec.normalized * mouse.x;
            nowVec = Vector3.up;
            ViewVector += nowVec.normalized * mouse.y;
            ViewVector.Normalize();
        }


        //�J��������
        Vector3 camForward = Vector3.Scale(playerCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
        //�ړ��x�N�g��
        Vector3 moveVelocity = (camForward * Input.GetAxis("Vertical")) + (playerCamera.transform.right * Input.GetAxis("Horizontal"));
        //�W�����v�x�N�g��
        Vector3 jumpVelocity = Vector3.zero;

        //���W�b�h�{�f�B���擾�A�d�͂̐ݒ�
        playerRigidbody = GetComponent<Rigidbody>();
        //�X�s�[�h�v�Z
        moveVelocity *= speed;
        //Y���ړ��͏d�͌v�Z�ɔC����
        moveVelocity.y = 0;

        if (IsGround)
        {
            //�X�v�����g
            if (Input.GetKey(KeyCode.LeftShift))
            {
                moveVelocity *= sprintSpeed;
            }
            //�W�����v
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //�W�����v����ꍇ�d�͂̏d�͂����Z���Ȃ�
                jumpVelocity = Vector3.up * jumpPower;
                moveVelocity.y = jumpVelocity.y;
                //�c�ړ����Ȃ���
                playerRigidbody.constraints = FloatingConstraints;
                //�����������iFloat�̌덷�̂����Őڒn���肪�Ԃꂳ���Ȃ��o�C�A�X�����邽�߁j
                transform.position = transform.position + new Vector3(0, 0.03f, 0);
            }
        }
        else
        {
            //���V���͕������Z�̗����ɔC����
            moveVelocity.y = playerRigidbody.velocity.y;
        }

        //�ړ������x��ݒ�
        playerRigidbody.velocity = moveVelocity + jumpVelocity;

        //�J�����ƃv���C���[�̎��_�ݒ�
        LookAtSet();
    }

    private void OnTriggerEnter(Collider other)
    {
        //�A�C�e������Ȃ��Ȃ牽�����Ȃ�
        if (other.gameObject.name != "ItemContent") return;

        //�A�C�e���̒ǉ�
        InventoryScript.Instance.TryAddItem(other.gameObject.transform.parent.GetComponent<ItemScript>());
    }

    /// <summary>
    ///���_�E�̕����Z�b�g
    /// </summary>
    void LookAtSet()
    {
        //�J�������v���C���[�̓��̏�ɃZ�b�g
        playerCamera.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 1.0f, this.transform.position.z);
        //���_���Z�b�g
        playerCamera.transform.LookAt(playerCamera.transform.position + ViewVector);
        //�v���C���[�����_�����ɉ�]
        transform.rotation = Quaternion.Euler(0, playerCamera.transform.rotation.eulerAngles.y, 0);
    }

    /// <summary>
    ///�ڒn
    /// </summary>
    public void OnTheGround(float distance)
    {
        IsGround = true;
        //�c�ړ����Ȃ���
        playerRigidbody.constraints = NormalConstraints;
        //�n�ʂ̏�։����߂�
        transform.position = transform.position + new Vector3(0, distance, 0);
    }

    /// <summary>
    ///���V
    /// </summary>
    public void Floating()
    {
        IsGround = false;
        //�c�ړ����s��
        playerRigidbody.constraints = FloatingConstraints;
    }
}
