using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentWeapon : DucMonobehaviour
{
    [Header("Current Weapon")]
    [SerializeField]
    public EquippableItemSO[] weaponItemSO; // Mảng chứa Vũ khí, Áo giáp, Nhẫn...

    [Header("Inventory")]
    [SerializeField]
    private InventorySO inventoryData;

    protected override void Awake()
    {
        base.Awake();
        HeroStateMachine hero = this.GetComponent<HeroStateMachine>();
        if (hero != null && hero.baseHero != null)
        {
            if (!hero.baseHero.name.Contains("(Clone)"))
            {
                hero.baseHero = Instantiate(hero.baseHero);
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(WaitAndApplyInitialStats());
    }

    private IEnumerator WaitAndApplyInitialStats()
    {
        yield return null;
        this.ApplyInitialEquipmentStats();
    }

    private void ApplyInitialEquipmentStats()
    {
        if (weaponItemSO == null) return;

        foreach (var item in weaponItemSO)
        {
            if (item != null && item.Modifiers != null)
            {
                foreach (var mod in item.Modifiers)
                {
                    if (mod.stat != null)
                    {
                        mod.stat.AffectCharacter(this.gameObject, mod.val1, mod.val2);
                    }
                }
            }
        }
    }

    public bool HasEquippedItem(EquippableItemSO itemToCheck)
    {
        if (itemToCheck == null) return false;

        for (int i = 0; i < weaponItemSO.Length; i++)
        {
            if (weaponItemSO[i] == itemToCheck)
            {
                return true;
            }
        }
        return false;
    }

    public void SetWeapon(int index, EquippableItemSO newItem)
    {
        if (index < 0 || index >= weaponItemSO.Length)
        {
            Debug.LogError("Vị trí ô trang bị không hợp lệ!");
            return;
        }

        EquippableItemSO oldItem = weaponItemSO[index];
        if (oldItem != null)
        {
            if (oldItem.Modifiers != null)
            {
                foreach (var mod in oldItem.Modifiers)
                {
                    if (mod.stat != null)
                    {
                        mod.stat.AffectCharacter(this.gameObject, -mod.val1, -mod.val2);
                    }
                }
            }

            this.inventoryData.AddItem(oldItem, 1);
            Debug.Log($"Đã tháo {oldItem.Name}, trừ chỉ số và trả vào kho.");
        }

        // 2. MẶC ĐỒ MỚI VÀO Ô ĐÓ (VÀ CỘNG CHỈ SỐ LÊN)
        this.weaponItemSO[index] = newItem;

        if (newItem != null)
        {
            if (newItem.Modifiers != null)
            {
                foreach (var mod in newItem.Modifiers)
                {
                    if (mod.stat != null)
                    {
                        // Cộng chỉ số mới
                        mod.stat.AffectCharacter(this.gameObject, mod.val1, mod.val2);
                    }
                }
            }
            Debug.Log($"Đã trang bị {newItem.Name} vào ô số {index} và cộng chỉ số.");
        }
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
    }
}