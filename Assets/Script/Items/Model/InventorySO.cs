using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Model (M) in MVC        
namespace Inventory.Model
{
    [CreateAssetMenu]
    public class InventorySO : ScriptableObject
    {
        [SerializeField]
        private List<InventoryItem> inventoryItems;
        [field: SerializeField]
        public int Size { get; private set; } = 10;

        public void Initialize()
        {
            this.inventoryItems = new List<InventoryItem>();
            for (int i = 0; i < Size; i++)
            {
                this.inventoryItems.Add(InventoryItem.GetEmptyItem());
            }
        }
        public void AddItem(ItemSO item, int quantity)
        {
            for (int i = 0; i < Size; ++i)
            {
                if (this.inventoryItems[i].IsEmpty)
                {
                    this.inventoryItems[i] = new InventoryItem
                    {
                        item = item,
                        quantity = quantity,
                    };
                }
            }
        }
        public Dictionary<int, InventoryItem> GetCurrentInventoryState()
        {
            Dictionary<int, InventoryItem> returnValue
                = new Dictionary<int, InventoryItem>();
            for (int i = 0; i < this.inventoryItems.Count; i++)
            {
                if (this.inventoryItems[i].IsEmpty) continue;
                returnValue[i] = this.inventoryItems[i];
            }
            return returnValue;
        }

        public InventoryItem GetItemAt(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= this.inventoryItems.Count)
            {
                Debug.LogWarning("Item index out of range");
                return InventoryItem.GetEmptyItem();
            }
            return this.inventoryItems[itemIndex];
        }
    }

    [Serializable]
    public struct InventoryItem
    {
        public int quantity;
        public ItemSO item;
        public bool IsEmpty => item == null;
        public InventoryItem ChangeQuantity(int newQuantity)
        {
            return new InventoryItem
            {
                quantity = newQuantity,
                item = this.item
            };
        }
        public static InventoryItem GetEmptyItem()
            => new InventoryItem
            {
                item = null,
                quantity = 0,
            };
    }
}


