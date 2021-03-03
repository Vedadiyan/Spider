using System;

namespace Spider.WebAPI.Annotations
{
    public class RouteAttribute : Attribute
    {
        public string Route {get;}
        public RouteAttribute(string route) {
            Route = route;
        }
    }
}