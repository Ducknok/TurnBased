using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public event Action<Dictionary<int, InventoryItem>> OnInventoryUpdated;

        public void Initialize()
        {
            this.inventoryItems = new List<InventoryItem>();
            for (int i = 0; i < this.Size; i++)
            {
                this.inventoryItems.Add(InventoryItem.GetEmptyItem());
            }
        }
        public int AddItem(ItemSO item, int quantity)
        {
            if(item.IsStackable == false)
            {
                for (int i = 0; i < this.inventoryItems.Count; i++)
                {
                    while(quantity > 0 && !this.IsInventoryFull()) 
                    {
                        quantity -= this.AddItemToFirstFreeSlot(item, 1);

                    }
                    this.InformAboutChange();
                    return quantity;
                    
                }
            }
            quantity = AddStackableItem(item, quantity);
            this.InformAboutChange();
            return quantity;
        }

        private int AddItemToFirstFreeSlot(ItemSO item, int quantity)
        {
            InventoryItem newItem = new InventoryItem
            {
                item = item,
                quantity = quantity
            };
            for (int i = 0; i < this.inventoryItems.Count; i++)
            {
                if (this.inventoryItems[i].IsEmpty)
                {
                    this.inventoryItems[i] = newItem;
                    return quantity;
                }
            }
            return 0;
        }

        private bool IsInventoryFull()
            => this.inventoryItems.Where(item => item.IsEmpty).Any() == false;

        private int AddStackableItem(ItemSO item, int quantity)
        {
            for (int i = 0; i < this.inventoryItems.Count; i++)
            {
                if (this.inventoryItems[i].IsEmpty) continue;
                if(this.inventoryItems[i].item.ID == item.ID)
                {
                    int amountPossibleToTake =
                        this.inventoryItems[i].item.MaxStackSize - this.inventoryItems[i].quantity;
                    if (quantity > amountPossibleToTake)
                    {
                        this.inventoryItems[i] = this.inventoryItems[i]
                            .ChangeQuantity(this.inventoryItems[i].item.MaxStackSize);
                        quantity -= amountPossibleToTake;
                    }
                    else
                    {
                        this.inventoryItems[i] = this.inventoryItems[i]
                            .ChangeQuantity(this.inventoryItems[i].quantity + quantity);
                        this.InformAboutChange();
                        return 0;
                    }
                }
            }
            while(quantity > 0 && !this.IsInventoryFull())
            {
                int newQuantity = Mathf.Clamp(quantity, 0, item.MaxStackSize);
                Debug.LogWarning(newQuantity);
                quantity -= newQuantity;
                this.AddItemToFirstFreeSlot(item, newQuantity);
            }
            return quantity;
        }

        internal void AddItem(InventoryItem item)
        {
            this.AddItem(item.item, item.quantity);
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

        public void SwapItems(int itemIndex1, int itemIndex2)
        {
            InventoryItem item1 = this.inventoryItems[itemIndex1]; // a = b, b = c, c = a 
            this.inventoryItems[itemIndex1] = this.inventoryItems[itemIndex2];
            this.inventoryItems[itemIndex2] = item1;
            this.InformAboutChange();
        }

        private void InformAboutChange()
        {
            OnInventoryUpdated?.Invoke(GetCurrentInventoryState());
        }

        public InventoryItem GetItemAt(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= this.inventoryItems.Count)
            {
                return InventoryItem.GetEmptyItem();
            }
            return this.inventoryItems[itemIndex];
        }

        public void RemoveItem(int itemIndex, int amount)
        {
            if (this.inventoryItems.Count > itemIndex)
            {
                if (this.inventoryItems[itemIndex].IsEmpty) return;
                int reminder = this.inventoryItems[itemIndex].quantity - amount;
                if(reminder <= 0)
                {
                    this.inventoryItems[itemIndex] = InventoryItem.GetEmptyItem();
                }
                else
                {
                    this.inventoryItems[itemIndex] = this.inventoryItems[itemIndex]
                        .ChangeQuantity(reminder);
                }
                this.InformAboutChange();
            }
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


