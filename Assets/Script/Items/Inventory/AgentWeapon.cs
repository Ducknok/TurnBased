using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentWeapon : DucMonobehaviour
{
    [Header("Danh sách Trang bị hiện tại")]
    [SerializeField]
    public EquippableItemSO[] weaponItemSO; // Mảng chứa Vũ khí, Áo giáp, Nhẫn...

    [Header("Kho đồ")]
    [SerializeField]
    private InventorySO inventoryData;

    // Hàm kiểm tra xem món đồ này có đang được mặc ở bất kỳ ô nào không
    public bool HasEquippedItem(EquippableItemSO itemToCheck)
    {
        if (itemToCheck == null) return false;

        for (int i = 0; i < weaponItemSO.Length; i++)
        {
            if (weaponItemSO[i] == itemToCheck)
            {
                return true;
            }
        }
        return false;
    }

    // Hàm gán trang bị vào 1 vị trí cụ thể (Slot Index)
    public void SetWeapon(int index, EquippableItemSO newItem)
    {
        // Kiểm tra an toàn tránh lỗi vượt quá giới hạn mảng
        if (index < 0 || index >= weaponItemSO.Length)
        {
            Debug.LogError("Vị trí ô trang bị không hợp lệ!");
            return;
        }

        // Lấy món đồ cũ đang nằm ở vị trí này ra
        EquippableItemSO oldItem = weaponItemSO[index];

        // 1. NẾU CÓ ĐỒ CŨ -> TRẢ LẠI VÀO KHO
        if (oldItem != null)
        {
            this.inventoryData.AddItem(oldItem, 1);
            Debug.Log($"Đã tháo {oldItem.Name} và trả vào kho.");
        }

        // 2. MẶC ĐỒ MỚI VÀO Ô ĐÓ
        this.weaponItemSO[index] = newItem;

        if (newItem != null)
        {
            Debug.Log($"Đã trang bị {newItem.Name} vào ô số {index}.");
        }
    }
}