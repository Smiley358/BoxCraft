using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �A�C�e�����\�ȃI�u�W�F�N�g
/// </summary>
public interface IItemizeObject : IMeshAccessor
{
    public InventoryItem GetItemData();
    public Vector3 GetMeshScale();
}
