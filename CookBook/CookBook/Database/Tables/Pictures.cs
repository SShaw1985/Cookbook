using System;
using SQLite.Net.Attributes;
namespace CookBook.Database.Tables
{
    public class Pictures :IEntity
    {
        public Pictures()
        {
        }
        [PrimaryKey, AutoIncrement]
        public int PKId { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }
        public int RecipeeId { get; set; }
       
        public byte[] Blob { get; set; }
		public int Type {get; set;}
    }
}
