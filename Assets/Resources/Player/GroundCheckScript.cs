using UnityEngine;
using UnityEngine.EventSystems;

public class GroundCheckScript : MonoBehaviour
{
    //親オブジェクト
    private GameObject parent;
    //重複防止用
    private bool isGround;

    void Start()
    {
        parent = transform.parent.gameObject;
        if (parent is null) Debug.Log("Nothing Parent : " + GetHashCode().ToString());
        isGround = false;
    }

    public void OnTriggerEnter(UnityEngine.Collider other)
    {
        //既に接地している場合呼ばない
        if (isGround) return;
        ExecuteEvents.Execute<IGroundCheck>(
            target: parent,
            eventData: null,
            functor: (IGroundCheck groundCheck, BaseEventData eventData) =>
            {
                groundCheck.OnTheGround();
            });
        isGround = true;
    }

    public void OnTriggerExit(UnityEngine.Collider other)
    {
        //既に浮いてる場合呼ばない
        if (!isGround) return;
        ExecuteEvents.Execute<IGroundCheck>(
            target: parent,
            eventData: null,
            functor: (IGroundCheck groundCheck, BaseEventData eventData) =>
            {
                groundCheck.Floating();
            });
        isGround = false;
    }

    public void OnTriggerStay(UnityEngine.Collider other)
    {
        //同じ処理なので使いまわす
        OnTriggerEnter(other);
    }
}

/// <summary>
/// EventSystemを使用した接地判定イベントハンドラーインターフェース
/// IGroundCheckerを継承していれば
/// GroundCheckerクラスからのイベントを受け取れる
/// </summary>
public interface IGroundCheck : IEventSystemHandler
{
    void OnTheGround();
    void Floating();
}
