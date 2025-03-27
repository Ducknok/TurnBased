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

    List<UIInventoryItem> listOfUIItems = new List<UIInventoryItem>();

    public Sprite image;
    public int quantity;
    public string title, reciveEffect, description;

    private void Awake()
    {
        this.Hide();
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

    private void HandleShowItemActions(UIInventoryItem obj)
    {
        
    }

    private void HandleEndDrag(UIInventoryItem obj)
    {
       
    }

    private void HandleSwap(UIInventoryItem obj)
    {
        
    }

    private void HandleBeginDrag(UIInventoryItem obj)
    {
        
    }

    private void HandleItemSelection(UIInventoryItem obj)
    {
        this.itemDescription.SetDescription(this.image, this.title, this.reciveEffect, this.description);
        this.listOfUIItems[0].Select();
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        this.itemDescription.ResetDescription();

        this.listOfUIItems[0].SetData(this.image, this.quantity);
    }
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
