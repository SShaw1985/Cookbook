using System;
using System.Collections.Generic;
using CookBook.Controls;
using CookBook.ViewModels;
using Xamarin.Forms;

namespace CookBook.Views
{
    public partial class BusinessScanner : ContentPage
    {
        public BusinessScanner()
        {
            BindingContext = App.BusinessScannerViewModel;
            InitializeComponent();
        }

        private double _startTranslationX { get; set; }
        private double _startTranslationY { get; set; }

        private double _startPositionX { get; set; }
        private double _startPositionY { get; set; }


        private double _endPositionX { get; set; }
        private double _endPositionY { get; set; }


        private void PanGestureRecognizer_OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            var box = (ContentView)sender;

            if (e.StatusType == GestureStatus.Started)
            {
                var position = GetScreenCoordinates(box);
                
                _startTranslationX = box.TranslationX;
                _startTranslationY = box.TranslationY;

                _startPositionX = position.X;
                _startPositionY = position.Y;
            }
            else if (e.StatusType == GestureStatus.Running)
            {

                box.TranslationX = _startTranslationX + e.TotalX;
                box.TranslationY = _startTranslationY + e.TotalY;
            }
            else if (e.StatusType == GestureStatus.Completed)
            {

                var position = GetScreenCoordinates(box);

                var x = _startPositionX + (_startTranslationX + box.TranslationX);
                var y = _startPositionY + (_startTranslationY + box.TranslationY);
                //handle drop here (depending on your requirements)

                bool translate = true;
                foreach (var child in PositionGrid.Children)
                {
                    var childPos = GetScreenCoordinates(child);
                    if (new Rectangle(childPos.X,childPos.Y,child.Width, child.Height).Contains( (new Rectangle(x,y,1,1))))
                    {
                        try
                        {
                            var textBox = child as Entry;
                            var content = box.Content as Label;

                            textBox.Text = content.Text;
                            translate = false;
                        }
                        catch(Exception ex)
                        {
                            string s = ex.Message;
                        }
                    }
                }

                box.TranslateTo(_startTranslationX, _startTranslationY);

            }
        }

        public (double X, double Y) GetScreenCoordinates(VisualElement view)
        {
            // A view's default X- and Y-coordinates are LOCAL with respect to the boundaries of its parent,
            // and NOT with respect to the screen. This method calculates the SCREEN coordinates of a view.
            // The coordinates returned refer to the top left corner of the view.

            // Initialize with the view's "local" coordinates with respect to its parent
            double screenCoordinateX = view.X;
            double screenCoordinateY = view.Y;

            // Get the view's parent (if it has one...)
            if (view.Parent.GetType() != typeof(App))
            {
                VisualElement parent = (VisualElement)view.Parent;


                // Loop through all parents
                while (parent != null)
                {
                    // Add in the coordinates of the parent with respect to ITS parent
                    screenCoordinateX += parent.X;
                    screenCoordinateY += parent.Y;

                    // If the parent of this parent isn't the app itself, get the parent's parent.
                    if (parent.Parent.GetType() == typeof(App))
                        parent = null;
                    else
                        parent = (VisualElement)parent.Parent;
                }
            }

            // Return the final coordinates...which are the global SCREEN coordinates of the view
            return (screenCoordinateX, screenCoordinateY);
        }
    }
}
