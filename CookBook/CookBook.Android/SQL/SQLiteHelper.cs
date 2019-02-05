
using System.IO;
using Android.App;
using Android.Content;
using Android.Net;
using CookBook.Database;
using SQLite.Net.Interop;

namespace CookBook.Android.SQL
{
	 public class SQLiteHelper : ISQLiteHelper
    {
		public string GetDBPath()
		{
			var fileName = "cookbook.db3";
			var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			var path = Path.Combine(documentsPath, fileName);
			return path;
		}

		public ISQLitePlatform GetPlatform()
        {
            return new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroidN();
        }

       
    }
}
