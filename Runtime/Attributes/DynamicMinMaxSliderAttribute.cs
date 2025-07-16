using System.Diagnostics;
using System;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Conditional("UNITY_EDITOR")]
    public class DynamicMinMaxSliderAttribute : Attribute
    {
        public string MinMemberValue { get; }
        public string MaxMemberValue { get; }

        public DynamicMinMaxSliderAttribute(string minMemberValue, string maxMemberValue)
        {
            MinMemberValue = minMemberValue;
            MaxMemberValue = maxMemberValue;
        }
    }
}