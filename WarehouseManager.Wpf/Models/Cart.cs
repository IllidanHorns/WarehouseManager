using System.Collections.ObjectModel;
using System.Linq;
using WarehouseManager.Wpf.Models;

namespace WarehouseManager.Wpf.Models
{
    public class Cart
    {
        private readonly ObservableCollection<CartItem> _items = new();

        public ObservableCollection<CartItem> Items => _items;

        public decimal TotalPrice => _items.Sum(item => item.TotalPrice);

        public int TotalItems => _items.Sum(item => item.Quantity);

        public void AddItem(CartItem item)
        {
            var existingItem = _items.FirstOrDefault(i => 
                i.Product.Id == item.Product.Id && 
                i.WarehouseId == item.WarehouseId);

            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                _items.Add(item);
            }
        }

        public void RemoveItem(CartItem item)
        {
            _items.Remove(item);
        }

        public void UpdateQuantity(CartItem item, int quantity)
        {
            if (quantity <= 0)
            {
                RemoveItem(item);
            }
            else
            {
                item.Quantity = quantity;
            }
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool IsEmpty => _items.Count == 0;
    }
}

