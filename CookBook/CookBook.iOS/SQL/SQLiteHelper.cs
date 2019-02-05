using System;
using System.IO;
using CookBook.Database;
using SQLite.Net.Interop;
using SQLite.Net.Platform.XamarinIOS;

namespace AgentAssist.Mobile.Ios.Services.SQL
{

	public class SQLiteHelper : ISQLiteHelper
	{
		public string GetDBPath()
		{
			var fileName = "cookbooik.db3";
			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var libraryPath = Path.Combine(documentsPath, "..", "Library");
			var path = Path.Combine(libraryPath, fileName);

			return path;
		}

		public ISQLitePlatform GetPlatform()
		{
			return new SQLitePlatformIOS();
		}
	}

}
