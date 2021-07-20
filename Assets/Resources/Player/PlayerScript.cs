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
    //通常時のコンストレイント（X・Y回転を固定、Y軸移動を固定）
    private const RigidbodyConstraints NormalConstraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
    //浮遊時のコンストレイント（X・Y回転を固定）
    private const RigidbodyConstraints FloatingConstraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

    //PlayerScriptのインスタンス
    public static PlayerScript Player { get; private set; }

    //HP
    public int HP { get; private set; }

    //接地判定
    private bool IsGround;
    //液体中判定
    private bool IsSwim;
    //視点ベクトル
    private Vector3 ViewVector;
    //ノックバックベクトル
    private Vector3 KnockBackVector;
    //PlayerのRigidbody
    private Rigidbody playerRigidbody;
    //攻撃可能フラグ
    private TimeSpanFlag isAttacking;
    //移動速度
    [SerializeField] private float speed;
    //ダッシュ速度
    [SerializeField] private float sprintSpeed;
    //マウス感度
    [SerializeField] private float mouseSensitivity;
    //ジャンプ力
    [SerializeField] private float jumpPower;
    //泳ぐ力
    [SerializeField] private float swimPower;
    //泳ぐ際の速度
    [SerializeField] private float swimSpeed;
    //水中での重力
    [SerializeField] private float swimGravity;
    //ノックバック力の減衰力
    [SerializeField] private float KnockBackAttenuation;
    //ノックバックの最小値
    [SerializeField] private float MinKnockBackPower;
    //ノックバック時の浮かす力
    [SerializeField] private float KnockBackPowerY;

    private void Awake()
    {
        //初期視点
        ViewVector = new Vector3(1.0f, 0.0f, 0.0f);
        //ノックバックベクトル初期化
        KnockBackVector = Vector3.zero;
        //最初は空中判定
        IsGround = false;
        //攻撃フラグ
        isAttacking = new TimeSpanFlag(350);
        //HP初期化
        HP = HPControler.MaxHP;
        //インスタンスをセット
        Player = this;
    }

    void Start()
    {
        //Rigidbody取得
        playerRigidbody = GetComponent<Rigidbody>() ?? throw new PlayerInitializeFailException(paramName: nameof(playerRigidbody), message: "playerRigidbody cannot be null");
    }

    private void FixedUpdate()
    {
        if (Mathf.Abs(Vector3.Distance(Vector3.zero, KnockBackVector)) >= MinKnockBackPower)
        {
            //ノックバック力の減衰
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

        //インベントリーが開いているときはカメラ動かさない
        if (!InventoryScript.Instance.IsActiveInventory)
        {
            //マウスベクトルの取得
            Vector2 mouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            mouse *= mouseSensitivity;

            //視点移動
            Vector3 nowVec = new Vector3(ViewVector.z, ViewVector.y, -ViewVector.x);
            ViewVector += nowVec.normalized * mouse.x;
            nowVec = Vector3.up;
            ViewVector += nowVec.normalized * mouse.y;
            ViewVector.Normalize();

            //攻撃判定
            if (!isAttacking && Input.GetMouseButton(0))
            {
                //攻撃判定開始
                isAttacking.Begin();

                //レイがあたったオブジェクト
                GameObject hitGameObject = RaycastManager.RaycastHitObject.collider?.gameObject;
                if (hitGameObject != null)
                {
                    //レイがあたったオブジェクトに攻撃判定
                    EXAttackableObject.AttackNotification(gameObject, hitGameObject);
                }
                else
                {
                    //空振り音を再生
                    SoundEmitter.FindClip("missing")?.Play();
                }
            }
        }


        //カメラ正面
        Vector3 camForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 1, 1)).normalized;
        //移動ベクトル
        Vector3 moveVelocity = (camForward * Input.GetAxis("Vertical")) + (Camera.main.transform.right * Input.GetAxis("Horizontal"));
        //ジャンプベクトル
        Vector3 jumpVelocity = Vector3.zero;

        //泳ぎ中なら
        if (IsSwim)
        {
            //縦移動を解禁
            playerRigidbody.constraints = FloatingConstraints;

            if (Input.GetKey(KeyCode.Space))
            {
                //ジャンプする場合重力の重力を加算しない
                jumpVelocity = Vector3.up * swimPower;
            }

            //水中での移動速度にする
            moveVelocity.Scale(new Vector3(swimSpeed, swimSpeed, swimSpeed));
            moveVelocity.y += swimGravity;
        }
        else
        {
            //スピード計算
            moveVelocity *= speed;
            //接地
            if (IsGround)
            {
                //Y軸移動は重力計算に任せる
                moveVelocity.y = 0;
                //スプリント
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    moveVelocity *= sprintSpeed;
                }
                //ジャンプ
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    //ジャンプする場合重力の重力を加算しない
                    jumpVelocity = Vector3.up * jumpPower;
                    //縦移動を解禁
                    playerRigidbody.constraints = FloatingConstraints;
                    //少し浮かす（Floatの誤差のせいで接地判定をぶれさせないように少しめり込ませているため）
                    transform.position = transform.position + new Vector3(0, 0.03f, 0);
                }
            }
            //浮遊中は物理演算の落下に任せる
            moveVelocity.y = playerRigidbody.velocity.y;
        }

        //移動加速度を設定
        playerRigidbody.velocity = moveVelocity + jumpVelocity + KnockBackVector;

        //カメラとプレイヤーの視点設定
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
                //アイテムの追加
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
    ///視点・体方向セット
    /// </summary>
    void LookAtSet()
    {
        //カメラをプレイヤーの頭の上にセット
        Camera.main.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 0.8f, this.transform.position.z);
        //視点をセット
        Camera.main.transform.LookAt(Camera.main.transform.position + ViewVector);
        //プレイヤーを視点方向に回転
        transform.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);
    }

    /// <summary>
    ///接地
    /// </summary>
    public void OnTheGround(float distance)
    {
        IsGround = true;
        //縦移動をなくす
        playerRigidbody.constraints = NormalConstraints;
        //地面の上へ押し戻し
        transform.position = transform.position + new Vector3(0, distance, 0);
    }

    /// <summary>
    ///浮遊
    /// </summary>
    public void Floating()
    {
        IsGround = false;
        //縦移動を行う
        playerRigidbody.constraints = FloatingConstraints;
    }

    //Implementation from Interface:IAttackableObject
    public void OnAttack(in GameObject attackerObject)
    {
        //インターフェース取得
        var attackableInterface = EXAttackableObject.GetInterface(attackerObject);
        //攻撃力取得
        int attackPower = attackableInterface.GetAttackPower();
        //HPを減らす
        HP -= attackPower;

        //ノックバック力
        float knockBackPower = attackableInterface.GetKnockBack();
        //ノックバックを受けるベクトル
        Vector3 knockBackVector = transform.position - attackerObject.transform.position;
        //攻撃者から真反対のベクトルを求める
        knockBackVector.y = 0;
        knockBackVector.Normalize();
        knockBackVector *= knockBackPower;

        playerRigidbody.constraints = FloatingConstraints;
        //少し浮かす（Floatの誤差対策）
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
