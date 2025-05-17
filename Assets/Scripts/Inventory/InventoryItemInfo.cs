using UnityEngine;

namespace Sketch.Inventory
{
    [CreateAssetMenu(menuName = "ScriptableObject/Inventory/InventoryItemInfo", fileName = "InventoryItemInfo")]
    public class InventoryItemInfo : ScriptableObject
    {
        public string Name;

        public Sprite Sprite;
        public int MaxStackSize;
    }
}