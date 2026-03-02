using Inventory.Model;

[System.Serializable]
public class HeroEquipment
{
    public EquippableItemSO weapon;
    public EquippableItemSO armor;
    public EquippableItemSO ring;

    public ItemSO GetByType(ItemType type)
    {
        return type switch
        {
            ItemType.Weapon => weapon,
            ItemType.Armor => armor,
            ItemType.Ring => ring,
            _ => null
        };
    }

    public void SetByType(ItemType type, EquippableItemSO item)
    {
        switch (type)
        {
            case ItemType.Weapon: weapon = item; break;
            case ItemType.Armor: armor = item; break;
            case ItemType.Ring: ring = item; break;
        }
    }
}
