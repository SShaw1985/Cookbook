using System;
using SQLite.Net.Attributes;


namespace CookBook.Database.Tables
{
	public class User : CLAgents.Common.POCO.User, IEquatable<User>, IEntity
	{
		[PrimaryKey, AutoIncrement]
		public int PKId { get; set; }

		public DateTime CreatedDate { get; set; }

		public DateTime UpdatedDate { get; set; }

		public User()
		{
		}

		public bool Equals(User other)
		{
			if (other == null)
			{
				return false;
			}

			return this.Id.Equals(other.Id);
		}
	}
}