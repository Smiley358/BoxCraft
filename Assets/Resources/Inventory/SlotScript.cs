using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotScript : MonoBehaviour
{
    [SerializeField]
    //スタック表示/管理スクリプト
    private StackScript stackScript;
    //アイテムデータ
    public InventoryItem Item { get; private set; } = null;
    //アイコン表示コンポーネント
    public Image ItemIcon { get; private set; }
    //インベントリー管理クラス
    public InventoryScript Inventory;

    private void Start()
    {
        //スタック管理クラスの取得
        stackScript = transform.Find("Stacks")?.GetComponent<StackScript>();
        if (stackScript == null)
        {
            Debug.Log("スロットの初期化に失敗");
        }
        //アイコン表示コンポーネントを取得
        ItemIcon = transform.Find("ItemIcon")?.GetComponent<Image>();
        if (ItemIcon == null)
        {
            Debug.Log("スロットの初期化に失敗");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void PrepareEventTrigger()
    {
        //イベントトリガー
        EventTrigger trigger = GetComponent<EventTrigger>();

        //ドラッグ中イベント
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Drag;
        entry.callback.AddListener((data) => { Inventory.OnDragDelegate((PointerEventData)data, (new InventoryScript.SlotData(gameObject, this))); });
        trigger.triggers.Add(entry);

        //ドロップイベント
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Drop;
        entry.callback.AddListener((data) => { Inventory.OnDropDelegate((PointerEventData)data, (new InventoryScript.SlotData(gameObject, this))); });
        trigger.triggers.Add(entry);

        //ドラッグ開始イベント
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.BeginDrag;
        entry.callback.AddListener((data) => { Inventory.OnBeginDragDelegate((PointerEventData)data, (new InventoryScript.SlotData(gameObject, this))); });
        trigger.triggers.Add(entry);

        //クリックイベント
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { Inventory.OnPinterClickDelegate((PointerEventData)data, (new InventoryScript.SlotData(gameObject, this))); });
        trigger.triggers.Add(entry);
    }

    /// <summary>
    /// アイテム使用
    /// </summary>
    /// <returns>アイテムが使用できたか</returns>
    public bool UseItem()
    {
        //アイテムデータなかったら失敗
        if (Item == null) return false;

        //アイテム使用
        bool isUsed = Item.UseDelegate?.Invoke() ?? false;

        //アイテムの使用が正常終了していたら
        if (isUsed)
        {
            //アイテム数を減らす
            stackScript.itemCount--;
            
            //使用できる数がない
            if (stackScript.itemCount <= 0)
            {
                //スロットをリセット
                ResetSlot();
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// アイテムが追加可能か
    /// </summary>
    /// <returns>追加可能数</returns>
    public int IsItemAddable(InventoryItem item)
    {
        //アイテムデータが空の時はスロットは空なのでそのアイテムの最大数を返す
        if (Item == null)
        {
            return item.MaxStackSize;
        }

        //アイテムがいっぱいなら
        if(Item.MaxStackSize <= stackScript.itemCount)
        {
            return 0;
        }
        return Item.MaxStackSize - stackScript.itemCount;
    }

    /// <summary>
    /// アイテムを追加
    /// </summary>
    /// <param name="count">追加数</param>
    /// <returns>追加できなかった数</returns>
    public int AddItem(int count, InventoryItem addItemData)
    {
        //アイテムデータを貰ってなかったら追加しない
        if (addItemData == null) return 0;
        //何もアイテムが入っていなかったら
        if (Item == null)
        {
            ItemSet(addItemData);
        }
        //アイテムが違ったら追加しない
        if (!CanAddItem(addItemData.ItemName)) return 0;

        //追加
        stackScript.itemCount += count;
        int addedItemOverFlowCount = Item.MaxStackSize - stackScript.itemCount;
        //スタックがオーバーフローしていたら
        if(addedItemOverFlowCount < 0)
        {
            //最大数にする
            stackScript.itemCount = Item.MaxStackSize;
            //追加でなかった分の数を返す
            return System.Math.Abs(addedItemOverFlowCount);
        }

        //入れられなかったアイテムはない
        return 0;
    }

    /// <summary>
    /// アイテムが追加できるか
    /// </summary>
    /// <param name="name">追加したいアイテム管理データ</param>
    /// <returns>追加できる場合True</returns>
    public bool CanAddItem(string name)
    {
        //アイテムが空なら追加可能
        if (Item == null) return true;
        //同じ名前なら格納できる
        if (name == Item.ItemName) return true;
        //格納できない
        return false;
    }

    /// <summary>
    /// アイテムをセットする
    /// </summary>
    /// <param name="item">アイテム管理データ</param>
    void ItemSet(InventoryItem item)
    {
        Item = item;
        ItemIcon.sprite = Item.ItemIcon;
        ItemIcon.enabled = true;
    }

    /// <summary>
    /// アイテム数取得
    /// </summary>
    /// <returns></returns>
    public int GetStackCount()
    {
        return stackScript.itemCount;
    }

    /// <summary>
    /// アイテム置き換え
    /// </summary>
    /// <param name="source"></param>
    public void Replace(SlotScript source)
    {
        //アイテム情報を消す
        ResetSlot();
        //アイテムを追加
        AddItem(source.GetStackCount(), source.Item);
    }

    /// <summary>
    /// スロットを初期化
    /// </summary>
    public void ResetSlot()
    {
        if (Item != null)
        {
            //セレクト解除デリゲートを実行
            Item?.DeselectDelegate?.Invoke(null);
        }
        //アイテム情報削除
        Item = null;
        //アイテムアイコン初期化
        if (ItemIcon != null)
        {
            ItemIcon.enabled = false;
            ItemIcon.sprite = null;
        }
        //スタック情報初期化
        if (stackScript != null)
        {
            stackScript.itemCount = 0;
        }
    }

    /// <summary>
    /// スロットが空か確かめる
    /// </summary>
    /// <returns>空ならTrue</returns>
    public bool IsEmpty()
    {
        return (Item == null);
    }
}
