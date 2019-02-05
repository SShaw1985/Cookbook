using System;
using System.Text;
using CookBook.Database.Tables;
using Newtonsoft.Json;
using Plugin.Messaging;
using SQLite.Net;
using SQLite.Net.Interop;
using Xamarin.Essentials;

namespace CookBook.Database
{
    public class DataRepository : IDataRepository
    {
        private SQLiteConnection _db;
        ISQLiteHelper helper { get; set; }
        public static IDataRepository Instance { get; set; }
        public DataRepository(ISQLiteHelper sqlConnection)
        {
            SQLite.Net.IBlobSerializer serializer = new SQLite.Net.BlobSerializerDelegate(SerializeObject, DeSerializeObject, CanSerialize);
            helper = sqlConnection;

            _db = new SQLiteConnection(sqlConnection.GetPlatform(), sqlConnection.GetDBPath(), false, serializer);

        }

        public void CreateDatabase()
        {
           // _db.CreateTable<Recipes>(CreateFlags.AutoIncPK);
           // _db.CreateTable<Pictures>(CreateFlags.AutoIncPK);
        }


        public T Get<T>(T entity) where T : class, IEntity, new()
        {
            return _db.Get<T>(entity.PKId);
        }

        TableQuery<T> IDataRepository.GetAll<T>()
        {
            return _db.Table<T>();
        }

        public T Save<T>(T entity) where T : IEntity, new()
        {
            entity.CreatedDate = DateTime.Now;
            entity.UpdatedDate = DateTime.Now;
            entity.PKId = _db.Insert(entity);
            return entity;
        }

        public void Delete<T>(IEntity item)
        {
            _db.Delete<T>(item.PKId);
        }

        public void DeleteAll<T>()
        {
            _db.DeleteAll<T>();
        }

        public void Logout()
        {
            _db.DeleteAll<Recipes>();
        }

        public void Update<T>(T entity) where T : IEntity, new()
        {
            _db.Update(entity);

        }

        public T ExecuteSql<T>(string query, object[] args)
        {
            return _db.ExecuteScalar<T>(query, args);
        }

        private byte[] SerializeObject(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);        // subst your own serializer
            return Encoding.UTF8.GetBytes(json);
        }

        private object DeSerializeObject(byte[] blob, Type type)
        {
            var json = Encoding.UTF8.GetString(blob);
            var result = JsonConvert.DeserializeObject(json, type);
            return result;
        }

        private bool CanSerialize(object obj)
        {
            return true;
        }

        T IDataRepository.GetById<T>(int pkid)
        {
            return _db.Get<T>(pkid);
        }

        public async void Export()
        {

            var email = new EmailMessageBuilder()
              .To("stephen.shaw85@gmail.com")
              .Subject("DBExport")
              .Body("Hello from your friends at Xamarin!")
                .WithAttachment(helper.GetDBPath(), "application/octet-stream").Build();
            if (CrossMessaging.Current.EmailMessenger.CanSendEmail)
            {
                CrossMessaging.Current.EmailMessenger.SendEmail(email);
            }
        }
    }
}