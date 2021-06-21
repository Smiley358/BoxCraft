using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxSpawnerScript : MonoBehaviour
{
    static public BoxSpawnerScript ScriptInstance;

    //ボックスのprefab
    private GameObject box;
    //外から変更する用のBox
    public GameObject NextBox;

    //画面中央の座標
    private Vector2 displayCenter;
    // ボックスを設置する位置
    private Vector3 boxSpawnPos;


    //設置予測用BOXのメッシュレンダラー
    private MeshRenderer predictionBoxMeshRenderer;
    //設置予測用BOX
    [SerializeField] private GameObject predictionBox;
    //設置予測用BOXの制御スクリプト
    [SerializeField] private PredictionBoxScript predictionBoxScript;

    //BOXを設置するためのデリゲート
    private Action<GameObject, Vector3, Quaternion> boxSpawnDelegate;

    private void Awake()
    {
        ScriptInstance = this;
    }

    void Start()
    {
        //画面中央の座標
        displayCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        //設置予測BOXのレンダー取得
        predictionBoxMeshRenderer = predictionBox.GetComponent<MeshRenderer>();
        //設置予測BOXの非活性化
        predictionBox.SetActive(false);

        //接地予測BOXを更新
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

        //レイを飛ばすベクトルのセット
        Ray ray = Camera.main.ScreenPointToRay(displayCenter);
        //レイが当たったオブジェクト
        RaycastHit raycastHit;
        //レイヤーマスク
        LayerMask layerMask = LayerMask.GetMask("Box");

        //レイを飛ばす
        if (!(InventoryScript.Instance.IsActiveInventory) && Physics.Raycast(ray, out raycastHit, 10.0f, layerMask))
        {

            //BOXの設置予測活性化
            predictionBox.SetActive(true);

            //BOXの回転
            if (Input.GetKeyDown(KeyCode.R))
            {
                //回転通知
                predictionBoxScript.Rotate();
            }

            //BOXの生成・削除処理
            if (isCreatable)
            {
                //レイが当たった面の法線方向にボックスを生成するため計算
                boxSpawnPos = raycastHit.normal + raycastHit.collider.transform.position;
                //予測BOXの座標を更新
                predictionBox.transform.position = boxSpawnPos;
                //Boxの生成が予約されている場合にBoxを生成
                boxSpawnDelegate?.Invoke(box, boxSpawnPos, predictionBoxScript.GetRotate());
            }

            if (Input.GetMouseButtonDown(0))
            {
                //レイがあたったオブジェクトを削除
                bool isCreate = ItemScript.Create(raycastHit.collider.gameObject);
                if (isCreate)
                {
                    Destroy(raycastHit.collider.gameObject);
                }
            }
        }
        else
        {
            //BOXの設置予測非活性化
            predictionBox.SetActive(false);
        }

        //Boxの更新
        if(NextBox != box)
        {
            box = NextBox;
        }
        //設置予測BOXの更新
        predictionBoxScript.AttachPrefab(box);
    }


    /// <summary>
    /// Boxの生成を予約する
    /// </summary>
    /// <param name="box">生成するBoxのprefab</param>
    /// <returns>生成予約出来たらTrue</returns>
    public bool ReservationSpawnBox(GameObject box)
    {
        //設置可能か調べる
        bool isCreatable = false;
        if (predictionBoxScript.IsValid())
        {
            isCreatable = predictionBoxScript.IsCreatable;
        }

        //既に予約済みならFalseを返す
        if (boxSpawnDelegate != null) return false;
        //現在生成不可状態ならFalseを返す
        if (!isCreatable) return false;

        //Boxの生成デリゲート作成
        boxSpawnDelegate = (GameObject box, Vector3 position, Quaternion rotation) =>
        {
            if (this.box != null)
            {
                // 生成位置の変数の座標にブロックを生成
                GameObject madeBox = Instantiate(box, position, rotation);
                madeBox.name = this.box.name;
            }
            boxSpawnDelegate = null;
        };

        return true;
    }
}
