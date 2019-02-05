using System;
using System.Windows.Input;
using CookBook.Database;
using Xamarin.Forms;

namespace CookBook.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";

            OpenWebCommand = new Command(() => Device.OpenUri(new Uri("https://xamarin.com/platform")));
            ExportCommand = new Command(() => DataRepository.Instance.Export());
        }

        public ICommand OpenWebCommand { get; }
        public ICommand ExportCommand { get; }
    }
}