using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManagerScript : MonoBehaviour
{
    //�����̃C���X�^���X
    public static ChunkManagerScript Instance { get; private set; }

    void Awake()
    {
        //�C���X�^���X����
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

    private void Update()
    {
        if (!IsCompleted)
        {
            CreateToOrder();
        }
    }

    //�`�����N���������t���O
    public static bool IsCompleted { get; private set; }
    //�S�`�����N�f�[�^
    public static Dictionary<ChunkScript.Index3D, ChunkScript> chunks { get; private set; }
    //�쐬���s�`�����N�̃I�t�Z�b�g���X�g
    private static List<ChunkScript.Index3D> createFailedList;
    //�`�����N�̐����X�^�b�N
    private static Queue<Vector3> chunkCreateOrder;
    //�`�����N�������t���O
    private static bool isCreate;

    /// <summary>
    /// �`�����N�̐����L���[�ɒǉ�����
    /// </summary>
    /// <param name="position">�������W</param>
    public static void CreateOrder(Vector3 position)
    {
        //�v�b�V��
        chunkCreateOrder.Enqueue(position);
        IsCompleted = false;
    }

    /// <summary>
    /// �`�����N������
    /// �Q�ƃG���[�΍�Ƃ��ăf�[�^�x�[�X���������
    /// </summary>
    /// <param name="chunk"></param>
    public static void ForceDestroyChunk(GameObject chunk)
    {
        if (chunk == null) return;

        //�`�����N�f�[�^���ꗗ�������
        chunks.Remove(chunk.GetComponent<ChunkScript>().WorldIndex);
        //�`�����N���폜
        Destroy(chunk);
    }

    /// <summary>
    /// �����Ƃ��ēn���ꂽ���W�̃`�����N���������s���Ă��邩�m�F����
    /// </summary>
    /// <param name="index">���s�m�F���������W</param>
    /// <returns>���s���Ă����true</returns>
    public static bool IsCreateFailed(ChunkScript.Index3D index)
    {
        if (createFailedList.Contains(index))
        {
            createFailedList.Remove(index);
            return true;
        }
        return false;
    }

    /// <summary>
    /// ���ԂɃ`�����N�����
    /// </summary>
    private static void CreateToOrder()
    {
        //�������t���O������Ă�����
        if (isCreate is false)
        {
            //�L���[���󂶂�Ȃ��Ƃ�
            if (chunkCreateOrder.Count > 0)
            {
                //���ɐ�������L���[�̎擾
                Vector3 position = chunkCreateOrder.Dequeue();

                //�����J�n
                Instance.StartCoroutine(Create(position));
            }
            else
            {
                //��������
                IsCompleted = true;
            }
        }
    }

    /// <summary>
    /// �`�����N�����
    /// �n�`�̐������I���܂ő҂�
    /// </summary>
    /// <param name="position">�������W</param>
    private static IEnumerator Create(Vector3 position)
    {
        //�������t���O���Ă�
        isCreate = true;

        //�`�����N����
        GameObject chunk = ChunkScript.Create(position);

        //����ĂȂ������琶�����s���X�g�ɓ����
        if (chunk == null)
        {
            //�����t���O������
            isCreate = false;
            //���s�������X�g�ɓ����
            createFailedList.Add(ChunkScript.CalcWorldIndex(position));
            //���f
            yield break;
        }

        //�X�N���v�g�擾
        ChunkScript script = chunk.GetComponent<ChunkScript>();

        //�n�`�����҂�
        while (true)
        {
            if (script.IsTerrainGenerateCompleted)
            {
                //�`�����N���X�g�ɒǉ�
                chunks.Add(script.WorldIndex, script);
                break;
            }
            yield return null;
        }

        //�����t���O������
        isCreate = false;
    }
}
