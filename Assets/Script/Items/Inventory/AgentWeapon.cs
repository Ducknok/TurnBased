using Inventory.Model;
using System;
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

    protected override void Awake()
    {
        base.Awake();

        // ========================================================
        // FIX BUG: TẠO BẢN SAO ĐỂ BẢO VỆ SCRIPTABLE OBJECT GỐC
        // ========================================================
        HeroStateMachine hero = this.GetComponent<HeroStateMachine>();
        if (hero != null && hero.baseHero != null)
        {
            // Kiểm tra xem nó đã là bản Clone chưa, nếu chưa thì tạo bản Clone
            if (!hero.baseHero.name.Contains("(Clone)"))
            {
                hero.baseHero = Instantiate(hero.baseHero);
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(WaitAndApplyInitialStats());
    }

    private IEnumerator WaitAndApplyInitialStats()
    {
        yield return null;
        this.ApplyInitialEquipmentStats();
    }

    private void ApplyInitialEquipmentStats()
    {
        if (weaponItemSO == null) return;

        // Vì các hàm AffectCharacter đã tự lo việc đồng bộ (như bạn cấu hình trong SO)
        // Nên ở đây ta chỉ cần gọi lệnh cộng đơn thuần là xong!
        foreach (var item in weaponItemSO)
        {
            if (item != null && item.Modifiers != null)
            {
                foreach (var mod in item.Modifiers)
                {
                    if (mod.stat != null)
                    {
                        mod.stat.AffectCharacter(this.gameObject, mod.val1, mod.val2);
                    }
                }
                Debug.Log($"[AgentWeapon] Đã nạp chỉ số của {item.Name} cho {gameObject.name}");
            }
        }
    }

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
        if (index < 0 || index >= weaponItemSO.Length)
        {
            Debug.LogError("Vị trí ô trang bị không hợp lệ!");
            return;
        }

        EquippableItemSO oldItem = weaponItemSO[index];

        // 1. TRỪ CHỈ SỐ CỦA MÓN ĐỒ CŨ (VÀ TRẢ LẠI VÀO KHO)
        if (oldItem != null)
        {
            if (oldItem.Modifiers != null)
            {
                foreach (var mod in oldItem.Modifiers)
                {
                    if (mod.stat != null)
                    {
                        // Trừ chỉ số bằng cách truyền giá trị âm (-val)
                        mod.stat.AffectCharacter(this.gameObject, -mod.val1, -mod.val2);
                    }
                }
            }

            this.inventoryData.AddItem(oldItem, 1);
            Debug.Log($"Đã tháo {oldItem.Name}, trừ chỉ số và trả vào kho.");
        }

        // 2. MẶC ĐỒ MỚI VÀO Ô ĐÓ (VÀ CỘNG CHỈ SỐ LÊN)
        this.weaponItemSO[index] = newItem;

        if (newItem != null)
        {
            if (newItem.Modifiers != null)
            {
                foreach (var mod in newItem.Modifiers)
                {
                    if (mod.stat != null)
                    {
                        // Cộng chỉ số mới
                        mod.stat.AffectCharacter(this.gameObject, mod.val1, mod.val2);
                    }
                }
            }
            Debug.Log($"Đã trang bị {newItem.Name} vào ô số {index} và cộng chỉ số.");
        }
    }
}