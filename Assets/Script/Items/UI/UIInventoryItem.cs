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
        [SerializeField]
        private TextMeshProUGUI quantityText;

        [SerializeField]
        private Image borderImage;
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
            this.borderImage.enabled = false;
        }
        public void SetData(Sprite sprite, int quantity)
        {
            this.itemImage.gameObject.SetActive(true);
            this.itemImage.sprite = sprite;
            this.quantityText.text = quantity + "";
            this.empty = false;
        }
        public void Select()
        {
            this.borderImage.enabled = true;
            this.OnItemSelected?.Invoke(this);
        }

        //public void OnBeginDrag(PointerEventData eventData)
        //{
        //    if (empty) return;
        //    OnItemBeginDrag?.Invoke(this);
        //}

        //public void OnEndDrag(PointerEventData eventData)
        //{
        //    OnItemEndDrag?.Invoke(this);
        //}

        //public void OnDrop(PointerEventData eventData)
        //{
        //    OnItemDroppedOn?.Invoke(this);
        //}

        //public void OnDrag(PointerEventData eventData)
        //{
            
        //}
    }
}