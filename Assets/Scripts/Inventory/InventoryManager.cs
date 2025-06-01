using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Sketch.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        [SerializeField, Tooltip("Prefab of the square that contains an item")]
        private GameObject _itemTilePrefab;

        [SerializeField, Tooltip("Object used to preview the item we are drag&dropping")]
        private Image _dragItem;

        [SerializeField, Tooltip("Transform of the inventory object")]
        private RectTransform _inventoryTransform;

        [SerializeField, Tooltip("Number of tiles we spawn")]
        private int _inventoryTileCount = 50;

        [SerializeField]
        private InventoryItemInfo[] _defaultItems;

        public static InventoryManager Instance { private set; get; }

        private readonly List<ItemTile> _tiles = new();

        // Tile we are currently drag and dropping
        public ItemTile DraggingTile { private set; get; }

        public ItemTile HoverredTile { set; get; }

        public void SetSelectedItem(ItemTile tile)
        {
            DraggingTile = tile;
            _dragItem.gameObject.SetActive(true);
            _dragItem.sprite = tile.ItemSprite;
        }

        public void ClearSelectedItem()
        {
            DraggingTile = null;
            _dragItem.gameObject.SetActive(false);
        }

        public bool TryAddItem(InventoryItemInfo item)
        {
            var matchingTile = _tiles.FirstOrDefault(x => x.ContainedItem != null && x.ContainedItem.Name == item.Name && x.Count < item.MaxStackSize);
            if (matchingTile == null)
            {
                var firstFree = _tiles.FirstOrDefault(x => x.ContainedItem == null);
                if (firstFree == null) return false; // No space left!

                firstFree.SetItem(item); // Tile was empty, we add the item
            }
            else matchingTile.AddItem(); // Add item to a stack

            return true;
        }

        private void Awake()
        {
            Instance = this;

            _dragItem.gameObject.SetActive(false);
            for (int i = 0; i < _inventoryTileCount; i++)
            {
                var go = Instantiate(_itemTilePrefab, _inventoryTransform);
                _tiles.Add(go.GetComponent<ItemTile>());
            }

            foreach (var i in _defaultItems)
            {
                TryAddItem(i);
            }
        }
    }
}