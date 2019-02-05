using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace CookBook.Views
{
    public partial class TakePhoto : ContentPage
    {
        public TakePhoto()
        {
            InitializeComponent();
            BindingContext = App.TakePhotoViewModel;
        }
    }
}
