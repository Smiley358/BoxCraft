using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// �U���\�I�u�W�F�N�g�p�C���^�[�t�F�[�X
/// </summary>
public interface IAttackableObject: IEventSystemHandler
{
    /// <summary>
    /// �U�����ꂽ�Ƃ��ɌĂ΂��C�x���g�n���h���[
    /// </summary>
    /// <param name="attackerObject">�U����</param>
    public void OnAttack(in GameObject attackerObject);
}

//�ȒP�ɃC�x���g���ĂԂ��߂̃��b�p�[�N���X
public static class EXAttackableObject
{
    /// <summary>
    /// �U���ʒm�𑗂�
    /// </summary>
    /// <param name="owner">�U���I�u�W�F�N�g</param>
    /// <param name="opponent">�팂�I�u�W�F�N�g</param>
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