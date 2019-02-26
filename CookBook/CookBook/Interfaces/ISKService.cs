using System;
using System.IO;
using Android.Graphics;

namespace CookBook.Interfaces
{
    public interface ISKService
    {
        Stream Resize(Stream streamFromPhoto,int width, int height);
    }

}
