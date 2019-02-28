using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using CookBook.Interfaces;
using FFImageLoading.Forms;
using FFImageLoading.Transformations;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Syncfusion.SfImageEditor.XForms;
using Tesseract;
using Xamarin.Forms;


namespace CookBook.ViewModels
{
    public class AzureBusinessScannerViewModel : BaseViewModel
    {
        private IAppNavigation Navi { get; set; }

    
        public string BusinessName { get; set; }
        public int BusinessNameIndex { get; set; }
        public string Title { get; set; }
        public int TitleIndex { get; set; }
        public string Email { get; set; }

        public ObservableCollection<string> PhoneNumber { get; set; }
        public ObservableCollection<string> Zip { get; set; }
        public string SelectedPhoneNumber { get; set; }
        public string SelectedZip { get; set; }

        public string Url { get; set; }
        public string StreetAddress { get; set; }
        public int StreetAddressIndex { get; set; }
        public string State { get; set; }
        public string City { get; set; }


        public string Raw { get; set; }
        public ObservableCollection<string> Others { get; set; }
        public List<int> ScanTypes { get; set; }
        public int ScanType { get; set; }

        public byte[] InstructionsBlob { get; set; }

        private ImageSource _imageSource;
        public ImageSource Image { get 
            { 
                return this._imageSource; 
            } 
            set 
            {
                _imageSource = value;
                OnPropertyChanged("Image");
            } 
        }

        public string Path { get; set; }

        public AzureBusinessScannerViewModel(IAppNavigation _navi)
        {
            Navi = _navi;
        }




        public ICommand TakePhotoCommand { get { return new Command(TakePhoto); } }
        public ICommand PickPhotoCommand { get { return new Command(PickPhoto); } }
        public ICommand ScanPhotoCommand { get { return new Command(ScanPhoto); } }


        List<string> removals { get; set; }
        Stream strm { get; set; }
        MemoryStream destination = new MemoryStream();
        MediaFile photo;
        private async void TakePhoto(object obj)
        {
            ShowResults = false;
            UserDialogs.Instance.ShowLoading("Scanning");

            strm = null;

            UserDialogs.Instance.HideLoading();


            if (CrossMedia.Current.IsCameraAvailable)
            {

                /*Func<object> func = () =>
                {
                    var layout = new RelativeLayout();
                    var image = new Image
                    {
                        Source = "frame"
                    };

                    Func<RelativeLayout, double> ImageHeight = (p) => image.Measure(layout.Width, layout.Height).Request.Height;
                    Func<RelativeLayout, double> ImageWidth = (p) => image.Measure(layout.Width, layout.Height).Request.Width;

                    layout.Children.Add(image,
                                Constraint.RelativeToParent(parent => parent.Width / 2 - ImageWidth(parent) / 2),
                                Constraint.RelativeToParent(parent => parent.Height / 2 - ImageHeight(parent) / 2)
                    );
                    return layout;
                };*/


                photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    PhotoSize = PhotoSize.Large,
                    Name = Guid.NewGuid().ToString() + ".jpg",
                    RotateImage = true,
                    AllowCropping = true,
                    //OverlayViewProvider = func
                });
            }
            else
            {
                photo = await CrossMedia.Current.PickPhotoAsync();

            }

            if (photo != null)
            {
                strm = photo.GetStream();
                Path = photo.Path;
                Image = ImageSource.FromFile(Path);
            }

            ResetFields();
            OnPropertyChanged("ShowResults");
        }

        private async void PickPhoto(object obj)
        {
            ShowResults = false;
            UserDialogs.Instance.ShowLoading("Scanning");

            strm = null;

            UserDialogs.Instance.HideLoading();


           
            photo = await CrossMedia.Current.PickPhotoAsync();


            if (photo != null)
            {
                strm = photo.GetStream();
                Path = photo.Path;
                Image = ImageSource.FromFile(Path);
            }


            ResetFields();

            OnPropertyChanged("ShowResults");
        }

        public bool ShowResults { get; set; }

        private async void ScanPhoto(object obj)
        {
            ResetFields();
            UserDialogs.Instance.ShowLoading("Scanning");
          


            Stream stream = null;
            if (SavedStream != null)
            {
                stream = SavedStream;
            }
            else
            {
                stream = File.OpenRead(Path);
            }
            if (stream != null)
            {

                var source = stream;
                var imageBytes = new byte[source.Length];
                InstructionsBlob = imageBytes;

                string input = await Helpers.Helper.ScanImage(photo.Path);

                if (!string.IsNullOrEmpty(input))
                {
                    StringBuilder sb = new StringBuilder();
                    Raw = input;
                    List<string> lines = Regex.Split(input, "\r\n|\r|\n|\t").ToList().Where(c => !string.IsNullOrEmpty(c)).ToList();
                    PhoneNumber = new ObservableCollection<string>(ScanTelephoneNumber(input));
                    Email = ScanEmail(input);
                    Url = ScanWebsite(input);
                    if (Url.Contains(Email))
                    {
                        Url.Replace(Email, "");
                    }
                    MasterAddress = new Address();

                    StreetAddress = ScanStreetAddress(input);
                    if (string.IsNullOrEmpty(StreetAddress))
                    {
                        ParseStreetAddresses(input);
                        StreetAddress = MasterAddress.Location + " " + MasterAddress.BuildingName + " " + MasterAddress.Floor;
                    }

                    City = ScanCity(input);

                    State = ScanState(input);

                    Zip = new ObservableCollection<string>(ScanZip(input));

                    Others = new ObservableCollection<string>(lines);
                    TitleIndex = 0;
                    BusinessName = Others.FirstOrDefault();
                    Title = Others.FirstOrDefault();
                }

                SelectedPhoneNumber = PhoneNumber.FirstOrDefault();
                SelectedZip = Zip.FirstOrDefault();

                BusinessNameIndex = Others.IndexOf(BusinessName);
                StreetAddressIndex = Others.IndexOf(StreetAddress);

                OnPropertyChanged("SelectedPhoneNumber");
                OnPropertyChanged("SelectedZip");

                OnPropertyChanged("BusinessName");
                OnPropertyChanged("BusinessNameIndex");

                OnPropertyChanged("Title");
                OnPropertyChanged("TitleIndex");
                OnPropertyChanged("Email");
                OnPropertyChanged("PhoneNumber");
                OnPropertyChanged("Url");
                OnPropertyChanged("StreetAddress");
                OnPropertyChanged("StreetAddressIndex");
                OnPropertyChanged("State");
                OnPropertyChanged("Zip");
                OnPropertyChanged("Raw");
                OnPropertyChanged("City");
                OnPropertyChanged("Title");
                OnPropertyChanged("Others");

                OnPropertyChanged("InstructionsBlob");

                ShowResults = true;
                OnPropertyChanged("ShowResults");

            }
            UserDialogs.Instance.HideLoading();

            MessagingCenter.Send<AzureBusinessScannerViewModel>(this, "FinishedScanning");
        }

        private void ResetFields()
        {
            BusinessName = string.Empty;
            StreetAddress = string.Empty;
            Title = string.Empty;
            TitleIndex = 0;
            Email = string.Empty;
            PhoneNumber = new ObservableCollection<string>(new List<string>());
            Url = string.Empty;
            StreetAddress = string.Empty;
            State = string.Empty;
            Zip = new ObservableCollection<string>(new List<string>());

            Raw = string.Empty;
            City = string.Empty;
            TitleIndex = 0;
            Others = new ObservableCollection<string>(new List<string>());

            OnPropertyChanged("BusinessName");
            OnPropertyChanged("StreetAddress");
            OnPropertyChanged("Title");
            OnPropertyChanged("Email");
            OnPropertyChanged("PhoneNumber");
            OnPropertyChanged("Url");
            OnPropertyChanged("StreetAddress");
            OnPropertyChanged("State");
            OnPropertyChanged("Zip");
            OnPropertyChanged("Raw");
            OnPropertyChanged("City");
            OnPropertyChanged("Title");
            OnPropertyChanged("Others");

        }

        public List<string> ScanTelephoneNumber(string text)
        {
            List<string> ret = new List<string>();
            var match = Regex.Matches(text,Helpers.RegexStrings.TelephoneRegex, RegexOptions.IgnoreCase);
            foreach (var phone in match)
            {
                ret.Add(phone.ToString());
            }
            return ret;
        }

        public string ScanEmail(string text)
        {
            string ret = string.Empty;
            var match = Regex.Matches(text, Helpers.RegexStrings.EmailRegex, RegexOptions.IgnoreCase);
            foreach (var phone in match)
            {
                ret += phone.ToString();
            }
            return ret;
        }
        public string ScanWebsite(string text)
        {
            string ret = string.Empty;
            var match = Regex.Matches(text, Helpers.RegexStrings.WebsiteRegex, RegexOptions.IgnoreCase);
            foreach (var phone in match)
            {
                ret += phone.ToString();
            }
            return ret;
        }

        public string ScanStreetAddress(string text)
        {
            string ret = string.Empty;
            var match = Regex.Matches(text, Helpers.RegexStrings.StreetAddressPattern, RegexOptions.IgnoreCase);
            foreach (var phone in match)
            {
                ret = phone.ToString();
            }
            return ret;
        }
        public List<string> ScanZip(string text)
        {
            List<string> ret = new List<string>();
            var match = Regex.Matches(text, Helpers.RegexStrings.zipPattern, RegexOptions.IgnoreCase);
            foreach (var phone in match)
            {
                ret.Add(phone.ToString());
            }
            return ret;
        }

        public string ScanCity(string text)
        {
            string ret = string.Empty;
            var match = Regex.Match(text, Helpers.RegexStrings.cityPattern, RegexOptions.IgnoreCase);
            foreach (var phone in match.Captures)
            {
                string[] cities = phone.ToString().Split(',');
                ret = cities.FirstOrDefault();
                break;
            }
            return ret;
        }

        public string ScanState(string text)
        {
            string ret = string.Empty;
            var match = Regex.Match(text, Helpers.RegexStrings.statePattern, RegexOptions.IgnoreCase);
            foreach (var phone in match.Captures)
            {
                ret += phone.ToString();
            }
            return ret;
        }

        Address MasterAddress { get; set; }

        private void ParseStreetAddresses(string inputAddress)
        {
            StringBuilder sb = new StringBuilder();
            var parser = new AddressParser();

            Console.WriteLine("input address: {0}", inputAddress);

            var address = parser.Parse(inputAddress);
            if (null == address)
            {
                Console.WriteLine("error: unable to parse address");
            }
            else
            {
                if (!String.IsNullOrEmpty(address.Location) && string.IsNullOrWhiteSpace(MasterAddress.Location))
                {
                    MasterAddress.Location = address.Location;

                }
                if (!String.IsNullOrEmpty(address.BuildingName) && string.IsNullOrWhiteSpace(MasterAddress.BuildingName))
                {
                    MasterAddress.BuildingName = address.BuildingName;

                }
                if (!String.IsNullOrEmpty(address.Floor) && string.IsNullOrWhiteSpace(MasterAddress.Floor))
                {
                    MasterAddress.Floor = address.Floor;

                }

            }


        }

        public void Editor_ImageSaving(object sender, ImageSavingEventArgs args)
        {
            SavedStream = args.Stream;
        }

        public Stream SavedStream { get; set; }
    }


}