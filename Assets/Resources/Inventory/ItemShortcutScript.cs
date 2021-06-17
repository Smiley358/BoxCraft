using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemShortcutScript : MonoBehaviour
{   
    //キー割り当て用インデックスリスト
    public static readonly int[] IndexList = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
    //現在の割り当て状況
    private int nowIndex = 0;

    //選択中のスロット
    public static int SelectIndex = UNSELECT_INDEX;
    //非選択中のインデックス
    public static readonly int UNSELECT_INDEX = -1;

    //ショートカットの移動コンポーネント
    private RectTransform itemShortcutRect;
    //ショートカットのサイズ
    private Vector2 itemShortcutSize;
    //アイテムショートカット用スロットのprefab
    public GameObject ItemShortcutSlotPrefab;
    //スロット間のパディング
    public Vector2 slotPadding;

    //スロット
    public Dictionary<int, ItemShortcutSlotData> itemShortcutSlots { get; private set; } = new Dictionary<int, ItemShortcutSlotData>(IndexList.Length);

    void Start()
    {
        //アイテムショートカットを作る
        CreateLayout();
    }

    void Update()
    {
        float scroll = -Input.GetAxis("Mouse ScrollWheel");
        //+なら1に、-なら-1に、0に近ければ0
        int indexShift = (Mathf.Abs(scroll) < 0.00001) ? 0 : (scroll > 0) ? 1 : -1;
        //マウスホイールまわしたとき
        if (indexShift != 0)
        {
            //-1~9までが入るように計算
            SelectIndex = (IndexList.Length + 1 + SelectIndex + indexShift + 1) % (IndexList.Length + 1) - 1;
            if (SelectIndex != -1)
            {
                //スロットを選択
                SlotScript.SelectSlot(itemShortcutSlots[IndexList[SelectIndex]].ItemShortcutSlotScript.slotData);
            }
            else
            {
                //現在選択されているスロットの選択解除をして、選択スロットをNullにする
                SlotScript.SelectSlot(null);
            }
        }
    }

    /// <summary>
    /// 選択しているインデックスをキーに登録されているインデックスに直す
    /// </summary>
    /// <returns></returns>
    public static int GetSelectIndexForSlotIndex()
    {
        return (SelectIndex != -1) ? IndexList[SelectIndex] : -1;
    }

    /// <summary>
    /// アイテムショートカットのレイアウトを整える
    /// </summary>
    private void CreateLayout()
    {
        //アイテムショートカットを生成

        //数字キー分個作る
        int itemShortCutSlots = IndexList.Length;
        //ショートカットのトランスフォームコンポーネント取得
        itemShortcutRect = GetComponent<RectTransform>();
        //スロットの大きさはUnityエディターの物を使用する
        float itemShortcutSlotSize = ItemShortcutSlotPrefab.GetComponent<RectTransform>().rect.width;
        //ショートカットのサイズを計算
        itemShortcutSize.x = itemShortCutSlots * (itemShortcutSlotSize + slotPadding.x) + slotPadding.x;
        itemShortcutSize.y = itemShortcutSlotSize + slotPadding.y * 2;
        //ショートカットのサイズをセット
        itemShortcutRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, itemShortcutSize.x);
        itemShortcutRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, itemShortcutSize.y);

        for (int x = 0; x < itemShortCutSlots; x++)
        {
            //スロットの生成
            ItemShortcutSlotData newSlot = ItemShortcutSlotScript.Create(ItemShortcutSlotPrefab, gameObject, IndexList, nowIndex);
            if (newSlot == null)
            {
                Debug.Log("Item Shortcut Initialize Failed");
                break;
            }
            //スロットのトランスフォームコンポーネントを取得
            RectTransform slotRectTransform = newSlot.Slot.GetComponent<RectTransform>();
            //名前を変更
            newSlot.Slot.name = "ShortcutSlot";
            //親をItemShortcutに
            newSlot.Slot.transform.SetParent(transform);
            //サイズと位置をセット
            Vector3 offset = new Vector3(itemShortcutRect.rect.width, itemShortcutRect.rect.height, 0);
            offset.Scale(new Vector3(itemShortcutRect.pivot.x, itemShortcutRect.pivot.y, 0));
            slotRectTransform.localPosition = new Vector3(slotPadding.x * (x + 1) + (itemShortcutSlotSize * x) - offset.x, -slotPadding.y + offset.y);
            slotRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, itemShortcutSlotSize);
            slotRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, itemShortcutSlotSize);

            //インデックスカウントアップ
            nowIndex++;

            //リストに格納
            itemShortcutSlots.Add(newSlot.ItemShortcutSlotScript.Index, newSlot);
        }
    }
}
