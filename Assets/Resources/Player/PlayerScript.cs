using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

class PlayerInitializeFailException : Exception
{
    private string paramName;

    public PlayerInitializeFailException(string paramName, string message) : base(message)
    {
        this.paramName = paramName;
    }
    public override string ToString()
    {
        return "Initialize failed : " + paramName + "\n\n" + Message;
    }
}

public class PlayerScript : MonoBehaviour, IGroundCheck, IAttackableObject
{
    //�ʏ펞�̃R���X�g���C���g�iX�EY��]���Œ�AY���ړ����Œ�j
    private const RigidbodyConstraints NormalConstraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
    //���V���̃R���X�g���C���g�iX�EY��]���Œ�j
    private const RigidbodyConstraints FloatingConstraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

    //PlayerScript�̃C���X�^���X
    public static PlayerScript Player { get; private set; }

    //HP
    public int HP { get; private set; }

    //�ڒn����
    private bool IsGround;
    //�t�̒�����
    private bool IsSwim;
    //���_�x�N�g��
    private Vector3 ViewVector;
    //�m�b�N�o�b�N�x�N�g��
    private Vector3 KnockBackVector;
    //Player��Rigidbody
    private Rigidbody playerRigidbody;
    //�U���\�t���O
    private TimeSpanFlag isAttacking;
    //�ړ����x
    [SerializeField] private float speed;
    //�_�b�V�����x
    [SerializeField] private float sprintSpeed;
    //�}�E�X���x
    [SerializeField] private float mouseSensitivity;
    //�W�����v��
    [SerializeField] private float jumpPower;
    //�j����
    [SerializeField] private float swimPower;
    //�j���ۂ̑��x
    [SerializeField] private float swimSpeed;
    //�����ł̏d��
    [SerializeField] private float swimGravity;
    //�m�b�N�o�b�N�͂̌�����
    [SerializeField] private float KnockBackAttenuation;
    //�m�b�N�o�b�N�̍ŏ��l
    [SerializeField] private float MinKnockBackPower;
    //�m�b�N�o�b�N���̕�������
    [SerializeField] private float KnockBackPowerY;

    private void Awake()
    {
        //�������_
        ViewVector = new Vector3(1.0f, 0.0f, 0.0f);
        //�m�b�N�o�b�N�x�N�g��������
        KnockBackVector = Vector3.zero;
        //�ŏ��͋󒆔���
        IsGround = false;
        //�U���t���O
        isAttacking = new TimeSpanFlag(350);
        //HP������
        HP = HPControler.MaxHP;
        //�C���X�^���X���Z�b�g
        Player = this;
    }

    void Start()
    {
        //Rigidbody�擾
        playerRigidbody = GetComponent<Rigidbody>() ?? throw new PlayerInitializeFailException(paramName: nameof(playerRigidbody), message: "playerRigidbody cannot be null");
    }

    private void FixedUpdate()
    {
        if (Mathf.Abs(Vector3.Distance(Vector3.zero, KnockBackVector)) >= MinKnockBackPower)
        {
            //�m�b�N�o�b�N�͂̌���
            KnockBackVector *= KnockBackAttenuation;
        }
        else
        {
            KnockBackVector = Vector3.zero;
        }
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.G))
        {
            HP -= 35;
            Debug.Log("HP:" + HP.ToString());
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            HP += 15;
            Debug.Log("HP:" + HP.ToString());
        }

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

            //�U������
            if (!isAttacking && Input.GetMouseButton(0))
            {
                //�U������J�n
                isAttacking.Begin();

                //���C�����������I�u�W�F�N�g
                GameObject hitGameObject = RaycastManager.RaycastHitObject.collider?.gameObject;
                if (hitGameObject != null)
                {
                    //���C�����������I�u�W�F�N�g�ɍU������
                    EXAttackableObject.AttackNotification(gameObject, hitGameObject);
                }
                else
                {
                    //��U�艹���Đ�
                    SoundEmitter.FindClip("missing")?.Play();
                }
            }
        }


        //�J��������
        Vector3 camForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 1, 1)).normalized;
        //�ړ��x�N�g��
        Vector3 moveVelocity = (camForward * Input.GetAxis("Vertical")) + (Camera.main.transform.right * Input.GetAxis("Horizontal"));
        //�W�����v�x�N�g��
        Vector3 jumpVelocity = Vector3.zero;

        //�j�����Ȃ�
        if (IsSwim)
        {
            //�c�ړ�������
            playerRigidbody.constraints = FloatingConstraints;

            if (Input.GetKey(KeyCode.Space))
            {
                //�W�����v����ꍇ�d�͂̏d�͂����Z���Ȃ�
                jumpVelocity = Vector3.up * swimPower;
            }

            //�����ł̈ړ����x�ɂ���
            moveVelocity.Scale(new Vector3(swimSpeed, swimSpeed, swimSpeed));
            moveVelocity.y += swimGravity;
        }
        else
        {
            //�X�s�[�h�v�Z
            moveVelocity *= speed;
            //�ڒn
            if (IsGround)
            {
                //Y���ړ��͏d�͌v�Z�ɔC����
                moveVelocity.y = 0;
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
                    //�c�ړ�������
                    playerRigidbody.constraints = FloatingConstraints;
                    //�����������iFloat�̌덷�̂����Őڒn������Ԃꂳ���Ȃ��悤�ɏ����߂荞�܂��Ă��邽�߁j
                    transform.position = transform.position + new Vector3(0, 0.03f, 0);
                }
            }
            //���V���͕������Z�̗����ɔC����
            moveVelocity.y = playerRigidbody.velocity.y;
        }

        //�ړ������x��ݒ�
        playerRigidbody.velocity = moveVelocity + jumpVelocity + KnockBackVector;

        //�J�����ƃv���C���[�̎��_�ݒ�
        LookAtSet();
    }

    private void LateUpdate()
    {
        var boxName = ChunkScript.GetPrefabName(transform.position);

        if (boxName == "BoxVariant_Water")
        {
            IsSwim = true;
        }
        else
        {
            IsSwim = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.name)
        {
            case "ItemContent":
                //�A�C�e���̒ǉ�
                InventoryScript.Instance.TryAddItem(other.gameObject.transform.parent.GetComponent<ItemScript>());
                break;

            default:
                return;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        switch (other.gameObject.name)
        {
            case "BoxVariant_Water":
                IsSwim = true;
                break;

            default:
                return;
        }
    }

    /// <summary>
    ///���_�E�̕����Z�b�g
    /// </summary>
    void LookAtSet()
    {
        //�J�������v���C���[�̓��̏�ɃZ�b�g
        Camera.main.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 0.8f, this.transform.position.z);
        //���_���Z�b�g
        Camera.main.transform.LookAt(Camera.main.transform.position + ViewVector);
        //�v���C���[�����_�����ɉ�]
        transform.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);
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

    //Implementation from Interface:IAttackableObject
    public void OnAttack(in GameObject attackerObject)
    {
        //�C���^�[�t�F�[�X�擾
        var attackableInterface = EXAttackableObject.GetInterface(attackerObject);
        //�U���͎擾
        int attackPower = attackableInterface.GetAttackPower();
        //HP�����炷
        HP -= attackPower;

        //�m�b�N�o�b�N��
        float knockBackPower = attackableInterface.GetKnockBack();
        //�m�b�N�o�b�N���󂯂�x�N�g��
        Vector3 knockBackVector = transform.position - attackerObject.transform.position;
        //�U���҂���^���΂̃x�N�g�������߂�
        knockBackVector.y = 0;
        knockBackVector.Normalize();
        knockBackVector *= knockBackPower;

        playerRigidbody.constraints = FloatingConstraints;
        //�����������iFloat�̌덷�΍�j
        transform.position = transform.position + new Vector3(0, 0.08f, 0);
        var velocity = playerRigidbody.velocity;
        velocity.y += KnockBackPowerY;
        playerRigidbody.velocity = velocity;

        KnockBackVector = knockBackVector;
    }

    //Implementation from Interface:IAttackableObject
    public int GetAttackPower()
    {
        return 1;
    }

    //Implementation from Interface:IAttackableObject
    public float GetKnockBack()
    {
        return 15f;
    }
}
