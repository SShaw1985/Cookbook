using CookBook;
using Autofac;
using CookBook.Database;
using CookBook.Android.SQL;
using Tesseract;
using Tesseract.Droid;
using Android.Content;
using CookBook.Interfaces;

namespace CookBook.Droid
{
	public class DroidSetup : AppSetup
	{
        private Context applicationContext;

        public DroidSetup(Context applicationContext)
        {
            this.applicationContext = applicationContext;
        }

        protected override void RegisterDepenencies(ContainerBuilder cb)
		{
			base.RegisterDepenencies(cb);

            //cb.RegisterType<DroidThemer>().As<IThemer>().SingleInstance(); //can't see what this does? class also commented out...
            cb.RegisterType<SQLiteHelper>().As<ISQLiteHelper>().SingleInstance();
            cb.RegisterType<SKService>().As<ISKService>().SingleInstance();
            cb.Register<ITesseractApi>((cont, parameters) =>
            {
                return new TesseractApi(applicationContext, AssetsDeployment.OncePerInitialization);
            });


        }
	}
}

