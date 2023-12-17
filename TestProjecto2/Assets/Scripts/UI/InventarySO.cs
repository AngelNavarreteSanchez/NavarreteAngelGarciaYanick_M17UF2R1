using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;

namespace Inventory.Model
{
    [CreateAssetMenu]
    public class InventarySO : ScriptableObject
    {
        [SerializeField] private List<InventoryItem> inventoryItems;
        [field: SerializeField] public int Size { get; private set; } = 16;
        public event Action<Dictionary<int, InventoryItem>> OnInventoryUpdated;
        public void Initialize()
        {
            inventoryItems = new List<InventoryItem>();
            for (int i = 0; i < Size; i++)
            {
                inventoryItems.Add(InventoryItem.GetEmptyItem());
            }
        }
        public int AddItem(ItemTemplate item, int quantity)
        {
            if(item.IsStackable == false)
            {
                for (int i = 0; i < inventoryItems.Count; i++)
                {
                    if (IsInventoryFull()) return quantity;
                    while(quantity > 0 && IsInventoryFull() == false)
                    {
                        quantity -= AddItemToFirstFreeSlot(item,1);
                        
                    }
                    InformAboutChange();
                    return quantity;
                    
                }
            }
            quantity = AddStackableItem(item,quantity);
            InformAboutChange();
            return quantity;
            
        }

        private int AddItemToFirstFreeSlot(ItemTemplate item, int quantity)
        {
            InventoryItem newItem = new InventoryItem
            {
                item = item,
                quantity = quantity
            };
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].IsEmpty || inventoryItems[i].item == null)
                {
                    inventoryItems[i] = newItem;
                    return quantity;
                }
            }
            return 0;
        }

        private bool IsInventoryFull() => inventoryItems.Where(x => x.IsEmpty || x.item == null).Any() == false;

        private int AddStackableItem(ItemTemplate item, int quantity)
        {
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].IsEmpty || inventoryItems[i].item == null) continue;
                if (inventoryItems[i].item.ID == item.ID)
                {
                    int amountPossibleToTake = inventoryItems[i].item.MaxStackSize - inventoryItems[i].quantity;
                    
                    if (quantity > amountPossibleToTake)
                    {
                        inventoryItems[i] = inventoryItems[i].ChangeQuantity(inventoryItems[i].item.MaxStackSize);
                        quantity -= amountPossibleToTake;
                    }
                    else
                    {
                        inventoryItems[i] = inventoryItems[i].ChangeQuantity(inventoryItems[i].quantity+quantity);
                        InformAboutChange();
                        return 0;
                    }
                }
            }
            while(quantity > 0 && IsInventoryFull() == false)
            {
                int newQuantity = Mathf.Clamp(quantity, 0, item.MaxStackSize);
                quantity -= newQuantity;
                AddItemToFirstFreeSlot(item,newQuantity);
            }
            return quantity;
        }

        public void AddItem(InventoryItem item)
        {
            AddItem(item.item, item.quantity);
        }
        public Dictionary<int, InventoryItem> GetCurrentInventoryState()
        {
            Dictionary<int, InventoryItem> returnValue = new Dictionary<int, InventoryItem>();
            for (int i = 0; i < Size; i++)
            {
                if (inventoryItems[i].IsEmpty || inventoryItems[i].item == null) continue;
                returnValue[i] = inventoryItems[i];
            }
            return returnValue;
        }

        public InventoryItem GetItemAt(int itemIndex)
        {
            return inventoryItems[itemIndex];
        }

        

        public void SwapItems(int itemIndex1, int itemIndex2)
        {
            InventoryItem item1 = inventoryItems[itemIndex1];
            inventoryItems[itemIndex1] = inventoryItems[itemIndex2];
            inventoryItems[itemIndex2] = item1;
            InformAboutChange();
        }

        public void InformAboutChange()
        {
            OnInventoryUpdated?.Invoke(GetCurrentInventoryState());
        }

        public void RemoveItem(int itemIndex, int amount)
        {
            if (inventoryItems.Count > itemIndex)
            {
                if (inventoryItems[itemIndex].IsEmpty || inventoryItems[itemIndex].item == null) return;
                int reminder = inventoryItems[itemIndex].quantity-amount;
                if (reminder <= 0) inventoryItems[itemIndex] = InventoryItem.GetEmptyItem();
                else inventoryItems[itemIndex] = inventoryItems[itemIndex].ChangeQuantity(reminder);
                InformAboutChange();
            }
        }
    }
    [Serializable]
    public struct InventoryItem
    {
        public int quantity;
        public ItemTemplate item;

        public bool IsEmpty { get; internal set; }

        public InventoryItem ChangeQuantity(int newQuantity)
        {
            return new InventoryItem
            {
                item = this.item,
                quantity = newQuantity,
            };
        }
        public static InventoryItem GetEmptyItem() => new InventoryItem
        {
            item = null,
            quantity = 0,
        };
    }

}

