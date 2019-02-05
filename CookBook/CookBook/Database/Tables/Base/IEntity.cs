using System;
using SQLite.Net.Attributes;

namespace CookBook.Database.Tables
{
	public interface IEntity
	{
		[PrimaryKey, AutoIncrement]
		int PKId { get; set; }

		DateTime CreatedDate{ get; set; }

		DateTime UpdatedDate{ get; set; }
	}
}