using Ninject;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using UsersDirectoryApp.Controllers;

namespace UsersDirectoryApp
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<IDataProvider>().To<UsersDataProvider>();
            Bind<HomeController>().ToSelf();
        }
    }

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            NinjectContainer.RegisterAssembly();
        }
    }
}
