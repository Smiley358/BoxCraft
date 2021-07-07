using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ChunkScript
{
    /// <summary>
    /// Playerからの最大距離内にいるかどうか
    /// </summary>
    /// <param name="index">確認したいインデックス</param>
    /// <returns>範囲内ならTrue</returns>
    private bool IsFitIntoFar(Index3D index)
    {
        //indexからplayerまでのXYZの各距離
        Index3D distance = PlayerIndex - index;
        //全てfarより近ければいい
        if ((Math.Abs(distance.x) <= far) &&
            (Math.Abs(distance.y) <= far/3) &&
            (Math.Abs(distance.z) <= far))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Boxが存在するか確認する
    /// 作成されていない場合は自動生成データを確認し
    /// 自動生成されるはずの箇所であればTrueを返す
    /// </summary>
    /// <param name="index">確認したいインデックス</param>
    /// <returns>存在もしくは自動生成される場合はTrue</returns>
    private bool IsBoxExist(Index3D index)
    {
        //当該インデックスのボックスデータを取得
        BoxData boxData = boxDatas?[index.x, index.y, index.z];
        //
        bool isExist = boxData != null;
        //存在しなかったら生成されていないだけの可能性がある
        if (!isExist)
        {
            //変更点取得
            var change = changes.Find(data => data.Index == index);
            //prefab名
            string prefabName = null;

            //変更なし
            if (change == null)
            {
                //地形生成データから取得
                prefabName = boxGenerateData?[index.x, index.y, index.z];
            }
            //変更有
            else
            {
                //変更データから取得
                prefabName = change.Name;
            }

            //削除による変更
            if (string.IsNullOrEmpty(prefabName))
            {
                //存在しない
                //isExist = false;
            }
            //追加による変更
            else
            {
                //存在する
                isExist = true;
            }
        }

        return isExist;
    }

    /// <summary>
    /// Boxを指定された３次元インデックスの場所に生成できるかどうか
    /// </summary>
    /// <param name="index">Boxを生成したい場所</param>
    /// <returns></returns>
    public bool CanSpawnBox(Index3D index)
    {
        return index.IsFitIntoRange(0, chunkSize - 1) && (boxDatas[index.x, index.y, index.z] == null);
    }

    /// <summary>
    /// Boxのチャンク内でのインデックスを
    /// ワールド座標に変換
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Vector3 CalcWorldPositionFromBoxLocalIndex(Index3D index)
    {
        //座標
        Vector3 position = new Vector3(index.x, index.y, index.z);
        //インデックス座標からローカル座標へ
        position -= new Vector3(chunkSize / 2f, chunkSize / 2f, chunkSize / 2f);
        //座標をBoxの中心へ
        position += new Vector3(boxSize / 2f, boxSize / 2f, boxSize / 2f);
        //ローカル座標からワールド座標へ
        position += center;

        return position;
    }

    /// <summary>
    /// Boxの座標をチャンク内でのローカルインデックスに変換
    /// </summary>
    /// <param name="position">変換する座標</param>
    /// <param name="chunkCenter">チャンクの中心座標</param>
    /// <returns>チャンク内でのローカルインデックス</returns>
    public Index3D CalcLocalIndexFromBoxWorldPosition(Vector3 position)
    {
        //ローカル座標の計算
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
