using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace CookBook.Views
{
    public partial class EditRecipe : ContentPage
    {
        public EditRecipe()
        {
            InitializeComponent();
            BindingContext = App.EditRecipeViewModel;
        }
    }
}
