using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sketch.Inventory
{
    public class ItemTile : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField]
        private Image _bgItem, _item;

        public Sprite ItemSprite => _item.sprite;

        private void Awake()
        {
            _bgItem.gameObject.SetActive(false);
            _item.gameObject.SetActive(false);
        }

        public void OnPointerDown(PointerEventData _)
        {
            InventoryManager.Instance.SetSelectedItem(this);
        }
    }
}