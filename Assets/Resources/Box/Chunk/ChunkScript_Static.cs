using System;
using UnityEngine;

public partial class ChunkScript
{

    //�`�����N�̈�ӂ̃T�C�Y
    private const int chunkSize = 32;
    //Box�̃T�C�Y
    private const float boxSize = 1;
    //������������
    private const int far = 0;
    //�m�C�Y�𑜓x�i���s�j
    private const float mapScaleHorizontal = 0.007f;
    //�m�C�Y�𑜓x�i�����j
    private const int mapResolutionVertical = 75;
    //�����_���V�[�h
    private const int seed = 12345;

    //Player�̂���C���f�b�N�X
    private static Index3D PlayerIndex;

    //���ݒn�`�������̃`�����N
    private static ChunkScript nowGenerateTerrainChunk;

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
    [Serializable]
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
        public bool IsFitIntoRange(int min, int max)
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

        //TODO:Debug
        if (chunkIndex.y < 0)
        {
            return null;
        }
        if (chunkIndex.y > 1)
        {
            return null;
        }

        //�C���f�b�N�X���o�^����Ă��Ȃ��i����Ă��Ȃ��j���m�F
        if (ChunkManagerScript.chunks.ContainsKey(chunkIndex))
        {
            return null;
        }

        //prefab
        var prefab = PrefabManager.Instance.GetPrefab("Chunk");
        //�`�����N����
        GameObject chunk = Instantiate(prefab, position, Quaternion.identity);
        //�X�N���v�g�̎擾
        ChunkScript script = chunk.GetComponent<ChunkScript>();
        //�C���f�b�N�X��ݒ�
        script.WorldIndex = chunkIndex;
        //���O��ύX
        chunk.name = prefab.name + script.WorldIndex.ToString();

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
    /// Box�̃C���f�b�N�X����`�����N�ւ̃I�t�Z�b�g���v�Z
    /// �`�����N���̏ꍇ�i0,0,0�j
    /// X+�����ɂ͂ݏo���Ă���ꍇ�i1,0,0�j
    /// 2�ȏ�܂����ł���ꍇ�͕s��̂��ƂȂ̂Ŏg�p�֎~
    /// </summary>
    /// <param name="index">Box�̃C���f�b�N�X</param>
    /// <returns>�`�����N�ւ̃I�t�Z�b�g</returns>
    public static Index3D CalcChunkOffsetFromBoxIndex(Index3D index)
    {
        return new Index3D(
            Math.Sign((index.x >= 0 && chunkSize > index.x ? 0 : index.x) / (float)chunkSize),
            Math.Sign((index.y >= 0 && chunkSize > index.y ? 0 : index.y) / (float)chunkSize),
            Math.Sign((index.z >= 0 && chunkSize > index.z ? 0 : index.z) / (float)chunkSize)
            );
    }

    /// <summary>
    /// Box�𐶐����Ď����Ń`�����N�ɏ���������
    /// </summary>
    /// <param name="prefab">prefab</param>
    /// <param name="position">�������W</param>
    /// <param name="rotation">��]</param>
    /// <returns></returns>
    public static GameObject CreateBoxAndAutoBelongToChunk(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        //�܂�Box�̐����ʒu�̃`�����N�𒲂ׂ�
        Index3D index = CalcWorldIndex(position);
        //�`�����N���Ȃ��̂ō��Ȃ�
        if (!ChunkManagerScript.chunks.ContainsKey(index)) return null;
        //�`�����N���擾
        ChunkScript chunk = ChunkManagerScript.chunks[index];

        //�����\�Ȃ琶��
        GameObject box = chunk.CreateBoxAndBelongToChunk(prefab, position, rotation);
        
        return box;
    }
}
