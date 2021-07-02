using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アイテム化可能なオブジェクト
/// </summary>
public interface IItemizeObject : IMeshAccessor
{
    public InventoryItem GetItemData();
    public Vector3 GetMeshScale();
}
