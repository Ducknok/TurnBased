using Inventory.UI;
using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

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
            this.inventoryUI.OnStartDragging += HandleStartDragging;
            this.inventoryUI.OnSwapItems += HandleSwapItems;
            this.inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
            this.inventoryUI.OnItemActionRequested += HandleItemActionRequest;


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
            InventoryItem inventoryItem = this.inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty) return;
            IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
            if (destroyableItem != null)
            {
                this.inventoryData.RemoveItem(itemIndex, 1);
            }

            IItemAction itemAction = inventoryItem.item as IItemAction;
            if (itemAction != null)
            {
                Debug.LogWarning("Performing action");
                itemAction.PerformAction(this.gameObject, inventoryItem.itemState);
            }
            
        }

        private void HandleDescriptionRequest(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
            {
                inventoryUI.ResetSelection();
                return;
            }
            ItemSO item = inventoryItem.item;
            string description = PrepareDescription(inventoryItem);
            inventoryUI.UpdateDescription(itemIndex, item.ItemImage, item.name
                , item.ReceiveEffect, description);
        }

        public string PrepareDescription(InventoryItem inventoryItem)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(inventoryItem.item.Description);
            //sb.AppendLine();
            for (int i = 0; i < inventoryItem.itemState.Count; i++)
            {
                sb.Append($" {inventoryItem.itemState[i].itemParameter.ParameterName}" + 
                    $": {inventoryItem.itemState[i].value}/" +
                    $"{inventoryItem.item.DefaultParameterList[i].value}");
                //sb.AppendLine();
            }
            return sb.ToString();
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