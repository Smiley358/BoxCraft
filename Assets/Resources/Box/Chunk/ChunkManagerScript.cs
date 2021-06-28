using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManagerScript : MonoBehaviour
{
    public static ChunkManagerScript Instance { get; private set; }

    void Awake()
    {
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

    //全チャンクデータ
    public static Dictionary<ChunkScript.Index3D, ChunkScript> chunks { get; private set; }
    private static List<ChunkScript.Index3D> createFailedList;
    //チャンクの生成スタック
    private static Queue<Vector3> chunkCreateOrder;
    //チャンク生成フラグ
    private static bool isCreate;

    public static void CreateOrder(Vector3 position)
    {
        //プッシュ
        chunkCreateOrder.Enqueue(position);

        //順番に作る
        CreateToOrder();
    }

    public static void ForceDestroyChunk(GameObject chunk)
    {
        //チャンクデータを一覧から消す
        chunks.Remove(chunk.GetComponent<ChunkScript>().worldIndex);
        //チャンクを削除
        Destroy(chunk);
    }

    public static bool IsCreateFailed(ChunkScript.Index3D index)
    {
        if (createFailedList.Contains(index))
        {
            createFailedList.Remove(index);
            return true;
        }
        return false;
    }

    private static void CreateToOrder()
    {
        //生成中フラグがおりていたら
        if (isCreate is false)
        {
            //キューが空だったら何もしない
            if (chunkCreateOrder.Count > 0)
            {
                //次に生成するキューの取得
                Vector3 position = chunkCreateOrder.Dequeue();

                Instance.StartCoroutine(Create(position));
            }
        }
    }

    private static IEnumerator Create(Vector3 position)
    {
        isCreate = true;


        //チャンク生成
        GameObject chunk = ChunkScript.Create(position);

        if (chunk is null)
        {
            //生成フラグを下す
            isCreate = false;
            //失敗したリストに入れる
            createFailedList.Add(ChunkScript.CalcWorldIndex(position));
            //次のキューを行う
            CreateToOrder();

            yield break;
        }

        ChunkScript script = chunk.GetComponent<ChunkScript>();

        while (true)
        {
            if (script.IsTerrainGenerateCompleted)
            {
                chunks.Add(script.worldIndex, script);
                break;
            }
            yield return null;
        }

        //生成フラグを下す
        isCreate = false;

        //次のキューを行う
        CreateToOrder();
    }
}
