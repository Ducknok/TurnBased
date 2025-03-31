using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//View (V) in MVC
namespace Inventory.UI
{
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
            itemImage.gameObject.SetActive(false);
            title.text = "";
            receiveEffect.text = "";
            description.text = "";
        }
        public void SetDescription(Sprite sprite, string itemName, string itemEffect, string itemDescription)
        {
            itemImage.gameObject.SetActive(true);
            itemImage.sprite = sprite;
            title.text = itemName;
            receiveEffect.text = itemEffect;
            description.text = itemDescription;
        }
    }
}