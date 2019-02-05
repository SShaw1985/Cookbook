using System;
using System.Collections.Generic;
using CookBook.ViewModels;
using Xamarin.Forms;

namespace CookBook.Views
{
    public partial class Recipes : ContentPage
    {
        public Recipes()
        {
            InitializeComponent();
            BindingContext = App.RecipesViewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            App.RecipesViewModel.OnNavigatedTo();
        }

        void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            var obj = (CookBook.Database.Tables.Recipes)e.Item;
            if (obj != null)
            {
                App.RecipesViewModel.ShowRecipes(obj);
                RecipesList.SelectedItem = null;
            }
        }

        void Handle_ItemTapped_1(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            var obj =(FilterModel) e.Item;
            if (obj != null)
            {
                App.RecipesViewModel.SelectFilter(obj);
                filterList.SelectedItem = null;
            }
        }
    }
}
