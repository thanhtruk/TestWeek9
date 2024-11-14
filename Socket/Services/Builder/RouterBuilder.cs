using Socket.Core;
using Socket.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socket.Services.Builder
{
    public class RouterBuilder
    {
        private Dictionary<string, Func<HttpRequest, string>> routes;

        public RouterBuilder()
        {
            routes = new Dictionary<string, Func<HttpRequest, string>>();
        }

        public RouterBuilder AddRoute(string path, Func<HttpRequest, string> handler)
        {
            routes[path] = handler;
            return this;
        }

        public Router Build()
        {
            return new Router(routes);
        }
    }
}
