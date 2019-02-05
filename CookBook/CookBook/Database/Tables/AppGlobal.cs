using System;
using SQLite.Net.Attributes;

namespace CookBook.Database.Tables
{
	public class AppGlobal : IEntity
	{

		[PrimaryKey, AutoIncrement]
		public int PKId { get; set; }

		public DateTime CreatedDate { get; set; }

		public DateTime UpdatedDate { get; set; }
	}
}
