using System;
using System.Windows.Input;
using CookBook.Database;
using CookBook.Database.Tables;

using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace CookBook.ViewModels
{
    public class TakePhotoViewModel : BaseViewModel
    {
        public TakePhotoViewModel()
        {
            Title = "Scan recipe";

        }

        // For printed text, change to TextRecognitionMode.Printed
        private const TextRecognitionMode textRecognitionMode =
            TextRecognitionMode.Handwritten;

        const int numberOfCharsInOperationId = 36;

        public ICommand TakePhotoCommand { get { return new Command(TakePhoto); } }

        private async void TakePhoto(object obj)
        {
            MediaFile photo;
            if (CrossMedia.Current.IsCameraAvailable)
            {
                photo = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                {
                    PhotoSize= PhotoSize.Large,
                    RotateImage = true
                    
                     
                });
            }
            else
            {
                photo = await CrossMedia.Current.PickPhotoAsync();
            }

            string apiKey = "efd1871133d44c9da588f54dd14b4377";
            string apiRoot = "https://westus.api.cognitive.microsoft.com";

            ComputerVisionClient computerVision = new ComputerVisionClient(
                new ApiKeyServiceClientCredentials(apiKey),
                new System.Net.Http.DelegatingHandler[] { });

            computerVision.Endpoint = apiRoot;
            var t2 = AnalyzeLocalAsync(computerVision,photo.Path);

        }

    

        // Analyze a local image
        private static async Task AnalyzeLocalAsync(
            ComputerVisionClient computerVision, string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                Console.WriteLine(
                    "\nUnable to open or read localImagePath:\n{0} \n", imagePath);
                return;
            }

            using (Stream imageStream = File.OpenRead(imagePath))
            {
                // Start the async process to recognize the text
                RecognizeTextInStreamHeaders textHeaders =
                    await computerVision.RecognizeTextInStreamAsync(
                        imageStream, textRecognitionMode);

                await GetTextAsync(computerVision, textHeaders.OperationLocation);
            }
        }

        private static async Task GetTextAsync(
            ComputerVisionClient computerVision, string operationLocation)
        {
            // Retrieve the URI where the recognized text will be
            // stored from the Operation-Location header
            string operationId = operationLocation.Substring(
                operationLocation.Length - numberOfCharsInOperationId);

            Console.WriteLine("\nCalling GetHandwritingRecognitionOperationResultAsync()");
            TextOperationResult result =
                await computerVision.GetTextOperationResultAsync(operationId);

            // Wait for the operation to complete
            int i = 0;
            int maxRetries = 10;
            while ((result.Status == TextOperationStatusCodes.Running ||
                    result.Status == TextOperationStatusCodes.NotStarted) && i++ < maxRetries)
            {
                Console.WriteLine(
                    "Server status: {0}, waiting {1} seconds...", result.Status, i);
                await Task.Delay(1000);

                result = await computerVision.GetTextOperationResultAsync(operationId);
            }

            StringBuilder text = new StringBuilder();
            // Display the results
            Console.WriteLine();
            var lines = result.RecognitionResult.Lines;
            foreach (Line line in lines)
            {
                text.AppendLine(line.Text);
            }

            Recipes recp = new Recipes();
            recp.Title = Guid.NewGuid().ToString();
            recp.Text = text.ToString();
            DataRepository.Instance.Save<Recipes>(recp);
           
        }
    }
}