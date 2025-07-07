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

    [SerializeField]
    private List<ItemParameter> parameterToModify, itemCurrentState;
    public void SetWeapon(int index, EquippableItemSO weaponItemSO, List<ItemParameter> itemState)
    {
        if (this.weaponItemSO != null)
        {
            this.inventoryData.AddItem(this.weaponItemSO[index], 1, this.itemCurrentState);
        }

        this.weaponItemSO[index] = weaponItemSO;
        this.itemCurrentState = new List<ItemParameter>(itemState);
        ModifyParameter();
    }
    private void ModifyParameter()
    {
        for (int i = 0; i < itemCurrentState.Count; i++)
        {
            foreach (var paramMod in parameterToModify)
            {
                if (itemCurrentState[i].itemParameter == paramMod.itemParameter)
                {
                    itemCurrentState[i] = new ItemParameter
                    {
                        itemParameter = itemCurrentState[i].itemParameter,
                        value = itemCurrentState[i].value + paramMod.value
                    };
                }
            }
        }
    }
}
