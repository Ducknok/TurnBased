using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentWeapon : MonoBehaviour
{
    [SerializeField]
    private EquippableItemSO weapon;

    [SerializeField]
    private InventorySO inventoryData;

    [SerializeField]
    private List<ItemParameter> parameterToModify, itemCurrentState;

    public void SetWeapon(EquippableItemSO weaponItemSO, List<ItemParameter> itemState)
    {
        if(this.weapon != null)
        {
            this.inventoryData.AddItem(this.weapon, 1, this.itemCurrentState);
        }
        this.weapon = weaponItemSO;
        this.itemCurrentState = new List<ItemParameter>(itemState);
        ModifyParamter();
    }

    private void ModifyParamter()
    {
        foreach (var parameter in parameterToModify)
        {
            if (this.itemCurrentState.Contains(parameter))
            {
                int index = this.itemCurrentState.IndexOf(parameter);
                float newValue = this.itemCurrentState[index].value + parameter.value;
                this.itemCurrentState[index] = new ItemParameter
                {
                    itemParameter = parameter.itemParameter,
                    value = newValue
                };
            }
        }
    }
}
