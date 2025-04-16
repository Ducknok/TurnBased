using Inventory.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainInventoryController : MonoBehaviour
{
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
        if (!this.isSelectingHero)
        {
            HandleButtonNavigation();  // Chỉ điều hướng khi đang chọn item

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                isSelectingHero = true;
                currentHeroIndex = 0;
                HighlightHero(currentHeroIndex);

                SetButtonUIInteractable(false);

                // ✅ Giữ lại highlight, nhưng tắt điều hướng
                allowButtonNavigation = false;
            }

        }
        else
        {
            HandleHeroNavigation(); // Điều hướng Hero UI

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                isSelectingHero = false;
                ResetHeroHighlight();

                SetButtonUIInteractable(true);

                allowButtonNavigation = true; // ✅ Cho điều hướng lại
                SelectButton(currentSelectedIndex);
            }

        }
        Debug.LogWarning(isSelectingHero);
    }
    private void SetButtonUIInteractable(bool state)
    {
        foreach (var button in buttonUI)
        {
            button.interactable = state;
        }
    }

    private void HandleButtonNavigation()
    {
        if (!allowButtonNavigation || buttonUI.Count == 0) return;

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
            Debug.Log("Selected Hero: " + currentHeroIndex);
            // Xử lý trang bị item cho hero[currentHeroIndex] nếu cần
        }
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
}
