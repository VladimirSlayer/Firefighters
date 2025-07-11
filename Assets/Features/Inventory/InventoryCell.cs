using UnityEngine;
using UnityEngine.UI;

public class InventoryCell : MonoBehaviour
{
    public Image iconImage;
    public GameObject iconContainer;

    private Item item;

    public void SetItem(Item newItem)
    {
        item = newItem;

        if (item != null)
        {
            iconContainer.SetActive(true);
            iconImage.sprite = item.icon;
        }
        else
        {
            Clear();
        }
    }

    public void Clear()
    {
        item = null;
        iconContainer.SetActive(false);
        iconImage.sprite = null;
    }

    public bool IsEmpty()
    {
        return item == null;
    }

    public Item GetItem()
    {
        return item;
    }
}
