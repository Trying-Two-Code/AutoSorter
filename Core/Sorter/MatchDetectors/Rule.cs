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
            List<FileInfo> allData,
            List<FileMoveInfo> userActions,
            List<Param> parameters,
            string startPath, 
            string endPath)
        {
            Paramaters = parameters;
            StartPath = startPath;
            EndPath = endPath;

            List<string> PotentialMatchingUserActions = userActions
                .Where(file => 
                    (Path.GetDirectoryName(file.OldPath) == Path.GetDirectoryName(StartPath)) &&
                    (Path.GetDirectoryName(file.NewPath) == Path.GetDirectoryName(EndPath)))
                .Select(file =>
                    { return file.NewPath; })
                .Distinct()
                .ToList();

            List<string> MatchingUserActions = Matches(PotentialMatchingUserActions);
            StartEstimation = MatchingUserActions.Count;

            List<string> allDataList = allData
                .Select(data =>
                {
                    return data.FullName;
                }).Distinct().ToList();

            List<string> matchingFileList = Matches(allDataList);

            List<string> MinimumMatchingFileList = matchingFileList.Intersect(MatchingUserActions).ToList();

            MatchingFiles = MinimumMatchingFileList;

            StartCertain = MinimumMatchingFileList.Count;
        }

        public List<Param> Paramaters { get; set; }
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
            if (File.Exists(Path.Combine(EndPath, fileInfo.Name)))
            {
                return false;
            }

            return IsMatch(fileInfo);
        }

        public List<string> MatchingFiles { get; set; } = new();
        public bool IsMatch(FileInfo fileInfo)
        {
            if (fileInfo.DirectoryName != Path.GetDirectoryName(StartPath)) { return false; }

            if (Paramaters.Any((parameter) =>
            {
                return parameter.IsMatch(fileInfo) == false;
            }))
            {
                //A parameter didn't match the fileInfo.
                //Check if it approximatly matches the fileInfo.
                foreach (Param param in Paramaters)
                {
                    if (param.IsMatch(fileInfo)) { continue; }

                    bool? isApproximateMatch = null;

                    if (param?.MyApproximateMatch != null)
                        isApproximateMatch = param?.MyApproximateMatch(fileInfo, param);
                    if (isApproximateMatch == true) { continue; }

                    return false;
                }
                return true;
            }
            else
            {
                return true;
            }

        }

        public int Strength
        {
            get => StartCertain * Paramaters.Count;
        }

        /// <summary>
        /// Detects if two parameters are checking the same 
        /// file property.
        /// </summary>
        public bool ContainsDuplicateParam
        {
            get
            {
                List<string> properties = Paramaters.Select(param => { return param.FileProperty; }).ToList();
                int notCertainUnique = properties.Count;
                properties = properties.Distinct().ToList();
                return properties.Count != notCertainUnique;
            }
        }

        /// <summary>
        /// Finds the file matches for the rule for a list of files
        /// </summary>
        /// <param name="rule">the rule to find matches for</param>
        /// <param name="corralationFiles">the list of files</param>
        /// <returns>The list of files that match the rule</returns>
        public List<string> Matches(List<string> corralationFiles)
        {
            List<string> UniqueCorralationFiles = /*new List<string>();

            for (int i = corralationFiles.Count; i < 0; i--)
            {
                string cFile = corralationFiles[i];
                if (UniqueCorralationFiles.Contains(cFile)) { }
                else
                {
                    UniqueCorralationFiles.Add(cFile);
                }
            }*/ corralationFiles.Distinct().ToList();

            foreach (Param param in Paramaters)
            {
                for (int i = UniqueCorralationFiles.Count - 1; i >= 0; i--)
                {
                    string filePath = UniqueCorralationFiles[i];
                    FileInfo fileInfo = new(filePath);
                    if (!param.IsMatch(fileInfo))
                    {
                        UniqueCorralationFiles.Remove(filePath);
                    }
                }
            }
            return UniqueCorralationFiles;
        }

        public bool CheapRuleEqualityCheck(Rule OtherRule)
        {
            if (ParamListCorralation(OtherRule.Paramaters) > 0.9f)
            {
                return true;
            }

            if (StringListCorralation(MatchingFiles, OtherRule.MatchingFiles) > 0.9f)
            {
                return true;
            }

            return false;
        }

        public float ParamListCorralation(List<Param> OtherParamList)
        {
            List<Param> longList = Paramaters.Count > OtherParamList.Count ? Paramaters : OtherParamList;
            List<Param> shortList = Paramaters.Count > OtherParamList.Count ? OtherParamList : Paramaters;

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

            return Math.Max(0, corralationAmmount / shortList.Count);
        }

        public float StringListCorralation(List<string> list1, List<string> list2)
        {
            List<string> longList = list1.Count > list2.Count ? list1 : list2;
            List<string> shortList = list1.Count > list2.Count ? list2 : list1;

            float corralationAmmount = 0;

            for (int i = 0; i < shortList.Count; i++)
            {
                string longString = longList[i];

                if (shortList.Contains(longString))
                {
                    corralationAmmount++;
                }
            }

            return Math.Max(0, corralationAmmount / shortList.Count);
        }
    }
}
