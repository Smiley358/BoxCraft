using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
class PlayerInitializeFailException : Exception
{
    public string paramName { get; private set; }

    public PlayerInitializeFailException(string paramName,string message) : base(message)
    {
        this.paramName = paramName;
    }
    public override string ToString()
    {
        return "Initialize failed : " + paramName + "\n\n" + Message;
    }
}

public class PlayerScript : MonoBehaviour
{
    //�ړ����x
    public float Speed;
    //�}�E�X���x
    public float MouseSensitivity;
    //�W�����v��
    public float JumpPower;

    //�v���C���[�̎��_�J����
    public GameObject PlayerCamera;

    //���_�x�N�g��
    private Vector3 ViewVector;
    //�ڒn����
    private bool IsGround;
    //Player��Rigidbody
    private Rigidbody playerRigidbody;

    //�C���x���g���[�Ǘ��N���X
    private InventoryScript inventoryScript;

    void Start()
    {
        //�������_
        ViewVector = new Vector3(1.0f, 0.0f, 0.0f);
        IsGround = false;
        //�C���x���g���[�̊Ǘ��N���X���擾
        inventoryScript = GameObject.Find("Inventory")?.GetComponent<InventoryScript>();
        //Rigidbody�擾
        playerRigidbody = GetComponent<Rigidbody>() ?? throw new PlayerInitializeFailException(paramName: nameof(playerRigidbody), message: "playerRigidbody cannot be null");
    }

    void Update()
    {
        //�C���x���g���[���J���Ă���Ƃ��̓J�����������Ȃ�
        if (!inventoryScript.IsActiveInventory)
        {
            //�}�E�X�x�N�g���̎擾
            Vector2 mouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            mouse *= MouseSensitivity;

            //���_�ړ�
            Vector3 nowVec = new Vector3(ViewVector.z, ViewVector.y, -ViewVector.x);
            ViewVector += nowVec.normalized * mouse.x;
            nowVec = Vector3.up;
            ViewVector += nowVec.normalized * mouse.y;
            ViewVector.Normalize();
        }


        //�J��������
        Vector3 camForward = Vector3.Scale(PlayerCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
        //�ړ��x�N�g��
        Vector3 moveVelocity = (camForward * Input.GetAxis("Vertical")) + (PlayerCamera.transform.right * Input.GetAxis("Horizontal"));
        //�W�����v�x�N�g��
        Vector3 jumpVelocity = Vector3.zero;

        //���W�b�h�{�f�B���擾�A�d�͂̐ݒ�
        playerRigidbody = GetComponent<Rigidbody>();
        moveVelocity *= Speed;
        moveVelocity.y = playerRigidbody.velocity.y;

        //�W�����v
        if (IsGround&&Input.GetKey(KeyCode.Space))
        {
            //�W�����v����ꍇ�d�͂̏d�͂����Z���Ȃ�
            jumpVelocity = Vector3.up * JumpPower;
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
        inventoryScript.TryAddItem(other.gameObject.transform.parent.GetComponent<ItemScript>());
    }

    //���_�E�̕����Z�b�g
    void LookAtSet()
    {
        //�J�������v���C���[�̓��̏�ɃZ�b�g
        PlayerCamera.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 1.0f, this.transform.position.z);
        //���_���Z�b�g
        PlayerCamera.transform.LookAt(PlayerCamera.transform.position + ViewVector);
        //�v���C���[�����_�����ɉ�]
        transform.rotation = Quaternion.Euler(0, PlayerCamera.transform.rotation.eulerAngles.y, 0);
    }

    //�ڒn
    public void OnTheGround()
    {
        IsGround = true;
    }

    //���V
    public void Floating()
    {
        IsGround = false;
    }
}
