// using System;
// using System.Text;
// using Spider.ArcheType;

// namespace Spider.Core.Routing {
//         public readonly struct Route
//     {
//         public string Template { get; }
//         public Parameter[] Parameters { get; }
//         public Parameter[] Query { get; }
//         public int Size => Parameters.Length;
//         public IRequest RequestHandler { get; }
//         public string Method { get; }
//         public string Hash { get; }
//         public string SortValue { get; }
//         public Route(string routeTemplate, string method, IRequest requestHandler)
//         {
//             Template = routeTemplate;
//             Method = method.ToLower();
//             RequestHandler = requestHandler;
//             string[] route = routeTemplate.Split('?');
//             SortValue = route[0];
//             string[] routeValues = route[0].Split('/');
//             Parameters = new Parameter[routeValues.Length];
//             string[] queryValues = null;
//             StringBuilder hashBuilder = new StringBuilder();
//             string[] routeElement = null;
//             hashBuilder.Append(method).Append(":");
//             for (int index = 0; index < routeValues.Length; index++)
//             {
//                 string reference = routeValues[index].TrimStart().TrimEnd();
//                 if (reference.StartsWith("{") && reference.EndsWith("}"))
//                 {
//                     hashBuilder.Append("Parameter/");
//                     string[] value = reference.TrimStart('{').TrimEnd('}').Split(':');
//                     string[] type = value[1].Split('.');
//                     Parameters[index] = new Parameter(index, value[0].ToLower(), null, (TypeCode)Enum.Parse(typeof(TypeCode), type[0]), false, type.Length == 2 && type[1].ToLower().Equals("array"));
//                 }
//                 else
//                 {
//                     hashBuilder.Append(reference).Append("/");
//                     routeElement = reference.ToLower().Split(':');
//                     if (routeElement.Length == 1)
//                     {
//                         Parameters[index] = new Parameter(index, routeElement[0], null, TypeCode.String, true, false);
//                     }
//                     else if(routeElement.Length == 2) {
//                         Parameters[index] = new Parameter(index, routeElement[0], routeElement[1], TypeCode.String, true, false);
//                     }
//                     else {
//                         throw new Exception("Unsupported Route Template");
//                     }
//                 }
//             }
//             Hash = BitConverter.ToString(System.Security.Cryptography.MD5.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(hashBuilder.ToString()))).Replace("-", "");
//             if (route.Length == 2)
//             {
//                 queryValues = route[1].Split('&');
//                 Query = new Parameter[queryValues.Length];
//                 for (int index = 0; index < queryValues.Length; index++)
//                 {
//                     string[] value = queryValues[index].Split(':');
//                     string[] type = value[1].Split('.');
//                     Query[index] = new Parameter(index, value[0].ToLower(), null, (TypeCode)Enum.Parse(typeof(TypeCode), type[0]), false, type.Length == 2 && type[1].ToLower().Equals("array"));

//                 }
//             }
//             else
//             {
//                 Query = null;
//             }
//         }
//     }

// }