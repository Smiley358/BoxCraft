using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �A�C�e�����\�ȃI�u�W�F�N�g
/// </summary>
public interface IItemizeObject
{
    InventoryItem GetItemData();
    Mesh GetMesh();
    Material GetMaterial();
    Vector3 GetMeshScale();
}
