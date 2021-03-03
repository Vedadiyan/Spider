using System;

namespace Spider.WebAPI.Annotations
{
    public class AutoWireAttribute : Attribute
    {
        public string Name { get; set; }
    }
}