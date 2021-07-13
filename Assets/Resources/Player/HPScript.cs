using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPScript : MonoBehaviour
{
    //HPのprefab
    private static GameObject prefab;

    /// <summary>
    /// HPのハートを一個作る
    /// </summary>
    /// <param name="index">何番目のハートか</param>
    /// <param name="basePosition">基準点</param>
    /// <returns>HPの制御スクリプト</returns>
    public static HPScript Create(int index, Vector3 basePosition)
    {
        //prefabの取得
        prefab ??= PrefabManager.Instance.GetPrefab("HP");
        if(prefab == null)
        {
            Debug.Log("HP prefab is Not Exist");
        }

        //ハートのトランスフォーム
        var rectTransform = prefab.GetComponent<RectTransform>();
        //ハートの幅
        float heartWidth = rectTransform.rect.width;
        //ハートの位置
        Vector3 position = new Vector3(
            basePosition.x + (heartWidth + HPControler.HPmargin) * index,
            basePosition.y,
            basePosition.z);

        var hp = Instantiate(prefab, position, Quaternion.identity);
        if (hp != null)
        {
            var script = hp.GetComponent<HPScript>();
            script.index = index;
            return script;
        }
        return null;
    }


    //何番目のハートか
    public int index { get; private set; }

    //ハートのスプライトを表示するコンポーネント
   [SerializeField] private Image HPImage;

    void Start()
    {
        //インスペクターからセットできていなかった場合にセットする
        HPImage ??= gameObject.transform.Find("Heart")?.GetComponent<Image>();
    }

    /// <summary>
    /// ハートを更新する
    /// </summary>
    public void UpdateHeart()
    {
        //端数を計算
        float edge = (PlayerScript.Player.HP - HPControler.HeartWeight * index);
        //端数がHPControler.HeartWeight未満の場合、ハートが削れているのは自分
        if (edge <= HPControler.HeartWeight)
        {
            //0未満なら0代入
            edge = edge < 0 ? 0 : edge;
            //ハートの塗りつぶし範囲の設定
            HPImage.fillAmount = edge / HPControler.HeartWeight;
        }
        else
        {
            //削られてるのは自分じゃないのでMax設定
            HPImage.fillAmount = 1f;
        }
    }
}
