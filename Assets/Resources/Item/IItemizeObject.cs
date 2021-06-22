using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アイテム化可能なオブジェクト
/// </summary>
public interface IItemizeObject
{
    InventoryItem GetItemData();
    MeshFilter GetMeshFilter();
    MeshRenderer GetMeshRenderer();
}
