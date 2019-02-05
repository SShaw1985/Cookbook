using Xamarin.Forms;

namespace CookBook.Interfaces
{
	public enum Pages
	{

        MainPage,
        About,
        Photo,
        Recipies,
        Items,
        Details,
        Add,
        Edit
    }

	public interface IPageFactory
	{
		Page GetPage(Pages page);
	}
}

