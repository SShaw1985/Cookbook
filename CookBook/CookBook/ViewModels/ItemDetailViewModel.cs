using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Acr.UserDialogs;
using CookBook.Database;
using CookBook.Database.Tables;
using CookBook.Interfaces;
using CookBook.Models;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace CookBook.ViewModels
{
    public class ItemDetailViewModel : BaseViewModel
    {
        public Recipes Item { get; set; }
        public string[] Tags { get; set; }
        IAppNavigation Navi { get; set; }
        IPageFactory PageFac { get; set; }
        public Dictionary<Pictures, ImageSource> ThesePictures { get; set; }

        public ImageSource MaxImage { get; set; }
        public bool ShowPhoto { get; set; }

        public ICommand ShowCommand { get { return new Command(Show); } }
        public ICommand DeletePhotoCommand { get { return new Command(DeletePhoto); } }

        public Pictures SelectedImage { get; set; }

        public ICommand CloseCommand { get { return new Command(Close); } }
        public ICommand TakePhotoCommand
        {
            get { return new Command(TakePhoto); }
        }
        public ICommand EditCommand { get { return new Command(Edit); } }

        public ItemDetailViewModel(IAppNavigation _navi, IPageFactory _page)
        {
            Navi = _navi;
            PageFac = _page;
        }

        public void GetDetails(Recipes recipe)
        {
            Title = recipe.Title;
            Item = recipe;
            if (recipe.Tags != null)
            {
                Tags = recipe.Tags.Split(',');
            }
            var recipes = DataRepository.Instance.GetAll<Pictures>().Where(c => c.RecipeeId == recipe.PKId).ToList();
            ThesePictures = new Dictionary<Pictures, ImageSource>();
            foreach (var recp in recipes)
            {
                var src = ImageSource.FromStream(() =>
                {
                    return new MemoryStream(recp.Blob);
                });
                ThesePictures.Add(recp, src);
            }

         
            ShowPhoto = false;
            MaxImage = null;

            OnPropertyChanged("ThesePictures");
            OnPropertyChanged("Title");
            OnPropertyChanged("Item");
            OnPropertyChanged("Tags");

            MessagingCenter.Send<ItemDetailViewModel, string>(this, "recipe", recipe.Tags);
        }

        public async void Close()
        {
            ShowPhoto = false;
            MaxImage = null;
            OnPropertyChanged("ShowPhoto");
            OnPropertyChanged("MaxImage");
        }

        public async void DeletePhoto()
        {
            if (await UserDialogs.Instance.ConfirmAsync("Are you sure you want to delete this photo?"))
            {
                DataRepository.Instance.Delete<Pictures>(SelectedImage);

                var recipes = DataRepository.Instance.GetAll<Pictures>().Where(c => c.RecipeeId == Item.PKId).ToList();
                ThesePictures = new Dictionary<Pictures, ImageSource>();
                foreach (var recp in recipes)
                {
                    var src = ImageSource.FromStream(() =>
                    {
                        return new MemoryStream(recp.Blob);
                    });
                    ThesePictures.Add(recp, src);
                }


                ShowPhoto = false;
                MaxImage = null;

                OnPropertyChanged("ThesePictures");

                OnPropertyChanged("ShowPhoto");
                OnPropertyChanged("MaxImage");
            }

        }

        private void Show(object obj)
        {
            try
            {
                var img = (Pictures)obj;
                if (img != null)
                {

                    var record = ThesePictures.FirstOrDefault(c => c.Key ==img);
                    MaxImage = record.Value;
                    SelectedImage = record.Key;
                    ShowPhoto = true;
                    OnPropertyChanged("ShowPhoto");
                    OnPropertyChanged("MaxImage");
                }
            }
            catch (Exception ex)
            {
            }
        }

        private async void TakePhoto(object obj)
        {
            MediaFile photo;
            if (CrossMedia.Current.IsCameraAvailable)
            {
                photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    PhotoSize = PhotoSize.Large,
                    Name = Guid.NewGuid().ToString() + ".jpg",
                    RotateImage = true
                });
            }
            else
            {
                photo = await CrossMedia.Current.PickPhotoAsync();
            }

            if (photo != null)
            {
                using (Stream imageStream = File.OpenRead(photo.Path))
                {
                    var Blob = Helpers.Helper.ReadFully(imageStream);
                   
                    var pic = DataRepository.Instance.Save<Pictures>(new Pictures()
                    {
                        Blob = Blob,
                        RecipeeId = Item.PKId,
                        Type = 3,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    });

                    ThesePictures.Add(pic,ImageSource.FromStream(() =>
                    {
                        return new MemoryStream(Blob);
                    }));
                }

                OnPropertyChanged("ThesePictures");
                UserDialogs.Instance.HideLoading();
            }
        }

        private async void Edit()
        {
            App.EditRecipeViewModel.OnNavigatedTo(Item.PKId);
            await Navi.PushPage(PageFac.GetPage(Pages.Edit));
        }
    }
}