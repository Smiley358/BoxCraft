using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 一列のタイル状テクスチャをUVキューブにマッピングする
/// </summary>
public class LinearTextureTileUVMappingScript : MonoBehaviour
{
    private static Vector2[] UV = null;

    void Start()
    {
        if (UV == null)
        {
            UV = new Vector2[24];
            //上面
            UV[4].x = 0 / 6.0f; UV[4].y = 0;
            UV[5].x = 1 / 6.0f; UV[5].y = 0;
            UV[8].x = 0 / 6.0f; UV[8].y = 1;
            UV[9].x = 1 / 6.0f; UV[9].y = 1;
            //前面
            UV[19].x = 2 / 6.0f; UV[19].y = 0;
            UV[17].x = 1 / 6.0f; UV[17].y = 1;
            UV[16].x = 1 / 6.0f; UV[16].y = 0;
            UV[18].x = 2 / 6.0f; UV[18].y = 1;
            //後面
            UV[23].x = 3 / 6.0f; UV[23].y = 0;
            UV[21].x = 2 / 6.0f; UV[21].y = 1;
            UV[20].x = 2 / 6.0f; UV[20].y = 0;
            UV[22].x = 3 / 6.0f; UV[22].y = 1;
            //右面
            UV[6].x = 3 / 6.0f; UV[6].y = 0;
            UV[7].x = 4 / 6.0f; UV[7].y = 0;
            UV[10].x = 3 / 6.0f; UV[10].y = 1;
            UV[11].x = 4 / 6.0f; UV[11].y = 1;
            //左面
            UV[2].x = 5 / 6.0f; UV[2].y = 1;
            UV[3].x = 4 / 6.0f; UV[3].y = 1;
            UV[0].x = 5 / 6.0f; UV[0].y = 0;
            UV[1].x = 4 / 6.0f; UV[1].y = 0;
            //下面
            UV[15].x = 6 / 6.0f; UV[15].y = 0;
            UV[13].x = 5 / 6.0f; UV[13].y = 1;
            UV[12].x = 5 / 6.0f; UV[12].y = 0;
            UV[14].x = 6 / 6.0f; UV[14].y = 1;
        }

        //UVをセット
        gameObject.transform.GetComponent<MeshFilter>().mesh.uv = UV;
    }
}
