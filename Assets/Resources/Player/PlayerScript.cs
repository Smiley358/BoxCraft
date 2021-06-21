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
    //通常時のコンストレイント（X・Y回転を固定、Y軸移動を固定）
    private const RigidbodyConstraints NormalConstraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
    //浮遊時のコンストレイント（X・Y回転を固定）
    private const RigidbodyConstraints FloatingConstraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    //接地判定
    private bool IsGround;
    //視点ベクトル
    private Vector3 ViewVector;
    //PlayerのRigidbody
    private Rigidbody playerRigidbody;
    //移動速度
    [SerializeField] private float speed;
    //ダッシュ速度
    [SerializeField] private float sprintSpeed;
    //マウス感度
    [SerializeField] private float mouseSensitivity;
    //ジャンプ力
    [SerializeField] private float jumpPower;
    //プレイヤーの視点カメラ
    [SerializeField] private GameObject playerCamera;

    void Start()
    {
        //初期視点
        ViewVector = new Vector3(1.0f, 0.0f, 0.0f);
        IsGround = false;
        //Rigidbody取得
        playerRigidbody = GetComponent<Rigidbody>() ?? throw new PlayerInitializeFailException(paramName: nameof(playerRigidbody), message: "playerRigidbody cannot be null");
    }

    void Update()
    {
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
        }


        //カメラ正面
        Vector3 camForward = Vector3.Scale(playerCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
        //移動ベクトル
        Vector3 moveVelocity = (camForward * Input.GetAxis("Vertical")) + (playerCamera.transform.right * Input.GetAxis("Horizontal"));
        //ジャンプベクトル
        Vector3 jumpVelocity = Vector3.zero;

        //リジッドボディを取得、重力の設定
        playerRigidbody = GetComponent<Rigidbody>();
        //スピード計算
        moveVelocity *= speed;
        //Y軸移動は重力計算に任せる
        moveVelocity.y = 0;

        if (IsGround)
        {
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
                moveVelocity.y = jumpVelocity.y;
                //縦移動をなくす
                playerRigidbody.constraints = FloatingConstraints;
                //少し浮かす（Floatの誤差のせいで接地判定がぶれさせないバイアスがあるため）
                transform.position = transform.position + new Vector3(0, 0.03f, 0);
            }
        }
        else
        {
            //浮遊中は物理演算の落下に任せる
            moveVelocity.y = playerRigidbody.velocity.y;
        }

        //移動加速度を設定
        playerRigidbody.velocity = moveVelocity + jumpVelocity;

        //カメラとプレイヤーの視点設定
        LookAtSet();
    }

    private void OnTriggerEnter(Collider other)
    {
        //アイテムじゃないなら何もしない
        if (other.gameObject.name != "ItemContent") return;

        //アイテムの追加
        InventoryScript.Instance.TryAddItem(other.gameObject.transform.parent.GetComponent<ItemScript>());
    }

    /// <summary>
    ///視点・体方向セット
    /// </summary>
    void LookAtSet()
    {
        //カメラをプレイヤーの頭の上にセット
        playerCamera.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 1.0f, this.transform.position.z);
        //視点をセット
        playerCamera.transform.LookAt(playerCamera.transform.position + ViewVector);
        //プレイヤーを視点方向に回転
        transform.rotation = Quaternion.Euler(0, playerCamera.transform.rotation.eulerAngles.y, 0);
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
}
