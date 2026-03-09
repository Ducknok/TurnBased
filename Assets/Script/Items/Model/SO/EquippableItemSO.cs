using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu]
    // BỎ IDestroyableItem đi để Kho đồ không tự động xóa sai món đồ.
    public class EquippableItemSO : ItemSO, IItemAction
    {
        public HeroType allowedWeapons;
        public string ActionName => "Equip";
        public AudioClip actionSFX { get; private set; }

        public bool PerformAction(int inventoryIndex, GameObject character)
        {
            HeroStateMachine hero = character.GetComponent<HeroStateMachine>();
            if (hero != null && hero.baseHero.heroType != this.allowedWeapons && this.allowedWeapons != HeroType.All)
            {
                Debug.Log("This weapon cannot be equipped by this hero.");
                return false;
            }

            AgentWeapon weaponSystem = character.GetComponent<AgentWeapon>();
            if (weaponSystem != null)
            {
                // 1. KIỂM TRA: Dùng hàm HasEquippedItem thay cho HasSameWeapon cũ
                if (weaponSystem.HasEquippedItem(this))
                {
                    Debug.Log("Hero đã trang bị món này rồi, không cộng dồn!");
                    return false;
                }

                // 2. TÌM Ô TRANG BỊ PHÙ HỢP (Tìm món đồ cùng loại đang mặc, hoặc ô trống)
                int slotIndex = -1;
                for (int i = 0; i < weaponSystem.weaponItemSO.Length; i++)
                {
                    if (weaponSystem.weaponItemSO[i] != null && weaponSystem.weaponItemSO[i].itemType == this.itemType)
                    {
                        slotIndex = i;
                        break;
                    }
                }

                // Nếu không có món đồ cùng loại nào đang mặc, tìm ô trống đầu tiên
                if (slotIndex == -1)
                {
                    for (int i = 0; i < weaponSystem.weaponItemSO.Length; i++)
                    {
                        if (weaponSystem.weaponItemSO[i] == null)
                        {
                            slotIndex = i;
                            break;
                        }
                    }
                }

                if (slotIndex != -1)
                {
                    // 3. Trừ chỉ số của món đồ cũ ở ô này (nếu có)
                    //EquippableItemSO oldItem = weaponSystem.weaponItemSO[slotIndex];
                    //if (oldItem != null)
                    //{
                    //    foreach (var mod in oldItem.Modifiers)
                    //    {
                    //        mod.stat.AffectCharacter(character, -mod.val1, -mod.val2);
                    //    }
                    //}

                    //// 4. Cộng chỉ số của món đồ mới
                    //foreach (var mod in this.Modifiers)
                    //{
                    //    mod.stat.AffectCharacter(character, mod.val1, mod.val2);
                    //}

                    // 5. Gán món đồ mới vào ô (Hàm SetWeapon sẽ tự động trả đồ cũ vào Kho)
                    weaponSystem.SetWeapon(slotIndex, this);
                    return true;
                }
                else
                {
                    Debug.LogWarning("Không tìm thấy ô trang bị phù hợp cho loại đồ này!");
                    return false;
                }
            }
            return false;
        }
    }
}