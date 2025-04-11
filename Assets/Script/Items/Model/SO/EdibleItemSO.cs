
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu]
    public  class EdibleItemSO : ItemSO, IDestroyableItem, IItemAction
    {
        [SerializeField]
        private List<ModifierData> modifiersData = new List<ModifierData>();
        public string ActionName => "Consume";

        //TODO: add sound effect for item here
        //[field: SerializeField]
        //public AudioClip actionSFX { get; private set; }
        public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
        {
            foreach (ModifierData data in modifiersData)
            {
                data.stat.AffectCharacter(character, data.val1, data.val2);
            }
            return true;
        }
    }

    public interface IDestroyableItem
    {
        
    }

    public interface IItemAction
    {
        public string ActionName { get; }
        //public AudioClip actionSFX { get; }
        bool PerformAction(GameObject character, List<ItemParameter> itemState);
    }
    
}

