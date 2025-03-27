using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [SerializeField]
    private UIInventoryPage inventoryUI;
    public int inventorySize = 10;

    private void Start()
    {
        this.inventoryUI.InitializeInventoryUI(this.inventorySize);
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if(this.inventoryUI.isActiveAndEnabled == false)
            {
                this.inventoryUI.Show();
            }
            else
            {
                this.inventoryUI.Hide();
            }
        }
    }
}
