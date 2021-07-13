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

    /// <summary>
    /// �U���͂̎擾
    /// </summary>
    /// <returns>�U����</returns>
    public int GetAttackPower();

    /// <summary>
    /// �m�b�N�o�b�N�͂̎擾
    /// </summary>
    /// <returns>�m�b�N�o�b�N��</returns>
    public float GetKnockBack();
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

    /// <summary>
    /// IAttackableObject�̎擾
    /// </summary>
    /// <param name="gameObject">�擾�Ώۂ̃Q�[���I�u�W�F�N�g</param>
    /// <returns>IAttackableObject</returns>
    public static IAttackableObject GetInterface(GameObject gameObject)
    {
        return gameObject?.GetComponent<IAttackableObject>();
    }
}