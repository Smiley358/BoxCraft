using UnityEngine;
using UnityEngine.EventSystems;

public class GroundCheckScript : MonoBehaviour
{
    //�e�I�u�W�F�N�g
    private GameObject parent;
    //�C�x���g�̏d���Ăяo���h�~�p
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
        //���C���΂��x�N�g���̃Z�b�g
        Ray ray = new Ray(parent.transform.position, Vector3.down);
        //���C�����������I�u�W�F�N�g
        RaycastHit raycastHit;
        //���C���[�}�X�N
        LayerMask layerMask = LayerMask.GetMask("Box");
        //�Փ˔��苗��
        float groundDistance = parent.transform.position.y - transform.position.y;

        //���C���΂�
        if (Physics.Raycast(ray, out raycastHit, Mathf.Infinity, layerMask))
        {
            //�n�ʂɏՓ�
            if(raycastHit.distance < groundDistance)
            {
                //�ڒn
                isGround = true;
            }
            else
            {
                //���V
                isGround = false;
            }
        }
        else
        {
            //�ޗ���������n�ʔ���ɂ��Ă���
            isGround = true;
        }

        //�O��̌��ʂƈقȂ��Ă�����
        if (isGround != isPreviousGround)
        {
            //���ʂ̕ۑ�
            isPreviousGround = isGround;
            
            //�ڒn���Ă�����
            if (isGround)
            {
                //�ڒn�ʒm
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
                //���V����
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
/// EventSystem���g�p�����ڒn����C�x���g�n���h���[�C���^�[�t�F�[�X
/// IGroundChecker���p�����Ă����
/// GroundChecker�N���X����̃C�x���g���󂯎���
/// </summary>
public interface IGroundCheck : IEventSystemHandler
{
    void OnTheGround(float distance);
    void Floating();
}
