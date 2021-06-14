using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript : MonoBehaviour
{
    //アイテムのプレハブ
    static public GameObject ItemPrefab;
    static public GameObject ItemContentPrefab;
    public enum ChildIndex
    {
        Physics,
        Content
    }

    public static bool Create(GameObject itemObject)
    {
        if (ItemPrefab == null)
        {
            ItemPrefab = Resources.Load("Item/Item") as GameObject;
        }
        if (ItemContentPrefab == null)
        {
            ItemContentPrefab = Resources.Load("Item/ItemContent") as GameObject;
        }
        //アイテム化するBOXの管理クラスを取得
        BoxBase box = itemObject.GetComponent<BoxBase>();
        //Box管理クラスを持っていなければアイテムデータもないので生成しない
        if (box == null)
        {
            return false;
        }
        //アイテムデータを生成してセット
        InventoryItem itemData = new InventoryItem(box.ItemData);
        //アイテムデータがなかったらアイテム生成を行わない
        if (itemData== null)
        {
            return false;
        }

        //アイテムのインスタンス化
        GameObject madeObject = Instantiate(ItemPrefab);
        //madeObject.transform.SetPositionAndRotation(itemObject.transform.position, Quaternion.Euler(Vector3.zero));
        madeObject.transform.position = itemObject.transform.position;
        //madeObject.transform.localPosition = Vector3.zero;
        madeObject.name = "Item_" + itemData.ItemName;

        //アイテム管理クラスの取得
        ItemScript itemScript = madeObject.GetComponent<ItemScript>();
        //アイテムデータの格納
        itemScript.ItemData = itemData;

        //アイテムの中身をインスタンス化
        GameObject madeContent = Instantiate(ItemContentPrefab, madeObject.transform);
        //本来の名前に変更（"(Clone)"とゆう接尾辞を消す）
        madeContent.name = ItemContentPrefab.name;
        //メッシュの設定
        madeContent.GetComponent<MeshFilter>().mesh = itemObject.GetComponent<MeshFilter>().mesh;
        //マテリアルの設定
        madeContent.GetComponent<MeshRenderer>().material = itemObject.GetComponent<MeshRenderer>().material;
        return true;
    }



    public GameObject content { get; private set; }
    private float Speed = 2.5f;
    private Rigidbody ownRigidbody;
    public int contentCount { get; private set; }
    private Vector3 moveVelocity;

    //アイテムデータ
    public InventoryItem ItemData { get; private set; }

    private void Start()
    {
        //子オブジェクトを保持しておく
        if (content == null)
        {
            content = transform.GetChild((int)ChildIndex.Content)?.gameObject;
            //格納されていたら
            if (content != null)
            {
                //コンテンツ数を増やす
                contentCount = 1;
            }
            //なぜか空アイテムなので自滅
            else
            {
                Destroy(gameObject);
            }
        }

        //リジッドボディを保持
        ownRigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        //本体を回転させない
        transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    private void FixedUpdate()
    {
        //近くのアイテムへ近づく
        //y軸移動はRigidBodyの重力を使用
        moveVelocity.Scale(new Vector3(1, 0, 1));
        moveVelocity.y = ownRigidbody.velocity.y;
        ownRigidbody.velocity = moveVelocity;
        moveVelocity = Vector3.zero;
    }

    private void OnTriggerStay(Collider other)
    {
        //アイテムじゃなければ何もしない
        if (other.gameObject.tag != "Item") return;
        //同じアイテムじゃなければ何もしない
        if (other.name != name) return;

        //別のアイテムへの方向
        Vector3 targetVector = (other.transform.position - transform.position).normalized;
        //Y軸は何もしない
        //targetVector.Scale(new Vector3(1, 0, 1));
        //targetVector.y = ownRigidbody.velocity.y;

        //ターゲットが現在の進行方向と大きく外れる場合処理しない
        if (Vector3.Angle(moveVelocity, targetVector) >= 90.0f) return;
        //移動ベクトルの算出
        moveVelocity = (moveVelocity + (targetVector * Speed)).normalized * Speed;
    }

    /// <summary>
    /// アイテムを結合（カウントを増やす）
    /// </summary>
    /// <param name="count">結合数</param>
    public void JoinItem(int count)
    {
        contentCount += count;
    }

    /// <summary>
    /// アイテムをインベントリーにしまう（カウントを減らす）
    /// </summary>
    /// <param name="count">しまう数</param>
    /// <returns>しまえた数</returns>
    public int StorageItem(int count)
    {
        if (count > contentCount)
        {
            count = contentCount;
        }
        contentCount -= count;

        //空になったら破棄
        if (contentCount <= 0)
        {
            Destroy(gameObject);
        }

        return count;
    }
}
