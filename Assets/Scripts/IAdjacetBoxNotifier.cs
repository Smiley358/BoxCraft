using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

///// <summary>
///// �א�Box�ւ̃C�x���g���s�p�C���^�[�t�F�[�X
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
//    /// �L�������ꂽ�Ƃ��ɌĂ΂��
//    /// </summary>
//    /// <param name="direction">�ʒm���̕���</param>
//    public void OnEnableNotification(Direction direction);
//    /// <summary>
//    /// ���������ꂽ�Ƃ��ɌĂ΂��
//    /// </summary>
//    /// <param name="direction">�ʒm���̕���</param>
//    public void OnDisableNotification(Direction direction);
//    /// <summary>
//    /// Destroy���ꂽ�Ƃ��ɌĂ΂��
//    /// </summary>
//    /// <param name="direction">�ʒm���̕���</param>
//    public void OnDestroyNotification(Direction direction);
//}

////�ȒP�ɃC�x���g���ĂԂ��߂̃��b�p�[�N���X
//public static class EXAdjacetBoxNotifier
//{
//    /// <summary>
//    /// �ʒm�𑗂�
//    /// </summary>
//    /// <param name="direction">�ʒm���I�u�W�F�N�g��target���猩������</param>
//    /// <param name="target">�ʒm��I�u�W�F�N�g</param>
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
//    /// �ʒm�𑗂�
//    /// </summary>
//    /// <param name="direction">�ʒm���I�u�W�F�N�g��target���猩������</param>
//    /// <param name="target">�ʒm��I�u�W�F�N�g</param>
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
//    /// �ʒm�𑗂�
//    /// </summary>
//    /// <param name="direction">�ʒm���I�u�W�F�N�g��target���猩������</param>
//    /// <param name="target">�ʒm��I�u�W�F�N�g</param>
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