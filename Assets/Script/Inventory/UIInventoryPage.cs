using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private int currentlyDraggedItemIndex = -1;
    private event Action<int> OnDescriptionRequested,
        OnItemActionRequested,
        OnStartDragging;

    public event Action<int, int> OnSwapItems;

    private void Awake()
    {
        this.Hide();
        this.mouseFollower.Toggle(false);
        this.itemDescription.ResetDescription();
    }

    public void InitializeInventoryUI(int inventorySize)
    {
        for (int i = 0; i < inventorySize; i++)
        {
            UIInventoryItem uiItem = Instantiate(this.itemPrefab, Vector3.zero, Quaternion.identity);
            uiItem.transform.SetParent(this.contentPanel);
            this.listOfUIItems.Add(uiItem);
            uiItem.OnItemClicked += HandleItemSelection;
            uiItem.OnItemBeginDrag += HandleBeginDrag;
            uiItem.OnItemDroppedOn += HandleSwap;
            uiItem.OnItemEndDrag += HandleEndDrag;
            uiItem.OnRightMouseBtnClick += HandleShowItemActions;
        }
    }

    public void UpdateData(int itemIndex, Sprite itemImage, int itemQuantity)
    {
        if(this.listOfUIItems.Count > itemIndex)
        {
            this.listOfUIItems[itemIndex].SetData(itemImage, itemQuantity);
        }
    }
    private void HandleShowItemActions(UIInventoryItem inventoryItemUI)
    {
        
    }

    private void HandleEndDrag(UIInventoryItem inventoryItemUI)
    {
        ResetDraggedItem();
    }

    private void HandleSwap(UIInventoryItem inventoryItemUI)
    {
        int index = this.listOfUIItems.IndexOf(inventoryItemUI);
        if (index == -1)
        {
            
            return;
        }

        OnSwapItems?.Invoke(this.currentlyDraggedItemIndex, index);
    }

    private void ResetDraggedItem()
    {
        this.mouseFollower.Toggle(false);
        this.currentlyDraggedItemIndex = -1;
    }

    private void HandleBeginDrag(UIInventoryItem inventoryItemUI)
    {
        int index = this.listOfUIItems.IndexOf(inventoryItemUI);
        if (index == -1) return;
        this.currentlyDraggedItemIndex = index;
        HandleItemSelection(inventoryItemUI);
        OnStartDragging?.Invoke(index);
    }

    public void CreatedDraggedItem(Sprite sprite, int quantity)
    {
        this.mouseFollower.Toggle(true);
        this.mouseFollower.SetData(sprite, quantity);
    }

    private void HandleItemSelection(UIInventoryItem inventoryItemUI)
    {
        int index = this.listOfUIItems.IndexOf(inventoryItemUI);
        if (index == -1) return;
        OnDescriptionRequested?.Invoke(index);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        ResetSelection();
    }

    private void ResetSelection()
    {
        this.itemDescription.ResetDescription();
        DeselectAllItems();
    }

    private void DeselectAllItems()
    {
        foreach (UIInventoryItem item in this.listOfUIItems)
        {
            item.Deselect();
        }
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
        ResetDraggedItem();
    }
}
