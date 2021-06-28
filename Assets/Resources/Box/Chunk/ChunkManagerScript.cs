using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManagerScript : MonoBehaviour
{
    public static ChunkManagerScript Instance { get; private set; }

    void Awake()
    {
        Instance = this;

        //�`�����N�̐����L���[
        chunkCreateOrder = new Queue<Vector3>();

        //�`�����N�ꗗ
        chunks =  new Dictionary<ChunkScript.Index3D, ChunkScript>();

        //�`�����N�������s�f�[�^
        createFailedList = new List<ChunkScript.Index3D>();
    }

    void Start()
    {
        //�ŏ��̈�����
        CreateOrder(new Vector3(0, 0, 0));
    }

    //�S�`�����N�f�[�^
    public static Dictionary<ChunkScript.Index3D, ChunkScript> chunks { get; private set; }
    private static List<ChunkScript.Index3D> createFailedList;
    //�`�����N�̐����X�^�b�N
    private static Queue<Vector3> chunkCreateOrder;
    //�`�����N�����t���O
    private static bool isCreate;

    public static void CreateOrder(Vector3 position)
    {
        //�v�b�V��
        chunkCreateOrder.Enqueue(position);

        //���Ԃɍ��
        CreateToOrder();
    }

    public static void ForceDestroyChunk(GameObject chunk)
    {
        //�`�����N�f�[�^���ꗗ�������
        chunks.Remove(chunk.GetComponent<ChunkScript>().worldIndex);
        //�`�����N���폜
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
        //�������t���O������Ă�����
        if (isCreate is false)
        {
            //�L���[���󂾂����牽�����Ȃ�
            if (chunkCreateOrder.Count > 0)
            {
                //���ɐ�������L���[�̎擾
                Vector3 position = chunkCreateOrder.Dequeue();

                Instance.StartCoroutine(Create(position));
            }
        }
    }

    private static IEnumerator Create(Vector3 position)
    {
        isCreate = true;


        //�`�����N����
        GameObject chunk = ChunkScript.Create(position);

        if (chunk is null)
        {
            //�����t���O������
            isCreate = false;
            //���s�������X�g�ɓ����
            createFailedList.Add(ChunkScript.CalcWorldIndex(position));
            //���̃L���[���s��
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

        //�����t���O������
        isCreate = false;

        //���̃L���[���s��
        CreateToOrder();
    }
}
