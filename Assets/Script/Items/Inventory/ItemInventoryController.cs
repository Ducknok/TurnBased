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
        //TODO: audio drop item here
        //[SerializeField]
        //private AudioClip dropClip;
        //[SerializeField]
        //private AudioSource audioSource;

        protected override void Start()
        {
            PrepareUI();
            PrepareInventoryData(); 
        }
        protected override void Awake()
        {
            base.Awake();
            this.LoadInventory();
            
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
            this.inventory = GameObject.Find("BattleCanvas").transform.Find("MainInventory").transform.Find("InventoryMenu");
            this.inventoryUI = this.inventory.GetComponent<UIInventoryPage>();
            this.RefreshCurrentTab();
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
            this.inventoryUI.ResetAllItems();
            foreach (var item in inventoryState)
            {
                this.inventoryUI.UpdateData(item.Key, item.Value.item.ItemImage,
                    item.Value.quantity);
            }
        }
        public void PrepareUI()
        {
            this.inventoryUI.InitializeInventoryUI(this.inventoryData.Size);
            //this.inventoryUI.OnStartDragging += HandleStartDragging;
            //this.inventoryUI.OnSwapItems += HandleSwapItems;
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
                // Không hiển thị nút gì cả — chuyển luôn sang chọn Hero
                this.inventoryUI.StartHeroSelection((HeroStateMachine selectedHero) =>
                {
                    if (itemAction.ActionName == "Consume" &&
                        selectedHero.baseHero.curHP == selectedHero.baseHero.baseHP &&
                        selectedHero.baseHero.curMP == selectedHero.baseHero.baseMP)
                        return;
                    // Nếu là item tiêu hao thì xóa nó khỏi inventory
                    if (destroyableItem != null)
                    {
                        this.inventoryData.RemoveItem(inventoryItem.item, 1);
                        UpdateFilteredInventoryUI(this.currentButtonIndex);
                    }
                    // Gọi hiệu ứng item lên hero
                    itemAction.PerformAction(itemIndex, selectedHero.gameObject);
                    // Reset UI sau khi dùng
                    this.inventoryUI.ResetSelection();
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
            this.inventoryUI.ResetAllItems();
            var inventoryState = this.inventoryData.GetCurrentInventoryState();
            this.currentFilteredItems.Clear();

            // Lấy tên tab hiện tại (để so sánh với itemType)
            string selectedTab = this.inventoryTabs[index].gameObject.name;
            ItemSO firstFilteredItem = null;

            foreach (var item in inventoryState)
            {
                if (item.Value.item.itemType.ToString() == selectedTab)
                {
                    this.currentFilteredItems.Add(item.Value);

                    // Lưu item đầu tiên (nếu có)
                    if (firstFilteredItem == null)
                    {
                        firstFilteredItem = item.Value.item as ItemSO;
                    }
                }
            }
            // Hiện panel phù hợp và gọi InitializeHeroBar(chỉ gọi 1 lần)
            foreach (GameObject bar in this.inventoryUI.heroInfoPanelList)
            {
                bool shouldShow = bar.name == selectedTab;
                bar.SetActive(shouldShow);

                if (shouldShow)
                {
                    this.inventoryUI.InitializeHeroBar(firstFilteredItem); // Truyền item vào để tính toán
                }
            }

            // Cập nhật UI slot
            for (int i = 0; i < this.currentFilteredItems.Count; i++)
            {
                this.inventoryUI.UpdateData(i, this.currentFilteredItems[i].item.ItemImage, this.currentFilteredItems[i].quantity);
            }

            // Set lại selected index
            if (this.currentFilteredItems.Count > 0)
            {
                this.inventoryUI.SetSelectedIndex(0);
            }
            else
            {
                this.inventoryUI.ResetSelection();
            }
        }
        //đổi qua lại giữa các button
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
                //Debug.LogWarning(inventoryTabs[previousTabIndex].transform.Find("Background"));
                inventoryTabs[previousTabIndex].transform.Find("Background").gameObject.GetComponent<Image>().enabled = false;
                inventoryTabs[currentButtonIndex].transform.Find("Background").gameObject.GetComponent<Image>().enabled = true;
                UpdateFilteredInventoryUI(currentButtonIndex);
            }
        }
        private void SwitchButtonInput()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                SwitchButton(-1); // Chuyển về trái
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                SwitchButton(1);  // Chuyển sang phải
            }
        }
        private void RefreshCurrentTab()
        {
            this.inventoryTabs[currentButtonIndex].transform.Find("Background").gameObject.gameObject.GetComponent<Image>().enabled = true;
            UpdateFilteredInventoryUI(currentButtonIndex);
        }
        public void OpenItemInventory()
        {
            //Debug.LogError("hello");
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

        //private void HandleSwapItems(int itemIndex1, int itemIndex2)
        //{
        //    this.inventoryData.SwapItems(itemIndex1, itemIndex2);
        //}
        //private void HandleStartDragging(int itemIndex)
        //{
        //    InventoryItem inventoryItem = this.inventoryData.GetItemAt(itemIndex);
        //    if (inventoryItem.IsEmpty) return;
        //    this.inventoryUI.CreatedDraggedItem(inventoryItem.item.ItemImage, inventoryItem.quantity);
        //}
        //private void DropItem(int itemIndex, int quantity)
        //{
        //    this.inventoryData.RemoveItem(itemIndex, quantity);
        //    this.inventoryUI.ResetSelection();
        //    //this.audioSource.PlayOnShot(dropClip);
        //}
        //public string PrepareDescription(InventoryItem inventoryItem)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.Append(inventoryItem.item.Description);
        //    //sb.AppendLine();
        //    for (int i = 0; i < inventoryItem.itemState.Count; i++)
        //    {
        //        sb.Append($" {inventoryItem.itemState[i].itemParameter.ParameterName}" + 
        //            $": {inventoryItem.itemState[i].value}/" +
        //            $"{inventoryItem.item.DefaultParameterList[i].value}");
        //        //sb.AppendLine();
        //    }
        //    return sb.ToString();
        //}

        //Lọc item theo button
        //private void UpdateFilteredInventoryUI(int index)
        //{
        //    this.inventoryUI.ResetAllItems();
        //    var inventoryState = this.inventoryData.GetCurrentInventoryState();
        //    this.currentFilteredItems.Clear();  // Xóa danh sách cũ trước khi thêm dữ liệu mới

        //    // Lọc các item đúng loại với tab đang chọn
        //    foreach (var item in inventoryState)
        //    {
        //        if (item.Value.item.itemType.ToString() == this.inventoryTabs[index].gameObject.name)
        //        {                   
        //            this.currentFilteredItems.Add(item.Value);
        //            foreach(GameObject bar in this.inventoryUI.heroInfoPanelList)
        //            {
        //                if (item.Value.item.itemType.ToString() == bar.name)
        //                {
        //                    bar.gameObject.SetActive(true);
        //                    //this.inventoryUI.InitializeHeroBar(item as ItemSO);
        //                }
        //                else bar.gameObject.SetActive(false);
        //            }
        //        }
        //    }
        //    // Cập nhật UI: Đưa item lọc được vào các slot đầu tiên
        //    for (int i = 0; i < this.currentFilteredItems.Count; i++)
        //    {
        //        this.inventoryUI.UpdateData(i, this.currentFilteredItems[i].item.ItemImage, this.currentFilteredItems[i].quantity);
        //    }
        //    // Đặt lại selection về item đầu tiên nếu có item
        //    if (this.currentFilteredItems.Count > 0)
        //    {
        //        this.inventoryUI.SetSelectedIndex(0);

        //    }
        //    else
        //    {
        //        this.inventoryUI.ResetSelection();
        //        //Debug.Log("No items after filter, resetting selection");
        //    }
        //} 
    }
}