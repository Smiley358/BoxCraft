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

    //Implementation from Interface:IAttackableObject
    public override int GetAttackPower()
    {
        return 3;
    }

    //Implementation from Interface:IAttackableObject
    public override float GetKnockBack()
    {
        return 20f;
    }

    //Implementation from Interface:IItemizeObject
    public override Mesh GetMesh()
    {
        return gameObject.transform.Find("Mesh")?.GetComponent<MeshFilter>().mesh;
    }

    //Implementation from Interface:IItemizeObject
    public override Material GetMaterial()
    {
        return gameObject.transform.Find("Mesh")?.GetComponent<MeshRenderer>().material;
    }

    //Implementation from Interface:IItemizeObject
    public override Vector3 GetMeshScale()
    {
        return gameObject.transform.Find("Mesh")?.transform.localScale ?? Vector3.one;
    }
}
