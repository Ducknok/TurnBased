using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//Model (M) in MVC
namespace Inventory.Model
{
    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        Ring,
        Miscellaneous
    }
    public enum Rarity
    {
        Normal, 
        Rare,
        Epic,
    }
    public abstract class ItemSO : ScriptableObject
    {

        [field: SerializeField]
        public ItemType itemType;
        [field: SerializeField]
        public Rarity rarity;
        [field: SerializeField]
        public bool IsStackable { get; set; }
        public int ID => GetInstanceID();

        [field: SerializeField]
        public int MaxStackSize { get; set; } = 99;
        [field: SerializeField]
        public string Name { get; set; }
        [field: SerializeField]
        public string ReceiveEffect { get; set; }
        [field: SerializeField]
        [field: TextArea]
        public string Description { get; set; }
        [field: SerializeField]
        public Sprite ItemImage { get; set; }
        [field: SerializeField]
        public List<ItemParameter> DefaultParameterList { get; set; }
        [field: SerializeField]
        public List<ModifierData> Modifiers { get; set; } = new List<ModifierData>();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying || string.IsNullOrWhiteSpace(Name)) return;
            string assetPath = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(assetPath)) return;
            string currentFileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            if (currentFileName != Name)
            {
                EditorApplication.delayCall += () =>
                {
                    if (this == null) return;
                    AssetDatabase.RenameAsset(assetPath, Name);
                    AssetDatabase.SaveAssets();
                };
            }
        }
#endif

    }
    [Serializable]
    public struct ItemParameter: IEquatable<ItemParameter>
    {
        public ItemParameterSO itemParameter;
        public float value;

        public bool Equals(ItemParameter other)
        {
            return other.itemParameter == itemParameter;
        }
    }
    [Serializable]
    public class ModifierData
    {
        public CharacterStatModifierSO stat;
        public int val1;
        public int val2;
    }
}

