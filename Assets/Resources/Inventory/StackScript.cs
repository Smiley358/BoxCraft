using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StackScript : MonoBehaviour
{
    //�A�C�e���̐�
    public int itemCount { get; set; }
    //�\���e�L�X�gUI
    public Text stackNumText;

    void Start()
    {
        itemCount = 0;
        stackNumText.text = "";
    }

    void Update()
    {
        //���������Ă��Ȃ��Ƃ��͐������o���Ȃ�
        if (itemCount > 0)
        {
            //�X�^�b�N�����X�V
            stackNumText.text = itemCount.ToString();
        }
        else
        {
            stackNumText.text = "";
        }
    }
}
