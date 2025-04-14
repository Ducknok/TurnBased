using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//View (V) in MVC
namespace Inventory.UI
{
    public class UIInventoryPage : MonoBehaviour
    {
        [SerializeField]
        private UIInventoryItem itemPrefab;
        [SerializeField]
        private RectTransform contentPanel;
        [SerializeField]
        private UIInventoryDescription itemDescription;
        [SerializeField]
        private MouseFollower mouseFollower;

        List<UIInventoryItem> listOfUIItems = new List<UIInventoryItem>();
        private int currentIndex = 0; // Index của item đang chọn

        private int currentlyDraggedItemIndex = -1;
        public event Action<int> OnItemActionRequested,
            OnStartDragging;

        public event Action<int, int> OnSwapItems;
        public event Action<int> OnDescriptionRequested;
        [SerializeField]
        private ItemActionPanel itemActionPanel;


        private void Awake()
        {
            Hide();
            mouseFollower.Toggle(false);
            itemDescription.ResetDescription();
        }
        private void Update()
        {
            this.SelectItem();
        }
        private void SelectItem()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                MoveSelection(-1);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                MoveSelection(1);
            }
            else if (Input.GetKeyDown(KeyCode.Return))  // Nhấn Enter để mở menu hành động
            {
                if (this.listOfUIItems.Count > 0)
                {
                    OnItemActionRequested?.Invoke(currentIndex);
                }
            }
        }

        private void MoveSelection(int direction)
        {
            if (listOfUIItems.Count == 0) return; // Nếu inventory rỗng thì không làm gì

            // Bỏ chọn item hiện tại
            listOfUIItems[currentIndex].Deselect();

            // Cập nhật index
            currentIndex += direction;
            if (currentIndex < 0)
                currentIndex = listOfUIItems.Count - 1;
            else if (currentIndex >= listOfUIItems.Count)
                currentIndex = 0;

            // Chọn item mới
            UpdateSelection();
        }

        private void UpdateSelection()
        {
            //this.ResetAllItems();
            listOfUIItems[currentIndex].Select();
            OnDescriptionRequested?.Invoke(currentIndex);
        }
        public void InitializeInventoryUI(int inventorySize)
        {
            for (int i = 0; i < inventorySize; i++)
            {
                UIInventoryItem uiItem = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
                uiItem.transform.SetParent(contentPanel);
                listOfUIItems.Add(uiItem);
                //uiItem.OnItemBeginDrag += HandleBeginDrag;
                //uiItem.OnItemDroppedOn += HandleSwap;
                //uiItem.OnItemEndDrag += HandleEndDrag;
            }
            this.UpdateSelection();
        }

        internal void ResetAllItems()
        {
            foreach (var item in this.listOfUIItems)
            {
                item.ResetData();
                item.Deselect();
            }
        }

        public void UpdateData(int itemIndex, Sprite itemImage, int itemQuantity)
        {
            if (listOfUIItems.Count > itemIndex)
            {
                listOfUIItems[itemIndex].SetData(itemImage, itemQuantity);
            }
        }
        private void HandleShowItemActions(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1)
            {

                return;
            }
            OnItemActionRequested?.Invoke(index);
        }

        //private void HandleEndDrag(UIInventoryItem inventoryItemUI)
        //{
        //    ResetDraggedItem();
        //}

        //private void HandleSwap(UIInventoryItem inventoryItemUI)
        //{
        //    int index = listOfUIItems.IndexOf(inventoryItemUI);
        //    if (index == -1)
        //    {

        //        return;
        //    }

        //    OnSwapItems?.Invoke(currentlyDraggedItemIndex, index);
        //    HandleItemSelection(inventoryItemUI);
        //}

        private void ResetDraggedItem()
        {
            mouseFollower.Toggle(false);
            currentlyDraggedItemIndex = -1;
        }

        //private void HandleBeginDrag(UIInventoryItem inventoryItemUI)
        //{
        //    int index = listOfUIItems.IndexOf(inventoryItemUI);
        //    if (index == -1) return;
        //    currentlyDraggedItemIndex = index;
        //    HandleItemSelection(inventoryItemUI);
        //    OnStartDragging?.Invoke(index);
        //}

        //public void CreatedDraggedItem(Sprite sprite, int quantity)
        //{
        //    mouseFollower.Toggle(true);
        //    mouseFollower.SetData(sprite, quantity);
        //}

        //private void HandleItemSelection(UIInventoryItem inventoryItemUI)
        //{
        //    int index = listOfUIItems.IndexOf(inventoryItemUI);
        //    if (index == -1) return;
        //    OnDescriptionRequested?.Invoke(index);
        //}

        public void Show()
        {
            gameObject.SetActive(true);
            ResetSelection();
        }

        public void ResetSelection()
        {
            itemDescription.ResetDescription();
            DeselectAllItems();
        }
        public void AddAction(string actionName, Action performAction)
        {
            this.itemActionPanel.AddButton(actionName, performAction);
        }
        public void ShowItemAction(int itemIndex)
        {
            this.itemActionPanel.Toggle(true);
            this.itemActionPanel.transform.position = listOfUIItems[itemIndex].transform.position;
        }

        private void DeselectAllItems()
        {
            foreach (UIInventoryItem item in listOfUIItems)
            {
                item.Deselect();
            }
            this.itemActionPanel.Toggle(false);
        }
        public void SetSelectedIndex(int index)
        {
            if (listOfUIItems.Count == 0) return;

            // Bỏ chọn item cũ
            listOfUIItems[currentIndex].Deselect();

            // Cập nhật index mới
            currentIndex = index;

            // Chọn item mới và hiển thị description đúng
            listOfUIItems[currentIndex].Select();
            OnDescriptionRequested?.Invoke(currentIndex);
        }

        public void Hide()
        {
            this.itemActionPanel.Toggle(false);
            gameObject.SetActive(false);
            ResetDraggedItem();
        }

        internal void UpdateDescription(int itemIndex, Sprite itemImage, string name, string receiveEffect, string description)
        {
            this.itemDescription.SetDescription(itemImage, name, receiveEffect, description);
            DeselectAllItems();
            this.listOfUIItems[itemIndex].Select();
        }
    }
}