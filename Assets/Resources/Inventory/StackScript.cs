using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StackScript : MonoBehaviour
{
    //アイテムの数
    public int itemCount { get; set; }
    //表示テキストUI
    public Text stackNumText;

    void Start()
    {
        itemCount = 0;
        stackNumText.text = "";
    }

    void Update()
    {
        //何も入っていないときは数字を出さない
        if (itemCount > 0)
        {
            //スタック数を更新
            stackNumText.text = itemCount.ToString();
        }
        else
        {
            stackNumText.text = "";
        }
    }
}
