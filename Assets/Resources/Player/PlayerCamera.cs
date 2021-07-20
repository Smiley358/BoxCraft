using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public static PlayerCamera Instance { get; private set; }
    public static Plane[] FrustumPlanes { get; private set; }
    //ƒJƒƒ‰‚Ì…’†”»’è
    public bool IsWaterFilter { get; private set; }


    private void Awake()
    {
        Instance = this;
        FrustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
    }

    private void Start()
    {
        //Application.targetFrameRate = 144;
    }

    private void LateUpdate()
    {
        var boxName = ChunkScript.GetPrefabName(transform.position);

        if (boxName == "BoxVariant_Water")
        {
            IsWaterFilter = true;
        }
        else
        {
            IsWaterFilter = false;
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        FrustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
#endif
    }
}
