using SQLite.Net;

namespace CookBook.Database
{
	public interface ISQLite
	{
		SQLiteConnection GetConnection();
	}
}
