using System;
using System.Windows.Input;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using CookBook.Database;
using CookBook.Database.Tables;
using System.Collections.Generic;
using System.Linq;
using CookBook.Interfaces;
using System.Collections.ObjectModel;
using Acr.UserDialogs;

namespace CookBook.ViewModels
{
    public class RecipesViewModel : BaseViewModel
    {
        public List<Recipes> AllRecipes { get; set; }
        public ICommand ShowRecipesCommand { get { return new Command(ShowRecipes); } }
        public ICommand AddCommand { get { return new Command(Add); } }
        public ICommand ShowFilterCommand { get { return new Command(ShowFilter); } }
        public ICommand FilterCommand { get { return new Command(Filter); } }


        public string SearchTerm { get; set; }
        public List<string> AllTags { get; set; }
        public ObservableCollection<FilterModel> Filters {get; set;}
        public bool IsResultsVisible { get; set; }
        public bool IsFilterVisible { get; set; }

        private IAppNavigation Navi { get; set; }
        private IPageFactory PageFac { get; set; }
        public RecipesViewModel(IAppNavigation _navi, IPageFactory _page)
        {
            Title = "Recipes";
            Navi = _navi;
            PageFac = _page;

            IsFilterVisible = false;
            IsResultsVisible = true;
        }
        
        public void OnNavigatedTo()
        {
            AllRecipes = new List<Recipes>();
            AllRecipes = DataRepository.Instance.GetAll<Recipes>().OrderBy(c=>c.Title).ToList();
            AllTags = string.Join(",", AllRecipes.Select(c => c.Tags).ToList()).Split(',').Distinct().ToList();

            Filters = new ObservableCollection<FilterModel>();

            foreach (var tag in AllTags)
            {
                Filters.Add(new FilterModel() { Checked = false, Selected = "unelected.png", Tag = tag });
            }

            OnPropertyChanged("AllRecipes");
            OnPropertyChanged("Filters");
        }

        public async void ShowRecipes(object obj)
        {
            Recipes recip = (Recipes)obj;
            if (recip != null)
            {
                App.ItemDetailViewModel.GetDetails(recip);
                await Navi.PushPage(PageFac.GetPage(Pages.Items));
            }
        }

        public async void Add()
        {
            App.AddRecipeViewModel.OnNavigatedTo();
            await Navi.PushModal(PageFac.GetPage(Pages.Add));
        }

        private void ShowFilter()
        {
            UserDialogs.Instance.ShowLoading();
            IsFilterVisible = !IsFilterVisible;
            IsResultsVisible = !IsResultsVisible;

            OnPropertyChanged("IsFilterVisible");
            OnPropertyChanged("IsResultsVisible");

            UserDialogs.Instance.HideLoading();
        }

        private void Filter()
        {
            var selected = Filters.Where(c => c.Checked).Select(c => c.Tag).ToList();
            AllRecipes = DataRepository.Instance.GetAll<Recipes>().ToList().Where(c=> c.Tags.Split(',').Any(selected.Contains) && c.Title.ToLower().Contains(SearchTerm.ToLower())).OrderBy(c=>c.Title).ToList();
            OnPropertyChanged("AllRecipes");
            ShowFilter();
        }

        public void SelectFilter(FilterModel filter)
        {
            var filters = Filters;
            foreach(var fil in filters)
            {
                if( fil== filter)
                {
                    fil.Checked = !fil.Checked;
                    fil.Selected = fil.Checked ? ImageSource.FromFile("selected.png") : ImageSource.FromFile("unselected.png");
                }
            }
            Filters = new ObservableCollection<FilterModel>(filters);
            OnPropertyChanged("Filters");
        }
    }

    public class FilterModel
    {
        public string Tag { get; set; }
        public ImageSource Selected {get; set;}
        public bool Checked { get; set; }
    }
}