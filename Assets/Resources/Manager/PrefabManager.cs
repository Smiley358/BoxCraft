using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PrefabData
{
    [SerializeField] public string Name;
    [SerializeField] public GameObject Prefab;

    public PrefabData(string name,GameObject prefab)
    {
        Name = name;
        Prefab = prefab;
    }
}

public class PrefabManager  :MonoBehaviour
{
    [SerializeField] private List<PrefabData> prefabData;
    [SerializeField] private List<GameObject> prefabs;

    public static PrefabManager Instance { get; private set; }

    private void Awake()
    {
        //‰Šú‰»
        Instance = this;
    }

    public GameObject GetPrefab(string prefabName)
    {
        //PrefabData containt = prefabData.Find(flagment => flagment.Name == prefabName);
        //return containt?.Prefab;
        GameObject prefab = prefabs.Find(flagment => flagment.name == prefabName);
        return prefab;
    }
}
