using System;

namespace Spider.WebAPI.Annotations
{
    public class VerbAttribute : Attribute
    {
        public string Verb { get; }
        public VerbAttribute(string verb)
        {
            Verb = verb;
        }
    }

}