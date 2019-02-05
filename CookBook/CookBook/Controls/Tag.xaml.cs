using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace CookBook.Controls
{
    public partial class Tag : Frame
    {
        public static readonly BindableProperty HeadingTextProperty = BindableProperty.Create(nameof(HeadingText), typeof(string), typeof(Tag), default(string), Xamarin.Forms.BindingMode.OneWay);

        public string HeadingText
        {
            get
            {
                return (string)GetValue(HeadingTextProperty);
            }

            set
            {
                SetValue(HeadingTextProperty, value);
            }
        }

        public Tag()
        {

            InitializeComponent();
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == HeadingTextProperty.PropertyName)
            {
                lbl.Text = HeadingText;
            }

        }

    }
}
