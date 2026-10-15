using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Sorter
{
    /// <summary>
    /// Contains one paramater for a file, and what the parameter should or should not be.
    /// </summary>
    public class Param(string FileProperty) : IDetectMatches
    {
        public delegate bool ApproximateMatch();
        public ApproximateMatch? MyApproximateMatch = null;
        public class ParamOptions(string name, Type T)
        {
            public Type ValueType { get; set; } = T;
            public string name { get; set; } = name;
            public ApproximateMatch? ApproximateMatchMethod { get; set; }
        };
        public static readonly List<ParamOptions> AllParamaters = new()
        {
            new ParamOptions("CreationTime", typeof(DateTime)),
            new ParamOptions("LastAccessTime", typeof(DateTime)),
            new ParamOptions("Extension", typeof(string)),
            new ParamOptions("Name", typeof(string)),
            new ParamOptions("Name", typeof(string))
            {
                ApproximateMatchMethod = () =>
                {
                    return false;
                }
            },
            new ParamOptions("IsReadOnly", typeof(bool)),
            new ParamOptions("Length", typeof(long)),
        };

        public string FileProperty { get; set; } = FileProperty;
        public string? PropertyContains { get; set; }
        public string? PropertyNotContains { get; set; }
        public List<string> MatchingFiles { get; set; } = new();

        /// <summary>
        /// Detects if a file matches the paramater.
        /// </summary>
        /// <param name="fileInfo">The file detected.</param>
        public bool IsMatch(FileInfo fileInfo)
        {
            string? value = 
                (string?)(typeof(FileInfo)
                ?.GetProperty(FileProperty)
                ?.GetValue(fileInfo)
                ?.ToString());

            if(value == null)
                throw new ParamValueNullError();

            bool HasProperContents = PropertyContains == null ? true : value.Contains(PropertyContains);
            bool HasNoBadContents = PropertyNotContains == null ? true : !value.Contains(PropertyNotContains);

            return HasProperContents && HasNoBadContents;
        }

        private class ParamValueNullError : Exception { }
    }
}
