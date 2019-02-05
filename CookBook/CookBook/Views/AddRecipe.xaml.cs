using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace CookBook.Views
{
    public partial class AddRecipe : ContentPage
    {
        public AddRecipe()
        {
            InitializeComponent();
            BindingContext = App.AddRecipeViewModel;
        }
    }
}
