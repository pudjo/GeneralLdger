using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DTO.AR
{
    public class PenjualanDetailDTO : INotifyPropertyChanged
    {
            private int _id;
            private int _penjualanId;
            private int _productId;
            private string _productName = string.Empty;
            private decimal _quantity;
            private decimal _price;
            private decimal _sellingPrice;
            private string? _note;

            public int Id { get => _id; set { if (_id == value) return; _id = value; OnPropertyChanged(nameof(Id)); } }
            public int PenjualanId { get => _penjualanId; set { if (_penjualanId == value) return; _penjualanId = value; OnPropertyChanged(nameof(PenjualanId)); } }

            public int ProductId
            {
                get => _productId;
                set
                {
                    if (_productId == value) return;
                    _productId = value;
                    OnPropertyChanged(nameof(ProductId));
                }
            }

            public string ProductName
            {
                get => _productName;
                set
                {
                    if (_productName == value) return;
                    _productName = value;
                    OnPropertyChanged(nameof(ProductName));
                }
            }

            public decimal Quantity
            {
                get => _quantity;
                set
                {
                    if (_quantity == value) return;
                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(Total));
                }
            }

            public decimal Price
            {
                get => _price;
                set
                {
                    if (_price == value) return;
                    _price = value;
                    OnPropertyChanged(nameof(Price));
                    OnPropertyChanged(nameof(Total));
                }
            }

            public decimal SellingPrice
            {
                get => _sellingPrice;
                set
                {
                    if (_sellingPrice == value) return;
                    _sellingPrice = value;
                    OnPropertyChanged(nameof(SellingPrice));
                    OnPropertyChanged(nameof(Total));
                }
            }

            public string? Note
            {
                get => _note;
                set
                {
                    if (_note == value) return;
                    _note = value;
                    OnPropertyChanged(nameof(Note));
                }
            }

            // Total now uses SellingPrice * Quantity
            public decimal Total => Quantity * SellingPrice;

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        }
    }
