using Inventory.UI;
using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//Controller (C) in MVC
namespace Inventory
{
    public class ItemInventoryController : Singleton<ItemInventoryController>
    {
        public bool isItemInventoryOpen = false;
        [SerializeField] private Transform inventory;
        [SerializeField] public UIInventoryPage inventoryUI;

        [SerializeField] private InventorySO inventoryData;
        public List<InventoryItem> initialItems = new List<InventoryItem>();
        private List<InventoryItem> currentFilteredItems = new List<InventoryItem>();

        [SerializeField] public List<Button> inventoryTabs = new List<Button>();
        private int currentButtonIndex = 0;

        protected override void Awake()
        {
            base.Awake();
            this.LoadInventory();
            this.PrepareUI(); // FIX LỖI: Chuyển PrepareUI lên Awake để sự kiện được nối mạng sớm nhất
        }

        protected override void Start()
        {
            PrepareInventoryData();
        }

        protected override void Update()
        {
            if (!this.isItemInventoryOpen)
            {
                return;
            }
            this.SwitchButtonInput();
        }

        public void LoadInventory()
        {
            this.inventory = GameObject.Find("BattleCanvas").transform.Find("UIMainInventory").transform.Find("UIInventory");
            this.inventoryUI = this.inventory.GetComponent<UIInventoryPage>();
        }

        private void PrepareInventoryData()
        {
            this.inventoryData.Initialize();
            this.inventoryData.OnInventoryUpdated += UpdateInventoryUI;
            foreach (InventoryItem item in this.initialItems)
            {
                if (item.IsEmpty) continue;

                this.inventoryData.AddItem(item);
            }
        }

        private void UpdateInventoryUI(Dictionary<int, InventoryItem> inventoryState)
        {
            this.UpdateFilteredInventoryUI(this.currentButtonIndex);
        }

        public void PrepareUI()
        {
            this.inventoryUI.InitializeInventoryUI(this.inventoryData.Size);
            this.inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
            this.inventoryUI.OnItemActionRequested += HandleItemActionRequest;
        }

        public void PerformAction(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= currentFilteredItems.Count)
                return;

            InventoryItem inventoryItem = this.currentFilteredItems[itemIndex];

            IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
            if (destroyableItem != null)
            {
                this.inventoryData.RemoveItem(inventoryItem.item, 1);
                this.UpdateFilteredInventoryUI(currentButtonIndex);
            }

            IItemAction itemAction = inventoryItem.item as IItemAction;
            if (itemAction != null)
            {
                itemAction.PerformAction(itemIndex, this.gameObject);

                if (itemIndex >= currentFilteredItems.Count || currentFilteredItems[itemIndex].IsEmpty)
                {
                    this.UpdateFilteredInventoryUI(currentButtonIndex);
                    this.inventoryUI.ResetSelection();
                }
            }
        }

        private void HandleItemActionRequest(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= currentFilteredItems.Count)
            {
                inventoryUI.ResetSelection();
                return;
            }

            InventoryItem inventoryItem = currentFilteredItems[itemIndex];
            if (inventoryItem.IsEmpty)
            {
                inventoryUI.ResetSelection();
                return;
            }

            IItemAction itemAction = inventoryItem.item as IItemAction;
            IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;

            if (itemAction != null)
            {
                this.inventoryUI.StartHeroSelection((HeroStateMachine selectedHero) =>
                {
                    if (itemAction.ActionName == "Consume" &&
                        selectedHero.baseHero.curHP == selectedHero.baseHero.baseHP &&
                        selectedHero.baseHero.curMP == selectedHero.baseHero.baseMP)
                        return;
                    itemAction.PerformAction(itemIndex, selectedHero.gameObject);

                    this.inventoryUI.InitializeHeroButton();
                    this.inventoryUI.InitializeHeroBar(inventoryItem.item);
                    selectedHero.GetComponent<HeroPanelHandler>().UpdateHeroPanel();
                    HealController.Instance.HPBar(selectedHero);
                    ManaController.Instance.UpdateManaBar(selectedHero);


                    if (destroyableItem != null)
                    {
                        this.inventoryData.RemoveItem(inventoryItem.item, 1);
                    }

                    var inventoryState = this.inventoryData.GetCurrentInventoryState();
                    bool isItemStillExist = false;
                    foreach (var item in inventoryState.Values)
                    {
                        if (item.item == inventoryItem.item && item.quantity > 0)
                        {
                            isItemStillExist = true;
                            // Cập nhật lại đúng cái số nhỏ nhỏ góc dưới ô đồ
                            this.inventoryUI.UpdateData(itemIndex, item.item.ItemImage, item.item.Name, item.quantity);
                            break;
                        }
                    }

                    if (!isItemStillExist)
                    {
                        UpdateFilteredInventoryUI(this.currentButtonIndex);
                        this.inventoryUI.ResetSelection();
                    }
                });
            }
        }

        private void HandleDescriptionRequest(int itemIndex)
        {
            if (this.currentFilteredItems == null || this.currentFilteredItems.Count == 0 || itemIndex >= this.currentFilteredItems.Count)
            {
                inventoryUI.ResetSelection();
                return;
            }

            InventoryItem inventoryItem = this.currentFilteredItems[itemIndex];

            if (inventoryItem.IsEmpty)
            {
                inventoryUI.ResetSelection();
                return;
            }

            ItemSO item = inventoryItem.item;
            this.inventoryUI.UpdateItemDescription(itemIndex, item.ItemImage, item.Name, item.ReceiveEffect, item.Description);
            this.inventoryUI.InitializeHeroBar(item);
        }

        private void UpdateFilteredInventoryUI(int index)
        {
            this.inventoryUI.ClearAndHideAllItems();

            var inventoryState = this.inventoryData.GetCurrentInventoryState();
            this.currentFilteredItems.Clear();

            string selectedTab = this.inventoryTabs[index].gameObject.name;
            ItemSO firstFilteredItem = null;

            foreach (var item in inventoryState)
            {
                if (item.Value.IsEmpty) continue;

                // if (item.Value.isEquipped) continue; 

                if (item.Value.item.itemType.ToString() == selectedTab)
                {
                    this.currentFilteredItems.Add(item.Value);

                    if (firstFilteredItem == null)
                    {
                        firstFilteredItem = item.Value.item as ItemSO;
                    }
                }
            }

            foreach (GameObject bar in this.inventoryUI.heroInfoPanelList)
            {
                bool shouldShow = bar.name == selectedTab;
                bar.SetActive(shouldShow);

                if (shouldShow)
                {
                    this.inventoryUI.InitializeHeroBar(firstFilteredItem);
                }
            }

            for (int i = 0; i < this.currentFilteredItems.Count; i++)
            {
                this.inventoryUI.UpdateData(i, this.currentFilteredItems[i].item.ItemImage, this.currentFilteredItems[i].item.Name, this.currentFilteredItems[i].quantity);
            }

            if (this.currentFilteredItems.Count > 0)
            {
                StartCoroutine(ForceSelectFirstItemDelayed());
            }
            else
            {
                this.inventoryUI.ResetSelection();
            }
        }


        private IEnumerator ForceSelectFirstItemDelayed()
        {
            // Đợi đến những mili-giây cuối cùng của frame (WaitForEndOfFrame)
            yield return new WaitForEndOfFrame();

            if (this.inventoryUI != null && this.currentFilteredItems.Count > 0)
            {
                // Chọn item đầu tiên và ép nó gọi lệnh hiển thị Mô tả
                this.inventoryUI.SetSelectedIndex(0);
                this.HandleDescriptionRequest(0);
            }
        }

        private void SwitchButton(int direction)
        {
            int previousTabIndex = currentButtonIndex;
            currentButtonIndex += direction;

            if (currentButtonIndex < 0)
                currentButtonIndex = inventoryTabs.Count - 1;
            else if (currentButtonIndex >= inventoryTabs.Count)
                currentButtonIndex = 0;

            if (previousTabIndex != currentButtonIndex)
            {
                inventoryTabs[previousTabIndex].transform.Find("Background").gameObject.GetComponent<Image>().enabled = false;
                inventoryTabs[currentButtonIndex].transform.Find("Background").gameObject.GetComponent<Image>().enabled = true;
                UpdateFilteredInventoryUI(currentButtonIndex);
            }
        }

        private void SwitchButtonInput()
        {
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SwitchButton(-1);
            }
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                SwitchButton(1);
            }
        }

        private void RefreshCurrentTab()
        {
            this.inventoryTabs[currentButtonIndex].transform.Find("Background").gameObject.GetComponent<Image>().enabled = true;
            UpdateFilteredInventoryUI(currentButtonIndex);
        }

        public void OpenItemInventory()
        {
            this.inventoryUI.InitializeHeroButton();
            this.inventoryUI.Show();
            this.RefreshCurrentTab();
        }

        public List<EquippableItemSO> GetEquipableItemsByType(ItemType type)
        {
            var result = new List<EquippableItemSO>();
            var inventory = inventoryData.GetCurrentInventoryState();

            foreach (var item in inventory.Values)
            {
                if (item.item is EquippableItemSO equipItem && equipItem.itemType == type && !item.IsEmpty)
                {
                    result.Add(equipItem);
                }
            }

            return result;
        }
    }
}