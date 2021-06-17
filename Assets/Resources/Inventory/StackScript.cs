using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StackScript : MonoBehaviour
{
    //アイテムの数
    [NonSerialized]  public int ItemCount;
    //表示テキストUI
    [SerializeField] private Text stackNumText;

    void Start()
    {
        ItemCount = 0;
        stackNumText.text = "";
    }

    void Update()
    {
        //何も入っていないときは数字を出さない
        if (ItemCount > 0)
        {
            //スタック数を更新
            stackNumText.text = ItemCount.ToString();
        }
        else
        {
            stackNumText.text = "";
        }
    }
}
