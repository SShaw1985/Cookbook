using CookBook.Controls;
using CookBook.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CookBook.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();

            BindingContext = App.ItemDetailViewModel;

            MessagingCenter.Subscribe<ItemDetailViewModel, string>(this, "recipe", (sender, arg) =>
             {
                 Device.BeginInvokeOnMainThread(() =>
                 {
                     tagContainer.Children.Clear();
                     string[] tags = arg.Split(',');
                     foreach (var tag in tags)
                     {
                         tagContainer.Children.Add(new Tag() { HeadingText = tag.Trim() });
                     }
                 });
             });
        }
    }
}