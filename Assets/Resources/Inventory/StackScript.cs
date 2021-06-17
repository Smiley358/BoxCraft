using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StackScript : MonoBehaviour
{
    //�A�C�e���̐�
    [NonSerialized]  public int ItemCount;
    //�\���e�L�X�gUI
    [SerializeField] private Text stackNumText;

    void Start()
    {
        ItemCount = 0;
        stackNumText.text = "";
    }

    void Update()
    {
        //���������Ă��Ȃ��Ƃ��͐������o���Ȃ�
        if (ItemCount > 0)
        {
            //�X�^�b�N�����X�V
            stackNumText.text = ItemCount.ToString();
        }
        else
        {
            stackNumText.text = "";
        }
    }
}
