using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInventoryPage : MonoBehaviour
{
    [SerializeField]
    private UIInventoryItem itemPrefab;

    [SerializeField]
    private RectTransform contentPanel;

    List<UIInventoryItem> listOfUIItems = new List<UIInventoryItem>();

    public void InitializeInventoryUI(int inventorySize)
    {
        UIInventoryItem item = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
        item.transform.SetParent(contentPanel);
        listOfUIItems.Add(item);
    }
}
