using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastManager : MonoBehaviour
{
    //画面中央の座標
    private Vector2 displayCenter;
    //レイキャストした結果
    public static RaycastHit RaycastHitObject { get; private set; }

    void Start()
    {
        //画面中央の座標
        displayCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        RaycastHitObject = new RaycastHit();
    }

    void Update()
    {
        //レイを飛ばすベクトルのセット
        Ray ray = Camera.main.ScreenPointToRay(displayCenter);
        //レイが当たったオブジェクト
        RaycastHit raycastHit;
        //レイヤーマスク
        LayerMask layerMask = LayerMask.GetMask("Box")| LayerMask.GetMask("Enemy");


        //インベントリ展開中はレイキャストを行わない
        if (!InventoryScript.Instance.IsActiveInventory)
        {
            //レイを飛ばす
            Physics.Raycast(ray, out raycastHit, 10.0f, layerMask);
            //衝突情報の格納
            RaycastHitObject = raycastHit;
        }
    }
}
