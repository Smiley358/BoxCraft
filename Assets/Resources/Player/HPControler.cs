using System;
using System.Collections.Generic;
using UnityEngine;

public class HPControler : MonoBehaviour
{
    //最大HP
    public const int MaxHP = 100;
    //ハートの数
    public const int HeartCount = 8;
    //ハート一個の重み
    public const float HeartWeight = (float)MaxHP / HeartCount;
    //ハート間の隙間
    public const float HPmargin = 4;

    //ハート
    private HPScript[] HPs = new HPScript[HeartCount];
    //1フレーム前のHP
    private int saveHP = MaxHP;

    void Start()
    {
        //HPの生成・配置
        var basePosition = transform.position;
        for (int i = 0; i < HPs.Length; i++)
        {
            HPs[i] = HPScript.Create(i, basePosition);
            HPs[i].transform.SetParent(transform);
        }
    }

    void Update()
    {
        //HPを取得
        int? HP = PlayerScript.Player?.HP;
        //HPが存在して、前回とHPが変わっている場合
        if ((HP != null) && (saveHP != HP))
        {
            //インデックス外に出ないように
            Func<int, int> Clamp = (value) =>
             {
                 //範囲のクランプ用
                 int max = HPs.Length - 1;
                 int min = 0;
                 return (value < min) ? min : (value > max) ? max : value;
             };

            //前回のハートの更新インデックス
            int iStart = Mathf.FloorToInt(saveHP / HeartWeight);
            iStart = Clamp(iStart);

            saveHP = (int)HP;

            //今回のハートの更新インデックス
            int iEnd = Mathf.FloorToInt(saveHP / HeartWeight);
            iEnd = Clamp(iEnd);

            //前回変更したハートから今回変更のあったハートまで
            int i = iStart;
            int move = Math.Sign(iEnd - iStart);
            do
            {
                //ハートを更新
                HPs[i].UpdateHeart();

                //iStartからiEndに向かってインデックスを動かす
                i += move;
            }
            while (i != iEnd + move);
        }
    }
}
