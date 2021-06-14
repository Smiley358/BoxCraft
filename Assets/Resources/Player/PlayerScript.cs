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
    //移動速度
    public float Speed;
    //マウス感度
    public float MouseSensitivity;
    //ジャンプ力
    public float JumpPower;

    //プレイヤーの視点カメラ
    public GameObject PlayerCamera;

    //視点ベクトル
    private Vector3 ViewVector;
    //接地判定
    private bool IsGround;
    //PlayerのRigidbody
    private Rigidbody playerRigidbody;

    //インベントリー管理クラス
    private InventoryScript inventoryScript;

    void Start()
    {
        //初期視点
        ViewVector = new Vector3(1.0f, 0.0f, 0.0f);
        IsGround = false;
        //インベントリーの管理クラスを取得
        inventoryScript = GameObject.Find("Inventory")?.GetComponent<InventoryScript>();
        //Rigidbody取得
        playerRigidbody = GetComponent<Rigidbody>() ?? throw new PlayerInitializeFailException(paramName: nameof(playerRigidbody), message: "playerRigidbody cannot be null");
    }

    void Update()
    {
        //インベントリーが開いているときはカメラ動かさない
        if (!inventoryScript.IsActiveInventory)
        {
            //マウスベクトルの取得
            Vector2 mouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            mouse *= MouseSensitivity;

            //視点移動
            Vector3 nowVec = new Vector3(ViewVector.z, ViewVector.y, -ViewVector.x);
            ViewVector += nowVec.normalized * mouse.x;
            nowVec = Vector3.up;
            ViewVector += nowVec.normalized * mouse.y;
            ViewVector.Normalize();
        }


        //カメラ正面
        Vector3 camForward = Vector3.Scale(PlayerCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
        //移動ベクトル
        Vector3 moveVelocity = (camForward * Input.GetAxis("Vertical")) + (PlayerCamera.transform.right * Input.GetAxis("Horizontal"));
        //ジャンプベクトル
        Vector3 jumpVelocity = Vector3.zero;

        //リジッドボディを取得、重力の設定
        playerRigidbody = GetComponent<Rigidbody>();
        moveVelocity *= Speed;
        moveVelocity.y = playerRigidbody.velocity.y;

        //ジャンプ
        if (IsGround&&Input.GetKey(KeyCode.Space))
        {
            //ジャンプする場合重力の重力を加算しない
            jumpVelocity = Vector3.up * JumpPower;
            moveVelocity.y = jumpVelocity.y;
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
        inventoryScript.TryAddItem(other.gameObject.transform.parent.GetComponent<ItemScript>());
    }

    //視点・体方向セット
    void LookAtSet()
    {
        //カメラをプレイヤーの頭の上にセット
        PlayerCamera.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 1.0f, this.transform.position.z);
        //視点をセット
        PlayerCamera.transform.LookAt(PlayerCamera.transform.position + ViewVector);
        //プレイヤーを視点方向に回転
        transform.rotation = Quaternion.Euler(0, PlayerCamera.transform.rotation.eulerAngles.y, 0);
    }

    //接地
    public void OnTheGround()
    {
        IsGround = true;
    }

    //浮遊
    public void Floating()
    {
        IsGround = false;
    }
}
