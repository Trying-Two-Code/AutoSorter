using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text;

namespace Core.Sorter
{
    public class Rule : IDetectMatches
    {
        public Rule(
            List<FileInfo> allData, 
            List<Param> parameters, 
            string startPath, 
            string endPath)
        {
            Paramaters = parameters;
            StartPath = startPath;
            EndPath = endPath;

            List<FileInfo> matchingFileList = Matches(allData);
            StartEstimation = matchingFileList.Count;

            List<string> MatchingFilesStrings = matchingFileList.Select(file => file.FullName).ToList();
            MatchingFiles = MatchingFilesStrings;
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
        /// the amount of files that match the rules paramaters
        /// in the folder at time of rule creation
        /// </summary>
        public int StartEstimation { get; set; } = 0;

        public bool ShouldMove(FileInfo fileInfo)
        {
            if (fileInfo.DirectoryName != StartPath)
            {
                return false;
            };

            if (File.Exists(Path.Combine(EndPath, fileInfo.Name)))
            {
                return false;
            }

            return IsMatch(fileInfo);
        }

        public List<string> MatchingFiles { get; set; } = new();
        public bool IsMatch(FileInfo fileInfo)
        {

            if (Paramaters.Any((parameter) =>
            {
                return parameter.IsMatch(fileInfo) == false;
            }))
            {
                //A parameter didn't match the fileInfo.
                foreach (Param param in Paramaters)
                {
                    if(param.IsMatch(fileInfo)) { continue; }
                    if(param?.MyApproximateMatch(fileInfo, param) == true) { continue; }
                    return false;
                }
                return true;
            }
            else
            {
                //All parameters matched.
                return true;
            }

        }

        public int strength()
        {
            return StartEstimation * Paramaters.Count;
        }

        public static List<Rule> ConvertParamsToRules(
            List<FileInfo> allData,
            string startPath,
            string endPath,
            List<List<Param>> paramLists)
        {
            List<Rule> allRules = new();
            foreach (List<Param> paramList in paramLists)
            {
                Rule newRule = new(
                    allData: allData, 
                    parameters: paramList,
                    startPath: startPath,
                    endPath: endPath
                    );

                allRules.Add(newRule);
            }
            return allRules;
        }

        /// <summary>
        /// Finds the file matches for the rule for a list of files
        /// </summary>
        /// <param name="rule">the rule to find matches for</param>
        /// <param name="corralationFiles">the list of files</param>
        /// <returns>The list of files that match the rule</returns>
        public List<FileInfo> Matches(List<FileInfo> corralationFiles)
        {
            corralationFiles = corralationFiles.Distinct().ToList();
            foreach (Param param in Paramaters)
            {
                for (int i = corralationFiles.Count - 1; i >= 0; i--)
                {
                    FileInfo fileInfo = corralationFiles[i];
                    if (!param.IsMatch(fileInfo))
                    {
                        corralationFiles.Remove(fileInfo);
                    }
                }
            }
            return corralationFiles;
        }
    }
}
