using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace CookBook.Views
{
    public partial class MainBusinessCardPage : ContentPage
    {
        public MainBusinessCardPage()
        {
            BindingContext = App.MainBusinessCardPageViewModel;
            InitializeComponent();
        }
    }
}
