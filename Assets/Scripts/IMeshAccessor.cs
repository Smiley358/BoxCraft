using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMeshAccessor
{
    public Mesh GetMesh();
    public Material GetMaterial();
    public MeshFilter GetMeshFilter();
    public MeshRenderer GetMeshRenderer();
}
