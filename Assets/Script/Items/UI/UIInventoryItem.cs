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
    public class UIInventoryItem : MonoBehaviour, IDropHandler /*IBeginDragHandler*//*IEndDragHandler*//*IDragHandler*/
    {
        [SerializeField]
        private Image itemImage;
        [SerializeField]
        private TextMeshProUGUI quantityText;

        [SerializeField]
        private Image borderImage;

        public event Action<UIInventoryItem> OnItemSelected, OnItemDroppedOn /*OnItemBeginDrag, OnItemEndDrag /*OnItemSold*/;

        private bool empty = true;

        public void Awake()
        {
            ResetData();
            Deselect();
        }
        public void ResetData()
        {
            itemImage.gameObject.SetActive(false);
            empty = true;
        }
        public void Deselect()
        {
            borderImage.enabled = false;
        }
        public void SetData(Sprite sprite, int quantity)
        {
            itemImage.gameObject.SetActive(true);
            itemImage.sprite = sprite;
            quantityText.text = quantity + "";
            empty = false;
        }
        public void Select()
        {
            borderImage.enabled = true;
            OnItemSelected?.Invoke(this);
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

        public void OnDrop(PointerEventData eventData)
        {
            OnItemDroppedOn?.Invoke(this);
        }

        //public void OnDrag(PointerEventData eventData)
        //{
            
        //}
    }
}