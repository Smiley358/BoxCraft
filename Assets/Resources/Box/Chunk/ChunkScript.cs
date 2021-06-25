using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkScript : MonoBehaviour
{
    public enum Direction
    {
        Top,
        Left,
        Forward,
        Behind,
        Right,
        Bottom
    }

    public class BoxData
    {
        public BoxBase Script { get; private set; }
        public GameObject Object { get; private set; }

        public BoxData(GameObject gameObject, BoxBase script)
        {
            Script = script;
            Object = gameObject;
        }
    }

    //チャンクの一辺のサイズ
    private const int chunkSize = 32;
    //ノイズ解像度（平行）
    private const float mapResolutionHorizontal = 50.0f;
    //ノイズ解像度（垂直）
    private const float mapResolutionVertical = 30.0f;
    //Boxのサイズ
    private const float boxSize = 1;

    //チャンク内のBoxデータ
    private BoxData[,,] chunkData;

    //チャンクのX,Y,Zサイズ
    [SerializeField] private Vector3 size;
    //チャンクの中心座標
    [SerializeField] private Vector3 center;

    //生成するオブジェクト
    [SerializeField] private GameObject prefab;


    void Start()
    {
        //チャンク内のBoxデータ配列
        chunkData = new BoxData[chunkSize, chunkSize,chunkSize];

        //チャンクサイズ
        size = new Vector3(chunkSize, chunkSize, chunkSize);
        //中心座標
        center = transform.position;

        //地形を生成
        GenerateTerrain();
    }

    /// <summary>
    /// Boxを削除
    /// </summary>
    /// <param name="box"></param>
    public void DestroyBox(GameObject box)
    {
        int x = (int)Mathf.Floor(box.transform.localPosition.x + (chunkSize / 2 - boxSize / 2));
        int y = (int)Mathf.Floor(box.transform.localPosition.y + (chunkSize / 2 - boxSize / 2));
        int z = (int)Mathf.Floor(box.transform.localPosition.z + (chunkSize / 2 - boxSize / 2));
        chunkData[x, y, z] = null;
        Destroy(box);
    }

    /// <summary>
    /// 地形を生成する
    /// </summary>
    private void GenerateTerrain()
    {
        //バウンディングボックスのX,Y,Z起点位置までのオフセット
        Vector3 offset = size / 2 - center;

        //chunkSize回
        for (int x = 0; x < chunkSize; x++)
        {
            //chunkSize回
            for (int z = 0; z < chunkSize; z++)
            {
                //ノイズ生成
                float noise = Mathf.PerlinNoise(
                    (transform.position.x + x) / mapResolutionHorizontal,
                    (transform.position.z + z) / mapResolutionHorizontal);
                //一番上の位置
                int top = (int)Mathf.Round(mapResolutionVertical * noise);
                //最低面から一番上のBoxまでを埋める
                for (int y = 0; y < top; y++)
                //int y = top;
                {
                    //生成
                    GameObject box = BoxBase.Create(
                        this,
                        prefab,
                        new Vector3(
                        boxSize * x + boxSize / 2.0f - offset.x,
                        boxSize * y + boxSize / 2.0f - offset.y,
                        boxSize * z + boxSize / 2.0f - offset.z),
                        Quaternion.identity);
                    //親に設定
                    box.transform.parent = gameObject.transform;

                    //データを保存
                    chunkData[x, y, z] = new BoxData(box, box.GetComponent<BoxBase>());
                }
            }
        }
    }

    /// <summary>
    /// 近隣のBoxを取得する
    /// </summary>
    /// <param name="baseBox">基準Box</param>
    /// <param name="direction">基準Boxからどの方向のBoxか</param>
    /// <returns></returns>
    public BoxData GetAdjacentBox(GameObject baseBox, Direction direction)
    {
        int x = (int)Mathf.Floor(baseBox.transform.localPosition.x + (chunkSize/2 - boxSize/2));
        int y = (int)Mathf.Floor(baseBox.transform.localPosition.y + (chunkSize/2 - boxSize/2));
        int z = (int)Mathf.Floor(baseBox.transform.localPosition.z + (chunkSize/2 - boxSize/2));
        if (x >= chunkSize || x < 0)
        {
            return null;
        }
        else if (y >= chunkSize || y < 0)
        {
            return null;
        }
        else if (z >= chunkSize || z < 0)
        {
            return null;
        }

        try
        {
            switch (direction)
            {
                case Direction.Top:
                    {
                        if (y + 1 >= chunkSize) return null;
                        return chunkData[x, y + 1, z];
                    }
                case Direction.Left:
                    {
                        if (x - 1 < 0) return null;
                        return chunkData[x - 1, y, z];
                    }
                case Direction.Forward:
                    {
                        if (z + 1 >= chunkSize) return null;
                        return chunkData[x, y, z + 1];
                    }
                case Direction.Behind:
                    {
                        if (z - 1 < 0) return null;
                        return chunkData[x, y, z - 1];
                    }
                case Direction.Right:
                    {
                        if (x + 1 >= chunkSize) return null;
                        return chunkData[x + 1, y, z];
                    }
                case Direction.Bottom:
                    {
                        if (y - 1 < 0) return null;
                        return chunkData[x, y - 1, z];
                    }
            }

        }
        catch
        {
            Debug.Log("");
        }


        return null;
    }

    //エディターでデバッグ表示する用
    private Color color = new Color(0, 1, 0);
    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawWireCube(center, size);
    }
}
