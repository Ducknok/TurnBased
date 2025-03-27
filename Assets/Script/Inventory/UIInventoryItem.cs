using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class UIInventoryItem : MonoBehaviour
{
    [SerializeField]
    private Image itemImage;
    [SerializeField]
    private TextMeshProUGUI quantityText;

    [SerializeField]
    private Image boderImage;

    public event Action<UIInventoryItem> OnItemClicked, OnItemDroppedOn, OnItemBeginDrag, OnItemEndDrag, OnItemSold, OnRightMouseBtnClick;

    private bool empty = true;

    public void Awake()
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
        this.boderImage.enabled = false;
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
        this.boderImage.enabled = true;
    }
    public void OnBeginDrag()
    {
        if (this.empty) return;
        OnItemBeginDrag?.Invoke(this);
    }
    public void OnDrop()
    {
        OnItemDroppedOn?.Invoke(this);
    }
    public void OnEndDrag()
    {
        OnItemEndDrag?.Invoke(this);
    }
    public void OnPointerClick(BaseEventData data)
    {
        PointerEventData pointer = (PointerEventData)data;
        if(pointer.button == PointerEventData.InputButton.Right)
        {
            OnRightMouseBtnClick?.Invoke(this);
        }
        else
        {
            OnItemClicked?.Invoke(this);
        }
    }
}
