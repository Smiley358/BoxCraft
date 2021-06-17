using UnityEngine;
using UnityEngine.EventSystems;

public class GroundCheckScript : MonoBehaviour
{
    //�e�I�u�W�F�N�g
    private GameObject parent;
    //�d���h�~�p
    private bool isGround;

    void Start()
    {
        parent = transform.parent.gameObject;
        if (parent is null) Debug.Log("Nothing Parent : " + GetHashCode().ToString());
        isGround = false;
    }

    public void OnTriggerEnter(UnityEngine.Collider other)
    {
        //���ɐڒn���Ă���ꍇ�Ă΂Ȃ�
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
        //���ɕ����Ă�ꍇ�Ă΂Ȃ�
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
        //���������Ȃ̂Ŏg���܂킷
        OnTriggerEnter(other);
    }
}

/// <summary>
/// EventSystem���g�p�����ڒn����C�x���g�n���h���[�C���^�[�t�F�[�X
/// IGroundChecker���p�����Ă����
/// GroundChecker�N���X����̃C�x���g���󂯎���
/// </summary>
public interface IGroundCheck : IEventSystemHandler
{
    void OnTheGround();
    void Floating();
}
