using System;
using SQLite.Net.Attributes;

namespace CookBook.Database.Tables
{
	public class AppSettings : IEntity
	{
		[PrimaryKey, AutoIncrement]
		public int PKId { get; set; }

		public DateTime CreatedDate { get; set; }

		public DateTime UpdatedDate { get; set; }

		public string Key { get; set; }

		public string Value { get; set; }

		public string DataType { get; set; }
	}
}
