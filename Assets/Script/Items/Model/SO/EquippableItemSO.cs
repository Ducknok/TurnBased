using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu]
    public class EquippableItemSO : ItemSO, IDestroyableItem, IItemAction
    {
        public HeroType allowedWeapons;
        //[SerializeField]
       //private List<ModifierData> modifiersData = new List<ModifierData>();
        public string ActionName => "Equip";
        public AudioClip actionSFX { get; private set; }

        public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
        {
            HeroStateMachine hero = character.GetComponent<HeroStateMachine>();
            if (hero != null && hero.baseHero.heroType != this.allowedWeapons)
            {
                Debug.Log("This weapon cannot be equipped by this hero.");
                return false;
            }

            AgentWeapon weaponSystem = character.GetComponent<AgentWeapon>();
            if(weaponSystem != null)
            {
                foreach (ModifierData data in Modifiers)
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