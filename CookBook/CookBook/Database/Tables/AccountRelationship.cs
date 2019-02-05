using System;
using SQLite.Net.Attributes;

namespace CookBook.Database.Tables
{
	public class AccountRelationship : CLAgents.Common.POCO.AccountRelationship, IEquatable<AccountRelationship>, IEntity
	{
		[PrimaryKey, AutoIncrement]
		public int PKId { get; set; }

		public DateTime CreatedDate { get; set; }

		public DateTime UpdatedDate { get; set; }

		public AccountRelationship()
		{
		}

		public bool Equals(AccountRelationship other)
		{
			if (other == null)
			{
				return false;
			}

			return this.PKId.Equals(other.PKId);
		}
	}
}