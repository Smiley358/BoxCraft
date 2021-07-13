using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 攻撃可能オブジェクト用インターフェース
/// </summary>
public interface IAttackableObject: IEventSystemHandler
{
    /// <summary>
    /// 攻撃されたときに呼ばれるイベントハンドラー
    /// </summary>
    /// <param name="attackerObject">攻撃者</param>
    public void OnAttack(in GameObject attackerObject);

    /// <summary>
    /// 攻撃力の取得
    /// </summary>
    /// <returns>攻撃力</returns>
    public int GetAttackPower();

    /// <summary>
    /// ノックバック力の取得
    /// </summary>
    /// <returns>ノックバック力</returns>
    public float GetKnockBack();
}

//簡単にイベントを呼ぶためのラッパークラス
public static class EXAttackableObject
{
    /// <summary>
    /// 攻撃通知を送る
    /// </summary>
    /// <param name="owner">攻撃オブジェクト</param>
    /// <param name="opponent">被撃オブジェクト</param>
    public static void AttackNotification(GameObject owner, GameObject opponent)
    {
        ExecuteEvents.Execute<IAttackableObject>(
            target: opponent,
            eventData: null,
            functor: (IAttackableObject opponent, BaseEventData eventData) =>
            {
                opponent.OnAttack(owner);
            });
    }

    /// <summary>
    /// IAttackableObjectの取得
    /// </summary>
    /// <param name="gameObject">取得対象のゲームオブジェクト</param>
    /// <returns>IAttackableObject</returns>
    public static IAttackableObject GetInterface(GameObject gameObject)
    {
        return gameObject?.GetComponent<IAttackableObject>();
    }
}