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
        /// BoxのデータをJsonで保存するためのデータ
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
        /// チャンクの変更点をセーブするためのデータ
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
        /// データをJsonにしてセーブ・ロードを行う
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

            //セーブデータ名
            public const string SaveFileName = "Save_Chunk.json";

            /// <summary>
            /// セーブデータを更新
            /// </summary>
            /// <param name="index">チャンクの３Dインデックス</param>
            /// <param name="changes">チャンク内の変更点リスト</param>
            public static void UpdateSavedata(ChunkScript.Index3D index, List<BoxSaveData> changes)
            {
                //変更点がなければ保存しない
                if (changes.Count <= 0) return;

                //セーブデータ用のチャンクデータ作成
                ChunkSaveData chunkSaveData = new ChunkSaveData(index, changes);

                //パス
                string path = Application.dataPath + "/" + SaveFileName;
                //ファイルがなかったら
                if (!File.Exists(path))
                {
                    //ファイルを作る
                    File.Create(path).Close();
                }
                //Json文字列
                string readJson;
                //StreamReader作成
                using (StreamReader reader = new StreamReader(path))
                {
                    //Json文字列を生成
                    readJson = reader.ReadToEnd();
                }

                //Listに変換、Listができなければデータが空なので作る
                List<ChunkSaveData> chunks = JsonUtility.FromJson<SaveData>(readJson)?.Chunks ?? 
                    new SaveData(new List<ChunkSaveData>()).Chunks;

                //チャンクデータの取得
                ChunkSaveData chunk = chunks.Find(data => data.Index == chunkSaveData.Index);
                //データがあればリストから削除
                chunks.Remove(chunk);
                //追加
                chunks.Add(chunkSaveData);

                //セーブデータ用のデータクラスの作成
                SaveData save = new SaveData(chunks);
                //書き込み用Json文字列を生成
                string writeSaveDataJsonString = JsonUtility.ToJson(save, true);
                //StreamWriter作成、上書き設定
                using (StreamWriter writer = new StreamWriter(path, false))
                {
                    //書き込んで閉じる
                    writer.WriteLine(writeSaveDataJsonString);
                    writer.Flush();
                }
            }

            /// <summary>
            /// セーブデータの読み込み
            /// </summary>
            /// <param name="index">チャンクの３Dインデックス</param>
            /// <returns></returns>
            public static ChunkSaveData LoadSaveData(ChunkScript.Index3D index)
            {
                //パス
                string path = Application.dataPath + "/" + SaveFileName;
                //ファイルがなかったら
                if (!File.Exists(path))
                {
                    //ロードしない
                    return null;
                }

                //FileInfo作成
                FileInfo info = new FileInfo(path);
                //Json文字列
                string json;
                //StreamReader作成
                using (StreamReader reader = new StreamReader(info.OpenRead()))
                {
                    //Json文字列を生成
                    json = reader.ReadToEnd();
                }
                //Listに変換
                List<ChunkSaveData> chunks = JsonUtility.FromJson<SaveData>(json)?.Chunks ?? 
                    new SaveData(new List<ChunkSaveData>()).Chunks;

                //チャンクデータの取得
                ChunkSaveData chunk = chunks.Find(data => data.Index == index);

                return chunk;
            }
        }
    }
}