using UnityEngine;
using UnityEngine.UI;

namespace Sketch.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _itemTilePrefab;

        [SerializeField]
        private Image _dragItem;

        [SerializeField]
        private RectTransform _inventoryTransform;

        [SerializeField]
        private int _inventoryTileCount = 50;

        public static InventoryManager Instance { private set; get; }

        public void SetSelectedItem(ItemTile tile)
        {
            _dragItem.gameObject.SetActive(true);
            _dragItem.sprite = tile.ItemSprite;
        }

        private void Awake()
        {
            Instance = this;

            _dragItem.gameObject.SetActive(false);
            for (int i = 0; i < _inventoryTileCount; i++)
            {
                Instantiate(_itemTilePrefab, _inventoryTransform);
            }
        }
    }
}