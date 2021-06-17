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
    //���_�x�N�g��
    private Vector3 ViewVector;
    //�ڒn����
    private bool IsGround;
    //Player��Rigidbody
    private Rigidbody playerRigidbody;
    //�ړ����x
    [SerializeField] private float speed;
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
        moveVelocity *= speed;
        moveVelocity.y = playerRigidbody.velocity.y;

        //�W�����v
        if (IsGround&&Input.GetKey(KeyCode.Space))
        {
            //�W�����v����ꍇ�d�͂̏d�͂����Z���Ȃ�
            jumpVelocity = Vector3.up * jumpPower;
            moveVelocity.y = jumpVelocity.y;
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
    public void OnTheGround()
    {
        IsGround = true;
    }

    /// <summary>
    ///���V
    /// </summary>
    public void Floating()
    {
        IsGround = false;
    }
}
