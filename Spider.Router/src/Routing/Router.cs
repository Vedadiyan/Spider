using System;
using System.Collections.Generic;
using System.Linq;

namespace Spider.Routing
{
    public class Router
    {
        public static Dictionary<string, Dictionary<int, List<Route>>> Routes { get; private set; }
        private static HashSet<string> track = new HashSet<string>();
        public static void RegisterRoutes(Route[] routes)
        {
            Routes = new Dictionary<string, Dictionary<int, List<Route>>>();
            foreach (var methodGroup in routes.GroupBy(g => g.Method))
            {
                Dictionary<int, List<Route>> routeCollection = new Dictionary<int, List<Route>>();
                foreach (var routeGroup in methodGroup.GroupBy(g => g.Size))
                {
                    List<Route> orderedRoutes = routeGroup.OrderByDescending(x => x.SortValue).ToList();
                    foreach (var routeItem in orderedRoutes)
                    {
                        if (!track.Add(routeItem.Hash))
                        {
                            throw new Exception("Duplicate Route");
                        }
                    }
                    routeCollection.Add(routeGroup.Key, orderedRoutes);
                }
                Routes.Add(methodGroup.Key, routeCollection);
            }
        }
        public static void RegisterRoute(Route route)
        {
            if (track.Add(route.Hash))
            {
                if (Routes.TryGetValue(route.Method, out Dictionary<int, List<Route>> outValue))
                {
                    if (outValue.TryGetValue(route.Size, out List<Route> routeValue))
                    {
                        routeValue.Add(route);
                    }
                    else
                    {
                        outValue.Add(route.Size, new List<Route> { route });
                    }
                }
                else
                {
                    Routes.Add(route.Method, new Dictionary<int, List<Route>> { [route.Size] = new List<Route> { route } });
                }

            }
        }
        public static void UnregisterRoute(Route route)
        {
            track.Remove(route.Hash);
            Routes[route.Method][route.Size].Remove(Routes[route.Method][route.Size].FirstOrDefault(x => x.Hash == route.Hash));
        }
        public static Route GetRoute(string method, string absolutePathWithoutQueryString)
        {
            string[] routeValues = absolutePathWithoutQueryString.Split('/').Select(x => x.ToLower()).ToArray();
            if (Routes.TryGetValue(method.ToLower(), out Dictionary<int, List<Route>> routeGroup))
            {
                if (routeGroup.TryGetValue(routeValues.Length, out List<Route> routes))
                {
                    bool result = true;
                    Route bestCandidate = default;
                    int score = 0;
                    int bestScore = -1;
                    foreach (var routeItem in routes)
                    {
                        result = true;
                        score = 0;
                        for (int i = 0; i < routeItem.Parameters.Length; i++)
                        {
                            if (routeItem.Parameters[i].ReadOnly && routeItem.Parameters[i].Name != routeValues[i])
                            {
                                result = false;
                                break;
                            }
                            else if (routeItem.Parameters[i].ReadOnly && routeItem.Parameters[i].Name == routeValues[i])
                            {
                                score++;
                            }
                        }
                        if (result && score > bestScore)
                        {
                            bestCandidate = routeItem;
                            bestScore = score;
                        }
                    }
                    return bestCandidate;
                }
            }
            return default;
        }
    }

}