using System;
using System.Collections;
using System.Collections.Generic;
using JsonConverter.ChunkJsonConverter;
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
