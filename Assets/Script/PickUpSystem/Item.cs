using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : DucMonobehaviour
{
    [field: SerializeField]
    public ItemSO InventoryItem { get; private set; }

    [field: SerializeField]
    public int Quantity { get; set; } = 1;

    [field: SerializeField]
    private float duration = 0.3f;

    protected override void Start()
    {
        GetComponent<SpriteRenderer>().sprite = this.InventoryItem.ItemImage;
    }
    internal void DestroyItem()
    {
        GetComponent<Collider2D>().enabled = false;
        StartCoroutine(AnimateItemPickUp());
    }
    private IEnumerator AnimateItemPickUp()
    {
        Vector3 startScale = this.transform.localScale;
        Vector3 endScale = Vector3.zero;
        float currentTime = 0;
        while(currentTime < duration)
        {
            currentTime += Time.deltaTime;
            this.transform.localScale = Vector3.Lerp(startScale, endScale, currentTime / duration);
            yield return null;
        }
        this.transform.localScale = endScale;
        Destroy(this.gameObject);
    }
}
