using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManagerScript : MonoBehaviour
{
    //自分のインスタンス
    public static ChunkManagerScript Instance { get; private set; }

    void Awake()
    {
        //インスタンスを代入
        Instance = this;

        //チャンクの生成キュー
        chunkCreateOrder = new Queue<Vector3>();

        //チャンク一覧
        chunks =  new Dictionary<ChunkScript.Index3D, ChunkScript>();

        //チャンク生成失敗データ
        createFailedList = new List<ChunkScript.Index3D>();
    }

    void Start()
    {
        //最初の一個を作る
        CreateOrder(new Vector3(0, 0, 0));
    }

    private void Update()
    {
        if (!IsCompleted)
        {
            CreateToOrder();
        }
    }

    //チャンク生成完了フラグ
    public static bool IsCompleted { get; private set; }
    //全チャンクデータ
    public static Dictionary<ChunkScript.Index3D, ChunkScript> chunks { get; private set; }
    //作成失敗チャンクのオフセットリスト
    private static List<ChunkScript.Index3D> createFailedList;
    //チャンクの生成スタック
    private static Queue<Vector3> chunkCreateOrder;
    //チャンク生成中フラグ
    private static bool isCreate;

    /// <summary>
    /// チャンクの生成キューに追加する
    /// </summary>
    /// <param name="position">生成座標</param>
    public static void CreateOrder(Vector3 position)
    {
        //プッシュ
        chunkCreateOrder.Enqueue(position);
        IsCompleted = false;
    }

    /// <summary>
    /// チャンクを消す
    /// 参照エラー対策としてデータベースからも消す
    /// </summary>
    /// <param name="chunk"></param>
    public static void ForceDestroyChunk(GameObject chunk)
    {
        if (chunk == null) return;

        //チャンクデータを一覧から消す
        chunks.Remove(chunk.GetComponent<ChunkScript>().WorldIndex);
        //チャンクを削除
        Destroy(chunk);
    }

    /// <summary>
    /// 引数として渡された座標のチャンクが生成失敗しているか確認する
    /// </summary>
    /// <param name="index">失敗確認したい座標</param>
    /// <returns>失敗していればtrue</returns>
    public static bool IsCreateFailed(ChunkScript.Index3D index)
    {
        if (createFailedList.Contains(index))
        {
            createFailedList.Remove(index);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 順番にチャンクを作る
    /// </summary>
    private static void CreateToOrder()
    {
        //生成中フラグがおりていたら
        if (isCreate is false)
        {
            //キューが空じゃないとき
            if (chunkCreateOrder.Count > 0)
            {
                //次に生成するキューの取得
                Vector3 position = chunkCreateOrder.Dequeue();

                //生成開始
                Instance.StartCoroutine(Create(position));
            }
            else
            {
                //生成完了
                IsCompleted = true;
            }
        }
    }

    /// <summary>
    /// チャンクを作る
    /// 地形の生成が終わるまで待つ
    /// </summary>
    /// <param name="position">生成座標</param>
    private static IEnumerator Create(Vector3 position)
    {
        //生成中フラグ立てる
        isCreate = true;

        //チャンク生成
        GameObject chunk = ChunkScript.Create(position);

        //作られてなかったら生成失敗リストに入れる
        if (chunk == null)
        {
            //生成フラグを下す
            isCreate = false;
            //失敗したリストに入れる
            createFailedList.Add(ChunkScript.CalcWorldIndex(position));
            //中断
            yield break;
        }

        //スクリプト取得
        ChunkScript script = chunk.GetComponent<ChunkScript>();

        //地形生成待ち
        while (true)
        {
            if (script.IsTerrainGenerateCompleted)
            {
                //チャンクリストに追加
                chunks.Add(script.WorldIndex, script);
                break;
            }
            yield return null;
        }

        //生成フラグを下す
        isCreate = false;
    }
}
