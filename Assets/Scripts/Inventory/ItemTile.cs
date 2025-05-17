using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sketch.Inventory
{
    public class ItemTile : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField]
        private Image _bgItem, _item;

        [SerializeField]
        private TMP_Text _countText;

        public Sprite ItemSprite => _item.sprite;

        public InventoryItemInfo ContainedItem { private set; get; }
        public int Count { private set; get; }

        private void Awake()
        {
            _bgItem.gameObject.SetActive(false);
            _item.gameObject.SetActive(false);
            _countText.gameObject.SetActive(false);
        }

        /// <param name="item">null to clear</param>
        public void SetItem(InventoryItemInfo item)
        {
            ContainedItem = item;
            Count = item == null ? 0 : 1;

            _bgItem.gameObject.SetActive(item != null);
            _item.gameObject.SetActive(item != null);
            _countText.gameObject.SetActive(Count > 1);
            _countText.text = Count.ToString();

            if (item != null) _item.sprite = item.Sprite;
        }

        public void AddItem()
        {
            Count++;
            _countText.gameObject.SetActive(Count > 1);
            _countText.text = Count.ToString();
        }

        public void OnPointerDown(PointerEventData _)
        {
            if (ContainedItem != null)
            {
                InventoryManager.Instance.SetSelectedItem(this);
            }
        }
    }
}