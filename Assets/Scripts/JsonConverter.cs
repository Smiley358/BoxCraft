using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace JsonConverter
{
    namespace ChunkJsonConverter
    {
        /// <summary>
        /// Box�̃f�[�^��Json�ŕۑ����邽�߂̃f�[�^
        /// </summary>
        [Serializable] public class BoxSaveData
        {
            public string Name;
            public ChunkScript.Index3D Index;
            public Quaternion Rotation;

            public BoxSaveData(string name, ChunkScript.Index3D index, Quaternion rotation)
            {
                Name = name;
                Index = index;
                Rotation = rotation;
            }

            public override bool Equals(object obj)
            {
                return obj is BoxSaveData data && Index == data.Index;
            }

            public override int GetHashCode()
            {
                return -2134847229 + Index.GetHashCode();
            }

            public static bool operator ==(BoxSaveData left, BoxSaveData right)
            {
                return left?.Index == right?.Index;
            }

            public static bool operator !=(BoxSaveData left, BoxSaveData right)
            {
                return left?.Index != right?.Index;
            }
        }

        /// <summary>
        /// �`�����N�̕ύX�_���Z�[�u���邽�߂̃f�[�^
        /// </summary>
        [Serializable] public class ChunkSaveData
        {
            public ChunkScript.Index3D Index;
            public List<BoxSaveData> Changes;

            public ChunkSaveData(ChunkScript.Index3D index, List<BoxSaveData> changes)
            {
                Index = index;
                Changes = changes;
            }

            public override bool Equals(object obj)
            {
                return obj is ChunkSaveData data && Index == data.Index;
            }

            public override int GetHashCode()
            {
                return -2134847229 + Index.GetHashCode();
            }

            public static bool operator ==(ChunkSaveData left, ChunkSaveData right)
            {
                return left?.Index == right?.Index;
            }

            public static bool operator !=(ChunkSaveData left, ChunkSaveData right)
            {
                return left?.Index != right?.Index;
            }
        }


        /// <summary>
        /// �f�[�^��Json�ɂ��ăZ�[�u�E���[�h���s��
        /// </summary>
        public static class Converter
        {
            [Serializable] class SaveData
            {
                public List<ChunkSaveData> Chunks;

                public SaveData(List<ChunkSaveData> chunks)
                {
                    Chunks = chunks;
                }
            }

            //�Z�[�u�f�[�^��
            public const string SaveFileName = "Save_Chunk.json";

            /// <summary>
            /// �Z�[�u�f�[�^���X�V
            /// </summary>
            /// <param name="index">�`�����N�̂RD�C���f�b�N�X</param>
            /// <param name="changes">�`�����N���̕ύX�_���X�g</param>
            public static void UpdateSavedata(ChunkScript.Index3D index, List<BoxSaveData> changes)
            {
                //�ύX�_���Ȃ���Εۑ����Ȃ�
                if (changes.Count <= 0) return;

                //�Z�[�u�f�[�^�p�̃`�����N�f�[�^�쐬
                ChunkSaveData chunkSaveData = new ChunkSaveData(index, changes);

                //�p�X
                string path = Application.dataPath + "/" + SaveFileName;
                //�t�@�C�����Ȃ�������
                if (!File.Exists(path))
                {
                    //�t�@�C�������
                    File.Create(path).Close();
                }
                //Json������
                string readJson;
                //StreamReader�쐬
                using (StreamReader reader = new StreamReader(path))
                {
                    //Json������𐶐�
                    readJson = reader.ReadToEnd();
                }

                //List�ɕϊ��AList���ł��Ȃ���΃f�[�^����Ȃ̂ō��
                List<ChunkSaveData> chunks = JsonUtility.FromJson<SaveData>(readJson)?.Chunks ?? 
                    new SaveData(new List<ChunkSaveData>()).Chunks;

                //�`�����N�f�[�^�̎擾
                ChunkSaveData chunk = chunks.Find(data => data.Index == chunkSaveData.Index);
                //�f�[�^������΃��X�g����폜
                chunks.Remove(chunk);
                //�ǉ�
                chunks.Add(chunkSaveData);

                //�Z�[�u�f�[�^�p�̃f�[�^�N���X�̍쐬
                SaveData save = new SaveData(chunks);
                //�������ݗpJson������𐶐�
                string writeSaveDataJsonString = JsonUtility.ToJson(save, true);
                //StreamWriter�쐬�A�㏑���ݒ�
                using (StreamWriter writer = new StreamWriter(path, false))
                {
                    //��������ŕ���
                    writer.WriteLine(writeSaveDataJsonString);
                    writer.Flush();
                }
            }

            /// <summary>
            /// �Z�[�u�f�[�^�̓ǂݍ���
            /// </summary>
            /// <param name="index">�`�����N�̂RD�C���f�b�N�X</param>
            /// <returns></returns>
            public static ChunkSaveData LoadSaveData(ChunkScript.Index3D index)
            {
                //�p�X
                string path = Application.dataPath + "/" + SaveFileName;
                //�t�@�C�����Ȃ�������
                if (!File.Exists(path))
                {
                    //���[�h���Ȃ�
                    return null;
                }

                //FileInfo�쐬
                FileInfo info = new FileInfo(path);
                //Json������
                string json;
                //StreamReader�쐬
                using (StreamReader reader = new StreamReader(info.OpenRead()))
                {
                    //Json������𐶐�
                    json = reader.ReadToEnd();
                }
                //List�ɕϊ�
                List<ChunkSaveData> chunks = JsonUtility.FromJson<SaveData>(json)?.Chunks ?? 
                    new SaveData(new List<ChunkSaveData>()).Chunks;

                //�`�����N�f�[�^�̎擾
                ChunkSaveData chunk = chunks.Find(data => data.Index == index);

                return chunk;
            }
        }
    }
}