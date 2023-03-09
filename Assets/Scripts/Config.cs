using System;

namespace SWAT
{
    [AttributeUsage(AttributeTargets.Field)]
    public class Config : Attribute
    {
        public string Id    { get; }
        public string Param { get; }
        
        public Config(string id, string param)
        {
            Id    = id;
            Param = param;
        }
    }
}