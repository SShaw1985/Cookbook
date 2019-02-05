using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using CookBook.Database;
using CookBook.Database.Tables;
using CookBook.Interfaces;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace CookBook.ViewModels
{
    public class AddRecipeViewModel : BaseViewModel
    {
        private IAppNavigation Navi { get; set; }
        public Recipes ThisRecipe { get; set; }
        public string RecipeTitle { get; set; }
        public string Ingredients { get; set; }
        public string Tags { get; set; }
        public string Instructions { get; set; }
        public byte[] InstructionsBlob { get; set; }
        public byte[] IngredientsBlob { get; set; }

        public AddRecipeViewModel(IAppNavigation _navi)
        {
            Navi = _navi;
        }

        public void OnNavigatedTo()
        {
            ThisRecipe = new Recipes();
            RecipeTitle = string.Empty;
            Ingredients = string.Empty;
            Instructions = string.Empty;
            Tags = string.Empty;
            OnPropertyChanged("ThisRecipe");
            OnPropertyChanged("RecipeTitle");
            OnPropertyChanged("Ingredients");
            OnPropertyChanged("Instructions");
            OnPropertyChanged("Tags");
        }



        public ICommand InstructionsCommand { get { return new Command(TakePhotoInstructions); } }
        public ICommand IngredientsCommand { get { return new Command(TakePhotoIngregients); } }
        public ICommand SaveCommand { get { return new Command(Save); } }

        public ICommand CloseCommand { get { return new Command(Close); } }

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
                    RotateImage = true, AllowCropping=true
                });
            }
            else
            {
                photo = await CrossMedia.Current.PickPhotoAsync();
            }
            if (photo != null)
            {
                UserDialogs.Instance.ShowLoading("Scanning");
             
                Ingredients  = await Helpers.Helper.ScanImage(photo.Path);
                OnPropertyChanged("Ingredients");

                using (Stream imageStream = File.OpenRead(photo.Path))
                {
                    IngredientsBlob = Helpers.Helper.ReadFully(imageStream);
                }
                UserDialogs.Instance.HideLoading();
            }
        }

    
        private async void Save()
        {
            if (ValidateRecipe())
            {
                try
                {
                    UserDialogs.Instance.ShowLoading("Saving");
                    ThisRecipe = new Recipes();
                    ThisRecipe.Title = RecipeTitle;
                    ThisRecipe.Text = Instructions;
                    ThisRecipe.Tags = Tags;
                    ThisRecipe.Ingredients = Ingredients;
                    ThisRecipe.CreatedDate = DateTime.Now;
                    ThisRecipe.UpdatedDate = DateTime.Now;
                    var recp = DataRepository.Instance.Save<Recipes>(ThisRecipe);

                    if (IngredientsBlob != null && IngredientsBlob.Length > 0)
                    {
                        Pictures pic = new Pictures()
                        {
                            Blob = IngredientsBlob,
                            RecipeeId = recp.PKId,
                            Type = 1
                        };
                        DataRepository.Instance.Save<Pictures>(pic);
                    }

                    if (InstructionsBlob != null && InstructionsBlob.Length > 0)
                    {
                        Pictures pic = new Pictures()
                        {
                            Blob = InstructionsBlob,
                            RecipeeId = recp.PKId,
                            Type = 2,
                        };
                        DataRepository.Instance.Save<Pictures>(pic);
                    }

                    UserDialogs.Instance.HideLoading();
                    UserDialogs.Instance.Toast("Recipe Added");
                    App.RecipesViewModel.OnNavigatedTo();
                    await Navi.PopModal();

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

        private void Close()
        {
            Navi.PopModal();
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
    }
}