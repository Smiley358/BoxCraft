using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastManager : MonoBehaviour
{
    //��ʒ����̍��W
    private Vector2 displayCenter;
    //���C�L���X�g��������
    public static RaycastHit RaycastHitObject { get; private set; }

    void Start()
    {
        //��ʒ����̍��W
        displayCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        RaycastHitObject = new RaycastHit();
    }

    void Update()
    {
        //���C���΂��x�N�g���̃Z�b�g
        Ray ray = Camera.main.ScreenPointToRay(displayCenter);
        //���C�����������I�u�W�F�N�g
        RaycastHit raycastHit;
        //���C���[�}�X�N
        LayerMask layerMask = LayerMask.GetMask("Box")| LayerMask.GetMask("Enemy");


        //�C���x���g���W�J���̓��C�L���X�g���s��Ȃ�
        if (!InventoryScript.Instance.IsActiveInventory)
        {
            //���C���΂�
            Physics.Raycast(ray, out raycastHit, 10.0f, layerMask);
            //�Փˏ��̊i�[
            RaycastHitObject = raycastHit;
        }
    }
}
