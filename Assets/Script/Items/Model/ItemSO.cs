using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Model (M) in MVC
namespace Inventory.Model
{
    [CreateAssetMenu]
    public class ItemSO : ScriptableObject
    {
        public int MyProperty { get; set; }
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

    }
}

