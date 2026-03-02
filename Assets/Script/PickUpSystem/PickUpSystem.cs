using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpSystem : DucMonobehaviour
{
    [SerializeField]
    private InventorySO inventoryData;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Item item = collision.GetComponent<Item>();
        if(item != null)
        {
            int reminder = this.inventoryData.AddItem(item.InventoryItem, item.Quantity);
            if (reminder == 0) item.DestroyItem();
            else item.Quantity = reminder;
        }
    }
}
