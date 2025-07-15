using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;

    [TextArea]
    public string description;

    public GameObject worldPrefab;
    public GameObject equippedPrefab;
}
