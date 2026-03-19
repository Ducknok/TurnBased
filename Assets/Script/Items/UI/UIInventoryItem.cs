using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

//View (V) in MVC
namespace Inventory.UI
{
    public class UIInventoryItem : DucMonobehaviour //IDropHandler /*IBeginDragHandler*//*IEndDragHandler*//*IDragHandler*/
    {
        [SerializeField]
        private Image itemImage;
        [SerializeField] private TextMeshProUGUI quantity;
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private Image bg;
        public event Action<UIInventoryItem> OnItemSelected, OnItemDroppedOn /*OnItemBeginDrag, OnItemEndDrag /*OnItemSold*/;
        private bool empty = true;

        protected override void Awake()
        {
            ResetData();
            Deselect();
        }
        public void ResetData()
        {
            this.itemImage.gameObject.SetActive(false);
            this.empty = true;
        }
        public void Deselect()
        {
            this.bg.enabled = false;
        }
        public void SetData(Sprite sprite, string itemName, int quantity)
        {
            this.itemImage.gameObject.SetActive(true);
            this.itemImage.sprite = sprite;
            this.itemName.text = itemName;
            this.quantity.text = quantity + "";
            this.empty = false;
        }
        public void Select()
        {
            this.bg.enabled = true;
            this.OnItemSelected?.Invoke(this);
        }
    }

}