using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

///// <summary>
///// 隣接Boxへのイベント発行用インターフェース
///// </summary>
//public interface IAdjacetBoxNotifier : IEventSystemHandler
//{
//    enum Direction
//    {
//        Top,
//        Left,
//        Forward,
//        Behind,
//        Right,
//        Bottom
//    }

//    /// <summary>
//    /// 有効化されたときに呼ばれる
//    /// </summary>
//    /// <param name="direction">通知元の方向</param>
//    public void OnEnableNotification(Direction direction);
//    /// <summary>
//    /// 無効化されたときに呼ばれる
//    /// </summary>
//    /// <param name="direction">通知元の方向</param>
//    public void OnDisableNotification(Direction direction);
//    /// <summary>
//    /// Destroyされたときに呼ばれる
//    /// </summary>
//    /// <param name="direction">通知元の方向</param>
//    public void OnDestroyNotification(Direction direction);
//}

////簡単にイベントを呼ぶためのラッパークラス
//public static class EXAdjacetBoxNotifier
//{
//    /// <summary>
//    /// 通知を送る
//    /// </summary>
//    /// <param name="direction">通知元オブジェクトのtargetから見た方向</param>
//    /// <param name="target">通知先オブジェクト</param>
//    public static void OnEnableNotification(IAdjacetBoxNotifier.Direction direction, GameObject target)
//    {
//        if (target is null) return;
//        ExecuteEvents.Execute<IAdjacetBoxNotifier>(
//            target: target,
//            eventData: null,
//            functor: (IAdjacetBoxNotifier opponent, BaseEventData eventData) =>
//            {
//                opponent.OnEnableNotification(direction);
//            });
//    }

//    /// <summary>
//    /// 通知を送る
//    /// </summary>
//    /// <param name="direction">通知元オブジェクトのtargetから見た方向</param>
//    /// <param name="target">通知先オブジェクト</param>
//    public static void OnDisableNotification(IAdjacetBoxNotifier.Direction direction, GameObject target)
//    {
//        if (target is null) return;
//        ExecuteEvents.Execute<IAdjacetBoxNotifier>(
//            target: target,
//            eventData: null,
//            functor: (IAdjacetBoxNotifier opponent, BaseEventData eventData) =>
//            {
//                opponent.OnDisableNotification(direction);
//            });
//    }
//    /// <summary>
//    /// 通知を送る
//    /// </summary>
//    /// <param name="direction">通知元オブジェクトのtargetから見た方向</param>
//    /// <param name="target">通知先オブジェクト</param>
//    public static void OnDestroyNotification(IAdjacetBoxNotifier.Direction direction, GameObject target)
//    {
//        if (target is null) return;
//        ExecuteEvents.Execute<IAdjacetBoxNotifier>(
//            target: target,
//            eventData: null,
//            functor: (IAdjacetBoxNotifier opponent, BaseEventData eventData) =>
//            {
//                opponent.OnDestroyNotification(direction);
//            });
//    }

//}