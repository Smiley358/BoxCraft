using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxSpawnerScript : MonoBehaviour
{
    //アイテムのレイヤー
    private const int ItemLayer = 6;

    //画面中央の座標
    private Vector2 displayCenter;

    // ボックスを設置する位置
    private Vector3 boxSpawnPos;

    //ボックスのprefab
    public GameObject Box;

    //設置予測用BOX
    public GameObject PredictionBox;
    //設置予測用BOXのメッシュレンダラー
    private MeshRenderer predictionBoxMeshRenderer;
    //設置予測用BOXの制御スクリプト
    private PredictionBoxScript predictionBoxScript;

    //インベントリ
    public GameObject Inventory;
    //インベントリ管理クラス
    public InventoryScript inventoryScript;
    //選択中のスロット
    public SlotScript selectSlot;


    void Start()
    {
        //画面中央の座標
        displayCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        //設置予測BOXの非活性化
        PredictionBox.SetActive(false);
        //設置予測BOXのレンダー取得
        predictionBoxMeshRenderer = PredictionBox.GetComponent<MeshRenderer>();
        //設置予測BOXの制御スクリプト取得
        predictionBoxScript = PredictionBox.GetComponent<PredictionBoxScript>();

        //インベントリセット
        inventoryScript = GameObject.Find("Inventory")?.GetComponent<InventoryScript>();

        //接地予測BOXを更新
        PredictionBoxUpdate();
    }

    void Update()
    {
        bool isCreatable = false;
        if (predictionBoxScript.IsValid())
        {
            isCreatable = predictionBoxScript.IsCreatable;
        }
        else
        {
            PredictionBoxUpdate();
        }

        //レイを飛ばすベクトルのセット
        Ray ray = Camera.main.ScreenPointToRay(displayCenter);
        //レイが当たったオブジェクト
        RaycastHit raycastHit;
        //レイヤーマスク
        LayerMask layerMask = LayerMask.GetMask("Box");

        //レイを飛ばす
        if (!(inventoryScript.IsActiveInventory) && Physics.Raycast(ray, out raycastHit, 10.0f, layerMask))
        {
            //レイが当たった面の法線方向にボックスを生成する
            boxSpawnPos = raycastHit.normal + raycastHit.collider.transform.position;

            //BOXの設置予測活性化
            PredictionBox.SetActive(true);
            PredictionBox.transform.position = boxSpawnPos;

            //BOXの回転
            if (Input.GetKeyDown(KeyCode.R))
            {
                //回転通知
                predictionBoxScript.Rotate();
            }

            //BOXの生成・削除処理
            if (Input.GetMouseButtonDown(1) && isCreatable && (Box != null))
            {
                if (Box != null)
                {
                    // 生成位置の変数の座標にブロックを生成
                    GameObject madeBox = Instantiate(Box, boxSpawnPos, predictionBoxScript.GetRotate());
                    madeBox.name = Box.name;
                    //設置した
                    selectSlot?.UseItem();
                }
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
            PredictionBox.SetActive(false);
        }
    }

    public void PredictionBoxUpdate()
    {
        predictionBoxScript.AttachPrefab(Box);
    }
}
