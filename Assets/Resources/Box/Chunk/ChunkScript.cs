using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkScript : MonoBehaviour
{
    public enum Direction
    {
        First = 0,
        TopLeftForward = First, //�㍶�O
        TopForward,             //��O
        TopRightForward,        //��E�O
        TopRigft,               //��E
        TopRightBehind,         //��E��
        TopBehind,              //���
        TopLeftBehind,          //�㍶��
        TopLeft,                //�㍶
        RightForward,           //�E�O
        LeftForward,            //���O
        Top,                    //��
        Left,                   //��
        Forward,                //�O
        Behind,                 //��
        Right,                  //�E
        Bottom,                 //��
        RightBehind,            //�E��
        LeftBehind,             //����
        BottomRigft,            //���E
        BottomRightForward,     //���E�O
        BottomForward,          //���O
        BottomLeftForward,      //�����O
        BottomLeft,             //����
        BottomLeftBehind,       //������
        BottomBehind,           //����
        BottomRightBehind,      //���E��
        Max = BottomRightBehind
    }

    public struct Index3D
    {
        public int x;
        public int y;
        public int z;

        public Index3D(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override bool Equals(object obj)
        {
            return obj is Index3D d &&
                   x == d.x &&
                   y == d.y &&
                   z == d.z;
        }

        public override int GetHashCode()
        {

            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
        }

        public static bool operator ==(Index3D left, Index3D rigft)
        {
            return (left.x == rigft.x) && (left.y == rigft.y) && (left.z == rigft.z);
        }

        public static bool operator !=(Index3D left, Index3D rigft)
        {
            return (left.x != rigft.x) || (left.y != rigft.y) || (left.z != rigft.z);
        }
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

    public readonly int[][] DirectionOffset = new int[(int)Direction.Max + 1][]
    {
       new int[]{-1, 1, 1 },//�㍶�O
       new int[]{ 0, 1, 1 },//��O
       new int[]{ 1, 1, 1 },//��E�O
       new int[]{ 1, 1, 0 },//��E
       new int[]{ 1, 1,-1 },//��E��
       new int[]{ 0, 1,-1 },//���
       new int[]{-1, 1,-1 },//�㍶��
       new int[]{-1, 1, 0 },//�㍶
       new int[]{ 1, 0, 1 },//�E�O
       new int[]{-1, 0, 1 },//���O
       new int[]{ 0, 1, 0 },//��
       new int[]{-1, 0, 0 },//��
       new int[]{ 0, 0, 1 },//�O
       new int[]{ 0, 0,-1 },//��
       new int[]{ 1, 0, 0 },//�E
       new int[]{ 0,-1, 0 },//��
       new int[]{ 1, 0,-1 },//�E��
       new int[]{-1, 0,-1 },//����
       new int[]{ 1,-1, 0 },//���E
       new int[]{-1,-1, 1 },//���E�O
       new int[]{ 0,-1, 1 },//���O
       new int[]{ 1,-1, 1 },//�����O
       new int[]{-1,-1, 0 },//����
       new int[]{ 1,-1,-1 },//������
       new int[]{ 0,-1,-1 },//����
       new int[]{-1,-1,-1 } //���E��
    };

    public readonly int[] DirectionOffsetCenter = new int[] { 1, 1, 1 };

    public static GameObject Create(Vector3 position)
    {
        //�C���f�b�N�X���v�Z
        Index3D chunkIndex = CalcWorldIndex(position);

        //�C���f�b�N�X���o�^����Ă��Ȃ��i����Ă��Ȃ��j���m�F
        if (ChunkManagerScript.chunks.ContainsKey(chunkIndex))
        {
            return null;
        }

        //�`�����N����
        GameObject chunk = Instantiate(PrefabManager.Instance.GetPrefab("Chunk"), position, Quaternion.identity);
        //�X�N���v�g�̎擾
        ChunkScript script = chunk.GetComponent<ChunkScript>();
        //�C���f�b�N�X��ݒ�
        script.worldIndex = chunkIndex;

        //�`�����N��Ԃ�
        return chunk;
    }

    public static Index3D CalcWorldIndex(Vector3 position)
    {
        Index3D index = new Index3D
            (
                (int)(Mathf.Round(position.x / chunkSize)),
                (int)(Mathf.Round(position.y / chunkSize)),
                (int)(Mathf.Round(position.z / chunkSize))
            );
        return index;
    }

    //�`�����N�̈�ӂ̃T�C�Y
    private const int chunkSize = 64;
    //�m�C�Y�𑜓x�i���s�j
    private const float mapResolutionHorizontal = 50.0f;
    //�m�C�Y�𑜓x�i�����j
    private const float mapResolutionVertical = 45.0f;
    //Box�̃T�C�Y
    private const float boxSize = 1;
    //������������
    private const float far = 1.5f;

    //�����������t���O
    public bool IsTerrainGenerateCompleted { get; private set; }
    //�L���^�C�}�[�������Ă��邩�ǂ���
    public bool IsKillTimerSet { get; private set; }
    //�`�����N�̔z��C���f�b�N�X
    public Index3D worldIndex { get; private set; }

    //�`�����N����Box�f�[�^
    private BoxData[,,] chunkData;
    //�אڃ`�����N
    private ChunkScript[,,] affiliationChunks;

    //�`�����N��X,Y,Z�T�C�Y
    [SerializeField] private Vector3 size;
    //�`�����N�̒��S���W
    [SerializeField] private Vector3 center;
    //��������I�u�W�F�N�g
    [SerializeField] private GameObject prefab;

    void Start()
    {
        //�`�����N����Box�f�[�^�z��
        chunkData = new BoxData[chunkSize, chunkSize, chunkSize];

        //�אڃ`�����N
        affiliationChunks ??= new ChunkScript[3, 3, 3];

        //�`�����N�T�C�Y
        size = new Vector3(chunkSize, chunkSize, chunkSize);
        //���S���W
        center = transform.position;

        //�n�`�𐶐�
        if (worldIndex.y == 0)
        {
            StartCoroutine(GenerateTerrain());
        }
        else
        {
            IsTerrainGenerateCompleted = true;
        }

        //�߂��̃`�����N�𐶐�
        StartCoroutine(GenerateChunkIfNeeded());
    }

    private void Update()
    {
        //�P�O�b������Player����̋��������ė��ꂷ���Ă�����폜����
        InvokeRepeating(nameof(DestroyIfNeeded), 0, 10);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name != "Player") return;
        PlayerMoveNotification();

        for (int direction = (int)Direction.First; direction <= (int)Direction.Max; direction++)
        {
            int x = DirectionOffset[direction][0] + DirectionOffsetCenter[0];
            int y = DirectionOffset[direction][1] + DirectionOffsetCenter[1];
            int z = DirectionOffset[direction][2] + DirectionOffsetCenter[2];

            //�`�����N���Ȃ�������
            if (affiliationChunks?[x, y, z] is null)
            {
                Vector3 offset = new Vector3(DirectionOffset[direction][0], DirectionOffset[direction][1], DirectionOffset[direction][2]);
                offset *= chunkSize;
                Index3D index3D = CalcWorldIndex(center + offset);
                //�S�`�����N�f�[�^����T��
                if (ChunkManagerScript.chunks.ContainsKey(index3D))
                {
                    affiliationChunks[x, y, z] = ChunkManagerScript.chunks[index3D];
                }
            }
            affiliationChunks[x, y, z]?.PlayerMoveNotification();
        }
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
    private IEnumerator GenerateTerrain()
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
            yield return null;
        }
        //�n�`���������t���O�𗧂Ă�
        IsTerrainGenerateCompleted = true;
    }

    private IEnumerator GenerateChunkIfNeeded()
    {
        affiliationChunks ??= new ChunkScript[3, 3, 3];

        //Player�Ƃ̋������m���߂�
        float playerDistanceMax = chunkSize * far;

        for (int direction = (int)Direction.First; direction <= (int)Direction.Max; direction++)
        {
            int x = DirectionOffset[direction][0] + DirectionOffsetCenter[0];
            int y = DirectionOffset[direction][1] + DirectionOffsetCenter[1];
            int z = DirectionOffset[direction][2] + DirectionOffsetCenter[2];

            //�`�����N���Ȃ�������
            if (affiliationChunks[x, y, z] is null)
            {
                Vector3 offset = new Vector3(DirectionOffset[direction][0], DirectionOffset[direction][1], DirectionOffset[direction][2]);
                offset *= chunkSize;
                Index3D index3D = CalcWorldIndex(center + offset);
                //�S�`�����N�f�[�^����T��
                if (ChunkManagerScript.chunks.ContainsKey(index3D))
                {
                    affiliationChunks[x, y, z] = ChunkManagerScript.chunks[index3D];
                }
                else
                {
                    Vector3 position = new Vector3(
                        DirectionOffset[direction][0] * chunkSize,
                        DirectionOffset[direction][1] * chunkSize,
                        DirectionOffset[direction][2] * chunkSize);
                    position += center;
                    if (Vector3.Distance(position, Camera.main.transform.position) <= playerDistanceMax)
                    {
                        ChunkManagerScript.CreateOrder(position);
                        while (true)
                        {
                            if (ChunkManagerScript.chunks.ContainsKey(index3D))
                            {
                                affiliationChunks[x, y, z] = ChunkManagerScript.chunks[index3D];
                                break;
                            }
                            else if (ChunkManagerScript.IsCreateFailed(index3D))
                            {
                                break;
                            }
                            yield return null;
                        }
                    }
                }
            }
        }
    }

    private void PlayerMoveNotification()
    {
        if (IsKillTimerSet)
        {
            //�폜�^�C�}�[���L�����Z��
            CancelInvoke(nameof(DestroyThis));
            IsKillTimerSet = false;
        }
        StartCoroutine(GenerateChunkIfNeeded());

    }

    private void DestroyIfNeeded()
    {
        //Player�Ƃ̋������m���߂�
        float playerDistanceMax = chunkSize * far;

        //Player�Ɨ��ꂷ���Ă�����
        if (Vector3.Distance(Camera.main.transform.position, transform.position) >= playerDistanceMax)
        {
            //�폜�\��
            Debug.Log("Kill Timer : " + transform.position.ToString());
            Invoke(nameof(DestroyThis), 10);
            IsKillTimerSet = true;
        }
    }

    private void DestroyThis()
    {
        ChunkManagerScript.ForceDestroyChunk(gameObject);
    }

    /// <summary>
    /// �ߗׂ�Box���擾����
    /// </summary>
    /// <param name="baseBox">�Box</param>
    /// <param name="direction">�Box����ǂ̕�����Box��</param>
    /// <returns>direction������Box</returns>
    public BoxData GetAdjacentBox(GameObject baseBox, Direction direction)
    {
        //baseBox���i�[����Ă���R�����z��̃C���f�b�N�X���v�Z
        int x = (int)Mathf.Floor(baseBox.transform.localPosition.x + (chunkSize / 2 - boxSize / 2));
        int y = (int)Mathf.Floor(baseBox.transform.localPosition.y + (chunkSize / 2 - boxSize / 2));
        int z = (int)Mathf.Floor(baseBox.transform.localPosition.z + (chunkSize / 2 - boxSize / 2));

        //baseBox����direction������Box�̃C���f�b�N�X���v�Z
        x += DirectionOffset[(int)direction][0];
        y += DirectionOffset[(int)direction][1];
        z += DirectionOffset[(int)direction][2];

        try
        {
            //�C���f�b�N�X�O�ł���΃`�����N�O�Ȃ̂�Box�����݂��锻��
            if (x >= chunkSize || x < 0) return null;
            if (y >= chunkSize || y < 0) return null;
            if (z >= chunkSize || z < 0) return null;

            //Box�����݂��Ă��邩��Ԃ�
            return chunkData?[x, y, z];
        }
        catch
        {
            Debug.Log(baseBox.ToString() + " : " + x.ToString() + " , " + y.ToString() + " , " + z.ToString());
        }


        return null;
    }

    /// <summary>
    /// �אڂ���Box�����邩�m�F����
    /// �`�����N�[�̏ꍇ��True
    /// </summary>
    /// <param name="baseBox">�Box</param>
    /// <param name="direction">�Box����ǂ̕�����Box��</param>
    /// <returns>direction������Box�����݂��邩�A�`�����N�[�Ȃ�True</returns>
    public bool IsAdjacetBoxExist(GameObject baseBox, Direction direction)
    {
        //baseBox���i�[����Ă���R�����z��̃C���f�b�N�X���v�Z
        int x = (int)Mathf.Floor(baseBox.transform.localPosition.x + (chunkSize / 2 - boxSize / 2));
        int y = (int)Mathf.Floor(baseBox.transform.localPosition.y + (chunkSize / 2 - boxSize / 2));
        int z = (int)Mathf.Floor(baseBox.transform.localPosition.z + (chunkSize / 2 - boxSize / 2));

        //baseBox����direction������Box�̃C���f�b�N�X���v�Z
        x += DirectionOffset[(int)direction][0];
        y += DirectionOffset[(int)direction][1];
        z += DirectionOffset[(int)direction][2];

        //�C���f�b�N�X�O�ł���΃`�����N�O�Ȃ̂�Box�����݂��锻��
        if (x >= chunkSize || x < 0) return true;
        if (y >= chunkSize || y < 0) return true;
        if (z >= chunkSize || z < 0) return true;

        //Box�����݂��Ă��邩��Ԃ�
        return chunkData?[x, y, z] != null;
    }



    //�G�f�B�^�[�Ńf�o�b�O�\������p
    private Color color = new Color(0, 1, 0);
    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawWireCube(center, size);
    }
}
