using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFilterScript : MonoBehaviour
{
    //
    [SerializeField] private Texture2D filter;

    private void Awake()
    {
        filter = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        filter.SetPixel(0, 0, new Color(0 / 255f, 5 / 255f, 100 / 255f, 175 / 255f));
        filter.Apply();
    }

    private void OnGUI()
    {
        if (PlayerCamera.Instance.IsWaterFilter)
        {
            GUI.DrawTexture(Camera.main.pixelRect, filter);
        }
    }
}
