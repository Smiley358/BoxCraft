using UnityEngine;
using UnityEngine.EventSystems;

public class GroundCheckScript : MonoBehaviour
{
    //親オブジェクト
    private GameObject parent;
    //イベントの重複呼び出し防止用
    private bool isGround;
    private bool isPreviousGround;
    //
    private float RoundingRrror = 0.01f;

    void Start()
    {
        parent = transform.parent.gameObject;
        if (parent is null) Debug.Log("Nothing Parent : " + GetHashCode().ToString());
        isGround = false;
        isPreviousGround = false;
    }

    private void Update()
    {
        //レイを飛ばすベクトルのセット
        Ray ray = new Ray(parent.transform.position, Vector3.down);
        //レイが当たったオブジェクト
        RaycastHit raycastHit;
        //レイヤーマスク
        LayerMask layerMask = LayerMask.GetMask("Box");
        //衝突判定距離
        float groundDistance = parent.transform.position.y - transform.position.y;

        //レイを飛ばす
        if (Physics.Raycast(ray, out raycastHit, Mathf.Infinity, layerMask))
        {
            //地面に衝突
            if(raycastHit.distance < groundDistance)
            {
                //接地
                isGround = true;
            }
            else
            {
                //浮遊
                isGround = false;
            }
        }
        else
        {
            //奈落だったら地面判定にしておく
            isGround = true;
        }

        //前回の結果と異なっていたら
        if (isGround != isPreviousGround)
        {
            //結果の保存
            isPreviousGround = isGround;
            
            //接地していたら
            if (isGround)
            {
                //接地通知
                Debug.Log("OnTheGround");
                ExecuteEvents.Execute<IGroundCheck>(
                    target: parent,
                    eventData: null,
                    functor: (IGroundCheck groundCheck, BaseEventData eventData) =>
                    {
                        groundCheck.OnTheGround(groundDistance - raycastHit.distance - RoundingRrror);
                    });
            }
            else
            {
                //浮遊判定
                Debug.Log("Floating");
                ExecuteEvents.Execute<IGroundCheck>(
                    target: parent,
                    eventData: null,
                    functor: (IGroundCheck groundCheck, BaseEventData eventData) =>
                    {
                        groundCheck.Floating();
                    });
            }
        }
    }
}

/// <summary>
/// EventSystemを使用した接地判定イベントハンドラーインターフェース
/// IGroundCheckerを継承していれば
/// GroundCheckerクラスからのイベントを受け取れる
/// </summary>
public interface IGroundCheck : IEventSystemHandler
{
    void OnTheGround(float distance);
    void Floating();
}
