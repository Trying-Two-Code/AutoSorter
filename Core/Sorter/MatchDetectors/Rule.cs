using Core.Sorter.Creator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;

namespace Core.Sorter.MatchDetectors
{
    public class Rule : IDetectMatches
    {
        [JsonConstructor]
        public Rule() { }

        public Rule(
            HashSet<FileInfo> allData,
            HashSet<FileInfo> userActions,
            List<Param> parameters,
            string startPath,
            string endPath)
        {
            //Debug.WriteIf(allData.Count <= 0, "all data no good\n");
            //Debug.WriteIf(userActions.Count <= 0, "useractions no good\n");
            Parameters = parameters;
            StartPath = startPath;
            EndPath = endPath;

            FixStartAndEndPath();

            List<string> MatchingUserActions = Matches(userActions);
            //Debug.WriteIf(MatchingUserActions.Count <= 0, "MatchingUserActions useractions no good\n");
            StartEstimation = MatchingUserActions.Count;

            List<string> matchingFileList = Matches(allData);

            //Debug.WriteLine($"matchingFiles:{matchingFileList.Count}\nMatchingUserActions:{MatchingUserActions.Count}");
            List<string> MinimumMatchingFileList = matchingFileList.Intersect(MatchingUserActions).ToList();

            MatchingFiles = MinimumMatchingFileList;

            StartCertain = MinimumMatchingFileList.Count;
        }

        public delegate void voidTask(Rule caller);

        private bool PathsFixed = false;
        private void FixStartAndEndPath()
        {
            if (PathsFixed) return;

            if (Path.EndsInDirectorySeparator(StartPath))
            {
                StartPath = Path.GetDirectoryName(StartPath);
            }

            if (Path.EndsInDirectorySeparator(EndPath))
            {
                EndPath = Path.GetDirectoryName(EndPath);
            }

            PathsFixed = true;
        }
        public List<Param> Parameters { get; set; }
        public string StartPath { get; set; }
        public string EndPath { get; set; }

        /// <summary>
        /// the amount of times the rule has been executed
        /// (includes Autosorter and human intervention)
        /// </summary>
        public int ExecutionCount { get; set; } = 0;

        /// <summary>
        /// the amount of files that are known to have moved
        /// from the start location to the end
        /// </summary>
        public int StartEstimation { get; set; } = 0;

        /// <summary>
        /// the amount of files that match the rules parameters
        /// and moved from the StartPath currently in the folder 
        /// at time of rule creation
        /// </summary>
        public int StartCertain { get; set; } = 0;

        public bool ShouldMove(FileInfo fileInfo)
        {
            FixStartAndEndPath();
            if (fileInfo.DirectoryName != (StartPath))
            {
                return false;
            }
            return IsMatch(fileInfo);
        }

        public List<string> MatchingFiles { get; set; } = new();
        public bool IsMatch(FileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                return false;
            }

            foreach (Param param in Parameters)
            {
                if (param.IsMatch(fileInfo)) { continue; }
                return false;
            }

            return true;

        }

        public int Strength
        {
            get
            {
                if (Parameters == null) return 0;
                return StartCertain * Parameters.Count;
            }
        }

        /// <summary>
        /// Detects if two parameters are checking the same 
        /// file property.
        /// </summary>
        public bool ContainsDuplicateParam
        {
            get
            {
                if(Parameters == null || Parameters?.Count == 0) return false;
                return Parameters.Distinct().Count() != Parameters.Count;
            }
        }

        /// <summary>
        /// Finds the file matches for the rule for a list of files
        /// </summary>
        /// <param name="rule">the rule to find matches for</param>
        /// <param name="corralationFiles">the list of files</param>
        /// <returns>The list of files that match the rule</returns>
        public List<string> Matches(HashSet<FileInfo> corralationFiles)
        {
            List<string> MatchingFiles = new();

            foreach(FileInfo file in corralationFiles)
            {
                if (IsMatch(file))
                {
                    MatchingFiles.Add(file.FullName);
                }
            }
            return MatchingFiles;
        }

        public bool CheapRuleEqualityCheck(Rule OtherRule)
        {
            if((OtherRule?.Parameters == null) != (Parameters == null))
            {
                return false;
            }

            if (ParamListCorralation(OtherRule.Parameters) > 0.9f)
            {
                return true;
            }

            if (StringListCorralation(MatchingFiles, OtherRule.MatchingFiles) > 0.9f)
            {
                return true;
            }

            return false;
        }

        public IEnumerable<Rule> CheapRuleEqualityCheck(IEnumerable<Rule> OtherRules)
        {
            return OtherRules.Where(otherRule => CheapRuleEqualityCheck(otherRule));
        }

        public float ParamListCorralation(List<Param> OtherParamList)
        {
            int shortListCount = Math.Min(OtherParamList.Count, Parameters.Count);
            if (shortListCount == 0)
                return 0;

            List<Param> longList = Parameters.Count > OtherParamList.Count ? Parameters : OtherParamList;
            List<Param> shortList = Parameters.Count > OtherParamList.Count ? OtherParamList : Parameters;

            float corralationAmmount = 0;

            for (int i = 0; i < shortList.Count; i++)
            {
                Param longParam = longList[i];
                Param shortParam = shortList[i];

                if (longParam.CheapEqual(shortParam))
                {
                    corralationAmmount++;
                }
            }

            return Math.Max(0, (float)corralationAmmount / (float)shortListCount);
        }
        delegate bool CorralationMethod<T>(T value, HashSet<T> inHash);

        public float StringListCorralation(List<string> list1, List<string> list2)
        {
            int shortListCount = Math.Min(list1.Count, list2.Count);

            if (shortListCount == 0)
                return 0;

            int CorralationCount = Corralation<string>(list1, list2, RegularContains);

            return Math.Max(0, (float)CorralationCount / (float)shortListCount);
        }
        
        public bool RegularContains<T>(T value, HashSet<T> inHash){
            return inHash.Contains(value);
        }

        private static int Corralation<T>(List<T> list1, List<T> list2, CorralationMethod<T> MyCorralation)
        {
            bool List1Bigger = (list1.Count > list2.Count);
            int SmallerListCount = List1Bigger ? list2.Count : list1.Count;

            if (SmallerListCount == 0)
                return 0;

            HashSet<T> SmallerList = List1Bigger ? new HashSet<T>(list2) : new HashSet<T>(list1);
            List<T> BiggerList = List1Bigger ? list1 : list2 ;

            int CorralationAmmount = 0;

            foreach (T value in BiggerList) {
                if (MyCorralation(value, SmallerList))
                {
                    CorralationAmmount++;
                }
                if (CorralationAmmount >= SmallerListCount)
                {
                    break;
                }
            }

            return CorralationAmmount;
        }
    }
}
