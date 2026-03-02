using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Spawner : DucMonobehaviour
{
    [Header("Spawner")]
    [SerializeField] protected Transform holder;

    [SerializeField] protected int spawnedCount = 0;
    public int SpawnedCount => spawnedCount;

    [SerializeField] protected List<Transform> prefabs;
    [SerializeField] protected List<Transform> poolObjs;

    protected override void Awake()
    {
        base.Awake();
        this.LoadComponents();
    }
    protected virtual void Reset()
    {
        this.LoadComponents();
    }

    protected override void OnEnable()
    {
        LoadComponents();
    }
    protected void LoadComponents()
    {
        this.LoadPrefabs();
        this.LoadHolder();
    }

    protected virtual void LoadHolder()
    {
        if (this.holder != null) return;

        this.holder = transform.Find("Holder");

        if (this.holder == null)
        {
            Debug.LogWarning(transform.name + ": KHÔNG tìm thấy con trực tiếp tên 'Holder'", gameObject);
        }
        else
        {
            Debug.Log(transform.name + ": LoadHolder thành công", gameObject);
        }
    }

    //Load Prefabs 
    protected virtual void LoadPrefabs()
    {
        // Đảm bảo list đã được khởi tạo để tránh lỗi NullReferenceException
        if (this.prefabs == null)
        {
            this.prefabs = new List<Transform>();
        }

        if (this.prefabs.Count > 0) return;

        Transform prefabObj = transform.Find("Prefabs");

        // CHECK NULL Ở ĐÂY LÀ QUAN TRỌNG NHẤT
        if (prefabObj == null)
        {
            Debug.LogError(transform.name + ": KHÔNG tìm thấy con trực tiếp tên 'Prefabs'. Hãy kiểm tra lại Hierarchy!", gameObject);
            return; // Dừng chạy hàm này để không bị văng lỗi ở foreach
        }

        foreach (Transform prefab in prefabObj)
        {
            this.prefabs.Add(prefab);
        }

        this.HidePrefabs(); // Đảm bảo hàm này của bạn cũng không bị lỗi nhé
        Debug.Log(transform.name + ": LoadPrefabs thành công", gameObject);
    }
    protected virtual void HidePrefabs()
    {
        foreach (Transform prefab in this.prefabs)
        {
            prefab.gameObject.SetActive(false);
        }
    }
    //Spawn object
    public virtual Transform Spawn(string prefabName, Vector3 spawnPos, Quaternion rotation)
    {
        Debug.Log(prefabName);
        Transform prefab = GetPrefabByName(prefabName);
        if (prefab == null)
        {
            Debug.LogWarning("Prefab was not found: " + prefab);
            return null;
        }
        return this.Spawn(prefab, spawnPos, rotation);
    }
    public virtual Transform Spawn(Transform prefab, Vector3 spawnPos, Quaternion rotation)
    {
        Transform newPrefab = this.GetObjectFromPool(prefab);
        newPrefab.SetPositionAndRotation(spawnPos, rotation);
        newPrefab.parent = this.holder;
        this.spawnedCount++;
        return newPrefab;
    }
    //Pooling Object
    protected virtual Transform GetObjectFromPool(Transform prefab)
    {
        foreach (Transform poolObj in this.poolObjs)
        {
            if (poolObj.name == prefab.name)
            {
                this.poolObjs.Remove(poolObj);
                return poolObj;
            }
        }

        Transform newPrefab = Instantiate(prefab);
        newPrefab.name = prefab.name;
        return newPrefab;
    }

    //Sau khi Object bi Destroy se vao pool
    public virtual void Despawn(Transform obj)
    {
        this.poolObjs.Add(obj);
        obj.gameObject.SetActive(false);
        this.spawnedCount--;
    }
    //Tim vien dan duoc goi
    public virtual Transform GetPrefabByName(string prefabName)
    {
        string cleanPrefabName = prefabName.Trim();

        foreach (Transform prefab in this.prefabs)
        {
            if (prefab.name.Contains(cleanPrefabName))
            {
                return prefab;
            }
        }

        Debug.LogWarning("Không tìm thấy Prefab nào chứa tên: [" + cleanPrefabName + "]");
        return null;
    }
    public virtual Transform RandomPrefab()
    {
        int rand = Random.Range(0, this.prefabs.Count);
        return this.prefabs[rand];
    }
}
