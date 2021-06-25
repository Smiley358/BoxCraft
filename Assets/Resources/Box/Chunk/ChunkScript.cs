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

    //�`�����N�̈�ӂ̃T�C�Y
    private const int chunkSize = 32;
    //�m�C�Y�𑜓x�i���s�j
    private const float mapResolutionHorizontal = 50.0f;
    //�m�C�Y�𑜓x�i�����j
    private const float mapResolutionVertical = 30.0f;
    //Box�̃T�C�Y
    private const float boxSize = 1;

    //�`�����N����Box�f�[�^
    private BoxData[,,] chunkData;

    //�`�����N��X,Y,Z�T�C�Y
    [SerializeField] private Vector3 size;
    //�`�����N�̒��S���W
    [SerializeField] private Vector3 center;

    //��������I�u�W�F�N�g
    [SerializeField] private GameObject prefab;


    void Start()
    {
        //�`�����N����Box�f�[�^�z��
        chunkData = new BoxData[chunkSize, chunkSize,chunkSize];

        //�`�����N�T�C�Y
        size = new Vector3(chunkSize, chunkSize, chunkSize);
        //���S���W
        center = transform.position;

        //�n�`�𐶐�
        GenerateTerrain();
    }

    /// <summary>
    /// Box���폜
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
    /// �n�`�𐶐�����
    /// </summary>
    private void GenerateTerrain()
    {
        //�o�E���f�B���O�{�b�N�X��X,Y,Z�N�_�ʒu�܂ł̃I�t�Z�b�g
        Vector3 offset = size / 2 - center;

        //chunkSize��
        for (int x = 0; x < chunkSize; x++)
        {
            //chunkSize��
            for (int z = 0; z < chunkSize; z++)
            {
                //�m�C�Y����
                float noise = Mathf.PerlinNoise(
                    (transform.position.x + x) / mapResolutionHorizontal,
                    (transform.position.z + z) / mapResolutionHorizontal);
                //��ԏ�̈ʒu
                int top = (int)Mathf.Round(mapResolutionVertical * noise);
                //�Œ�ʂ����ԏ��Box�܂ł𖄂߂�
                for (int y = 0; y < top; y++)
                //int y = top;
                {
                    //����
                    GameObject box = BoxBase.Create(
                        this,
                        prefab,
                        new Vector3(
                        boxSize * x + boxSize / 2.0f - offset.x,
                        boxSize * y + boxSize / 2.0f - offset.y,
                        boxSize * z + boxSize / 2.0f - offset.z),
                        Quaternion.identity);
                    //�e�ɐݒ�
                    box.transform.parent = gameObject.transform;

                    //�f�[�^��ۑ�
                    chunkData[x, y, z] = new BoxData(box, box.GetComponent<BoxBase>());
                }
            }
        }
    }

    /// <summary>
    /// �ߗׂ�Box���擾����
    /// </summary>
    /// <param name="baseBox">�Box</param>
    /// <param name="direction">�Box����ǂ̕�����Box��</param>
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

    //�G�f�B�^�[�Ńf�o�b�O�\������p
    private Color color = new Color(0, 1, 0);
    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawWireCube(center, size);
    }
}
