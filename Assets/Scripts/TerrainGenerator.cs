//using UnityEngine;

//public class TerrainGenerator : MonoBehaviour
//{
//    //�`�����N�̈�ӂ̃T�C�Y
//    [SerializeField] private const int chunkSize = 32;
//    //�m�C�Y�𑜓x�i���s�j
//    [SerializeField] private const float mapResolutionHorizontal = 50.0f;
//    //�m�C�Y�𑜓x�i�����j
//    [SerializeField] private const float mapResolutionVertical = 30.0f;

//    //Box�̃T�C�Y
//    [SerializeField] private float boxSize;
//    //��������I�u�W�F�N�g
//    [SerializeField] private GameObject prefab;

//    void Start()
//    {
//        size = new Vector3(chunkSize, chunkSize, chunkSize);
//        center = transform.position;
//        Vector3 offset = size / 2 - center;

//        //X�T�C�Y��
//        for (int x = 0; x < chunkSize; x++)
//        {
//            //Z�T�C�Y��
//            for (int z = 0; z < chunkSize; z++)
//            {
//                //�m�C�Y����
//                float noise = Mathf.PerlinNoise((transform.position.x + x) / mapResolutionHorizontal, (transform.position.z + z) / mapResolutionHorizontal);
//                //��ԏ�̈ʒu
//                int top = (int)Mathf.Round(mapResolutionVertical * noise);
//                //�Œ�ʂ����ԏ�̒n�`�܂�Box�𖄂߂�
//                //for (int y = 0; y < top; y++)
//                int y = top;
//                {
//                    //����
//                    GameObject box = BoxBase.Create(
//                        prefab, 
//                        new Vector3(
//                        boxSize * x + boxSize / 2.0f - offset.x, 
//                        boxSize * y + boxSize / 2.0f - offset.y, 
//                        boxSize * z + boxSize / 2.0f - offset.z), 
//                        Quaternion.identity);
//                    //�e�ɐݒ�
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