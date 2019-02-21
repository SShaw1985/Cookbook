using System;
using System.Collections.Generic;
using CookBook.ViewModels;
using Xamarin.Forms;

namespace CookBook.Views
{
    public partial class BusinessScanner : ContentPage
    {
        public BusinessScanner()
        {
            BindingContext = App.BusinessScannerViewModel;
            InitializeComponent();
            editor.ImageSaving += App.BusinessScannerViewModel.Editor_ImageSaving;

        }
    }
}
