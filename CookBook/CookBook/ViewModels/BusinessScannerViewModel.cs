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
using SkiaSharp;
using Syncfusion.SfImageEditor.XForms;
using Tesseract;
using Xamarin.Forms;


namespace CookBook.ViewModels
{
    public class BusinessScannerViewModel : BaseViewModel
    {
        private IAppNavigation Navi { get; set; }
        private readonly ITesseractApi _tesseractApi;
        private readonly ISKService SKService;

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

        public BusinessScannerViewModel(IAppNavigation _navi, ITesseractApi tesseractApi, ISKService _skservice)
        {
            Navi = _navi;
            _tesseractApi = tesseractApi;
            SKService = _skservice;
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
            SavedStream = null;
            UserDialogs.Instance.ShowLoading("Scanning");

            strm = null;

            UserDialogs.Instance.HideLoading();


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
            strm = photo.GetStream();
            Path = photo.Path;
            Image = ImageSource.FromFile(Path);

            OnPropertyChanged("ShowResults");
        }

        private async void PickPhoto(object obj)
        {
            try
            {

                ShowResults = false;
                SavedStream = null;
                UserDialogs.Instance.ShowLoading("Scanning");

                strm = null;

                UserDialogs.Instance.HideLoading();


                MediaFile photo;

                photo = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions(){
                    PhotoSize = PhotoSize.Large
                });
                Stream streamFromPhoto = photo.GetStream();


                Stream bit = SKService.Resize(streamFromPhoto, 400, 300);
              
                Path = photo.Path;
                Image = ImageSource.FromStream(() => { return bit; });

                OnPropertyChanged("ShowResults");
            }
            catch(Exception ex)
            {
                string ss = ex.Message;

            }
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
               stream= File.OpenRead(Path);
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

        //Resize
        public SKBitmap Resize(SKBitmap bmp, int newWidth, int newHeight)
        {

            SKBitmap temp = (SKBitmap)bmp;

            SKBitmap bmap = new SKBitmap(newWidth, newHeight);

            double nWidthFactor = (double)temp.Width / (double)newWidth;
            double nHeightFactor = (double)temp.Height / (double)newHeight;

            double fx, fy, nx, ny;
            int cx, cy, fr_x, fr_y;
            SKColor color1 = new SKColor();
            SKColor color2 = new SKColor();
            SKColor color3 = new SKColor();
            SKColor color4 = new SKColor();
            byte nRed, nGreen, nBlue;

            byte bp1, bp2;

            for (int x = 0; x < bmap.Width; ++x)
            {
                for (int y = 0; y < bmap.Height; ++y)
                {

                    fr_x = (int)Math.Floor(x * nWidthFactor);
                    fr_y = (int)Math.Floor(y * nHeightFactor);
                    cx = fr_x + 1;
                    if (cx >= temp.Width) cx = fr_x;
                    cy = fr_y + 1;
                    if (cy >= temp.Height) cy = fr_y;
                    fx = x * nWidthFactor - fr_x;
                    fy = y * nHeightFactor - fr_y;
                    nx = 1.0 - fx;
                    ny = 1.0 - fy;

                    color1 = temp.GetPixel(fr_x, fr_y);
                    color2 = temp.GetPixel(cx, fr_y);
                    color3 = temp.GetPixel(fr_x, cy);
                    color4 = temp.GetPixel(cx, cy);

                    // Blue
                    bp1 = (byte)(nx * color1.Blue + fx * color2.Blue);

                    bp2 = (byte)(nx * color3.Blue + fx * color4.Blue);

                    nBlue = (byte)(ny * (double)(bp1) + fy * (double)(bp2));

                    // Green
                    bp1 = (byte)(nx * color1.Green + fx * color2.Green);

                    bp2 = (byte)(nx * color3.Green + fx * color4.Green);

                    nGreen = (byte)(ny * (double)(bp1) + fy * (double)(bp2));

                    // Red
                    bp1 = (byte)(nx * color1.Red + fx * color2.Red);

                    bp2 = (byte)(nx * color3.Red + fx * color4.Red);

                    nRed = (byte)(ny * (double)(bp1) + fy * (double)(bp2));

                    bmap.SetPixel(x, y,SKColor.Parse(Helper.GetHexString(Color.FromRgb(nRed, nGreen, nBlue))));
                }
            }



            bmap = SetGrayscale(bmap);
            bmap = RemoveNoise(bmap);

            return bmap;

        }

     
        //SetGrayscale
        public SKBitmap SetGrayscale(SKBitmap img)
        {
            SKBitmap temp = (SKBitmap)img;
            SKBitmap bmap = (SKBitmap)temp.Copy();
            SKColor c;
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    byte gray = (byte)(.299 * c.Red + .587 * c.Green + .114 * c.Blue);
                    var colo = Color.FromRgb(gray, gray, gray);
                    bmap.SetPixel(i, j, SKColor.Parse(Helper.GetHexString(colo)));
                }
            }
            return (SKBitmap)bmap.Copy();

        }

        //RemoveNoise
        public SKBitmap RemoveNoise(SKBitmap bmap)
        {

            for (var x = 0; x < bmap.Width; x++)
            {
                for (var y = 0; y < bmap.Height; y++)
                {
                    var pixel = bmap.GetPixel(x, y);
                    if (pixel.Red < 162 && pixel.Green < 162 && pixel.Blue < 162)
                        bmap.SetPixel(x, y, SKColor.Parse("#111111"));
                    else if (pixel.Red > 162 && pixel.Green > 162 && pixel.Blue > 162)
                        bmap.SetPixel(x, y, SKColor.Parse("#ffffff")  );
                }
            }

            return bmap;
        }
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

    public static class Helper
    {
        public static string GetHexString(this Xamarin.Forms.Color color)
        {
            var red = (int)(color.R * 255);
            var green = (int)(color.G * 255);
            var blue = (int)(color.B * 255);
            var alpha = (int)(color.A * 255);
            var hex = $"#{alpha:X2}{red:X2}{green:X2}{blue:X2}";

            return hex;
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}