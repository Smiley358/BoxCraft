using System;
using System.Collections;
using System.Collections.Generic;
using JsonConverter.ChunkJsonConverter;
using UnityEngine;

public partial class ChunkScript
{
    /// <summary>
    /// Player����̍ő勗�����ɂ��邩�ǂ���
    /// </summary>
    /// <param name="index">�m�F�������C���f�b�N�X</param>
    /// <returns>�͈͓��Ȃ�True</returns>
    private bool IsFitIntoFar(Index3D index)
    {
        //index����player�܂ł�XYZ�̊e����
        Index3D distance = PlayerIndex - index;
        //�S��far���߂���΂���
        if ((Math.Abs(distance.x) <= far) &&
            //((distance.y <= (0 - far / 2)) && (Math.Abs(distance.y) <= far + far / 2)) &&
            (Math.Abs(distance.y) <= far) &&
            (Math.Abs(distance.z) <= far))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Box���w�肳�ꂽ�R�����C���f�b�N�X�̏ꏊ�ɐ����ł��邩�ǂ���
    /// </summary>
    /// <param name="index">Box�𐶐��������ꏊ</param>
    /// <returns></returns>
    public bool CanSpawnBox(Index3D index)
    {
        return index.IsFitIntoRange(0, chunkSize - 1) && (boxDatas[index.x, index.y, index.z] == null);
    }

    /// <summary>
    /// Box�̃`�����N���ł̃C���f�b�N�X��
    /// ���[���h���W�ɕϊ�
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Vector3 CalcWorldPositionFromBoxLocalIndex(Index3D index)
    {
        //���W
        Vector3 position = new Vector3(index.x, index.y, index.z);
        //�C���f�b�N�X���W���烍�[�J�����W��
        position -= new Vector3(chunkSize / 2f, chunkSize / 2f, chunkSize / 2f);
        //���W��Box�̒��S��
        position += new Vector3(boxSize / 2f, boxSize / 2f, boxSize / 2f);
        //���[�J�����W���烏�[���h���W��
        position += center;

        return position;
    }

    /// <summary>
    /// Box�̍��W���`�����N���ł̃��[�J���C���f�b�N�X�ɕϊ�
    /// </summary>
    /// <param name="position">�ϊ�������W</param>
    /// <param name="chunkCenter">�`�����N�̒��S���W</param>
    /// <returns>�`�����N���ł̃��[�J���C���f�b�N�X</returns>
    public Index3D CalcLocalIndexFromBoxWorldPosition(Vector3 position)
    {
        //���[�J�����W�̌v�Z
        Vector3 localPosition = new Vector3(
            position.x + (chunkSize / 2),
            position.y + (chunkSize / 2),
            position.z + (chunkSize / 2)) - center;

        return new Index3D(
            (int)Mathf.Floor(localPosition.x),
            (int)Mathf.Floor(localPosition.y),
            (int)Mathf.Floor(localPosition.z));
    }

    /// <summary>
    /// �p�[�����m�C�Y���g�p����Y�̍������v�Z����
    /// </summary>
    /// <param name="x">Y�̍��������߂���X���W</param>
    /// <param name="z">Y�̍��������߂���Z���W</param>
    /// <returns>Y���W</returns>
    public int CalcNoiseHeight(float x,float z)
    {
        UnityEngine.Random.InitState(seed);
        int addCount = 2;
        //�m�C�Y����
        float noise = 0;
        for (int i = 0; i < addCount; i++)
        {
            float random = UnityEngine.Random.value;
            float randomScale = UnityEngine.Random.RandomRange(0.5f, 1.5f);
            noise += Mathf.PerlinNoise(random + x * mapScaleHorizontal * randomScale, random + z * mapScaleHorizontal / randomScale);
        }
        noise /= addCount;
        //�n�`�̈�ԏ�̈ʒu
        int top = (int)Mathf.Round(mapResolutionVertical * noise);
        top -= chunkSize / 2;

        return top;
    }
}
