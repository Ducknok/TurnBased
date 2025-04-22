using Inventory.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Inventory;

public class MainInventoryController : MonoBehaviour
{
    public bool isMainInventoryOpen = false;
    [SerializeField] private ItemInventoryController itemInventoryController;
    [SerializeField] private EquipMenuController equipMenuController;
    [SerializeField] private SkillMenuController skillMenuController;
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
    [SerializeField] private CombatStateMachine cbm;
    [SerializeField] public UIInventoryDescription itemDescription;

    [Header("Button UI")]
    [SerializeField] private List<Button> buttonUI = new List<Button>();
    private int currentSelectedIndex = 0;
    private bool allowButtonNavigation = true;

    [Header("Hero UI")]
    [SerializeField] private GameObject infoHeroUIPrefab;
    [SerializeField] private Transform heroManageSpacer;
    private List<GameObject> heroUIList = new List<GameObject>();
    private bool isSelectingHero = false;
    private int currentHeroIndex = 0;
    [SerializeField] private Color selectedColor = new Color(0.8f, 0.8f, 0.8f, 1f);   // Xám nhạt
    [SerializeField] private Color unselectedColor = new Color(1f, 1f, 1f, 0f);     // Trắng nhạt (alpha thấp)

    [Header("MenuPanel")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject mainManagePanel;
    [SerializeField] private GameObject equipPanel;
    [SerializeField] private GameObject orderPanel;
    [SerializeField] private GameObject skillsPanel;
    [SerializeField] private GameObject itemsPanel;





    private void Start()
    {
        this.CreateHeroUI();
        if (buttonUI.Count > 0)
        {
            SelectButton(0); // Chọn button đầu tiên khi mở lên
        }
    }
    private void Update()
    {
        this.OpenMainMenu();
        // Khi đang ở menu chính
        if (!this.isSelectingHero && currentState == UIState.MainMenu)
        {
            HandleButtonNavigation();

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                isSelectingHero = true;
                currentHeroIndex = 0;
                HighlightHero(currentHeroIndex);
                allowButtonNavigation = false;
                LockSelectedButton();
            }
        }
        // Khi đang chọn hero
        else if (isSelectingHero && currentState == UIState.MainMenu)
        {
            HandleHeroNavigation();

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                isSelectingHero = false;
                ResetHeroHighlight();
                SelectButton(currentSelectedIndex);
            }

            LockSelectedButton(); // Giữ selection
        }
        // Khi đang ở trong một UI phụ như Equip, Skill, Item,...
        else if (IsInSubUI() && Input.GetKeyDown(KeyCode.LeftControl))
        {
            ReturnToMainMenu();
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
        //orderPanel.SetActive(false);
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

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            var hero = cbm.playersInCombat[currentHeroIndex].GetComponent<HeroStateMachine>();
            mainManagePanel.SetActive(false); // Ẩn menu chính

            switch (currentSelectedIndex)
            {
                case 0:
                    ShowEquipUI(hero);
                    break;
                case 1:
                    ShowSkillUI(hero);
                    break;
                case 2:
                    ShowItemUI(hero);
                    break;
                case 3:
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
        this.equipMenuController.isEquipMenuOpen = true;
        this.equipMenuController.LoadWeaponUI(hero);
        this.equipMenuController.LoadHero(hero);
        this.equipMenuController.LoadHeroStat(hero);
        this.equipMenuController.CreateHeroSwapButton(cbm);
        // equipUI.LoadForHero(hero);
    }

    private void ShowOrderUI(HeroStateMachine hero)
    {
        Debug.Log("Hiện order cho: " + hero.name);
        //this.currentState = UIState.OrderUI;
        // statsUI.SetActive(true);
        // statsUI.SetData(hero);
    }

    private void ShowSkillUI(HeroStateMachine hero)
    {
        Debug.Log("Hiện kỹ năng cho: " + hero.name);
        this.currentState = UIState.SkillsUI;
        skillsPanel.SetActive(true);
        this.skillMenuController.isSkillMenuOpen = true;
        this.skillMenuController.LoadHero(hero);
        this.skillMenuController.LoadSkillUI(hero);
        // skillUI.LoadSkills(hero);
    }

    private void ShowItemUI(HeroStateMachine hero)
    {
        Debug.Log("Hiện item cho: " + hero.name);
        this.currentState = UIState.ItemUI;
        this.itemInventoryController.isItemInventoryOpen = true;
        this.itemInventoryController.OpenItemInventory();
        itemsPanel.SetActive(true);
        // itemUI.Setup(hero);
    }

    private void HighlightHero(int index)
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
            UpdateButtonHighlight(index);
        }
    }

    private void UpdateButtonHighlight(int selectedIndex)
    {
        // Duyệt qua tất cả các button và thay đổi màu sắc của chúng
        for (int i = 0; i < buttonUI.Count; i++)
        {
            Image buttonImage = buttonUI[i].GetComponent<Image>();
            if (buttonImage != null)
            {
                // Nếu button đang được chọn, thay đổi màu sắc
                buttonImage.color = (i == selectedIndex) ? selectedColor : unselectedColor;
            }
        }
    }

    private void CreateHeroUI()
    {
        // Clear old buttons (nếu cần)
        //foreach (Transform child in this.heroManageSpacer)
        //{
        //    Destroy(child.gameObject);
        //}

        //this.heroUIList.Clear();
        for (int i = 0; i < 3; i++)
        {
            HeroStateMachine hero = (i < this.cbm.playersInCombat.Count)
                ? this.cbm.playersInCombat[i].GetComponent<HeroStateMachine>()
                : null;

            GameObject newHeroUIInCombat = Instantiate(this.infoHeroUIPrefab, this.heroManageSpacer.Find("PlayerInCombat"));
            GameObject newHeroUINotInCombat = Instantiate(this.infoHeroUIPrefab, this.heroManageSpacer.Find("PlayerNotInCombat"));

            this.heroUIList.Add(newHeroUIInCombat);
            this.itemDescription.SetHeroUIDescription(newHeroUIInCombat, hero);
            this.itemDescription.SetHeroUIDescription(newHeroUINotInCombat, hero);
        }
    }
    private void OpenMainMenu()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if(mainManagePanel.activeSelf)
            {
                mainManagePanel.SetActive(false);
                this.isMainInventoryOpen = false;
                this.equipMenuController.isEquipMenuOpen = false;
                this.skillMenuController.isSkillMenuOpen = false;
                this.itemInventoryController.isItemInventoryOpen = false;
                
            }
            else
            {
                
                mainManagePanel.SetActive(true);
                this.isMainInventoryOpen = true;
            }
        }
    }
}
