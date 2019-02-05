using System;
using CookBook.Interfaces;
using CookBook.Views;
using Xamarin.Forms;

namespace CookBook.Services
{
	public class PageFactory : IPageFactory
	{
		public Page GetPage(Pages page)
		{
			switch (page)
            {
                case Pages.MainPage: return new MainPage();
                case Pages.Photo: return App.TakePhoto;
                case Pages.Recipies: return App.Recipes;
                case Pages.About: return App.AboutPage;
                case Pages.Items: return App.ItemDetailPage;
                case Pages.Add: return App.AddRecipe;
                case Pages.Edit: return App.EditRecipe;


                default: throw new ArgumentException(string.Format("Unknown page type {0}", page));
			}
		}
	}
}