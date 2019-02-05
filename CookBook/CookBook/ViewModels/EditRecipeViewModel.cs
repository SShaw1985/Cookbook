using System;
using System.IO;
using System.Text;
using System.Windows.Input;
using Acr.UserDialogs;
using CookBook.Database;
using CookBook.Database.Tables;
using CookBook.Interfaces;
using Plugin.Media;
using Plugin.Media.Abstractions;

using Xamarin.Forms;
namespace CookBook.ViewModels
{
    public class EditRecipeViewModel : BaseViewModel
    {
        private Recipes ThisRecipe { get; set; }

        public string Title { get; set; }
        public string Instructions { get; set; }
        public string Ingredients { get; set; }
        public string Tags { get; set; }

        public byte[] InstructionsBlob { get; set; }
        public byte[] IngredientsBlob { get; set; }
        private IAppNavigation Navi { get; set; }
        private IPageFactory PageFac { get; set; }

        public ICommand SaveCommand { get { return new Command(Save); } }
        public ICommand DeleteCommand { get { return new Command(Delete); } }

        public ICommand InstructionsCommand { get { return new Command(TakePhotoInstructions); } }

        public ICommand IngredientsCommand { get { return new Command(TakePhotoInstructions); } }

        public EditRecipeViewModel(IAppNavigation _navi, IPageFactory _page)
        {
            Navi = _navi;
            PageFac = _page;
        }

        public void OnNavigatedTo(int ID)
        {
            ThisRecipe  = DataRepository.Instance.GetById<Recipes>(ID);

            Title = ThisRecipe.Title;
            Instructions = ThisRecipe.Text;
            Ingredients = ThisRecipe.Ingredients;
            Tags = ThisRecipe.Tags;

            OnPropertyChanged("Title");
            OnPropertyChanged("Instructions");
            OnPropertyChanged("Ingredients");
            OnPropertyChanged("Tags");

        }

        private async void Save(object obj)
        {
            if (ValidateRecipe())
            {
                try
                {
                    UserDialogs.Instance.ShowLoading("Saving...");

                    ThisRecipe.UpdatedDate = DateTime.Now;
                    ThisRecipe.Title = Title;
                    ThisRecipe.Text = Instructions;
                    ThisRecipe.Ingredients = Ingredients;
                    ThisRecipe.Tags = Tags;

                    DataRepository.Instance.Update<Recipes>(ThisRecipe);

                    App.RecipesViewModel.OnNavigatedTo();
                    App.ItemDetailViewModel.GetDetails(ThisRecipe);

                    await Navi.PopPageAsync();
                    UserDialogs.Instance.HideLoading();
                }
                catch (Exception ex)
                {
                    UserDialogs.Instance.HideLoading();
                    UserDialogs.Instance.Alert(ex.Message);
                }
            }
            else
            {
                await UserDialogs.Instance.AlertAsync(ErrorMessage);
            }
        }

        private async void Delete()
        {
            if (await UserDialogs.Instance.ConfirmAsync("Are you sure you want to remove this recipe? once its gone, it's gone for good!"))
            {
                UserDialogs.Instance.ShowLoading("Deleteing...");

                DataRepository.Instance.Delete<Recipes>(ThisRecipe);
                App.RecipesViewModel.OnNavigatedTo();

                await Navi.PopToRoot();
                UserDialogs.Instance.HideLoading();
            }
        }

        private bool ValidateRecipe()
        {
            bool retVal = true;

            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(ThisRecipe.Title))
                errors.AppendLine("Title is required");

            if (string.IsNullOrWhiteSpace(ThisRecipe.Text))
                errors.AppendLine("Instructions are required");

            if (string.IsNullOrWhiteSpace(ThisRecipe.Ingredients))
                errors.AppendLine("Ingredients are required");

            if (string.IsNullOrWhiteSpace(ThisRecipe.Tags))
                errors.AppendLine("Tags are required");

            ErrorMessage = errors.ToString();
            return retVal;
        }

        private async void TakePhotoInstructions(object obj)
        {
            MediaFile photo;
            if (CrossMedia.Current.IsCameraAvailable)
            {
                photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    PhotoSize = PhotoSize.Large,
                    Name = Guid.NewGuid().ToString() + ".jpg",
                    RotateImage = true,
                    AllowCropping = true
                });
            }
            else
            {
                photo = await CrossMedia.Current.PickPhotoAsync();
            }

            if (photo != null)
            {
                UserDialogs.Instance.ShowLoading("Scanning");

                this.Instructions = await Helpers.Helper.ScanImage(photo.Path);

                OnPropertyChanged("Instructions");

                using (Stream imageStream = File.OpenRead(photo.Path))
                {
                    InstructionsBlob = Helpers.Helper.ReadFully(imageStream);
                }
                UserDialogs.Instance.HideLoading();
            }
        }

        private async void TakePhotoIngregients(object obj)
        {
            MediaFile photo;
            if (CrossMedia.Current.IsCameraAvailable)
            {
                photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    PhotoSize = PhotoSize.Large,
                    Name = Guid.NewGuid().ToString() + ".jpg",
                    RotateImage = true,
                    AllowCropping = true
                });
            }
            else
            {
                photo = await CrossMedia.Current.PickPhotoAsync();
            }
            if (photo != null)
            {
                UserDialogs.Instance.ShowLoading("Scanning");

                Ingredients = await Helpers.Helper.ScanImage(photo.Path);
                OnPropertyChanged("Ingredients");

                using (Stream imageStream = File.OpenRead(photo.Path))
                {
                    IngredientsBlob = Helpers.Helper.ReadFully(imageStream);
                }
                UserDialogs.Instance.HideLoading();
            }
        }

    }
}
