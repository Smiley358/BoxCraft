using System;
using System.Collections.Generic;
using UnityEngine;

public class HPControler : MonoBehaviour
{
    //�ő�HP
    public const int MaxHP = 100;
    //�n�[�g�̐�
    public const int HeartCount = 8;
    //�n�[�g��̏d��
    public const float HeartWeight = (float)MaxHP / HeartCount;
    //�n�[�g�Ԃ̌���
    public const float HPmargin = 4;

    //�n�[�g
    private HPScript[] HPs = new HPScript[HeartCount];
    //1�t���[���O��HP
    private int saveHP = MaxHP;

    void Start()
    {
        //HP�̐����E�z�u
        var basePosition = transform.position;
        for (int i = 0; i < HPs.Length; i++)
        {
            HPs[i] = HPScript.Create(i, basePosition);
            HPs[i].transform.SetParent(transform);
        }
    }

    void Update()
    {
        //HP���擾
        int? HP = PlayerScript.Player?.HP;
        //HP�����݂��āA�O���HP���ς���Ă���ꍇ
        if ((HP != null) && (saveHP != HP))
        {
            //�C���f�b�N�X�O�ɏo�Ȃ��悤��
            Func<int, int> Clamp = (value) =>
             {
                 //�͈͂̃N�����v�p
                 int max = HPs.Length - 1;
                 int min = 0;
                 return (value < min) ? min : (value > max) ? max : value;
             };

            //�O��̃n�[�g�̍X�V�C���f�b�N�X
            int iStart = Mathf.FloorToInt(saveHP / HeartWeight);
            iStart = Clamp(iStart);

            saveHP = (int)HP;

            //����̃n�[�g�̍X�V�C���f�b�N�X
            int iEnd = Mathf.FloorToInt(saveHP / HeartWeight);
            iEnd = Clamp(iEnd);

            //�O��ύX�����n�[�g���獡��ύX�̂������n�[�g�܂�
            int i = iStart;
            int move = Math.Sign(iEnd - iStart);
            do
            {
                //�n�[�g���X�V
                HPs[i].UpdateHeart();

                //iStart����iEnd�Ɍ������ăC���f�b�N�X�𓮂���
                i += move;
            }
            while (i != iEnd + move);
        }
    }
}
