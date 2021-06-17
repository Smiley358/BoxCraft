using UnityEngine;

public class ItemContentScript : MonoBehaviour
{
    //浮遊移動のストローク
    private static float FloatStroke = 0.1f;
    //生成された時間
    private float startTime;
    //結合処理状況
    private bool OnJoin;

    void Start()
    {
        //生成された時間
        startTime = Time.time;
    }
 
    void Update()
    {
        //回転
        Quaternion rotation = Quaternion.Euler(new Vector3(0, ((Time.time + startTime) % 30) / 30.0f * 360, 0));
        transform.localRotation = rotation;
        transform.localPosition = new Vector3(0, 0.5f, 0);

        //浮遊アニメーション
        Vector3 offset = new Vector3(0, Mathf.Sin(Time.time + startTime) * FloatStroke, 0);
        transform.localPosition = offset;
    }

    private void OnTriggerEnter(Collider other)
    {
        //アイテムじゃなければ何もしない
        if (other.gameObject.tag != "Item") return;
        //同じアイテムじゃなければ何もしない
        if (other.name != name) return;
        //相手が結合判定の場合何もしない
        if (other.gameObject.GetComponent<ItemContentScript>().OnJoin == true) return;

        //親に結合通知
        other.transform.parent.gameObject.SendMessage("JoinItem", transform.parent.gameObject.GetComponent<ItemScript>().contentCount);
        OnJoin = true;
        Destroy(transform.parent.gameObject);
    }
}
