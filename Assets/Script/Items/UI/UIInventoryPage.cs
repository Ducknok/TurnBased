using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//View (V) in MVC
namespace Inventory.UI
{
    public enum InventoryTab
    {
        Consumable,
        Weapon,
        Armor,
        Shield,
        Ring
    }

    public class UIInventoryPage : DucMonobehaviour
    {
        [Header("LinkClass")]
        [SerializeField] private UIInventoryItem itemPrefab;
        [SerializeField] private RectTransform contentPanel;
        [SerializeField] public UIInventoryDescription itemDescription;

        [Header("ListOfUIItem")]
        List<UIInventoryItem> listOfUIItems = new List<UIInventoryItem>();
        private int currentIndex = 0; // Index của item đang chọn

        public event Action<int> OnItemActionRequested, OnDescriptionRequested;
        public event Action<InventoryTab> OnTabChanged;
        private Action<HeroStateMachine> onHeroSelectedCallback;

        private InventoryTab currentTab = InventoryTab.Consumable; // Tab mặc định

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
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                this.isSelectingHero = false;
                this.UnHighlightHeroButton(this.currentHeroIndex);
                this.SetSelectedIndex(0);
                HeroStateMachine selectedHero = CombatController.Instance.CBM.playersInCombat[currentHeroIndex].GetComponent<HeroStateMachine>();
                this.onHeroSelectedCallback?.Invoke(selectedHero);
            }
            else if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                this.isSelectingHero = false;
                this.UnHighlightHeroButton(this.currentHeroIndex);

                if (this.infoHeroPanelSpacer != null)
                {
                    this.infoHeroPanelSpacer.gameObject.SetActive(false);
                }

                this.SetSelectedIndex(this.currentIndex);
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
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (this.listOfUIItems.Count > 0 && this.listOfUIItems[currentIndex].gameObject.activeInHierarchy)
                {
                    this.OnItemActionRequested?.Invoke(this.currentIndex);
                }
            }
        }

        // =====================================
        // CHUYỂN TAB TỪ GIAO DIỆN (NÚT BẤM)
        // =====================================
        public void ClickConsumableTab() { ChangeTab(InventoryTab.Consumable); }
        public void ClickWeaponTab() { ChangeTab(InventoryTab.Weapon); }
        public void ClickShieldTab() { ChangeTab(InventoryTab.Shield); }
        public void ClickRingTab() { ChangeTab(InventoryTab.Ring); }

        private void ChangeTab(InventoryTab newTab)
        {
            if (this.currentTab == newTab) return;

            this.currentTab = newTab;

            // Xóa UI cũ trước khi load đồ mới
            this.itemDescription.ResetDescription();

            // Báo cho Controller biết để ném Data mới tương ứng với Tab này
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
            this.heroButtonSpacer.DetachChildren();

            this.heroButtons.Clear();

            for (int i = 0; i < CombatController.Instance.CBM.playersInCombat.Count; i++)
            {
                HeroStateMachine hero = CombatController.Instance.CBM.playersInCombat[i].GetComponent<HeroStateMachine>();
                Image newImage = Instantiate(this.heroImagePrefab, this.heroButtonSpacer);
                this.heroButtons.Add(newImage);

                this.itemDescription.SetHeroBarDescription(newImage, hero);
            }
        }

        // =====================================
        // KHỞI TẠO BẢNG THÔNG TIN HERO BÊN PHẢI (HERO BAR)
        // =====================================
        public void InitializeHeroBar(ItemSO item)
        {
            // --- FIX DỨT ĐIỂM: NHẬN DIỆN PANEL DỰA TRÊN LOẠI MÓN ĐỒ ---
            // Bất kể bạn bấm phím Q/E hay click chuột, loại món đồ luôn chính xác 100%
            if (item != null)
            {
                foreach (GameObject panel in this.heroInfoPanelList)
                {
                    if (panel == null) continue;

                    if (item is EquippableItemSO)
                    {
                        // Nếu là Vũ Khí, Khiên, Nhẫn -> Bật Weapon Panel
                        panel.SetActive(panel.name.Contains("Weapon"));
                    }
                    else
                    {
                        // Nếu là Thuốc -> Bật Consumable Panel
                        panel.SetActive(panel.name.Contains("Consumable"));
                    }
                }
            }

            this.infoHeroPanelSpacer.gameObject.SetActive(true);

            // Xóa sạch các Hero Bar cũ trong Spacer
            foreach (Transform child in this.infoHeroPanelSpacer)
            {
                Destroy(child.gameObject);
            }
            this.infoHeroPanelSpacer.DetachChildren();

            this.heroPanel.Clear();

            // Nhận diện xem có phải đang hiển thị Nhẫn không
            bool isRingUI = false;
            if (item != null && item is EquippableItemSO checkEq)
            {
                if (checkEq.itemType == ItemType.Ring) isRingUI = true;
            }
            else if (this.currentTab == InventoryTab.Ring)
            {
                isRingUI = true; // Fallback khi tab rỗng
            }

            for (int i = 0; i < CombatController.Instance.CBM.playersInCombat.Count; i++)
            {
                HeroStateMachine hero = CombatController.Instance.CBM.playersInCombat[i].GetComponent<HeroStateMachine>();

                Image selectedPrefab = null;

                if (isRingUI)
                {
                    // Chắc chắn lấy RingHeroBar (Prefab số 1)
                    if (this.heroInfoPanelPrefab.Count > 1)
                        selectedPrefab = this.heroInfoPanelPrefab[1];
                }
                else
                {
                    // Lấy WeaponHeroBar (Prefab số 0)
                    if (this.heroInfoPanelPrefab.Count > 0)
                        selectedPrefab = this.heroInfoPanelPrefab[0];
                }

                // Bảo hiểm nếu lỡ rớt Index
                if (selectedPrefab == null && this.heroInfoPanelPrefab.Count > 0)
                {
                    selectedPrefab = this.heroInfoPanelPrefab[0];
                }

                if (selectedPrefab != null)
                {
                    // Đẻ Prefab vào bên trong Spacer
                    Image newBar = Instantiate(selectedPrefab, this.infoHeroPanelSpacer);
                    this.heroPanel.Add(newBar);

                    // ĐỔ DỮ LIỆU
                    if (isRingUI)
                    {
                        this.SetupRingHeroBarUI(newBar, hero);
                    }
                    else if (item is EquippableItemSO equipItem) // Các Tab khác
                    {
                        if (hero.baseHero.heroType == equipItem.allowedWeapons || equipItem.allowedWeapons == HeroType.All)
                        {
                            this.itemDescription.SetATKDescription(newBar, hero, equipItem);
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

        // --- HÀM XỬ LÝ ĐỔ DỮ LIỆU VÀO ĐÚNG CẤU TRÚC RING HEROBAR CỦA BẠN ---
        private void SetupRingHeroBarUI(Image ringBar, HeroStateMachine hero)
        {
            // 1. Lấy danh sách các nhẫn Normal đang mặc
            List<EquippableItemSO> equippedRings = new List<EquippableItemSO>();
            AgentWeapon agentWeapon = hero.GetComponent<AgentWeapon>();

            if (agentWeapon != null)
            {
                foreach (var eqItem in agentWeapon.weaponItemSO)
                {
                    if (eqItem != null && eqItem.itemType == ItemType.Ring && eqItem.rarity == Rarity.Normal)
                    {
                        equippedRings.Add(eqItem);
                    }
                }
            }

            // 2. Gán Avatar khuôn mặt Hero vào ô HeroIcon
            Transform heroIconParent = ringBar.transform.Find("HeroIcon");
            if (heroIconParent != null)
            {
                Image hIcon = heroIconParent.Find("Icon")?.GetComponent<Image>();
                if (hIcon != null && hero.baseHero != null && hero.baseHero.heroImage != null)
                {
                    hIcon.sprite = hero.baseHero.heroImage;
                    hIcon.color = Color.white;
                }
            }

            // 3. Đổ dữ liệu vào ô Nhẫn 1 (RingBG1)
            Transform ringBG1 = ringBar.transform.Find("RingBG1");
            if (ringBG1 != null)
            {
                TextMeshProUGUI nameTxt = ringBG1.Find("RingName_txt")?.GetComponent<TextMeshProUGUI>();
                Image iconImg = ringBG1.Find("Icon")?.GetComponent<Image>();

                if (equippedRings.Count > 0)
                {
                    if (nameTxt != null) { nameTxt.text = equippedRings[0].Name.ToUpper(); nameTxt.color = Color.white; }
                    if (iconImg != null) { iconImg.sprite = equippedRings[0].ItemImage; iconImg.color = Color.white; }
                }
                else
                {
                    if (nameTxt != null) { nameTxt.text = "------"; nameTxt.color = new Color(0.5f, 0.5f, 0.5f, 1f); }
                    if (iconImg != null) { iconImg.color = new Color(1f, 1f, 1f, 0f); }
                }
            }

            // 4. Đổ dữ liệu vào ô Nhẫn 2 (RingBG2)
            Transform ringBG2 = ringBar.transform.Find("RingBG2");
            if (ringBG2 != null)
            {
                TextMeshProUGUI nameTxt = ringBG2.Find("RingName_txt")?.GetComponent<TextMeshProUGUI>();
                Image iconImg = ringBG2.Find("Icon")?.GetComponent<Image>();

                if (equippedRings.Count > 1)
                {
                    if (nameTxt != null) { nameTxt.text = equippedRings[1].Name.ToUpper(); nameTxt.color = Color.white; }
                    if (iconImg != null) { iconImg.sprite = equippedRings[1].ItemImage; iconImg.color = Color.white; }
                }
                else
                {
                    if (nameTxt != null) { nameTxt.text = "------"; nameTxt.color = new Color(0.5f, 0.5f, 0.5f, 1f); }
                    if (iconImg != null) { iconImg.color = new Color(1f, 1f, 1f, 0f); }
                }
            }
        }

        // =====================================
        // HÀM DỌN DẸP VÀ HIỂN THỊ UI
        // =====================================
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