using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//View (V) in MVC
namespace Inventory.UI
{
    public enum InventoryTab
    {
        Consumable,
        Weapon
    }

    public class UIInventoryPage : DucMonobehaviour
    {
        [Header("LinkClass")]
        [SerializeField] private UIInventoryItem itemPrefab;
        [SerializeField] private RectTransform contentPanel;
        [SerializeField] public UIInventoryDescription itemDescription;

        [Header("ListOfUIItem")]
        List<UIInventoryItem> listOfUIItems = new List<UIInventoryItem>();
        private int currentIndex = 0; 

        public event Action<int> OnItemActionRequested, OnDescriptionRequested;
        public event Action<InventoryTab> OnTabChanged; 
        private Action<HeroStateMachine> onHeroSelectedCallback;

        private InventoryTab currentTab = InventoryTab.Consumable;

        [Header("ItemActionPanel")]
        [SerializeField] private ItemActionPanel itemActionPanel;

        [Header("HeroButton")]
        [SerializeField] private Image heroImagePrefab;
        [SerializeField] private Transform heroButtonSpacer;
        private List<Image> heroButtons = new List<Image>();
        public bool isSelectingHero = false;
        private int currentHeroIndex = 0;

        [Header("ItemInfoPanel")]
        [SerializeField] public List<GameObject> heroInfoPanelList = new List<GameObject>();
        [SerializeField] public List<Image> heroInfoPanelPrefab = new List<Image>();
        [SerializeField] public Transform infoHeroPanelSpacer;
        private List<Image> heroPanel = new List<Image>();

        protected override void Awake()
        {
            Hide();
        }

        protected override void Start()
        {
            if (this.itemDescription != null)
            {
                this.itemDescription.ResetDescription();
            }
            this.UpdateSelection();
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
            HighlightHeroButton(this.currentHeroIndex);
        }

        private void SelectHero()
        {
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                this.currentHeroIndex = (this.currentHeroIndex + 1) % this.heroButtons.Count;
                HighlightHeroButton(this.currentHeroIndex);
            }
            else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                this.currentHeroIndex = (this.currentHeroIndex - 1 + this.heroButtons.Count) % this.heroButtons.Count;
                HighlightHeroButton(this.currentHeroIndex);
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                this.isSelectingHero = false;
                this.UnHighlightHeroButton(this.currentHeroIndex);
                this.SetSelectedIndex(0);

                // Bẫy lỗi an toàn khi chọn Hero
                if (this.currentHeroIndex >= 0 && this.currentHeroIndex < CombatController.Instance.CBM.playersInCombat.Count)
                {
                    HeroStateMachine selectedHero = CombatController.Instance.CBM.playersInCombat[currentHeroIndex].GetComponent<HeroStateMachine>();
                    this.onHeroSelectedCallback?.Invoke(selectedHero);
                }
            }
            else if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                this.isSelectingHero = false;
                this.UnHighlightHeroButton(this.currentHeroIndex);

                if (this.infoHeroPanelSpacer != null)
                {
                    this.infoHeroPanelSpacer.gameObject.SetActive(false);
                }

                if (this.listOfUIItems != null && this.currentIndex >= 0 && this.currentIndex < this.listOfUIItems.Count)
                {
                    this.SetSelectedIndex(this.currentIndex);
                }
                else
                {
                    this.SetSelectedIndex(0);
                }
            }
        }

        private void SelectItem()
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveSelection(-1);
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveSelection(1);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Q))
            {
                SwitchToPreviousTab();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.E))
            {
                SwitchToNextTab();
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                if (this.listOfUIItems.Count > 0 && this.listOfUIItems[currentIndex].gameObject.activeInHierarchy)
                {
                    this.OnItemActionRequested?.Invoke(this.currentIndex);
                }
            }
        }

        private void SwitchToNextTab()
        {
            int totalTabs = Enum.GetValues(typeof(InventoryTab)).Length;

            int nextTabIndex = ((int)currentTab + 1) % totalTabs;
            ChangeTab((InventoryTab)nextTabIndex);
        }

        private void SwitchToPreviousTab()
        {
            int totalTabs = Enum.GetValues(typeof(InventoryTab)).Length;

            // Chuyển về Tab trước đó (nếu đang ở đầu thì nhảy xuống cuối)
            int prevTabIndex = ((int)currentTab - 1 + totalTabs) % totalTabs;
            ChangeTab((InventoryTab)prevTabIndex);
        }

        private void ChangeTab(InventoryTab newTab)
        {
            if (this.currentTab == newTab) return; 

            this.currentTab = newTab;

            this.itemDescription.ResetDescription();

            this.OnTabChanged?.Invoke(this.currentTab);
        }

        private void HighlightHeroButton(int index)
        {
            for (int i = 0; i < heroButtons.Count; i++)
            {
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
            if (this.listOfUIItems.Count == 0) return;

            int activeItemCount = 0;
            for (int i = 0; i < this.listOfUIItems.Count; i++)
            {
                if (this.listOfUIItems[i].gameObject.activeInHierarchy)
                {
                    activeItemCount++;
                }
            }

            if (activeItemCount == 0) return;

            this.listOfUIItems[currentIndex].Deselect();

            this.currentIndex += direction;
            if (this.currentIndex < 0)
                this.currentIndex = activeItemCount - 1;
            else if (this.currentIndex >= activeItemCount)
                this.currentIndex = 0;

            UpdateSelection();
        }

        private void UpdateSelection()
        {
            this.listOfUIItems[this.currentIndex].Select();
            this.OnDescriptionRequested?.Invoke(this.currentIndex);
        }

        public void InitializeInventoryUI(int inventorySize)
        {
            for (int i = 0; i < inventorySize; i++)
            {
                UIInventoryItem uiItem = Instantiate(this.itemPrefab, Vector3.zero, Quaternion.identity);
                uiItem.transform.SetParent(this.contentPanel);
                uiItem.transform.localScale = Vector3.one;


                uiItem.gameObject.SetActive(false);

                this.listOfUIItems.Add(uiItem);
            }
        }

        public void InitializeHeroButton()
        {
            foreach (Transform child in this.heroButtonSpacer)
            {
                Destroy(child.gameObject);
            }

            this.heroButtons.Clear();

            for (int i = 0; i < CombatController.Instance.CBM.playersInCombat.Count; i++)
            {
                HeroStateMachine hero = CombatController.Instance.CBM.playersInCombat[i].GetComponent<HeroStateMachine>();
                Image newImage = Instantiate(this.heroImagePrefab, this.heroButtonSpacer);
                this.heroButtons.Add(newImage);

                this.itemDescription.SetHeroBarDescription(newImage, hero);
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

                    if (item is EquippableItemSO weapon)
                    {
                        if (hero.baseHero.heroType == weapon.allowedWeapons)
                        {
                            this.itemDescription.SetATKDescription(newBar, hero, weapon);
                        }
                        else
                        {
                            this.itemDescription.SetATKDescription(newBar, hero, null);
                        }
                    }
                    else
                    {
                        this.itemDescription.SetATKDescription(newBar, hero, null);
                    }
                }
            }
        }

        public void ClearAndHideAllItems()
        {
            foreach (var item in this.listOfUIItems)
            {
                item.ResetData();
                item.Deselect();
                item.gameObject.SetActive(false); // Ẩn ô đi
            }
        }

        public void UpdateData(int itemIndex, Sprite itemImage, string itemName, int itemQuantity)
        {
            if (listOfUIItems.Count > itemIndex)
            {
                listOfUIItems[itemIndex].gameObject.SetActive(true); // Có data thì bật ô lên
                listOfUIItems[itemIndex].SetData(itemImage, itemName, itemQuantity);
            }
        }

        private void HandleShowItemActions(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1) return;
            OnItemActionRequested?.Invoke(index);
        }

        public void Show()
        {
            this.gameObject.SetActive(true);
            ResetSelection();

            // Ép buộc tải lại tab hiện tại khi bật túi đồ lên
            OnTabChanged?.Invoke(this.currentTab);
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
            if (listOfUIItems.Count == 0 || !listOfUIItems[index].gameObject.activeInHierarchy) return;

            listOfUIItems[currentIndex].Deselect();
            currentIndex = index;
            listOfUIItems[currentIndex].Select();
            OnDescriptionRequested?.Invoke(currentIndex);
        }

        public void Hide()
        {
            this.itemActionPanel.Toggle(false);
            gameObject.SetActive(false);
        }

        internal void UpdateItemDescription(int itemIndex, Sprite itemImage, string name, string receiveEffect, string description)
        {
            this.itemDescription.SetItemDescription(itemImage, name, receiveEffect, description);
            DeselectAllItems();
            this.listOfUIItems[itemIndex].Select();
        }
    }
}