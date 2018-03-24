using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsersDirectoryApp.Controllers;

namespace UsersDirectoryApp.Tests
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<IDataProvider>().To<UsersDataProviderStub>();
            Bind<HomeController>().ToSelf();
        }
    }
}
