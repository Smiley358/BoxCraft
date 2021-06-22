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
}