using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// スロットリストに保存するデータ
/// </summary>
public class SlotData
{

    public static bool operator ==(SlotData slotData1, SlotData slotData2) => slotData1?.Slot == slotData2?.Slot;
    public static bool operator !=(SlotData slotData1, SlotData slotData2) => slotData1?.Slot != slotData2?.Slot;

    public GameObject Slot;
    public SlotScript SlotScript;

    public SlotData(GameObject slot, SlotScript slotScript)
    {
        Slot = slot;
        SlotScript = slotScript;
    }

    public override bool Equals(object obj)
    {
        return obj is SlotData data &&
               EqualityComparer<GameObject>.Default.Equals(Slot, data.Slot) &&
               EqualityComparer<SlotScript>.Default.Equals(SlotScript, data.SlotScript);
    }

    public override int GetHashCode()
    {
        int hashCode = 1553962839;
        hashCode = hashCode * -1521134295 + EqualityComparer<GameObject>.Default.GetHashCode(Slot);
        hashCode = hashCode * -1521134295 + EqualityComparer<SlotScript>.Default.GetHashCode(SlotScript);
        return hashCode;
    }
}

public class SlotScript : MonoBehaviour
{
    //選択中のスロット
    public static SlotData selectSlotData { get; private set; }

    /// <summary>
    /// スロットの作成と初期設定の簡略化
    /// </summary>
    /// <param name="prefab">作成するprefab</param>
    /// <param name="inventory"></param>
    /// <returns></returns>
    public static SlotData Create(GameObject prefab)
    {
        //スロットの生成
        GameObject newSlot = Instantiate(prefab);

        SlotScript slotScript = newSlot.GetComponent<SlotScript>();
        //イベントハンドラーの設定
        slotScript.PrepareEventTrigger();
        slotScript.SlotData = new SlotData(newSlot, slotScript);
        return slotScript.SlotData;
    }

    /// <summary>
    /// スロットを選択する
    /// 既に選択されているものがある場合選択解除デリゲートを実行する
    /// </summary>
    /// <param name="slotData">次に選択するスロットデータ。
    /// nullを指定した場合、現在選択されているスロットを解除する。</param>
    public static void SelectSlot(SlotData slotData)
    {
        //既に選択中だったら
        if (slotData == selectSlotData)
        {
            //選択解除
            selectSlotData?.SlotScript?.Deselect();
            selectSlotData = null;
            return;
        }
        //選択中のスロットがあれば選択解除
        selectSlotData?.SlotScript?.Deselect();
        //アイテム選択
        slotData?.SlotScript?.Select();
        //選択中スロットをセット
        selectSlotData = slotData;
    }

    /// <summary>
    /// 指定されたスロットが選択されていた場合スロットの選択状態を解除
    /// </summary>
    /// <param name="slotData">選択解除したいスロット。
    /// 現在選択されているものでない場合何も行わない</param>
    public static void DeselectSlot(SlotData slotData)
    {
        if (slotData != selectSlotData) return;
        //アイテム選択解除
        slotData?.SlotScript.Deselect();
        //選択中スロットをなくす
        selectSlotData = null;
    }

    //アイテムデータ
    public InventoryItem Item { get; set; } = null;
    //インベントリーで管理しているスロットデータクラス
    public SlotData SlotData { get; protected set; }
    //アイテムアイコン表示コンポーネント
    protected Image itemIcon;
    //スロットのアイコン
    protected Image slotIcon;
    //スタック表示/管理スクリプト
    [SerializeField] protected StackScript stackScript;

    protected void Start()
    {
        //アイコン表示コンポーネントを取得
        itemIcon = transform.Find("ItemIcon")?.GetComponent<Image>();
        if (itemIcon == null)
        {
            Debug.Log("スロットの初期化に失敗");
        }
        slotIcon = GetComponent<Image>();
        if (slotIcon == null)
        {
            Debug.Log("スロットの初期化に失敗");
        }
    }

    /// <summary>
    /// イベントハンドラーの設定
    /// </summary>
    public void PrepareEventTrigger()
    {
        //イベントトリガー
        EventTrigger trigger = GetComponent<EventTrigger>();

        //ドラッグ中イベント
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Drag;
        entry.callback.AddListener((data) => { InventoryScript.Instance.OnDragDelegate((PointerEventData)data, (new SlotData(gameObject, this))); });
        trigger.triggers.Add(entry);

        //ドロップイベント
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Drop;
        entry.callback.AddListener((data) => { InventoryScript.Instance.OnDropDelegate((PointerEventData)data, (new SlotData(gameObject, this))); });
        trigger.triggers.Add(entry);

        //ドラッグ開始イベント
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.BeginDrag;
        entry.callback.AddListener((data) => { InventoryScript.Instance.OnBeginDragDelegate((PointerEventData)data, (new SlotData(gameObject, this))); });
        trigger.triggers.Add(entry);

        //クリックイベント
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { InventoryScript.Instance.OnPinterClickDelegate((PointerEventData)data, (new SlotData(gameObject, this))); });
        trigger.triggers.Add(entry);
    }

    /// <summary>
    /// 選択状態に
    /// </summary>
    protected virtual void Select()
    {
        //アイコンを選択中に
        slotIcon.sprite = InventoryScript.Instance.SelectSprite;
        //選択時のデリゲートを実行
        Item?.SelectDelegate?.Invoke(this);
    }

    /// <summary>
    /// 選択状態解除
    /// </summary>
    protected virtual void Deselect()
    {
        //アイコンを非選択中に
        slotIcon.sprite = InventoryScript.Instance.UnselectSprite;
        //選択解除時のデリゲートを実行
        Item?.DeselectDelegate?.Invoke(this);
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
            stackScript.ItemCount--;

            //使用できる数がない
            if (stackScript.ItemCount <= 0)
            {
                //全アイテム使用デリゲート実行
                Item.UsedupDelegate?.Invoke(this);
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
        if (Item.MaxStackSize <= stackScript.ItemCount)
        {
            return 0;
        }
        return Item.MaxStackSize - stackScript.ItemCount;
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
        stackScript.ItemCount += count;
        int addedItemOverFlowCount = Item.MaxStackSize - stackScript.ItemCount;
        //スタックがオーバーフローしていたら
        if (addedItemOverFlowCount < 0)
        {
            //最大数にする
            stackScript.ItemCount = Item.MaxStackSize;
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
        itemIcon.sprite = Item.ItemIcon;
        itemIcon.enabled = true;
    }

    /// <summary>
    /// アイテム数取得
    /// </summary>
    /// <returns></returns>
    public int GetStackCount()
    {
        return stackScript.ItemCount;
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
    /// スロットをリセット
    /// </summary>
    /// <param name="isDelegateExecute">デリゲートを実行するか</param>
    public void ResetSlot(bool isDelegateExecute = true)
    {
        if ((Item != null) && isDelegateExecute)
        {
            //選択中なら解除
            DeselectSlot(SlotData);
        }
        //アイテム情報削除
        Item = null;
        //アイテムアイコン初期化
        if (itemIcon != null)
        {
            itemIcon.enabled = false;
            itemIcon.sprite = null;
        }
        //スタック情報初期化
        if (stackScript != null)
        {
            stackScript.ItemCount = 0;
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
