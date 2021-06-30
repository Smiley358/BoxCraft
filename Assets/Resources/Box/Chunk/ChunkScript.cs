using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkScript : MonoBehaviour
{
    /// <summary>
    /// Box�̕���
    /// </summary>
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

    /// <summary>
    /// Box�����[���h�̂ǂ��ɂ���̂�
    /// �u���b�N�P�ʂł̍��W
    /// </summary>
    public struct Index3D
    {
        public int x;
        public int y;
        public int z;

        /// <summary>
        /// �����W����͂ݏo�Ă��Ȃ����`�F�b�N
        /// </summary>
        /// <param name="min">�ŏ��l�i�����W���ōŏ��̒l�j</param>
        /// <param name="max">�ő�l�i�����W���ōő�̒l�j</param>
        /// <returns>�����W���Ȃ�True</returns>
        public bool IsFitIntoRange(int min,int max)
        {
            return (x >= min) && (x <= max) && (y >= min) && (y <= max) && (z >= min) && (z <= max);
        }

        public Index3D(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Index3D(int xyz)
        {
            this.x = xyz;
            this.y = xyz;
            this.z = xyz;
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

        public override string ToString()
        {
            return "Index{ " + x.ToString() + " , " + y.ToString() + " , " + z.ToString() + " }";
        }

        public static bool operator ==(Index3D left, Index3D rigft)
        {
            return (left.x == rigft.x) && (left.y == rigft.y) && (left.z == rigft.z);
        }

        public static bool operator !=(Index3D left, Index3D rigft)
        {
            return (left.x != rigft.x) || (left.y != rigft.y) || (left.z != rigft.z);
        }

        public static Index3D operator +(Index3D left, Index3D rigft)
        {
            return new Index3D(left.x + rigft.x, left.y + rigft.y, left.z + rigft.z);
        }

        public static Index3D operator -(Index3D left, Index3D rigft)
        {
            return new Index3D(left.x - rigft.x, left.y - rigft.y, left.z - rigft.z);
        }
    }

    /// <summary>
    /// Box��GameObject�ƃX�N���v�g���܂Ƃ߂��N���X
    /// </summary>
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

    /// <summary>
    /// DirectionOffset�̓Y�����A�N�Z�X�p
    /// </summary>
    public const int X = 0, Y = 1, Z = 2;

    /// <summary>
    /// ChunkScript.Direction�Ԗڂ̒��g��
    /// ���̕����ւ̃I�t�Z�b�g�ɂȂ��Ă���
    /// <summary>
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

    /// <summary>
    /// affiliationChunks�t�B�[���h�p
    /// �����̈ʒu
    /// </summary>
    public readonly int[] DirectionOffsetCenter = new int[] { 1, 1, 1 };

    /// <summary>
    /// �`�����N�𐶐�����i�d���ł̐������u���b�N����@�\�t���j
    /// </summary>
    /// <param name="position">�����ʒu</param>
    /// <returns>�����ł��Ă����GameObject�A�������s��null</returns>
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

    /// <summary>
    /// ���W�����[���h�ł̃u���b�N�P�ʂ̍��W�ɕϊ�����
    /// </summary>
    /// <param name="position">�ϊ�������W</param>
    /// <returns>���[���h�ł̃u���b�N�P�ʂ̍��W</returns>
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

    /// <summary>
    /// Box�𐶐����Ď����Ń`�����N�ɏ���������
    /// </summary>
    /// <param name="prefab">prefab</param>
    /// <param name="position">�������W</param>
    /// <param name="rotation">��]</param>
    /// <returns></returns>
    public static GameObject CreateBoxAndAutomBelongToChunk(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        //�܂�Box�̐����ʒu�̃`�����N�𒲂ׂ�
        Index3D index = CalcWorldIndex(position);
        //�`�����N���Ȃ��̂ō��Ȃ�
        if (!ChunkManagerScript.chunks.ContainsKey(index)) return null;
        //�`�����N���擾
        ChunkScript chunk = ChunkManagerScript.chunks[index];
        //���[�J�����W���v�Z
        Vector3 localPosition = (chunk.transform.position - position + new Vector3(chunkSize / 2, chunkSize / 2, chunkSize / 2)) / boxSize;
        //���[�J���C���f�b�N�X���v�Z
        Index3D localIndex = new Index3D
            (
                (int)(Mathf.Floor(localPosition.x / boxSize)),
                (int)(Mathf.Floor(localPosition.y / boxSize)),
                (int)(Mathf.Floor(localPosition.z / boxSize))
            );
        //�͈͊O�Ȃ���Ȃ�
        if (!localIndex.IsFitIntoRange(0, chunkSize - 1)) return null;
        //�`�����N���ɐ����s�Ȃ琶�����Ȃ�
        if (!chunk.CanSpawnBox(localIndex)) return null;

        //�����܂ŗ��������
        GameObject box = BoxBase.Create(chunk, prefab, position, rotation);
        //�`�����N�ɒǉ�
        chunk.chunkData[localIndex.x, localIndex.y, localIndex.z] = new BoxData(box, box.GetComponent<BoxBase>());
        //�`�����N��e��
        box.transform.parent = chunk.transform;

        return box;
    }

    //�`�����N�̈�ӂ̃T�C�Y
    private const int chunkSize = 64;
    //Box�̃T�C�Y
    private const float boxSize = 1;
    //������������
    private const float far = 1f;
    //�m�C�Y�𑜓x�i���s�j
    private const float mapResolutionHorizontal = 50.0f;
    //�m�C�Y�𑜓x�i�����j
    private const float mapResolutionVertical = 25.0f;

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

    private void Start()
    {
        //�`�����N����Box�f�[�^�z��
        chunkData = new BoxData[chunkSize, chunkSize, chunkSize];

        //�אڃ`�����N
        affiliationChunks ??= new ChunkScript[3, 3, 3];

        //�`�����N�T�C�Y
        size = new Vector3(chunkSize, chunkSize, chunkSize);
        //���S���W
        center = transform.position;

        //�R���C�_�[�̃T�C�Y���Z�b�g
        GetComponent<BoxCollider>().size = size;

        //�n�\�`�����N�ł���Βn�`�𐶐�
        if (worldIndex.y == 0)
        {
            //�n�`����
            StartCoroutine(GenerateTerrain());
        }
        else
        {
            //�n�\�`�����N�łȂ��ꍇ�n�`���������t���O�����Ă���
            IsTerrainGenerateCompleted = true;
        }

        //�߂��̃`�����N�𐶐�
        StartCoroutine(GenerateChunkIfNeeded());

        //�P�O�b������Player����̋��������ė��ꂷ���Ă�����폜����
        InvokeRepeating(nameof(KillTimerSetIfNeeded), 0, 10);
    }

    private void OnDestroy()
    {
        //�`�����N�f�[�^�̏�����
        for (int x = 0; x < chunkData.GetLength(0); x++)
        {
            for (int y = 0; y < chunkData.GetLength(1); y++)
            {
                for (int z = 0; z < chunkData.GetLength(2); z++)
                {
                    chunkData[x, y, z] = null;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Player�󂵂��󂯕t���Ȃ�
        if (other.name != "Player") return;
        //�������g��Player�̈ړ��ʒm
        PlayerMoveNotification();

        //�S���ʕ����[�v
        for (int direction = (int)Direction.First; direction <= (int)Direction.Max; direction++)
        {
            //�אڃ`�����N�̍��W�����߂�
            int x = DirectionOffset[direction][X] + DirectionOffsetCenter[X];
            int y = DirectionOffset[direction][Y] + DirectionOffsetCenter[Y];
            int z = DirectionOffset[direction][Z] + DirectionOffsetCenter[Z];

            //�`�����N���Ȃ�������
            if (affiliationChunks?[x, y, z] is null)
            {
                Vector3 offset = new Vector3(DirectionOffset[direction][X], DirectionOffset[direction][Y], DirectionOffset[direction][Z]);
                offset *= chunkSize;
                Index3D index3D = CalcWorldIndex(center + offset);
                //�S�`�����N�f�[�^����T��
                if (ChunkManagerScript.chunks.ContainsKey(index3D))
                {
                    affiliationChunks[x, y, z] = ChunkManagerScript.chunks[index3D];
                }
            }
            //�ړ��ʒm
            affiliationChunks?[x, y, z]?.PlayerMoveNotification();
        }
    }

    /// <summary>
    /// Box���w�肳�ꂽ�R�����C���f�b�N�X�̏ꏊ�ɐ����ł��邩�ǂ���
    /// </summary>
    /// <param name="index">Box�𐶐��������ꏊ</param>
    /// <returns></returns>
    public bool CanSpawnBox(Index3D index)
    {
        return index.IsFitIntoRange(0,chunkSize) && (chunkData[index.x, index.y, index.z] == null);
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

    /// <summary>
    /// �K�v�ɉ����Ď��ӂɃ`�����N�𐶐����ĕێ�����
    /// ������Player�̈ړ��ʒm���o��
    /// </summary>
    private IEnumerator GenerateChunkIfNeeded()
    {
        //Player�Ƃ̋������m���߂�
        float playerDistanceMax = chunkSize * far;

        for (int direction = (int)Direction.First; direction <= (int)Direction.Max; direction++)
        {
            int x = DirectionOffset[direction][X] + DirectionOffsetCenter[X];
            int y = DirectionOffset[direction][Y] + DirectionOffsetCenter[Y];
            int z = DirectionOffset[direction][Z] + DirectionOffsetCenter[Z];

            //�`�����N���Ȃ�������
            if (affiliationChunks?[x, y, z] is null)
            {
                Vector3 offset = new Vector3(DirectionOffset[direction][X], DirectionOffset[direction][Y], DirectionOffset[direction][Z]);
                offset *= chunkSize;
                Index3D index3D = CalcWorldIndex(center + offset);
                //�S�`�����N�f�[�^����T��
                if (ChunkManagerScript.chunks.ContainsKey(index3D))
                {
                    //����������ێ�
                    affiliationChunks[x, y, z] = ChunkManagerScript.chunks[index3D];
                }
                else
                {
                    //����������W���v�Z
                    Vector3 position = new Vector3(
                        DirectionOffset[direction][Z] * chunkSize,
                        DirectionOffset[direction][Y] * chunkSize,
                        DirectionOffset[direction][Z] * chunkSize);
                    //������̂����␳
                    position += center;
                    //Player���痣��߂��Ă����琶�����Ȃ�
                    if (Vector3.Distance(position, Camera.main.transform.position) <= playerDistanceMax)
                    {
                        //�����L���[�֒ǉ�
                        ChunkManagerScript.CreateOrder(position);
                        //��������������܂ő҂�
                        while (true)
                        {
                            //��������Ă����Ƃ�
                            if (ChunkManagerScript.chunks.ContainsKey(index3D))
                            {
                                affiliationChunks[x, y, z] = ChunkManagerScript.chunks[index3D];
                                break;
                            }
                            //�������s���Ă����Ƃ�
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

    /// <summary>
    /// Player���`�����N���ړ������Ƃ��ɌĂ΂��
    /// </summary>
    private void PlayerMoveNotification()
    {
        //�L���^�C�}�[�������Ă�����
        if (IsKillTimerSet)
        {
            //�폜�^�C�}�[���L�����Z��
            Debug.Log("Kill Timer Cancel: " + worldIndex.ToString());
            CancelInvoke(nameof(DestroyThis));
            IsKillTimerSet = false;
        }
        //�K�v�ɉ����ă`�����N�̐����Ƌߗ׃`�����N�ֈړ��ʒm
        StartCoroutine(GenerateChunkIfNeeded());
    }

    /// <summary>
    /// �`�����N�̒��S���W��Player��
    /// chunkSize * far����Ă����ꍇ
    /// �L���^�C�}�[������
    /// </summary>
    private void KillTimerSetIfNeeded()
    {
        //�L���^�C�}�[�������Ă���Ƃ��͏������Ȃ�
        if (IsKillTimerSet) return;

        //Player�Ƃ̋������m���߂�
        float playerDistanceMax = chunkSize * far;

        //Player�Ɨ��ꂷ���Ă���
        if (Vector3.Distance(Camera.main.transform.position, transform.position) >= playerDistanceMax)
        {
            //�폜�\��
            Debug.Log("Kill Timer Begin: " + worldIndex.ToString());
            Invoke(nameof(DestroyThis), 10);
            IsKillTimerSet = true;
        }
    }

    /// <summary>
    /// �������폜
    /// �폜�ƈꏏ�Ƀf�[�^�x�[�X���������
    /// �Q�ƃG���[�΍��p
    /// </summary>
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
        x += DirectionOffset[(int)direction][X];
        y += DirectionOffset[(int)direction][Y];
        z += DirectionOffset[(int)direction][Z];

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
        x += DirectionOffset[(int)direction][X];
        y += DirectionOffset[(int)direction][Y];
        z += DirectionOffset[(int)direction][Z];

        //�C���f�b�N�X�O�ł���΃`�����N�O�Ȃ̂�Box�����݂��锻��
        if (x >= chunkSize || x < 0) return true;
        if (y >= chunkSize || y < 0) return true;
        if (z >= chunkSize || z < 0) return true;

        //Box�����݂��Ă��邩��Ԃ�
        return chunkData?[x, y, z] != null;
    }
}
