//using UnityEngine;

//public class TerrainGenerator : MonoBehaviour
//{
//    //チャンクの一辺のサイズ
//    [SerializeField] private const int chunkSize = 32;
//    //ノイズ解像度（平行）
//    [SerializeField] private const float mapResolutionHorizontal = 50.0f;
//    //ノイズ解像度（垂直）
//    [SerializeField] private const float mapResolutionVertical = 30.0f;

//    //Boxのサイズ
//    [SerializeField] private float boxSize;
//    //生成するオブジェクト
//    [SerializeField] private GameObject prefab;

//    void Start()
//    {
//        size = new Vector3(chunkSize, chunkSize, chunkSize);
//        center = transform.position;
//        Vector3 offset = size / 2 - center;

//        //Xサイズ回
//        for (int x = 0; x < chunkSize; x++)
//        {
//            //Zサイズ回
//            for (int z = 0; z < chunkSize; z++)
//            {
//                //ノイズ生成
//                float noise = Mathf.PerlinNoise((transform.position.x + x) / mapResolutionHorizontal, (transform.position.z + z) / mapResolutionHorizontal);
//                //一番上の位置
//                int top = (int)Mathf.Round(mapResolutionVertical * noise);
//                //最低面から一番上の地形までBoxを埋める
//                //for (int y = 0; y < top; y++)
//                int y = top;
//                {
//                    //生成
//                    GameObject box = BoxBase.Create(
//                        prefab, 
//                        new Vector3(
//                        boxSize * x + boxSize / 2.0f - offset.x, 
//                        boxSize * y + boxSize / 2.0f - offset.y, 
//                        boxSize * z + boxSize / 2.0f - offset.z), 
//                        Quaternion.identity);
//                    //親に設定
//                    box.transform.parent = gameObject.transform;
//                }
//            }
//        }
//    }

//    private Color color = new Color(0, 1, 0);
//    private Vector3 size;
//    private Vector3 center;
//    private void OnDrawGizmos()
//    {
//        Gizmos.color = color;
//        Gizmos.DrawWireCube(center, size);
//    }
//}