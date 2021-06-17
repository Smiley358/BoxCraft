using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Threading;
public class InventoryScript : MonoBehaviour
{
    //インベントリーのインスタンス
    public static InventoryScript InventoryScriptInstance { get; private set; }
    //インベントリーの移動コンポーネント
    private RectTransform inventoryRect;
    //インベントリーのサイズ
    private Vector2 inventorySize;
    //スロット数
    public int Slots;
    //行数
    public int Rows;
    //スロット間のパディング
    public Vector2 slotPadding;

    //スロットのprefab
    public GameObject SlotPrefab;
    //スロットリスト
    private List<SlotData> allSlots;

    //ドラッグ用ホバースロット
    public GameObject HoverSlot;
    //ドラッグ用ホバースロットの管理クラス
    private SlotScript hoverSlotScript;
    //ドラッグ用ホバースロットのアイコン
    private Image hoverSlotIconImage;
    //ドラッグ元のスロット
    private SlotData dragSlotData;

    //選択中
    public Sprite SelectSprite;
    //非選択
    public Sprite UnselectSprite;

    //インベントリーの有効フラグ
    public bool IsActiveInventory { get; private set; } = false;


    void Start()
    {
        InventoryScriptInstance = this;

        //インベントリーのレイアウトを作る
        CreateLayout();

        //ホバー用のスロットの初期設定
        hoverSlotIconImage = HoverSlot.transform.Find("ItemIcon")?.GetComponent<Image>();
        hoverSlotScript = HoverSlot.GetComponent<SlotScript>();
        //一番最下位へ（最前面に表示するため）
        HoverSlot.transform.SetAsLastSibling();
        //非活性化
        HoverSlot.SetActive(false);

        //インベントリーは閉じておく
        HideInventory();
    }

    private void Update()
    {
        //Tabでインベントリー開く
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            IsActiveInventory = !IsActiveInventory;

            if (IsActiveInventory)
            {
                ShowInventory();
            }
            else
            {
                HideInventory();
            }
        }

        //マウスボタン離したらホバースロットは消す
        if (Input.GetMouseButtonUp(0))
        {
            //ドラッグ元データの保持を破棄
            dragSlotData = null;
            //ホバースロットを無効化
            HoverSlot.SetActive(false);
            hoverSlotScript.ResetSlot();
        }
    }

    //インベントリーを開く
    public void ShowInventory()
    {
        //マウスカーソル戻す
        Cursor.lockState = CursorLockMode.None;
        gameObject.transform.localScale = Vector3.one;
    }

    //インベントリーを閉じる
    public void HideInventory()
    {
        //マウスカールが外に出ないように
        Cursor.lockState = CursorLockMode.Locked;
        gameObject.transform.localScale = Vector3.zero;
    }

    /// <summary>
    /// アイテムの追加を試みる
    /// </summary>
    /// <param name="item">フィールドアイテムオブジェクト用クラス</param>
    /// <returns>すべて追加できた時はTrue</returns>
    public bool TryAddItem(ItemScript item)
    {
        while (true)
        {
            //まずは空いているスロットを探す
            foreach (var slot in allSlots)
            {
                //アイテムオブジェクトにスタックされたアイテムがもうない
                if (item.contentCount <= 0)
                {
                    break;
                }
                //アイテムの種類が違うなら何もしない
                if (!slot.SlotScript.CanAddItem(item.ItemData.ItemName)) continue;

                //スロットに入れることができるアイテムの数
                int canStorageableCount = slot.SlotScript.IsItemAddable(item.ItemData);
                //スロットに追加できないなら何もしない
                if (canStorageableCount <= 0) continue;

                //アイテムオブジェクトからスロット入れる分のカウントを減らす
                int storageCount = item.StorageItem(canStorageableCount);
                //スロットにアイテムを入れる
                int couldNotAddedCount = slot.SlotScript.AddItem(storageCount, item.ItemData);
                //余剰分をアイテムオブジェクトに戻す
                item.JoinItem(couldNotAddedCount);
            }

            //追加できていないものがある
            if (item.contentCount > 0)
            {
                Debug.LogWarning("アイテム追加失敗");
                return false;
            }

            break;
        }
        return true;
    }


    /// <summary>
    /// インベントリーのレイアウトを整える
    /// </summary>
    private void CreateLayout()
    {
        //まずインベントリー本体を生成

        //スロットリスト
        allSlots = new List<SlotData>();
        //インベントリーのトランスフォームコンポーネント取得
        inventoryRect = GetComponent<RectTransform>();
        //スロットの大きさはUnityエディターの物を使用する
        float SlotSize = SlotPrefab.GetComponent<RectTransform>().rect.width;
        //インベントリーのサイズを計算
        inventorySize.x = (Slots / Rows) * (SlotSize + slotPadding.x) + slotPadding.x;
        inventorySize.y = Rows * (SlotSize + slotPadding.y) + slotPadding.y;
        //インベントリーのサイズをセット
        inventoryRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, inventorySize.x);
        inventoryRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, inventorySize.y);

        int columns = Slots / Rows;

        for (int y = 0; y < Rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                SlotData slotData = SlotScript.Create(SlotPrefab, gameObject);
                //スロットのトランスフォームコンポーネントを取得
                RectTransform slotRectTransform = slotData.Slot.GetComponent<RectTransform>();
                //名前を変更
                slotData.Slot.name = "Slot";
                //親をインベントリに
                slotData.Slot.transform.SetParent(transform);
                //サイズと位置をセット
                slotRectTransform.localPosition = new Vector3(slotPadding.x * (x + 1) + (SlotSize * x), -slotPadding.y * (y + 1) - (SlotSize * y));
                slotRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, SlotSize);
                slotRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, SlotSize);

                //リストに格納
                allSlots.Add(slotData);
            }
        }
    }


    /// <summary>
    /// アイテムの使用
    /// </summary>
    /// <param name="slot">使用するアイテムのスロット</param>
    /// <returns>正常に使用できたらTrue</returns>
    public bool UseItem(SlotData slot)
    {
        //スロットデータなかったら失敗
        if (slot == null) return false;

        //アイテムの使用
        return slot.SlotScript.UseItem();
    }


    public void OnBeginDragDelegate(PointerEventData data, SlotData slot)
    {
        //左クリック以外受け付けない
        if (!(data.button == PointerEventData.InputButton.Left)) return;
        //空のスロットはドラッグできない
        if (slot.SlotScript.IsEmpty()) return;

        //ホバースロットを有効に
        HoverSlot.SetActive(true);
        //ホバースロット情報の置き換え
        hoverSlotScript.Replace(slot.SlotScript);
        //ドラッグ元のスロットデータ
        dragSlotData = slot;
    }

    public void OnDragDelegate(PointerEventData data, SlotData slot)
    {
        Vector3 position = new Vector3(data.position.x, data.position.y, 0);
        //アイコンをマウスカーソルの位置へ
        HoverSlot.transform.position = position;
    }

    public void OnDropDelegate(PointerEventData data, SlotData slot)
    {
        //ドラッグ元のデータがある場合
        if (dragSlotData != null)
        {
            //ドロップ元のスロットにドロップ先スロットのデータを移す
            dragSlotData.SlotScript.Replace(slot.SlotScript);
            //ドロップ元が選択中なら選択解除
            SlotScript.DeselectSlot(dragSlotData);
            //ドロップ先のスロットにドラッグ元(ホバースロット)スロットのデータを移す
            slot.SlotScript.Replace(hoverSlotScript);
            //ドロップ先が選択中なら
            if(SlotScript.selectSlotData == slot)
            {
                SlotScript.SelectSlot(slot);
            }
            //ドラッグ元データの保持を破棄
            dragSlotData = null;
        }

        //ホバースロットを無効に
        HoverSlot.SetActive(false);
        hoverSlotScript.ResetSlot(false);
    }

    public void OnPinterClickDelegate(PointerEventData data, SlotData slot)
    {
        //右クリック以外受け付けない
        if (!(data.button == PointerEventData.InputButton.Right)) return;
        //空のスロットはクリックできない
        if (slot.SlotScript.IsEmpty()) return;

        //右クリックで使用か選択か
        if (slot.SlotScript.Item?.IsImmediateUse == true)
        {
            //アイテム使用
            UseItem(slot);
        }
        else
        {
            //同じスロットなら選択解除
            SlotScript.DeselectSlot(slot);
            //アイテムスロット選択
            SlotScript.SelectSlot(slot);
        }
    }
}
