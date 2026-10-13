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
        public static readonly Dictionary<string, Type> AllParamaters =
            new Dictionary<string, Type>()
            {
            { "CreationTime", typeof(DateTime) },
            { "LastAccessTime", typeof(DateTime) },
            { "Extension", typeof(string)},
            { "Name" , typeof(string) },
            { "IsReadOnly", typeof(bool) },
            { "Length" , typeof(long) },
            };

        public string FileProperty { get; set; } = FileProperty;
        public string? PropertyContains { get; set; }
        public string? PropertyNotContains { get; set; }
        public int? strength = null;
        public List<FileInfo> MatchingFiles { get; set; } = new();

        /// <summary>
        /// Detects if a file matches the paramater.
        /// </summary>
        /// <param name="fileInfo">The file detected.</param>
        public bool IsMatch(FileInfo fileInfo)
        {
            string? value = (string?)(typeof(FileInfo)?.GetProperty(FileProperty)?.GetValue(fileInfo)?.ToString());

            if (value == null) return true;

            bool HasProperContents = PropertyContains == null ? true : value.Contains(PropertyContains);
            bool HasNoBadContents = PropertyNotContains == null ? true : !value.Contains(PropertyNotContains);

            return HasProperContents && HasNoBadContents;
        }
    }
}
