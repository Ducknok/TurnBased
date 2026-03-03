using Inventory.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipableItemUI : DucMonobehaviour
{
    private EquippableItemSO itemData;
    private EquipMenuController controller;

    protected override void Awake()
    {
        this.controller = FindAnyObjectByType<EquipMenuController>();
    }

    public void Setup(EquippableItemSO data, EquipMenuController equipMenuController)
    {
        this.itemData = data;
        this.controller = equipMenuController;

        // G?n ?nh và tên
        Image itemImage = transform.Find("Button/WeaponImage")?.GetComponent<Image>();
        TextMeshProUGUI itemName = transform.Find("WeaponName")?.GetComponent<TextMeshProUGUI>();

        if (itemImage != null) itemImage.sprite = data.ItemImage;
        if (itemName != null) itemName.text = data.Name;
    }

}
