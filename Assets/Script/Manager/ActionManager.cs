using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

public class ActionManager : MonoBehaviour
{
    
    public GameObject actionPanel; // Panel chứa các button
    private List<Button> buttons = new List<Button>();
    private int selectedIndex = 0;

    void Start()
    {
        RefreshButtons();
    }

    void Update()
    {
        if (buttons.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            selectedIndex = (selectedIndex + 1) % buttons.Count;
            SelectButton(selectedIndex);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            selectedIndex = (selectedIndex - 1 + buttons.Count) % buttons.Count;
            SelectButton(selectedIndex);
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            buttons[selectedIndex].onClick.Invoke();
        }
    }

    public void RefreshButtons()
    {
        // Xóa danh sách cũ
        buttons.Clear();

        // Lấy tất cả Button con trong Action Panel
        Button[] foundButtons = actionPanel.transform.Find("ActionSpacer").GetComponentsInChildren<Button>();
        foreach (Button btn in foundButtons)
        {
            buttons.Add(btn);
        }

        // Đặt focus vào button đầu tiên nếu có
        if (buttons.Count > 0)
        {
            selectedIndex = 0;
            SelectButton(selectedIndex);
        }
    }

    private void SelectButton(int index)
    {
        EventSystem.current.SetSelectedGameObject(buttons[index].gameObject);
    }

}
