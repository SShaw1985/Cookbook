using System;
using System.Threading.Tasks;
using CookBook.Database.Tables;
using SQLite.Net;

namespace CookBook.Database
{
	public interface IDataRepository
	{
		/// <summary>
		/// Initial setup of the DB
		/// </summary>
		void CreateDatabase();

		/// <summary>
		/// Gets a specifics entry in the DB by pkid of IEntity
		/// </summary>
		/// <returns>The type T.</returns>
		/// <param name="entity">Entity.</param>
		/// <typeparam name="T">The type of object to query.</typeparam>
		T Get<T>(T entity) where T : class,IEntity, new();

        T GetById<T>(int pkid) where T : class, IEntity, new();

        /// <summary>
        /// Get all of type T
        /// </summary>
        /// <returns>The type T.</returns>
        /// <typeparam name="T">The type of object to query.</typeparam>
        TableQuery<T> GetAll<T>() where T : class, IEntity, new();

		/// <summary>
		/// Inserts the object of type IEntity
		/// </summary>
		/// <param name="entity">Entity implementing IEntity</param>
		/// <typeparam name="T">The type of object passed in.</typeparam>
		T Save<T>(T entity) where T : IEntity, new();

		void Delete<T>(IEntity item);

		/// <summary>
		/// Deletes all from the table of type T.
		/// </summary>
		/// <typeparam name="T">the type of table to wipe.</typeparam>
		void DeleteAll<T>();

		/// <summary>
		/// Wipe all local data
		/// </summary>
		void Logout();

		/// <summary>
		/// Update the specified entity.
		/// </summary>
		/// <param name="entity">Entity.</param>
		/// <typeparam name="T">The type of object passed in.</typeparam>
		void Update<T>(T entity) where T : IEntity, new();

		/// <summary>
		/// Executes custom sql
		/// </summary>
		/// <returns>Type T passed in.</returns>
		/// <param name="query">Query.</param>
		/// <param name="args">Arguments.</param>
		/// <typeparam name="T">The type of object to query.</typeparam>
		T ExecuteSql<T>(string query, object[] args);

        void Export();
	}
}
