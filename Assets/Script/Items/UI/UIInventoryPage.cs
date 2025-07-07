using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//View (V) in MVC
namespace Inventory.UI
{
    public class UIInventoryPage : DucMonobehaviour
    {
        [Header("LinkClass")]
        [SerializeField] private UIInventoryItem itemPrefab;
        [SerializeField] private RectTransform contentPanel;
        [SerializeField] public UIInventoryDescription itemDescription;
        //[SerializeField] private MouseFollower mouseFollower;
       
        [Header("ListOfUIItem")]
        List<UIInventoryItem> listOfUIItems = new List<UIInventoryItem>();
        private int currentIndex = 0; // Index của item đang chọn

        // private int currentlyDraggedItemIndex = -1;
        //Event
        public event Action<int> OnItemActionRequested, OnDescriptionRequested /*OnStartDragging*/;
        private Action<HeroStateMachine> onHeroSelectedCallback;

        [Header("ItemActionPanel")]
        [SerializeField] private ItemActionPanel itemActionPanel;

        [Header("HeroButton")]
        [SerializeField] private Image heroImagePrefab;
        [SerializeField] private Transform heroButtonSpacer;
        private List<Image> heroButtons = new List<Image>();
        private bool isSelectingHero = false;
        private int currentHeroIndex = 0;

        [Header("ItemInfoPanel")]
        [SerializeField] public List<GameObject> heroInfoPanelList = new List<GameObject>();
        [SerializeField] public List<Image> heroInfoPanelPrefab = new List<Image>();
        [SerializeField] public Transform infoHeroPanelSpacer;
        private List<Image> heroPanel = new List<Image>();

        protected override void Awake()
        {
            Hide();
            this.itemDescription.ResetDescription();
        }
        protected override void Update()
        {
            this.CheckState();
        }
        public override void CheckState()
        {
            base.CheckState();
            if (this.isSelectingHero)
            {
                SelectHero();
            }
            else
            {
                SelectItem();
            }
        }
        public void StartHeroSelection(Action<HeroStateMachine> onSelected)
        {
            this.isSelectingHero = true;
            this.currentHeroIndex = 0;
            this.onHeroSelectedCallback = onSelected;
            HighlightHeroButton(this.currentHeroIndex); // Làm nổi bật hero đầu tiên
        }
        private void SelectHero()
        {
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                this.currentHeroIndex = (this.currentHeroIndex + 1) % this.heroButtons.Count;
                //Debug.LogWarning("Nut S");

                HighlightHeroButton(this.currentHeroIndex);
            }
            else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                this.currentHeroIndex = (this.currentHeroIndex - 1 + this.heroButtons.Count) % this.heroButtons.Count;
                ///Debug.LogWarning("Nut W");

                HighlightHeroButton(this.currentHeroIndex);
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                this.isSelectingHero = false;
                this.UnHighlightHeroButton(this.currentHeroIndex);
                this.SetSelectedIndex(0);
                HeroStateMachine selectedHero = CombatController.Instance.CBM.playersInCombat[currentHeroIndex].GetComponent<HeroStateMachine>();
                this.onHeroSelectedCallback?.Invoke(selectedHero);
            }
            else if (Input.GetKeyDown(KeyCode.LeftControl)) // 👈 quay về chọn item
            {
                this.isSelectingHero = false;
                this.UnHighlightHeroButton(this.currentHeroIndex);
                this.SetSelectedIndex(0);
            }
        }
        private void SelectItem()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                //Debug.LogWarning("Nut A");

                MoveSelection(-1);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                //Debug.LogWarning("Nut D");

                MoveSelection(1);
            }
            else if (Input.GetKeyDown(KeyCode.Return))  // Nhấn Enter để mở menu hành động
            {
                if (this.listOfUIItems.Count > 0)
                {
                    this.OnItemActionRequested?.Invoke(this.currentIndex);
                }
            }
        }
        private void HighlightHeroButton(int index)
        {
            for (int i = 0; i < heroButtons.Count; i++)
            {
                // ⚠️ Kiểm tra null hoặc bị destroy
                if (heroButtons[i] == null || heroButtons[i].gameObject == null)
                    continue;

                Transform heroIcon = heroButtons[i].transform.Find("HeroIcon")?.Find("Choose");
                if (heroIcon != null)
                {
                    heroIcon.gameObject.SetActive(i == index);
                }
            }
        }
        private void UnHighlightHeroButton(int index)
        {
            for (int i = 0; i < heroButtons.Count; i++)
            {
                if (heroButtons[i] == null || heroButtons[i].gameObject == null)
                    continue;

                Transform heroIcon = heroButtons[i].transform.Find("HeroIcon")?.Find("Choose");
                if (heroIcon != null)
                {
                    heroIcon.gameObject.SetActive(false);
                }
            }
        }
        private void MoveSelection(int direction)
        {
            if (this.listOfUIItems.Count == 0) return; // Nếu inventory rỗng thì không làm gì

            // Bỏ chọn item hiện tại
            this.listOfUIItems[currentIndex].Deselect();

            // Cập nhật index
            this.currentIndex += direction;
            if (this.currentIndex < 0)
                this.currentIndex = this.listOfUIItems.Count - 1;
            else if (this.currentIndex >= this.listOfUIItems.Count)
                this.currentIndex = 0;

            // Chọn item mới
            UpdateSelection();
        }
        private void UpdateSelection()
        {
            //this.ResetAllItems();
            this.listOfUIItems[this.currentIndex].Select();
            this.OnDescriptionRequested?.Invoke(this.currentIndex);
        }
        public void InitializeInventoryUI(int inventorySize)
        {
            for (int i = 0; i < inventorySize; i++)
            {
                UIInventoryItem uiItem = Instantiate(this.itemPrefab, Vector3.zero, Quaternion.identity);
                uiItem.transform.SetParent(this.contentPanel);
                this.listOfUIItems.Add(uiItem);
                //uiItem.OnItemBeginDrag += HandleBeginDrag;
                //uiItem.OnItemDroppedOn += HandleSwap;
                //uiItem.OnItemEndDrag += HandleEndDrag;
            }
            this.UpdateSelection();
        }
        public void InitializeHeroButton()
        {
            foreach (Transform child in this.heroButtonSpacer)
            {
                Destroy(child.gameObject);
            }

            this.heroButtons.Clear();

            // Instantiate button và fill dữ liệu
            for (int i = 0; i < CombatController.Instance.CBM.playersInCombat.Count; i++)
            {
                HeroStateMachine hero = CombatController.Instance.CBM.playersInCombat[i].GetComponent<HeroStateMachine>();
                Image newImage = Instantiate(this.heroImagePrefab, this.heroButtonSpacer);
                this.heroButtons.Add(newImage);

                // Gọi hàm fill dữ liệu vào button
                this.itemDescription.SetHeroBarDescription(newImage, hero);

                // Optional: Add onClick event
                //int index = i;
                //newButton.onClick.AddListener(() => OnHeroButtonClicked(index));
            }
        }
        public void InitializeHeroBar(ItemSO item)
        {
            foreach (Transform child in this.infoHeroPanelSpacer)
            {
                Destroy(child.gameObject);
            }
            this.heroPanel.Clear();

            for (int i = 0; i < CombatController.Instance.CBM.playersInCombat.Count; i++)
            {
                HeroStateMachine hero = CombatController.Instance.CBM.playersInCombat[i].GetComponent<HeroStateMachine>();
                foreach (var bar in this.heroInfoPanelPrefab)
                {
                    Image newBar = Instantiate(bar, this.infoHeroPanelSpacer);
                    this.heroButtons.Add(newBar);

                    // Chỉ hiển thị bonus nếu item là EquippableItemSO và hợp với hero
                    if (item is EquippableItemSO weapon)
                    {
                        if (hero.baseHero.heroType == weapon.allowedWeapons)
                        {
                            this.itemDescription.SetATKDescription(newBar, hero, weapon);
                        }
                        else
                        {
                            this.itemDescription.SetATKDescription(newBar, hero, null); // hoặc không set gì cả
                        }
                    }
                    else
                    {
                        this.itemDescription.SetATKDescription(newBar, hero, null);
                    }
                }
            }
        }
        internal void ResetAllItems()
        {
            foreach (var item in this.listOfUIItems)
            {
                item.ResetData();
                item.Deselect();
            }
        }
        public void UpdateData(int itemIndex, Sprite itemImage, int itemQuantity)
        {
            if (listOfUIItems.Count > itemIndex)
            {
                listOfUIItems[itemIndex].SetData(itemImage, itemQuantity);
            }
        }
        private void HandleShowItemActions(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1)
            {

                return;
            }
            OnItemActionRequested?.Invoke(index);
        }
        public void Show()
        {
            //Debug.LogError("Show");
            this.gameObject.SetActive(true);
            ResetSelection();
        }
        public void ResetSelection()
        {
            this.itemDescription.ResetDescription();
            DeselectAllItems();
        }
        public void AddAction(string actionName, Action performAction)
        {
            this.itemActionPanel.AddButton(actionName, performAction);
        }
        public void ShowItemAction(int itemIndex)
        {
            this.itemActionPanel.Toggle(true);
            this.itemActionPanel.transform.position = listOfUIItems[itemIndex].transform.position;
        }
        private void DeselectAllItems()
        {
            foreach (UIInventoryItem item in listOfUIItems)
            {
                item.Deselect();
            }
            this.itemActionPanel.Toggle(false);
        }
        public void SetSelectedIndex(int index)
        {
            if (listOfUIItems.Count == 0) return;

            // Bỏ chọn item cũ
            listOfUIItems[currentIndex].Deselect();

            // Cập nhật index mới
            currentIndex = index;

            // Chọn item mới và hiển thị description đúng
            listOfUIItems[currentIndex].Select();
            OnDescriptionRequested?.Invoke(currentIndex);
        }
        public void Hide()
        {
            this.itemActionPanel.Toggle(false);
            gameObject.SetActive(false);
            //ResetDraggedItem();
        }
        internal void UpdateItemDescription(int itemIndex, Sprite itemImage, string name, string receiveEffect, string description)
        {
            this.itemDescription.SetItemDescription(itemImage, name, receiveEffect, description);
            DeselectAllItems();
            this.listOfUIItems[itemIndex].Select();
        }
    }
    //private void HandleEndDrag(UIInventoryItem inventoryItemUI)
    //{
    //    ResetDraggedItem();
    //}

    //private void HandleSwap(UIInventoryItem inventoryItemUI)
    //{
    //    int index = listOfUIItems.IndexOf(inventoryItemUI);
    //    if (index == -1)
    //    {

    //        return;
    //    }

    //    OnSwapItems?.Invoke(currentlyDraggedItemIndex, index);
    //    HandleItemSelection(inventoryItemUI);
    //}

    //private void ResetDraggedItem()
    //{
    //    //mouseFollower.Toggle(false);
    //    //currentlyDraggedItemIndex = -1;
    //}

    //private void HandleBeginDrag(UIInventoryItem inventoryItemUI)
    //{
    //    int index = listOfUIItems.IndexOf(inventoryItemUI);
    //    if (index == -1) return;
    //    currentlyDraggedItemIndex = index;
    //    HandleItemSelection(inventoryItemUI);
    //    OnStartDragging?.Invoke(index);
    //}

    //public void CreatedDraggedItem(Sprite sprite, int quantity)
    //{
    //    mouseFollower.Toggle(true);
    //    mouseFollower.SetData(sprite, quantity);
    //}

    //private void HandleItemSelection(UIInventoryItem inventoryItemUI)
    //{
    //    int index = listOfUIItems.IndexOf(inventoryItemUI);
    //    if (index == -1) return;
    //    OnDescriptionRequested?.Invoke(index);
    //}

}