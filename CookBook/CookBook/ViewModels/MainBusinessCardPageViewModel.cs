using System;
using System.Windows.Input;
using CookBook.Interfaces;
using Xamarin.Forms;

namespace CookBook.ViewModels
{
    public class MainBusinessCardPageViewModel : BaseViewModel
    {

        IAppNavigation Navi { get; set; }

        public ICommand GoogleCommand { get { return new Command(Google); } }

        private void Google(object obj)
        {
            //Navi.PushPage(App.StartupPage);
        }

        public ICommand TesseractCommand { get { return new Command(Tesseract); } }

        private void Tesseract(object obj)
        {
            Navi.PushPage(App.BusinessScanner);
        }

        public ICommand AzureCommand { get { return new Command(Azure); } }

        private void Azure(object obj)
        {
            Navi.PushPage(App.AzureBusinessScanner);
        }

        public MainBusinessCardPageViewModel(IAppNavigation _navi)
        {

            Navi = _navi;
        }
    }
}
