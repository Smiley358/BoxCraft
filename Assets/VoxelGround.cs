using UnityEngine;

public class VoxelGround : MonoBehaviour
{
    private float sizeX = 100f;
    private float sizeY = 10f;
    private float sizeZ = 100f;
    private float sizeW = 10f;

    [SerializeField]
    private GameObject prefab;

    void Start()
    {
        //var material = this.GetComponent<MeshRenderer>().material;
        for (int x = 0; x < sizeX; x++)
        {
            for (int z = 0; z < sizeZ; z++)
            {
                float noise = Mathf.PerlinNoise(x / sizeW, z / sizeW);
                int yTop = (int)Mathf.Round(sizeY * noise);
                for (int y = 0; y < yTop; y++)
                {
                    GameObject cube = Instantiate(prefab);
                    cube.transform.SetParent(transform);
                    cube.transform.localPosition = new Vector3(x, y, z);
                }
            }
        }
        transform.localPosition = new Vector3(-sizeX / 2, 0, -sizeZ / 2);
    }
}