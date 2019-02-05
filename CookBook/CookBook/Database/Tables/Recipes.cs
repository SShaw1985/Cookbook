using System;
using System.Collections.Generic;
using SQLite.Net.Attributes;
namespace CookBook.Database.Tables
{
    public class Recipes :IEntity
    {
        public Recipes()
        {
        }
        [PrimaryKey, AutoIncrement]
        public int PKId { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }
        public string Ingredients { get; set; }
        public string Text { get; set; }
        public string Title { get; set; }
        public string Tags { get; set; }

        [Ignore]
        public List<Pictures> Pictures { get; set; }
    }
}