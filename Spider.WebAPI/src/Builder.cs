using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Spider.ArcheType;
using Spider.Routing;
using Spider.Core.Web;
using Spider.WebAPI.Abstraction;
using Spider.WebAPI.Annotations;
using Spider.WebAPI.Handlers;
using Spider.WebAPI.Renders;

namespace Spider.WebAPI
{
    public class Builder
    {
        public static WebServer Build(String[] prefixes)
        {
            TypeInfo[] definedTypes = Assembly.GetEntryAssembly().DefinedTypes.ToArray();
            List<Route> routes = new List<Route>();
            foreach (TypeInfo typeInfo in definedTypes)
            {
                ControllerAttribute controllerAttribute = typeInfo.GetCustomAttribute<ControllerAttribute>();
                RouteAttribute routeAttribute = typeInfo.GetCustomAttribute<RouteAttribute>();
                if (controllerAttribute != null)
                {
                    var properties = typeInfo.GetProperties().Select(x => new { Property = x, Attribute = x.GetCustomAttribute<AutoWireAttribute>() });
                    var contextProperty = properties.FirstOrDefault(x => x.Attribute != null && x.Property.PropertyType == typeof(IContext));
                    properties = properties.Where(x => x.Attribute != null && x.Property.PropertyType != typeof(IContext)).ToArray();
                    Func<Object> instanceGenerator = null;
                    foreach (MethodInfo methodInfo in typeInfo.GetMethods(BindingFlags.Static | BindingFlags.Public))
                    {
                        if (methodInfo.GetCustomAttribute<ControllerInstanceAttribute>() != null)
                        {
                            instanceGenerator = (Func<Object>)methodInfo.CreateDelegate(typeof(Func<Object>));
                        }
                    }
                    if (instanceGenerator == null)
                    {
                        instanceGenerator = () =>
                        {
                            return Activator.CreateInstance(typeInfo);
                        };
                    }
                    foreach (MethodInfo methodInfo in typeInfo.GetMethods())
                    {
                        Boolean isAction = (methodInfo.ReturnType == typeof(IActionResult));
                        Boolean isAsyncAction = (methodInfo.ReturnType == typeof(Task<IActionResult>));
                        if (isAction || isAsyncAction)
                        {
                            String _route = routeAttribute?.Route.TrimStart('/').TrimEnd('/') ?? typeInfo.Name.ToLower().Replace("controller", "");
                            VerbAttribute verbAttribute = methodInfo.GetCustomAttribute<VerbAttribute>();
                            RouteAttribute methodRouteAttribute = methodInfo.GetCustomAttribute<RouteAttribute>();
                            var parameters = methodInfo.GetCustomAttributes<ParameterAttribute>();
                            var filters = methodInfo.GetCustomAttributes<ExecAttribute>();
                            var beforeExecution = filters?.Where(x => x.ExecutionScope == ExecutionScope.Before).OrderBy(x => x.Order).ToArray();
                            var afterExecution = filters?.Where(x => x.ExecutionScope == ExecutionScope.After).Select(x => new
                            {
                                Filter = x,
                                AutoWireProperties = x.GetType().GetProperties().Where(y => y.GetCustomAttribute<AutoWireAttribute>() != null).ToArray()
                            }).OrderBy(x => x.Filter.Order).ToArray();
                            if (methodRouteAttribute != null)
                            {
                                if (methodRouteAttribute.Route.StartsWith("/"))
                                {
                                    _route = methodRouteAttribute.Route.TrimStart('/').TrimEnd('/');
                                }
                                else
                                {
                                    _route = _route.Split('?')[0] + '/' + methodRouteAttribute.Route.TrimStart('/').TrimEnd('/');
                                }

                            }
                            var __route = _route.Split('?');
                            if (parameters != null)
                            {
                                foreach (var parameter in parameters)
                                {
                                    String name = $"{{{parameter.Name}}}";
                                    if (__route[0].Contains(name))
                                    {
                                        _route = _route.Replace(name, $"{{{parameter.Name}:{parameter.TypeCode.ToString()}{(parameter.IsArray ? ".Array" : "")}}}");
                                    }
                                    else if (__route.Length > 1 && __route[1].Contains(name))
                                    {
                                        _route = _route.Replace(name, $"{parameter.Name}:{parameter.TypeCode.ToString()}{(parameter.IsArray ? ".Array" : "")}");
                                    }
                                    else
                                    {
                                        throw new Exception("Invalid route parameter");
                                    }
                                }
                            }
                            Func<Services, IContext, Task<IActionResult>> actionDelegate = async (services, context) =>
                            {
                                Object instance = instanceGenerator();
                                if (contextProperty != null)
                                {
                                    contextProperty.Property.SetValue(instance, context);
                                }
                                foreach (var property in properties)
                                {
                                    Object value = services.GetService(property.Attribute.Name ?? property.Property.PropertyType.Name);
                                    property.Property.SetValue(instance, value);
                                }
                                IActionResult result = null;
                                if (beforeExecution != null)
                                {
                                    foreach (var i in beforeExecution)
                                    {
                                        Continuation next = await i.Run(context);
                                        switch (next.ContinuationState)
                                        {
                                            case ContinuationState.Continue:
                                                {
                                                    break;
                                                }
                                            case ContinuationState.Cancel:
                                                {
                                                    return new ContentResult(System.Net.HttpStatusCode.NoContent, default, default);
                                                }
                                            case ContinuationState.CancelWithError:
                                                {
                                                    return new ContentResult(next.StatusCode, next.ContentType, next.Content);
                                                }
                                            case ContinuationState.CancelWithRedirection:
                                                {
                                                    return new RedirectResult(next.Url);
                                                }
                                        }
                                    }
                                }
                                Exception exception = null;
                                try
                                {
                                    if (isAsyncAction)
                                    {
                                        result = await (Task<IActionResult>)methodInfo.Invoke(instance, null);
                                    }
                                    else
                                    {
                                        result = (IActionResult)methodInfo.Invoke(instance, null);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    exception = ex;
                                }
                                if (afterExecution != null)
                                {
                                    foreach (var i in afterExecution)
                                    {
                                        if (exception != null)
                                        {
                                            PropertyInfo exceptionProperty = i.AutoWireProperties.SingleOrDefault(x => x.PropertyType == typeof(Exception));
                                            if (exceptionProperty != null)
                                            {
                                                exceptionProperty.SetValue(i.Filter, exception);
                                            }
                                        }
                                        Continuation next = await i.Filter.Run(context);
                                        switch (next.ContinuationState)
                                        {
                                            case ContinuationState.Continue:
                                                {
                                                    break;
                                                }
                                            case ContinuationState.Cancel:
                                                {
                                                    return new ContentResult(System.Net.HttpStatusCode.NoContent, default, default);
                                                }
                                            case ContinuationState.CancelWithError:
                                                {
                                                    return new ContentResult(next.StatusCode, next.ContentType, next.Content);
                                                }
                                            case ContinuationState.CancelWithRedirection:
                                                {
                                                    return new RedirectResult(next.Url);
                                                }
                                        }
                                    }
                                }

                                return result;
                            };
                            routes.Add(new Route(_route, verbAttribute?.Verb ?? "Get", new RequestHandler(actionDelegate)));
                        }
                    }
                }
            }
            Router.RegisterRoutes(routes.ToArray());
            WebServer server = new WebServer(prefixes);
            return server;
        }
    }
}