using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.Dialogs.Core.ViewModel;
using DevExpress.Mvvm;
using DevExpress.XtraEditors.Mask.Design;
using DevExpress.XtraRichEdit.Model;
using DevExpress.XtraScheduler.Outlook.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.ServiceModel.Channels;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;

namespace DXDivDef.ViewModels
{
    public class Items : BindableBase
    {
        public string RowNo { get; set; }
        public string Parent { get; set; }
        public string No { get; set; }
        public string SysNo { get; set; }
        public string Component {  get; set; }
        public double Qty_1 { get; set; }
        public double PlanOnOrder { get; set; }
        public double QtyNeed { get; set; }
        public double BalanceOnFstDay { get; set; }
        public double QtyComing { get; set; }
        public double QtyOutGo { get; set; }
        public double BalanceSRPUSX { get; set; }
        public double Formula1 { get; set; }
        public double Formula2 { get; set; }
        public string Note {  get; set; }
        public double AnotherDivs { get; set; }
        public double CountOut { get;set; }
        //public int Visits { get; set; }
        //public DateTime? Birthday { get; set; }
    }
    public class MainViewModel : ViewModelBase
    {
        private ObservableCollection<Items> _items;
        public ObservableCollection<Items> Source
        {
            get { return _items; }
            set
            {
                _items = value;
                base.RaisePropertiesChanged("Source");
            }
        }
        public MainViewModel()
        {
            Source = new ObservableCollection<Items>();
        }
        public void SetItems(DataTable linkDT)
        {
            Source.Clear();
            foreach (DataRow row in linkDT.Rows)
            {
                Source.Add(new Items {  RowNo = row[4].ToString(), 
                                        Parent = row[5].ToString(), 
                                        No = row[0].ToString(),
                                        SysNo = row[1].ToString(),
                                        Component = row[2].ToString(),
                                        Qty_1 = double.Parse(row[3].ToString()),
                                        PlanOnOrder = double.Parse(row[6].ToString()),
                                        QtyNeed = double.Parse(row[7].ToString()),
                                        BalanceOnFstDay = double.Parse(row[8].ToString()),
                                        QtyComing = double.Parse(row[9].ToString()),
                                        QtyOutGo = double.Parse(row[10].ToString()),
                                        BalanceSRPUSX = double.Parse(row[11].ToString()),
                                        Formula1 = (double.Parse(row[8].ToString())
                                            + double.Parse(row[9].ToString())
                                            + double.Parse(row[11].ToString())
                                            - double.Parse(row[10].ToString()))
                                            - double.Parse(row[7].ToString()),
                                        Formula2 = (double.Parse(row[8].ToString())
                                            + double.Parse(row[9].ToString())
                                            + double.Parse(row[11].ToString())
                                            - double.Parse(row[10].ToString()))
                                            / double.Parse(row[3].ToString()),
                                        Note = row[12].ToString(),
                                        AnotherDivs = double.Parse(row[13].ToString()),
                                        CountOut = double.Parse(row[14].ToString())
                });
            }
        }
    }
    public class ProductsViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<string> _products;
        public ObservableCollection<string> Products
        {
            get { return _products; }
            set
            {
                _products = value;
                OnPropertyChanged(nameof(Products));
            }
        }

        private string _selectedProduct;
        public string SelectedProduct
        {
            get { return _selectedProduct; }
            set
            {
                _selectedProduct = value;
                OnPropertyChanged(nameof(SelectedProduct));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ProductsViewModel()
        {
            Products = new ObservableCollection<string>();
        }

        public void SetProducts(DataTable linkDT)
        {
            Products.Clear();
            foreach (DataRow row in linkDT.Rows)
            {
                Products.Add(row[1].ToString());
            }
            //if (Products.Count > 0) SelectedProduct = _products[0];
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class OrdersViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<string> _orders;
        public ObservableCollection<string> Orders
        {
            get { return _orders; }
            set
            {
                _orders = value;
                OnPropertyChanged(nameof(Orders));
            }
        }

        private string _selectedOrder;
        public string SelectedOrder
        {
            get { return _selectedOrder; }
            set
            {
                _selectedOrder = value;
                OnPropertyChanged(nameof(SelectedOrder));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public OrdersViewModel()
        {
            Orders = new ObservableCollection<string>();
        }

        public void SetOrders(DataTable linkDT)
        {
            Orders.Clear();
            foreach (DataRow row in linkDT.Rows)
            {
                Orders.Add(row[0].ToString());
            }
            //if (Orders.Count > 0) SelectedOrder = _orders[0];
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
