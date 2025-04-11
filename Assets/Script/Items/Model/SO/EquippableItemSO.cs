using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu]
    public class EquippableItemSO : ItemSO, IDestroyableItem, IItemAction
    {
        [SerializeField]
        private List<ModifierData> modifiersData = new List<ModifierData>();
        public string ActionName => "Equip";
        public AudioClip actionSFX { get; private set; }

        public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
        {
            AgentWeapon weaponSystem = character.GetComponent<AgentWeapon>();
            if(weaponSystem != null)
            {
                foreach (ModifierData data in modifiersData)
                {
                    data.stat.AffectCharacter(character, data.val1, data.val2);
                }
                weaponSystem.SetWeapon(this, itemState == null ?
                    DefaultParameterList : itemState);
                return true;
            }
            return false;
        }
    }
    
}