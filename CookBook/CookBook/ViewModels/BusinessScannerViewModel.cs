using System;
using System.Collections.Generic;
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
    public class BusinessScannerViewModel : BaseViewModel
    {
        private IAppNavigation Navi { get; set; }
        private readonly ITesseractApi _tesseractApi;

        private string TelephoneRegex = @"\(?\d{3}\)?[-\.]? *\d{3}[-\.]? *[-\.]?\d{4}";
        private string EmailRegex = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
        private string WebsiteRegex = @"((www\.)?([\w\-\.\/])*(\.[a-zA-Z]{2,3}\/?))[^\s\b\n|]*[^.,;:\?\!\@\^\$ -]";
        private string zipPattern = @"^\d{5}-\d{4}|\d{5}|[A-Z]\d[A-Z] \d[A-Z]\d$";
        private string statePattern = @"\b(?:Ala(?:(?:bam|sk)a)|American Samoa|Arizona|Arkansas|(?:^(?!Baja )California)|Colorado|Connecticut|Delaware|District of Columbia|Florida|Georgia|Guam|Hawaii|Idaho|Illinois|Indiana|Iowa|Kansas|Kentucky|Louisiana|Maine|Maryland|Massachusetts|Michigan|Minnesota|Miss(?:(?:issipp|our)i)|Montana|Nebraska|Nevada|New (?:Hampshire|Jersey|Mexico|York)|North (?:(?:Carolin|Dakot)a)|Ohio|Oklahoma|Oregon|Pennsylvania|Puerto Rico|Rhode Island|South (?:(?:Carolin|Dakot)a)|Tennessee|Texas|Utah|Vermont|Virgin(?:ia| Island(s?))|Washington|West Virginia|Wisconsin|Wyoming|A[KLRSZ]|C[AOT]|D[CE]|FL|G[AU]|HI|I[ADLN]|K[SY]|LA|M[ADEINOST]|N[CDEHJMVY]|O[HKR]|P[AR]|RI|S[CD]|T[NX]|UT|V[AIT]|W[AIVY])\b";
        private string cityPattern = @"\b([A-Za-z]+(?: [A-Za-z]+)*),? ([A-Za-z]{2}) \d{5}\b";

        public string BusinessName { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Url { get; set; }
        public string StreetAddress { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string Raw { get; set; }
        public List<string> Others { get; set; }
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

        public BusinessScannerViewModel(IAppNavigation _navi, ITesseractApi tesseractApi)
        {
            Navi = _navi;
            _tesseractApi = tesseractApi;
            ScanTypes = new List<int>();
            for (int iii = 1; iii <= 13;iii++)
            {
                ScanTypes.Add(iii);
            }
            ScanType = ScanTypes.FirstOrDefault();
        }




        public ICommand TakePhotoCommand { get { return new Command(TakePhoto); } }
        public ICommand PickPhotoCommand { get { return new Command(PickPhoto); } }
        public ICommand ScanPhotoCommand { get { return new Command(ScanPhoto); } }


        List<string> removals { get; set; }
        List<string> FurtherRemovals { get; set; }
        Stream strm { get; set; }
        MemoryStream destination = new MemoryStream();
        private async void TakePhoto(object obj)
        {
            ShowResults = false;
            UserDialogs.Instance.ShowLoading("Scanning");

            strm = null;

            UserDialogs.Instance.HideLoading();


            MediaFile photo;
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
            OnPropertyChanged("ShowResults");
        }

        private async void PickPhoto(object obj)
        {
            ShowResults = false;
            UserDialogs.Instance.ShowLoading("Scanning");

            strm = null;

            UserDialogs.Instance.HideLoading();


            MediaFile photo;
           
            photo = await CrossMedia.Current.PickPhotoAsync();


            if (photo != null)
            {
                strm = photo.GetStream();
                Path = photo.Path;
                Image = ImageSource.FromFile(Path);
            }



            OnPropertyChanged("ShowResults");
        }

        public bool ShowResults { get; set; }

        private async void ScanPhoto(object obj)
        {
            BusinessName = string.Empty;
            Title = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            Url = string.Empty;
            StreetAddress = string.Empty;
            State = string.Empty;
            Zip = string.Empty;
            Raw = string.Empty;
            City = string.Empty;
            Title = string.Empty;
            Others = new List<string>();

            OnPropertyChanged("BusinessName");
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

            UserDialogs.Instance.ShowLoading("Scanning");
            if (!_tesseractApi.Initialized)
            {
                await _tesseractApi.Init("eng");
            }
            PageSegmentationMode type = PageSegmentationMode.OsdOnly;
            switch (ScanType)
            {
                case 1:
                    type = PageSegmentationMode.OsdOnly;
                    break;

                case 2:
                    type = PageSegmentationMode.AutoOsd;
                    break;

                case 3:
                    type = PageSegmentationMode.AutoOnly;
                    break;

                case 4:
                    type = PageSegmentationMode.Auto;
                    break;

                case 5:
                    type = PageSegmentationMode.SingleColumn;
                    break;

                case 6:
                    type = PageSegmentationMode.SingleBlockVertText;
                    break;

                case 7:
                    type = PageSegmentationMode.SingleBlock;
                    break;

                case 8:
                    type = PageSegmentationMode.SingleLine;
                    break;

                case 9:
                    type = PageSegmentationMode.SingleWord;
                    break;

                case 10:
                    type = PageSegmentationMode.CircleWord;
                    break;

                case 11:
                    type = PageSegmentationMode.SingleChar;
                    break;

                case 12:
                    type = PageSegmentationMode.SparseText;
                    break;

                case 13:
                    type = PageSegmentationMode.SparseTextOsd;
                    break;

            }

            _tesseractApi.SetPageSegmentationMode(type);


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

                var tessResult = await _tesseractApi.SetImage(source);
                if (tessResult)
                {
                    StringBuilder sb = new StringBuilder();
                    string input = _tesseractApi.Text;
                    Raw = input;
                    List<string> lines = Regex.Split(input, "\r\n|\r|\n|\t").ToList().Where(c=>!string.IsNullOrEmpty(c)).ToList();
                    removals = new List<string>();
                    foreach (string line in lines)
                    {
                        string ret = ScanEmail(line);

                        if (!string.IsNullOrEmpty(ret))
                        {
                            Email = ret;
                            removals.Add(line);
                        }
                        else
                        {
                            ret = ScanTelephoneNumber(line);

                            if (!string.IsNullOrEmpty(ret) && !removals.Contains(line))
                            {
                                PhoneNumber = ret;
                                removals.Add(line);
                            }
                            else
                            {
                                ret = ScanWebsite(line);

                                if (!string.IsNullOrEmpty(ret) && !removals.Contains(line))
                                {
                                    Url = ScanWebsite(line);
                                    removals.Add(line);

                                }
                            }
                        }
                    }

                    foreach (var line in removals)
                    {
                        lines.Remove(line);
                    }

                    MasterAddress = new Address();

                    string state = string.Empty;
                    string ZipCode = string.Empty;
                    string city = string.Empty;

                    FurtherRemovals = new List<string>();
                    bool add;
                    foreach (var line in lines)
                    {
                        add = false;
                        if (line.Trim() == string.Empty)
                        {
                            FurtherRemovals.Remove(line);
                            continue;
                        }

                        ParseStreetAddresses(line);

                        city = ScanCity(line);
                        if (!string.IsNullOrEmpty(city))
                        {
                            MasterAddress.City = city;
                            add = true;
                        }

                        state = ScanState(line);
                        if (!string.IsNullOrEmpty(state))
                        {
                            MasterAddress.State = state;
                            add = true;
                        }

                        ZipCode = ScanZip(line);
                        if (!string.IsNullOrEmpty(ZipCode))
                        {
                            MasterAddress.Zip = ZipCode;
                            add = true;
                        }

                        if (add)
                        {
                            FurtherRemovals.Add(line);
                        }
                    }

                    StreetAddress = MasterAddress.Location + " " + MasterAddress.BuildingName + " " + MasterAddress.Floor;
                    Zip = MasterAddress.Zip;
                    State = MasterAddress.State;
                    City = MasterAddress.City;


                    foreach (var line in FurtherRemovals)
                    {
                        lines.Remove(line);
                    }


                    // try to scan the name and title
                    Others = lines;
                    Title = Others.FirstOrDefault();
                    BusinessName = Others.FirstOrDefault();

                }
                OnPropertyChanged("BusinessName");
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

                OnPropertyChanged("InstructionsBlob");

                ShowResults = true;
                OnPropertyChanged("ShowResults");
            }
            UserDialogs.Instance.HideLoading();
        }




        public string ScanTelephoneNumber(string text)
        {
            string ret = string.Empty;
            var match = Regex.Match(text, TelephoneRegex, RegexOptions.IgnoreCase);
            foreach (var phone in match.Captures)
            {
                ret += phone.ToString();
            }
            return ret;
        }

        public string ScanEmail(string text)
        {
            string ret = string.Empty;
            var match = Regex.Match(text, EmailRegex, RegexOptions.IgnoreCase);
            foreach (var phone in match.Captures)
            {
                ret += phone.ToString();
            }
            return ret;
        }
        public string ScanWebsite(string text)
        {
            string ret = string.Empty;
            var match = Regex.Match(text, WebsiteRegex, RegexOptions.IgnoreCase);
            foreach (var phone in match.Captures)
            {
                ret += phone.ToString();
            }
            return ret;
        }

        public string ScanZip(string text)
        {
            string ret = string.Empty;
            var match = Regex.Match(text, zipPattern, RegexOptions.IgnoreCase);
            foreach (var phone in match.Captures)
            {
                ret += phone.ToString();
            }
            return ret;
        }

        public string ScanCity(string text)
        {
            string ret = string.Empty;
            var match = Regex.Match(text, cityPattern, RegexOptions.IgnoreCase);
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
            var match = Regex.Match(text, statePattern, RegexOptions.IgnoreCase);
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

                FurtherRemovals.Add(inputAddress);

            }


        }

        public void Editor_ImageSaving(object sender, ImageSavingEventArgs args)
        {
            SavedStream = args.Stream;
        }

        public Stream SavedStream { get; set; }
    }


    internal class Address
    {
        public string Location
        {
            get;
            set;
        }
        public string BuildingName
        {
            get;
            set;
        }
        public string Floor
        {
            get;
            set;
        }
        public string State
        {
            get;
            set;
        }

        public string City
        {
            get;
            set;
        }

        public string Zip
        {
            get;
            set;
        }
    }

    internal class AddressParser
    {
        public Address Parse(string inputAddress)
        {
            const string streetNumberPattern = @"(?<streetNumber>\d+\w+\s*(-\s*\d+\w*\s)?)";
            const string locationPattern = @"(?<location>\w.*)";
            const string floorPattern = @"(?<floor>\d+\w+\s+Floor)";
            const string commaPattern = @"(?:\s*,\s*)";

            const string streetNumberKey = "streetNumber";
            const string locationKey = "location";
            const string floorKey = "floor";
            const string undefined = "undefined";

            // search for pattern 1: floor number , location (e.g. '1St Floor, Stanmore House')
            var match = Regex.Match(
                inputAddress,
                floorPattern + commaPattern + locationPattern,
                RegexOptions.IgnoreCase
            );
            if (match.Success && match.Groups.Count == 3)
            {
                _showMatchGroupValues(match);

                var address = new Address();
                address.Location = match.Groups[locationKey].Success ? match.Groups[locationKey].Value : undefined;
                address.Floor = match.Groups[floorKey].Success ? match.Groups[floorKey].Value : undefined;
                return address;
            }

            // search for pattern 2: location , floor number (e.g.: 'Dome Building, 2Nd Floor')
            match = Regex.Match(
                inputAddress,
                locationPattern + commaPattern + floorPattern,
                RegexOptions.IgnoreCase
            );
            if (match.Success && match.Groups.Count == 3)
            {
                _showMatchGroupValues(match);

                var address = new Address();
                address.Location = match.Groups[locationKey].Success ? match.Groups[locationKey].Value : undefined;
                address.Floor = match.Groups[floorKey].Success ? match.Groups[floorKey].Value : undefined;
                return address;
            }

            // search for pattern 3: street number + street name (e.g.: '56A South Clifton Street' or '16B-18B Nicholas Street')
            match = Regex.Match(
               inputAddress,
               streetNumberPattern + locationPattern,
               RegexOptions.IgnoreCase
           );
            if (match.Success && match.Groups.Count == 4)
            {
                _showMatchGroupValues(match);

                var address = new Address();
                address.Location = String.Format(
                    "{0} {1}",
                    match.Groups[streetNumberKey].Success ? match.Groups[streetNumberKey].Value : undefined,
                    match.Groups[locationKey].Success ? match.Groups[locationKey].Value : undefined
                );
                return address;
            }

            return null;
        }

        private void _showMatchGroupValues(Match match)
        {
            var ctGroups = match.Groups.Count;
            Console.WriteLine("\tMatch group count: {0}", ctGroups);

            int x = 0;
            foreach (Group group in match.Groups)
            {
                Console.WriteLine("\tGroup #{0}, value: {1}",
                    ++x,
                    string.IsNullOrEmpty(group.Value) ? "undefined" : group.Value
                );
            }
        }

    }
}