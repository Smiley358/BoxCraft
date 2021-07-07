using System;
using System.Collections;
using System.Collections.Generic;
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
            (Math.Abs(distance.y) <= far/3) &&
            (Math.Abs(distance.z) <= far))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Box�����݂��邩�m�F����
    /// �쐬����Ă��Ȃ��ꍇ�͎��������f�[�^���m�F��
    /// �������������͂��̉ӏ��ł����True��Ԃ�
    /// </summary>
    /// <param name="index">�m�F�������C���f�b�N�X</param>
    /// <returns>���݂������͎������������ꍇ��True</returns>
    private bool IsBoxExist(Index3D index)
    {
        //���Y�C���f�b�N�X�̃{�b�N�X�f�[�^���擾
        BoxData boxData = boxDatas?[index.x, index.y, index.z];
        //
        bool isExist = boxData != null;
        //���݂��Ȃ������琶������Ă��Ȃ������̉\��������
        if (!isExist)
        {
            //�ύX�_�擾
            var change = changes.Find(data => data.Index == index);
            //prefab��
            string prefabName = null;

            //�ύX�Ȃ�
            if (change == null)
            {
                //�n�`�����f�[�^����擾
                prefabName = boxGenerateData?[index.x, index.y, index.z];
            }
            //�ύX�L
            else
            {
                //�ύX�f�[�^����擾
                prefabName = change.Name;
            }

            //�폜�ɂ��ύX
            if (string.IsNullOrEmpty(prefabName))
            {
                //���݂��Ȃ�
                //isExist = false;
            }
            //�ǉ��ɂ��ύX
            else
            {
                //���݂���
                isExist = true;
            }
        }

        return isExist;
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
}
