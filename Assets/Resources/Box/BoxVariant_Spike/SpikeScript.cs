using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeScript : BoxBase
{
    void Start()
    {
        InitializeBox();
        canRotate = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        EXAttackableObject.AttackNotification(gameObject, other.gameObject);
    }

    //Implementation from Interface:IItemizeObject
    public override MeshFilter GetMeshFilter()
    {
        return gameObject.transform.Find("Mesh")?.GetComponent<MeshFilter>();
    }

    //Implementation from Interface:IItemizeObject
    public override MeshRenderer GetMeshRenderer()
    {
        return gameObject.transform.Find("Mesh")?.GetComponent<MeshRenderer>();
    }
}
