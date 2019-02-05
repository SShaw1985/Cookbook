using Autofac;
using CookBook.Database;
using CookBook.Interfaces;
using CookBook.Services;
using CookBook.ViewModels; 

namespace CookBook
{
	// we can inherit and override this class on each platform to make services specific
	public class AppSetup
	{
		public string PackageFolder { get; set; }
		public string ImportPackagePath { get; set; }

		/// <summary>
		/// Creates an instance of the AutoFac container
		/// </summary>
		/// <returns>A new instance of the AutoFac container</returns>
		/// <remarks>
		/// https://github.com/autofac/Autofac/wiki
		/// </remarks>
		public IContainer CreateContainer()
		{
			var cb = new ContainerBuilder();

			RegisterDepenencies(cb);

			return cb.Build();
		}

		protected virtual void RegisterDepenencies(ContainerBuilder cb)
		{
			// Services
			
			cb.RegisterType<NavigationService>().As<INavigationService>().SingleInstance();
			cb.RegisterType<AppNavigation>().As<IAppNavigation>().SingleInstance();
            cb.RegisterType<PageFactory>().As<IPageFactory>().SingleInstance();
            cb.RegisterType<DataRepository>().As<IDataRepository>().SingleInstance();

            // View Models
            cb.RegisterType<AboutViewModel>().SingleInstance();
            cb.RegisterType<RecipesViewModel>().SingleInstance();
            cb.RegisterType<TakePhotoViewModel>().SingleInstance();
            cb.RegisterType<ItemDetailViewModel>().SingleInstance();
            cb.RegisterType<AddRecipeViewModel>().SingleInstance();
            cb.RegisterType<EditRecipeViewModel>().SingleInstance();
        }
	}
}

