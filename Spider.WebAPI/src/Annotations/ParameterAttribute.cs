using System;

namespace Spider.WebAPI.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]  
    public class ParameterAttribute : Attribute
    {
        public string Name { get; }
        public TypeCode TypeCode { get; }
        public bool IsArray { get; }
        public ParameterAttribute(string name, TypeCode typeCode, bool isArray)
        {
            Name = name;
            TypeCode = typeCode;
            IsArray = isArray;
        }
    }
}