using Inventory.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Inventory;
using UnityEngine.SceneManagement;

public class MainInventoryController : DucMonobehaviour
{
    public bool isMainInventoryOpen = false;
    [SerializeField] protected ItemInventoryController itemInventoryController;
    public ItemInventoryController ItemInventoryController => itemInventoryController;
    [SerializeField] protected EquipMenuController equipMenuController;
    public EquipMenuController EquipMenuController => equipMenuController;
    [SerializeField] private SkillMenuController skillMenuController;
    public SkillMenuController SkillMenuController => skillMenuController;

    private enum UIState
    {
        MainMenu,      // Đang ở menu chính
        HeroSelecting, // Đang chọn hero
        EquipUI,
        OrderUI,
        SkillsUI,
        ItemUI
    }

    private UIState currentState = UIState.MainMenu;
    [SerializeField] public UIInventoryDescription itemDescription;

    [Header("Button UI")]
    [SerializeField] private List<Button> buttonUI = new List<Button>();
    private int currentSelectedIndex = 0;

    [Header("Hero UI")]
    [SerializeField] private GameObject infoHeroUIPrefab;
    [SerializeField] private Transform heroManageSpacer;
    private List<GameObject> heroUIList = new List<GameObject>();
    private bool isSelectingHero = false;
    private int currentHeroIndex = 0;

    // ---- THÊM: BIẾN CHO HỆ THỐNG ORDER 2 BƯỚC VÀ XUNG ĐỘT CTRL ----
    private bool wasSelectingHeroInItemUI = false; // Trí nhớ cho frame trước
    private int sourceSwapIndex = -1; // -1 nghĩa là chưa khóa Hero nào
    [SerializeField] private Color lockedColor = new Color(1f, 0.6f, 0f, 0.6f); // Màu Cam đậm khi Hero bị khóa chờ đổi

    [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.3f, 0.4f); // Màu Vàng sáng khi trỏ chuột
    [SerializeField] private Color unselectedColor = new Color(1f, 1f, 1f, 0f);

    [Header("MenuPanel")]
    [SerializeField] private GameObject mainManagePanel;
    [SerializeField] private GameObject equipPanel;
    [SerializeField] private GameObject skillsPanel;
    [SerializeField] private GameObject itemsPanel;

    protected override void Start()
    {
        if (buttonUI.Count > 0)
        {
            SelectButton(0); // Chọn button đầu tiên khi mở lên
        }
    }

    protected override void Update()
    {
        this.CheckState();
    }

    protected override void Awake()
    {
        this.LoadComponent();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.LoadComponent();
    }

    public override void CheckState()
    {
        if (CombatController.Instance.CBZ.isInCombat) return;
        else
        {
            this.OpenMainMenu();

            // ---- FIX LỖI XUNG ĐỘT PHÍM CTRL (CHỈ XỬ LÝ BẢNG CHỌN HERO) ----
            bool isItemHeroSelecting = false;

            if (currentState == UIState.ItemUI)
            {
                // Dùng FindObjectOfType quét toàn bộ màn hình để đảm bảo 100% tìm thấy UIInventoryPage
                UIInventoryPage invUI = FindAnyObjectByType<UIInventoryPage>();
                if (invUI != null)
                {
                    isItemHeroSelecting = invUI.isSelectingHero;
                }
            }

            // Khi đang ở menu chính
            if (!this.isSelectingHero && currentState == UIState.MainMenu)
            {
                HandleButtonNavigation();
                this.ConfirmSelectHero();
            }
            // Khi đang chọn hero
            else if (isSelectingHero && (currentState == UIState.MainMenu || currentState == UIState.OrderUI))
            {
                HandleHeroNavigation();
                this.UndoSelectHero();
                LockSelectedButton(); // Giữ selection
            }
            // Khi đang ở trong một UI phụ như Equip, Skill, Item,...
            else if (IsInSubUI() && Input.GetKeyDown(KeyCode.LeftControl) && !this.equipMenuController.isItemPanel)
            {
                // Nếu đang ở ItemUI và (đang ở bảng chọn Hero HOẶC frame trước vừa mới chọn Hero)
                if (currentState == UIState.ItemUI && (isItemHeroSelecting || wasSelectingHeroInItemUI))
                {
                    // LỚP 1: BỎ QUA - Giữ nguyên giao diện Items, nhường cho UIInventoryPage tự Hủy chọn Hero
                }
                else
                {
                    // LỚP 2: Đang rảnh rỗi lướt Items, bấm Ctrl thì mới thoát hẳn ra Menu Tổng
                    ReturnToMainMenu();
                }
            }

            // Lưu lại trạng thái của frame này để dùng cho frame tiếp theo
            this.wasSelectingHeroInItemUI = isItemHeroSelecting;
        }
    }

    //LoadComponent
    public void LoadComponent()
    {
        this.LoadItemInventoryController();
        this.LoadEquipMenuController();
        this.LoadSkillMenuController();
    }

    private void LoadItemInventoryController()
    {
        if (this.itemInventoryController != null) return;
        this.itemInventoryController = ItemInventoryController.Instance;
    }

    private void LoadEquipMenuController()
    {
        if (this.equipMenuController != null) return;
        this.equipMenuController = FindAnyObjectByType<EquipMenuController>();
    }

    private void LoadSkillMenuController()
    {
        if (this.skillMenuController != null) return;
        this.skillMenuController = FindAnyObjectByType<SkillMenuController>();
    }

    //Confirm && Undo select hero
    private void ConfirmSelectHero()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            isSelectingHero = true;
            currentHeroIndex = 0;
            sourceSwapIndex = -1; // Reset trạng thái khóa khi mới vào

            // Ép trạng thái sang OrderUI nếu nút đang chọn bên trái là nút Order (Index 3)
            if (currentSelectedIndex == 3)
            {
                currentState = UIState.OrderUI;
            }

            HighlightHero(currentHeroIndex);
            LockSelectedButton();
        }
    }

    private void UndoSelectHero()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            // Bẫy lỗi: Nếu đang Order mà đã KHÓA 1 Hero -> Hủy khóa Hero đó chứ ko thoát menu
            if (currentState == UIState.OrderUI && sourceSwapIndex != -1)
            {
                sourceSwapIndex = -1;
                HighlightHero(currentHeroIndex);
                return;
            }

            isSelectingHero = false;
            currentState = UIState.MainMenu;
            ResetHeroHighlight();
            SelectButton(currentSelectedIndex);
        }
    }

    // Kiểm tra xem đang ở UI phụ không
    private bool IsInSubUI()
    {
        return currentState == UIState.EquipUI ||
               currentState == UIState.SkillsUI || currentState == UIState.ItemUI;
    }

    // Quay lại menu chính
    private void ReturnToMainMenu()
    {
        this.itemInventoryController.isItemInventoryOpen = false;
        this.equipMenuController.isEquipMenuOpen = false;
        this.skillMenuController.isSkillMenuOpen = false;

        // Thoát thì reset luôn index đang swap
        this.sourceSwapIndex = -1;

        currentState = UIState.MainMenu;
        CloseAllPanels();

        mainManagePanel.SetActive(true);
        isSelectingHero = false;
        ResetHeroHighlight();
        SelectButton(currentSelectedIndex);
    }

    // Ẩn tất cả các UI phụ
    private void CloseAllPanels()
    {
        equipPanel.SetActive(false);
        skillsPanel.SetActive(false);
        itemsPanel.SetActive(false);
    }

    private void LockSelectedButton()
    {
        // Chỉ set lại selected GameObject đúng 1 lần
        EventSystem.current.SetSelectedGameObject(buttonUI[currentSelectedIndex].gameObject);
    }

    private void HandleButtonNavigation()
    {
        if (buttonUI.Count == 0 || this.isSelectingHero == true) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSelectedIndex = (currentSelectedIndex - 1 + buttonUI.Count) % buttonUI.Count;
            SelectButton(currentSelectedIndex);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSelectedIndex = (currentSelectedIndex + 1) % buttonUI.Count;
            SelectButton(currentSelectedIndex);
        }
    }

    private void HandleHeroNavigation()
    {
        if (heroUIList.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentHeroIndex = (currentHeroIndex - 1 + heroUIList.Count) % heroUIList.Count;
            HighlightHero(currentHeroIndex);

        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentHeroIndex = (currentHeroIndex + 1) % heroUIList.Count;
            HighlightHero(currentHeroIndex);
        }

        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            // ---- XỬ LÝ 2 BƯỚC CHO TAB ORDER TẠI ĐÂY ----
            if (currentState == UIState.OrderUI)
            {
                if (sourceSwapIndex == -1)
                {
                    // Bước 1: Khóa Hero đầu tiên
                    sourceSwapIndex = currentHeroIndex;
                    HighlightHero(currentHeroIndex);
                }
                else
                {
                    // Bước 2: Bấm Enter vào Hero thứ 2 -> Đổi chỗ
                    if (sourceSwapIndex != currentHeroIndex)
                    {
                        SwapHeroes(sourceSwapIndex, currentHeroIndex);
                    }

                    // Tráo xong thì nhả khóa ra
                    sourceSwapIndex = -1;
                    HighlightHero(currentHeroIndex);
                }
                return; // Dừng, không chạy các lệnh mở UI khác
            }

            var hero = CombatController.Instance.CBM.playersInCombat[currentHeroIndex].GetComponent<HeroStateMachine>();

            switch (currentSelectedIndex)
            {
                case 0:
                    this.mainManagePanel.SetActive(false); // Ẩn menu chính
                    this.equipMenuController.isEquipMenuOpen = true;
                    ShowEquipUI(hero);
                    break;
                case 1:
                    this.mainManagePanel.SetActive(false);
                    this.skillMenuController.isSkillMenuOpen = true;
                    ShowSkillUI(hero);
                    break;
                case 2:
                    this.mainManagePanel.SetActive(false);
                    this.itemInventoryController.isItemInventoryOpen = true;
                    ShowItemUI(hero);
                    break;
                    // Case 3 (Order) đã được xử lý ở IF trên cùng rồi.
            }
        }
    }

    private void ShowEquipUI(HeroStateMachine hero)
    {
        Debug.Log("Hiện giao diện trang bị cho: " + hero.name);
        this.currentState = UIState.EquipUI;
        equipPanel.SetActive(true);

        this.equipMenuController.LoadHero(hero);
        this.equipMenuController.LoadHeroStat(hero);
        this.equipMenuController.CreateHeroSwapButton(CombatController.Instance.CBM, hero);
    }

    private void ShowSkillUI(HeroStateMachine hero)
    {
        Debug.Log("Hiện kỹ năng cho: " + hero.name);
        this.currentState = UIState.SkillsUI;
        skillsPanel.SetActive(true);
        this.skillMenuController.LoadHero(hero);
        this.skillMenuController.LoadSkillUI(hero);
        this.skillMenuController.CreateHeroSwapButton(CombatController.Instance.CBM, hero);
    }

    private void ShowItemUI(HeroStateMachine hero)
    {
        Debug.Log("Hiện item cho: " + hero.name);
        this.currentState = UIState.ItemUI;
        this.itemInventoryController.OpenItemInventory();
        itemsPanel.SetActive(true);
    }

    // ĐÃ UPDATE: Hàm Highlight kết hợp 2 màu (Màu Khóa và Màu Trỏ)
    public void HighlightHero(int index)
    {
        for (int i = 0; i < heroUIList.Count; i++)
        {
            Image bg = heroUIList[i].GetComponent<Image>();
            if (bg != null)
            {
                if (i == sourceSwapIndex)
                {
                    // Hero đang bị khóa -> Tô màu Cam
                    bg.color = lockedColor;
                }
                else if (i == index)
                {
                    // Hero đang được trỏ chuột tới -> Tô màu Vàng
                    bg.color = selectedColor;
                }
                else
                {
                    // Các Hero khác -> Trong suốt
                    bg.color = unselectedColor;
                }
            }
        }
    }

    private void ResetHeroHighlight()
    {
        foreach (var heroUI in heroUIList)
        {
            Image bg = heroUI.GetComponent<Image>();
            if (bg != null)
                bg.color = unselectedColor;
        }
    }

    private void SelectButton(int index)
    {
        // Chọn lại button và thay đổi màu sắc ngay lập tức
        if (index >= 0 && index < buttonUI.Count)
        {
            EventSystem.current.SetSelectedGameObject(buttonUI[index].gameObject);
        }
    }

    public void CreateHeroUI()
    {
        // Clear old buttons (nếu cần)
        var heroInCombat = this.heroManageSpacer.Find("HeroInCombat");
        var heroNotInCombat = this.heroManageSpacer.Find("HeroNotInCombat");
        if (heroInCombat == null)
        {
            Debug.LogWarning("Không tìm thấy HeroInCombat trong heroManageSpacer");
            return;
        }

        foreach (Transform child in heroInCombat)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in heroNotInCombat)
        {
            Destroy(child.gameObject);
        }
        this.heroUIList.Clear();
        for (int i = 0; i < 3; i++)
        {
            HeroStateMachine hero = (i < CombatController.Instance.CBM.playersInCombat.Count)
                ? CombatController.Instance.CBM.playersInCombat[i].GetComponent<HeroStateMachine>()
                : null;

            GameObject newHeroUIInCombat = Instantiate(this.infoHeroUIPrefab, this.heroManageSpacer.Find("HeroInCombat"));
            GameObject newHeroUINotInCombat = Instantiate(this.infoHeroUIPrefab, this.heroManageSpacer.Find("HeroNotInCombat"));

            this.heroUIList.Add(newHeroUIInCombat);
            this.itemDescription.SetHeroUIDescription(newHeroUIInCombat, hero);
            this.itemDescription.SetHeroUIDescription(newHeroUINotInCombat, hero);
        }

        // FIX UI: CHỈ HIỂN THỊ VIỀN KHI ĐÃ SANG BƯỚC CHỌN HERO
        if (this.isSelectingHero)
        {
            this.HighlightHero(currentHeroIndex);
        }
        else
        {
            this.ResetHeroHighlight();
        }
    }

    // Hàm Swap Xịn Xò đổi chỗ trực tiếp
    public void SwapHeroes(int indexA, int indexB)
    {
        var cbm = CombatController.Instance.CBM;

        var temp = cbm.playersInCombat[indexA];
        cbm.playersInCombat[indexA] = cbm.playersInCombat[indexB];
        cbm.playersInCombat[indexB] = temp;

        PartyManager.Instance.SyncWithCBM(cbm.playersInCombat);

        // 3. Nếu vị trí Đầu Đoàn (Index 0) bị thay đổi, cập nhật ngay Leader & Camera
        if (indexA == 0 || indexB == 0)
        {
            var newLeader = cbm.playersInCombat[0].GetComponent<HeroStateMachine>();
            PartyManager.Instance.SetLeader(newLeader);
            CameraController.Instance.SetCameraFollowHero(newLeader);
            Debug.Log("Leader mới: " + newLeader.name);
        }

        // 4. Vẽ lại UI ngay lập tức
        this.CreateHeroUI();
    }

    private void OpenMainMenu()
    {
        if (this.itemInventoryController.isItemInventoryOpen || this.equipMenuController.isEquipMenuOpen || this.skillMenuController.isSkillMenuOpen) return;
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (mainManagePanel.activeSelf)
            {
                mainManagePanel.SetActive(false);
                this.isMainInventoryOpen = false;
                this.equipMenuController.isEquipMenuOpen = false;
                this.skillMenuController.isSkillMenuOpen = false;
                this.itemInventoryController.isItemInventoryOpen = false;

                this.sourceSwapIndex = -1; // Reset Order khi đóng Tab
                this.isSelectingHero = false;
            }
            else
            {
                // Reset trạng thái chọn Hero về false trước khi tạo UI
                this.isSelectingHero = false;

                this.CreateHeroUI();
                mainManagePanel.SetActive(true);
                this.isMainInventoryOpen = true;

                // FIX: Ép hệ thống chọn nút Equip (Index 0) mỗi khi vừa bấm TAB mở lên
                this.currentSelectedIndex = 0;
                this.SelectButton(this.currentSelectedIndex);
            }
        }
    }
}