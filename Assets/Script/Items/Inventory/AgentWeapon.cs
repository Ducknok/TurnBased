using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentWeapon : DucMonobehaviour
{
    [SerializeField]
    public EquippableItemSO[] weaponItemSO;

    [SerializeField]
    private InventorySO inventoryData;

    public void SetWeapon(int index, EquippableItemSO weaponItemSO)
    {
        if (this.weaponItemSO != null)
        {
            this.inventoryData.AddItem(this.weaponItemSO[index], 1);
        }

        this.weaponItemSO[index] = weaponItemSO;
    }
}