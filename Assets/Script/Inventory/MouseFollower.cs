using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseFollower : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private UIInventoryItem inventoryItem;

    public void Awake()
    {
        this.canvas = this.transform.root.GetComponent<Canvas>();
        this.inventoryItem = GetComponentInChildren<UIInventoryItem>();
    }

    public void SetData(Sprite sprite, int quantity)
    {
        this.inventoryItem.SetData(sprite, quantity);
    }
    private void Update()
    {
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)this.canvas.transform,
            Input.mousePosition,
            canvas.worldCamera,
            out position);
        this.transform.position = this.canvas.transform.TransformPoint(position);
    }
    public void Toggle(bool toggle)
    {
        Debug.Log($"item toggle {toggle}");
        this.gameObject.SetActive(toggle);
    }
}
