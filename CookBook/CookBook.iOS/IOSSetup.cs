using AgentAssist.Mobile.Ios.Services.SQL;
using Autofac;
using CookBook;
using CookBook.Database;

namespace CookBook.iOS
{
    public class IOSSetup : AppSetup
    {
        protected override void RegisterDepenencies(ContainerBuilder cb)
        {
            base.RegisterDepenencies(cb);

            cb.RegisterType<SQLiteHelper>().As<ISQLiteHelper>().SingleInstance();
        }
    }
}

