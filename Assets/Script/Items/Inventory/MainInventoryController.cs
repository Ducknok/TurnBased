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
    [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.3f, 0.4f);
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
            // Khi đang ở menu chính
            if (!this.isSelectingHero && currentState == UIState.MainMenu)
            {
                HandleButtonNavigation();
                this.ConfirmSelectHero();
            }
            // Khi đang chọn hero
            else if (isSelectingHero && currentState == UIState.MainMenu)
            {
                HandleHeroNavigation();
                this.UndoSelectHero();
                LockSelectedButton(); // Giữ selection
            }
            // Khi đang ở trong một UI phụ như Equip, Skill, Item,...
            else if (IsInSubUI() && Input.GetKeyDown(KeyCode.LeftControl) && !this.equipMenuController.isItemPanel)
            {
                ReturnToMainMenu();
            }
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
            HighlightHero(currentHeroIndex);
            LockSelectedButton();
        }
    }

    private void UndoSelectHero()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isSelectingHero = false;
            ResetHeroHighlight();
            SelectButton(currentSelectedIndex);
        }
    }

    // Kiểm tra xem đang ở UI phụ không
    private bool IsInSubUI()
    {
        return currentState == UIState.EquipUI || currentState == UIState.OrderUI ||
               currentState == UIState.SkillsUI || currentState == UIState.ItemUI;
    }

    // Quay lại menu chính
    private void ReturnToMainMenu()
    {
        this.itemInventoryController.isItemInventoryOpen = false;
        this.equipMenuController.isEquipMenuOpen = false;
        this.skillMenuController.isSkillMenuOpen = false;
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
                case 3:
                    this.mainManagePanel.SetActive(true);
                    ShowOrderUI(hero);
                    break;
                default:
                    Debug.LogWarning("Chưa có hành động cho lựa chọn này!");
                    break;
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

    private void ShowOrderUI(HeroStateMachine hero)
    {
        Debug.Log("Hiện order cho: " + hero.name);
        this.currentState = UIState.OrderUI;
        this.ReorderHero(currentHeroIndex);
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

    public void HighlightHero(int index)
    {
        for (int i = 0; i < heroUIList.Count; i++)
        {
            Image bg = heroUIList[i].GetComponent<Image>();
            if (bg != null)
            {
                bg.color = (i == index) ? selectedColor : unselectedColor;
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

            // Đảm bảo rằng button hiện tại được highlight
            //UpdateButtonHighlight(index);
        }
    }

    //private void UpdateButtonHighlight(int selectedIndex)
    //{
    //    // Duyệt qua tất cả các button và thay đổi màu sắc của chúng
    //    for (int i = 0; i < buttonUI.Count; i++)
    //    {
    //        Image buttonImage = buttonUI[i].GetComponent<Image>();
    //        if (buttonImage != null)
    //        {
    //            // Nếu button đang được chọn, thay đổi màu sắc
    //            buttonImage.color = (i == selectedIndex) ? selectedColor : unselectedColor;
    //        }
    //    }
    //}

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
    }

    public void ReorderHero(int selectedIndex)
    {
        var cbm = CombatController.Instance.CBM;

        if (selectedIndex < 0 || selectedIndex >= cbm.playersInCombat.Count) return;

        var selectedHero = cbm.playersInCombat[selectedIndex];
        cbm.playersInCombat.RemoveAt(selectedIndex);
        cbm.playersInCombat.Insert(0, selectedHero);

        Debug.Log("Đã đưa " + selectedHero.name + " lên đầu danh sách CBM.");

        // Đồng bộ lại PartyManager (nếu dùng hệ thống riêng)
        PartyManager.Instance.SyncWithCBM(cbm.playersInCombat);

        // Cập nhật leader
        PartyManager.Instance.SetLeader(selectedHero.GetComponent<HeroStateMachine>());
        CameraController.Instance.SetCameraFollowHero(selectedHero.GetComponent<HeroStateMachine>());

        Debug.Log("Leader mới: " + selectedHero.name);

        this.CreateHeroUI();
        currentHeroIndex = 0;
        this.HighlightHero(currentHeroIndex);
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

            }
            else
            {
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