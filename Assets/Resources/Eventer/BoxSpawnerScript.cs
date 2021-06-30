using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxSpawnerScript : MonoBehaviour
{
    static public BoxSpawnerScript ScriptInstance;


    //外から変更する用のBox
    public GameObject NextBox;
    //ボックスのprefab
    private GameObject box;
    // ボックスを設置する位置
    private Vector3 boxSpawnPos;
    //BOXを設置するためのデリゲート
    private Action<GameObject, Vector3, Quaternion> boxSpawnDelegate;

    //設置予測用BOX
    [SerializeField] private GameObject predictionBox;
    //設置予測用BOXの制御スクリプト
    [SerializeField] private PredictionBoxScript predictionBoxScript;


    private void Awake()
    {
        ScriptInstance = this;
    }

    void Start()
    {
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

        //レイキャスト情報を取得
        RaycastHit raycastHit = RaycastManager.RaycastHitObject;

        //インベントリを開いておらず、レイが何かに当たっている
        if (!(InventoryScript.Instance.IsActiveInventory) && (raycastHit.collider != null))
        {
            //ボックスでなければ何もしない
            if(raycastHit.collider.tag == "Box")
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
                ChunkScript.CreateBoxAndAutomBelongToChunk(box, position, rotation);
            }
            boxSpawnDelegate = null;
        };

        return true;
    }
}
