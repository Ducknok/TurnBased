using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIInventoryDescription : MonoBehaviour
{
    [SerializeField]
    private Image itemImage;
    [SerializeField]
    private TextMeshProUGUI title;
    [SerializeField]
    private TextMeshProUGUI receiveEffect;
    [SerializeField]
    private TextMeshProUGUI description;

    public void Awake()
    {
        ResetDescription();
    }
    public void ResetDescription()
    {
        this.itemImage.gameObject.SetActive(false);
        this.title.text = "";
        this.receiveEffect.text = "";
        this.description.text = "";
    }
    public void SetDescription(Sprite sprite, string itemName,string itemEffect, string itemDescription)
    {
        this.itemImage.gameObject.SetActive(true);
        this.itemImage.sprite = sprite;
        this.title.text = itemName;
        this.receiveEffect.text = itemEffect;
        this.description.text = itemDescription;
    }
}
