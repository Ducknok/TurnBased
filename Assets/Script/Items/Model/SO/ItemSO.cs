using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Model (M) in MVC
namespace Inventory.Model
{
    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        Miscellaneous
    }
    public abstract class ItemSO : ScriptableObject
    {

        [field: SerializeField]
        public ItemType itemType;
        [field: SerializeField]
        public bool IsStackable { get; set; }
        public int ID => GetInstanceID();

        [field: SerializeField]
        public int MaxStackSize { get; set; } = 99;
        [field: SerializeField]
        public string Name { get; set; }
        [field: SerializeField]
        public string ReceiveEffect { get; set; }
        [field: SerializeField]
        [field: TextArea]
        public string Description { get; set; }
        [field: SerializeField]
        public Sprite ItemImage { get; set; }
        [field: SerializeField]
        public List<ItemParameter> DefaultParameterList { get; set; }
        
    }
    [Serializable]
    public struct ItemParameter: IEquatable<ItemParameter>
    {
        public ItemParameterSO itemParameter;
        public float value;

        public bool Equals(ItemParameter other)
        {
            return other.itemParameter == itemParameter;
        }
    }

}

