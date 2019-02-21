using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CookBook.Views;
using CookBook.ViewModels;
using CookBook.Interfaces;
using CookBook.Services;
using Autofac;
using CookBook.Database;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace CookBook
{
    public partial class App : Application
    {
        public static string Version { get; set; }
        public static string GooglePlayCode = "";

        private static IContainer _container;

        private static NavigationService NaviService;

        public static NavigationPage _firstPage;
        public static Page StartupPage
        {
            get { return _firstPage; }
            set
            {
                _firstPage = new NavigationPage(value);
                // navigation page root should be search page
                NaviService.Navi = StartupPage.Navigation;
                NaviService.myPage = StartupPage;
            }
        }

        public static Acr.UserDialogs.IUserDialogs UserDialogService { get; set; }

        public App()
        {
            InitializeComponent();

            AboutPage = new AboutPage();
            AddRecipe = new AddRecipe();
            ItemDetailPage = new ItemDetailPage();
            NewItemPage = new NewItemPage();
            Recipes = new Recipes();
            TakePhoto = new TakePhoto();
            EditRecipe = new EditRecipe();
            BusinessScanner = new BusinessScanner();
            NaviService = _container.Resolve<INavigationService>() as NavigationService;

            var pageFactory = _container.Resolve<IPageFactory>();
            var home = BusinessScanner;// pageFactory.GetPage(Pages.MainPage) as TabbedPage;
            StartupPage = home;
            MainPage = StartupPage;
        }



        public static void Init(AppSetup appSetup)
        {
            _container = appSetup.CreateContainer();
            DataRepository.Instance = _container.Resolve<IDataRepository>();
            DataRepository.Instance.CreateDatabase();
            AboutViewModel = _container.Resolve<AboutViewModel>();
            TakePhotoViewModel = _container.Resolve<TakePhotoViewModel>();
            ItemDetailViewModel = _container.Resolve<ItemDetailViewModel>();
            RecipesViewModel = _container.Resolve<RecipesViewModel>();
            AddRecipeViewModel = _container.Resolve<AddRecipeViewModel>();
            EditRecipeViewModel = _container.Resolve<EditRecipeViewModel>();
            BusinessScannerViewModel = _container.Resolve<BusinessScannerViewModel>();
        }





        public static void SetNavService(Page page)
        {
            NaviService.Navi = page.Navigation;
            NaviService.myPage = page;
        }





        public static DateTime AppLoadedDateTime { get; set; }
        public static DateTime PageOpenedDateTime { get; set; }



        public static AboutViewModel AboutViewModel { get; set; }
        public static TakePhotoViewModel TakePhotoViewModel { get; set; }
        public static ItemDetailViewModel ItemDetailViewModel { get; set; }
        public static RecipesViewModel RecipesViewModel { get; set; }
        public static AddRecipeViewModel AddRecipeViewModel { get; set; }
        public static EditRecipeViewModel EditRecipeViewModel { get; set; }
        public static BusinessScannerViewModel BusinessScannerViewModel { get; set; }

        public static AboutPage AboutPage { get; set; }
        public static AddRecipe AddRecipe { get; set; }
        public static ItemDetailPage ItemDetailPage { get; set; }
        public static NewItemPage NewItemPage { get; set; }
        public static Recipes Recipes { get; set; }
        public static TakePhoto TakePhoto { get; set; }
        public static EditRecipe EditRecipe { get; set; }
        public static BusinessScanner BusinessScanner { get; set; }
    }
}
