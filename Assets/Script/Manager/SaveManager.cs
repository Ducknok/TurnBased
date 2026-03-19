using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inventory.Model;

// --- CLASS TRUNG GIAN ĐỂ LƯU DỮ LIỆU ---
[System.Serializable]
public class EquipmentSaveData
{
    public List<string> weaponItemNames = new List<string>();
}

[System.Serializable]
public class InventorySaveData
{
    [System.Serializable]
    public struct SavedSlot
    {
        public string itemName;
        public int quantity;
    }
    public List<SavedSlot> slots = new List<SavedSlot>();
}

public class SaveManager : MonoBehaviour
{
    // --- BƯỚC 1: TẠO SINGLETON ---
    public static SaveManager Instance { get; private set; }

    [Header("Base Hero Data")]
    public List<BaseClass> characters = new List<BaseClass>();

    [Header("Weapon Equiped Data")]
    public List<AgentWeapon> heroWeapons = new List<AgentWeapon>();

    [Header("Items Data")]
    public List<ScriptableObject> itemDatabase = new List<ScriptableObject>();

    private void Awake()
    {
        // --- THIẾT LẬP SINGLETON KHI GAME BẮT ĐẦU ---
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        itemDatabase.Clear();

        itemDatabase.AddRange(Resources.LoadAll<ItemSO>("Items"));

    }

    private void Start()
    {
        LoadGame();
    }

    private string GetCharacterSavePath(BaseClass character)
    {
        if (character == null || string.IsNullOrEmpty(character.theName))
            return Application.persistentDataPath + "/unknown_hero_" + character.GetInstanceID() + "_stats.json";
        return Application.persistentDataPath + "/" + character.theName + "_stats.json";
    }

    private string GetEquipmentSavePath(AgentWeapon weaponData)
    {
        var hero = weaponData.GetComponent<HeroStateMachine>();
        string heroName = (hero != null && hero.baseHero != null) ? hero.baseHero.theName : weaponData.gameObject.name;

        return Application.persistentDataPath + "/" + heroName + "_equipment.json";
    }

    private ScriptableObject FindItemByName(string searchName)
    {
        if (string.IsNullOrEmpty(searchName)) return null;

        foreach (var item in itemDatabase)
        {
            if (item != null && item.name == searchName)
            {
                return item;
            }
        }
        return null;
    }

    public void SaveGame()
    {

        foreach (BaseClass character in characters)
        {
            if (character == null) continue;
            string json = JsonUtility.ToJson(character, true);
            File.WriteAllText(GetCharacterSavePath(character), json);
        }

        foreach (AgentWeapon weaponObj in heroWeapons)
        {
            if (weaponObj == null || weaponObj.weaponItemSO == null) continue;

            EquipmentSaveData equipData = new EquipmentSaveData();

            foreach (var item in weaponObj.weaponItemSO)
            {
                if (item != null)
                    equipData.weaponItemNames.Add(item.name);
                else
                    equipData.weaponItemNames.Add("");
            }

            string json = JsonUtility.ToJson(equipData, true);
            File.WriteAllText(GetEquipmentSavePath(weaponObj), json);
        }

    }

    public void LoadGame()
    {
        foreach (BaseClass character in characters)
        {
            if (character == null) continue;
            string savePath = GetCharacterSavePath(character);
            if (File.Exists(savePath))
            {
                JsonUtility.FromJsonOverwrite(File.ReadAllText(savePath), character);
            }
        }

        foreach (AgentWeapon weaponObj in heroWeapons)
        {
            if (weaponObj == null) continue;
            string savePath = GetEquipmentSavePath(weaponObj);

            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                EquipmentSaveData equipData = JsonUtility.FromJson<EquipmentSaveData>(json);

                if (weaponObj.weaponItemSO == null || weaponObj.weaponItemSO.Length != equipData.weaponItemNames.Count)
                {
                    weaponObj.weaponItemSO = new EquippableItemSO[equipData.weaponItemNames.Count];
                }

                for (int i = 0; i < equipData.weaponItemNames.Count; i++)
                {
                    string savedItemName = equipData.weaponItemNames[i];

                    if (!string.IsNullOrEmpty(savedItemName))
                    {
                        ScriptableObject foundItem = FindItemByName(savedItemName);
                        weaponObj.weaponItemSO[i] = foundItem as EquippableItemSO;
                    }
                    else
                    {
                        weaponObj.weaponItemSO[i] = null;
                    }
                }
            }
        }
    }

    [ContextMenu("Reset Data")]
    public void ResetData()
    {
        foreach (BaseClass character in characters)
        {
            if (character == null) continue;
            string path = GetCharacterSavePath(character);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        // 2. Xoá file save Trang bị & cởi hết đồ trên người
        foreach (AgentWeapon weaponObj in heroWeapons)
        {
            if (weaponObj == null) continue;
            string path = GetEquipmentSavePath(weaponObj);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            // Cởi hết đồ hiện tại trên người nhân vật (set null) để UI cập nhật ngay lập tức
            if (weaponObj.weaponItemSO != null)
            {
                for (int i = 0; i < weaponObj.weaponItemSO.Length; i++)
                {
                    weaponObj.weaponItemSO[i] = null;
                }
            }
        }

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) SaveGame();

        if (Input.GetKeyDown(KeyCode.L)) LoadGame();
    }
}