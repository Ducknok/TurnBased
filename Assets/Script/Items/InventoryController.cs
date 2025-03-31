using Inventory.UI;
using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Controller (C) in MVC
namespace Inventory
{
    public class InventoryController : MonoBehaviour
    {
        [SerializeField]
        private UIInventoryPage inventoryUI;

        [SerializeField]
        private InventorySO inventoryData;
        public List<InventoryItem> initialItems = new List<InventoryItem>();

        private void Start()
        {
            PrepareUI();
            PrepareInventoryData();
        }

        private void PrepareInventoryData()
        {
            this.inventoryData.Initialize();
            this.inventoryData.OnInventoryUpdated += UpdateInventoryUI;
            foreach (InventoryItem item in this.initialItems)
            {
                if (item.IsEmpty) continue;
                this.inventoryData.AddItem(item);
            }
        }

        private void UpdateInventoryUI(Dictionary<int, InventoryItem> inventoryState)
        {
            this.inventoryUI.ResetAllItems();
            foreach (var item in inventoryState)
            {
                this.inventoryUI.UpdateData(item.Key, item.Value.item.ItemImage,
                    item.Value.quantity);
            }
        }

        private void PrepareUI()
        {
            this.inventoryUI.InitializeInventoryUI(this.inventoryData.Size);
            this.inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
            this.inventoryUI.OnItemActionRequested += HandleItemActionRequest;
            this.inventoryUI.OnStartDragging += HandleStartDragging;
            this.inventoryUI.OnSwapItems += HandleSwapItems;
        }

        private void HandleSwapItems(int itemIndex1, int itemIndex2)
        {
            this.inventoryData.SwapItems(itemIndex1, itemIndex2);
        }

        private void HandleStartDragging(int itemIndex)
        {
            InventoryItem inventoryItem = this.inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty) return;
            this.inventoryUI.CreatedDraggedItem(inventoryItem.item.ItemImage, inventoryItem.quantity);
        }

        private void HandleItemActionRequest(int itemIndex)
        {

        }

        private void HandleDescriptionRequest(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
            {
                Debug.LogWarning("Item is empty");
                inventoryUI.ResetSelection();
                return;
            }
            ItemSO item = inventoryItem.item;
            inventoryUI.UpdateDescription(itemIndex, item.ItemImage, item.name, item.ReceiveEffect, item.Description);
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (inventoryUI.isActiveAndEnabled == false)
                {
                    inventoryUI.Show();
                    foreach (var item in inventoryData.GetCurrentInventoryState())
                    {
                        inventoryUI.UpdateData(item.Key,
                            item.Value.item.ItemImage, item.Value.quantity);
                    }
                }
                else
                {
                    inventoryUI.Hide();
                }
            }
        }
    }
}