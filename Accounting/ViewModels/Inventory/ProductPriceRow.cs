using Accounting.DTO;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.ViewModels.Inventory
{
    internal class ProductPriceRow : BaseViewModel
    {
        private decimal _editablePrice;

        public ProductDTO Product { get; }

        public decimal EditablePrice
        {
            get => _editablePrice;
            set => SetProperty(ref _editablePrice, value);
        }

        public IRelayCommand SaveCommand { get; }

        internal ProductPriceRow(ProductDTO product, Func<ProductPriceRow, Task> saveHandler)
        {
            Product = product ?? throw new ArgumentNullException(nameof(product));
            _editablePrice = product.CurrentSellingPrice;
            if (saveHandler == null) throw new ArgumentNullException(nameof(saveHandler));
            SaveCommand = new RelayCommand(async () => await saveHandler(this));
        }

        // NEW: public helper to notify UI about product-level changes
        public void NotifyProductChanged()
        {
            // call protected OnPropertyChanged from inside the class (allowed)
            OnPropertyChanged(nameof(Product));
        }
    }
}



