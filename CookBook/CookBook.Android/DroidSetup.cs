using CookBook;
using Autofac;
using CookBook.Database;
using CookBook.Android.SQL;

namespace CookBook.Droid
{
	public class DroidSetup : AppSetup
	{
		protected override void RegisterDepenencies(ContainerBuilder cb)
		{
			base.RegisterDepenencies(cb);

			//cb.RegisterType<DroidThemer>().As<IThemer>().SingleInstance(); //can't see what this does? class also commented out...
            cb.RegisterType<SQLiteHelper>().As<ISQLiteHelper>().SingleInstance();
            //cb.Register<IDevice>(t => AndroidDevice.CurrentDevice);
			
		}
	}
}

