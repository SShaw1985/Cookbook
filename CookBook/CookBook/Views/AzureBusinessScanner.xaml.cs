using System;
using System.Collections.Generic;
using System.Linq;
using CookBook.ViewModels;
using Xamarin.Forms;

namespace CookBook.Views
{
    public partial class AzureBusinessScanner : ContentPage
    {
        public AzureBusinessScanner()
        {
            InitializeComponent();
            BindingContext = App.AzureBusinessScannerViewModel;
            editor.ImageSaving += App.AzureBusinessScannerViewModel.Editor_ImageSaving;

            MessagingCenter.Subscribe<AzureBusinessScannerViewModel>(this, "FinishedScanning", (obj) =>
            {
                businessname.SelectedItem = App.AzureBusinessScannerViewModel.BusinessName;
                title.SelectedItem = App.AzureBusinessScannerViewModel.Title;
                streetaddress.SelectedItem = App.AzureBusinessScannerViewModel.StreetAddress;
                phonenumbers.SelectedItem = App.AzureBusinessScannerViewModel.SelectedPhoneNumber;
                zip.SelectedItem = App.AzureBusinessScannerViewModel.SelectedZip;

                var bindex = App.AzureBusinessScannerViewModel.Others.IndexOf(App.AzureBusinessScannerViewModel.BusinessName);
                var tindex = App.AzureBusinessScannerViewModel.Others.IndexOf(App.AzureBusinessScannerViewModel.Title);
                var sindex = App.AzureBusinessScannerViewModel.Others.IndexOf(App.AzureBusinessScannerViewModel.StreetAddress);
                var pindex = App.AzureBusinessScannerViewModel.PhoneNumber.IndexOf(App.AzureBusinessScannerViewModel.SelectedPhoneNumber);
                var zindex = App.AzureBusinessScannerViewModel.Zip.IndexOf(App.AzureBusinessScannerViewModel.SelectedZip);

                businessname.SelectedIndex = bindex;
                title.SelectedIndex = tindex;
                streetaddress.SelectedIndex = sindex;
                phonenumbers.SelectedIndex = pindex;
                zip.SelectedIndex = zindex;


            });

        }
    }
}
