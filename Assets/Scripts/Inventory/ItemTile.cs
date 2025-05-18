using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sketch.Inventory
{
    public class ItemTile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
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
        public void SetItem(InventoryItemInfo item, int count = 1)
        {
            ContainedItem = item;
            Count = item == null ? 0 : count;

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

        public void OnPointerUp(PointerEventData _)
        {
            if (InventoryManager.Instance.DraggingTile != null)
            {
                if (ContainedItem == null) // Nothing here, we just move the item
                {
                    Debug.Log("No item");
                    SetItem(InventoryManager.Instance.DraggingTile.ContainedItem, InventoryManager.Instance.DraggingTile.Count);
                    InventoryManager.Instance.DraggingTile.SetItem(null);
                }
                else if (ContainedItem.Name == InventoryManager.Instance.DraggingTile.ContainedItem.Name)
                {
                    Debug.Log("Same item");
                }
                else
                {
                    Debug.Log("Switch item");
                    (InventoryItemInfo TmpItem, int TmpCount) = (ContainedItem, Count);
                    SetItem(InventoryManager.Instance.DraggingTile.ContainedItem, InventoryManager.Instance.DraggingTile.Count);
                    InventoryManager.Instance.DraggingTile.SetItem(TmpItem, TmpCount);
                }
                InventoryManager.Instance.ClearSelectedItem();
            }
        }
    }
}