using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

interface IItemJoiner : IEventSystemHandler
{
    void JoinItem(int count);
}

public class ItemScript : MonoBehaviour,IItemJoiner
{
    //アイテムのprefab
    public static GameObject ItemPrefab;
    //アイテムの中身のprefab
    public static GameObject ItemContentPrefab;
    //
    public const string ItemPrefix = "Item_";

    private static Action InitializeOnceDelegate = () =>
    {
        ItemPrefab = Resources.Load("Item/Item") as GameObject;
        ItemContentPrefab = Resources.Load("Item/ItemContent") as GameObject;
        InitializeOnceDelegate = null;
    };

    void Awake()
    {
        //一応初期化しておく
        InitializeOnceDelegate?.Invoke();
    }

    /// <summary>
    /// アイテムを生成する
    /// </summary>
    /// <param name="itemizeObject">生成するアイテムオブジェクト</param>
    /// <returns>生成できたかどうか</returns>
    public static bool Create(GameObject itemizeObject)
    {
        //タイミング的に呼ばれていなかった時用
        InitializeOnceDelegate?.Invoke();

        //アイテム化インターフェースを取得
        IItemizeObject itemize = itemizeObject.GetComponent<IItemizeObject>();
        //IItemizeObjectにキャストできなければアイテム化不可能オブジェクト
        if (itemize == null)
        {
            return false;
        }
        //アイテムデータを生成してセット
        InventoryItem itemData = new InventoryItem(itemize.GetItemData());
        //アイテムデータがなかったらアイテム生成を行わない
        if (itemData == null)
        {
            return false;
        }

        //アイテムのインスタンス化
        GameObject item = Instantiate(ItemPrefab);
        //座標を設定
        item.transform.position = itemizeObject.transform.position;
        //名前はItemPrefix+アイテム名
        item.name = ItemPrefix + itemData.ItemName;

        //アイテム管理クラスの取得
        ItemScript itemScript = item.GetComponent<ItemScript>();
        //アイテムデータの格納
        itemScript.ItemData = itemData;

        //アイテムの中身をインスタンス化
        GameObject itemContent = Instantiate(ItemContentPrefab, item.transform);
        //アイテム本体の座標にセット
        itemContent.transform.position = item.transform.position;
        //本来の名前に変更（"(Clone)"とゆう接尾辞を消す）
        itemContent.name = ItemContentPrefab.name;
        //メッシュの設定
        itemContent.GetComponent<MeshFilter>().mesh = itemize.GetMeshFilter()?.mesh;
        //マテリアルの設定
        itemContent.GetComponent<MeshRenderer>().material = itemize.GetMeshRenderer()?.material;
        //親オブジェクトのスクリプトを設定
        itemContent.GetComponent<ItemContentScript>().ParentScript = itemScript;
        return true;
    }



    //アイテムデータ
    public InventoryItem ItemData { get; private set; }
    //中身
    public GameObject Content { get; private set; }
    //中身の数
    public int contentCount { get; private set; }
    //移動速度
    private float Speed = 2.5f;
    //リジッドボディ
    private Rigidbody ownRigidbody;
    //移動べクトル
    private Vector3 moveVelocity;


    void Start()
    {
        //子オブジェクトを保持しておく
        if (Content == null)
        {
            Content = transform.Find("ItemContent")?.gameObject;
            //格納されていたら
            if (Content != null)
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

    void FixedUpdate()
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
