using System;
using SQLite.Net.Interop;
namespace CookBook.Database
{
	public interface ISQLiteHelper
    {
        ISQLitePlatform GetPlatform();
		string GetDBPath();
	}
}
