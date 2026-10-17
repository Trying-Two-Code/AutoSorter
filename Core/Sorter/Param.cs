using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Core.Sorter
{
    public delegate bool ApproximateMatch(FileInfo forFile, Param self);

    /// <summary>
    /// Contains one paramater for a file, and what the parameter should or should not be.
    /// </summary>
    public class Param : IDetectMatches
    {

        public Param(string fileProperty, string myApproximateMatchName = "")
        {
            FileProperty = fileProperty;
            MyApproximateMatchName = myApproximateMatchName;
            updateApproximateMatch();
        }

        public string MyApproximateMatchName { get; set; }
        public ApproximateMatch? MyApproximateMatch = null;
        public string FileProperty { get; set; }
        public string? PropertyContains { get; set; }
        public string? PropertyNotContains { get; set; }
        public List<string> MatchingFiles { get; set; } = new();


        private void updateApproximateMatch()
        {
            if (MyApproximateMatchName != "")
            {
                MyApproximateMatch =
                    ParamApproximates
                    .defualtApproximateMatchMethods
                    [MyApproximateMatchName];
            }
        }

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
            bool PerfectMatch = (HasProperContents && HasNoBadContents);

            if (!PerfectMatch)
            {
                updateApproximateMatch();
                bool? approximateMatch = MyApproximateMatch?.Invoke(fileInfo, this);
                if (approximateMatch == true) return true;
            }

            return PerfectMatch;
        }

        private class ParamValueNullError : Exception { }
    }

    public class ParamOptions(string name, Type T, string ApproximateMatchMethodName = "")
    {
        public Type ValueType { get; set; } = T;
        public string name { get; set; } = name;
        public string ApproximateMatchMethodName { get; set; } = ApproximateMatchMethodName;
        public ApproximateMatch? ApproximateMatchMethod { get; set; }

        public static readonly List<ParamOptions> AllParamaters = new()
            {
                new ParamOptions("CreationTime", typeof(DateTime)),
                new ParamOptions("LastAccessTime", typeof(DateTime)),
                new ParamOptions("LastAccessTime", typeof(DateTime), "DateApproximate"),
                new ParamOptions("LastAccessTime", typeof(DateTime), "WeekdayApproximate"),
                new ParamOptions("Extension", typeof(string)),
                new ParamOptions("Name", typeof(string)),
                new ParamOptions("Name", typeof(string), "StringApproximate"),
                new ParamOptions("Name", typeof(string), "StrongStringApproximate"),
                new ParamOptions("Length", typeof(long)),
                new ParamOptions("Length", typeof(long), "NumberApproximate"),
            };
    };

    static class ParamApproximates
    {
        //References:
        public static Dictionary<string, ApproximateMatch> defualtApproximateMatchMethods = new()
        {
            { "DateApproximate", (FileInfo forFile, Param self) =>
                {
                    return ParamApproximates.DateApproximate(
                        date1: self.FileProperty,
                        date2Time: forFile.LastAccessTime,
                        SecondDifference: 500)
                    > 0.5f;
                } },
            { "WeekdayApproximate", (FileInfo forFile, Param self) =>
                {
                    return ParamApproximates.WeekdayApproximate(
                        date1: self.FileProperty,
                        date2Time: forFile.LastAccessTime)
                    == 1f;
                } },
            { "StringApproximate", (FileInfo forFile, Param self) =>
                {
                    return ParamApproximates.StringApproximate(
                        forFile.Name,
                        self?.PropertyContains)
                    > 0.5f;
                } },
            { "StrongStringApproximate", (FileInfo forFile, Param self) =>
                {
                    return ParamApproximates.StringApproximate(
                        forFile.Name,
                        self?.PropertyContains)
                    > 0.9f;
                } },
            { "NumberApproximate", (FileInfo forFile, Param self) =>
                {
                    return ParamApproximates.NumberApproximate(
                        num1Float: forFile.Length,
                        num2: self?.PropertyContains,
                        NumberDifference: 100f)
                    > 0.8f;
                }
            }
        };


        //Implementations:
        public static float StringApproximate(string string1, string string2)
        {
            bool string1Greater = string1.Length > string2.Length;

            string shortString = string1Greater ? string2 : string1;
            string longString = string1Greater ? string1 : string2;

            int length = shortString.Length;
            Debug.Assert(length > 0);

            int SameCount = 0;

            for (int i = 0; i < length; i++)
            {
                char shortChar = shortString[i];
                char longChar = longString[i];

                if (shortChar == longChar)
                {
                    SameCount++;
                }
            }

            return (float)SameCount / (float)length;
        }

        public static TimeSpan? DateDifference(
            string? date1 = null, string? date2 = null,
            DateTime? date1Time = null, DateTime? date2Time = null)
        {
            if (date1Time == null)
            {
                DateTime result1;
                DateTime.TryParse(date1, out result1);
                date1Time = result1;
            }
            if (date2Time == null)
            {
                DateTime result2;
                DateTime.TryParse(date2, out result2);
                date2Time = result2;
            }

            TimeSpan? compare = date1Time - date2Time;
            return compare;
        }

        public static float DateApproximate(
            string? date1 = null, string? date2 = null, 
            DateTime? date1Time = null, DateTime? date2Time = null,
            int SecondDifference = 100)
        {
            int compare = Math.Abs((int)DateDifference(date1, date2, date1Time, date2Time)?.Seconds);
            float Aproximate = Math.Max(0, (SecondDifference - compare) / SecondDifference);
            return Aproximate;
        }

        public static float WeekdayApproximate(
            string? date1 = null, string? date2 = null,
            DateTime? date1Time = null, DateTime? date2Time = null
            )
        {
            TimeSpan difference = (TimeSpan)DateDifference(date1, date2, date1Time, date2Time);
            if(difference.TotalDays % 7 == 0)
            {
                return 1f;
            }
            return 0f;
        }

        public static float NumberApproximate(
            string? num1 = null, string? num2 = null,
            float? num1Float = null, float? num2Float = null,
            float NumberDifference = 100)
        {
            if (num1Float == null) 
            {
                num1Float = float.Parse(num1);
            }
            if (num2Float == null)
            {
                num2Float = float.Parse(num2);
            }

            float difference = Math.Abs((float)num1Float - (float)num2Float);
            return Math.Max(0, (NumberDifference - difference) / NumberDifference);
        }
    }
}