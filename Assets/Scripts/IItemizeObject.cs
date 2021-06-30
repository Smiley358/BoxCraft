using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アイテム化可能なオブジェクト
/// </summary>
public interface IItemizeObject
{
    InventoryItem GetItemData();
    Mesh GetMesh();
    Material GetMaterial();
    Vector3 GetMeshScale();
}
