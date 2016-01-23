/*
Copyright 2012, Marcus McKinnon, E.J. Wilburn, Kevin Williams
This program is distributed under the terms of the GNU General Public License.

This file is part of Palaver.

Palaver is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 2 of the License, or
(at your option) any later version.

Palaver is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Palaver.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Configuration;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNet.SignalR.Hubs;
using Palaver2.Models;
using CodeFirstMembership.Models;

namespace Palaver2
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapConnection<MessageConnection>("echo", "/echo");
            //RouteTable.Routes.MapHubs();

            routes.MapRoute(
                "Thread_Comment", // Route name
                "Thread/{threadId}/{commentId}", // URL with parameters
                new { controller = "Home", action = "GetComment" } // Parameter defaults
            );
            routes.MapRoute(
                "Thread", // Route name
                "Thread/{threadId}", // URL with parameters
                new { view="Home", controller = "Home", action = "GetThread", threadId = UrlParameter.Optional } // Parameter defaults
            );
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{threadId}", // URL with parameters
                new { controller = "Home", action = "Index", threadId = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            Database.SetInitializer<PalaverDb>(new DropCreateDatabaseIfModelChanges<Palaver2.Models.PalaverDb>());
            //Database.SetInitializer<PalaverDb>(new DropCreateDatabaseAlways<Palaver2.Models.PalaverDb>());

            //Database.SetInitializer<PalaverDb>(new CodeFirstContextInit());
            //var Context = new PalaverDb();
            //Context.Users.FirstOrDefault();

            // Update the database if needed.
            var migratorConfig = new Palaver2.Migrations.Configuration();
            var dbMigrator = new DbMigrator(migratorConfig);
            dbMigrator.Update();
            
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            //GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds( 20 );
        }
    }
}