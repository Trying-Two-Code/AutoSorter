using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text;

namespace Core.Sorter
{
    public class Rule : IDetectMatches
    {
        private StrengthDetector _myStrengthDetector;

        public Rule(List<FileInfo> allData)
        {
            _myStrengthDetector = new StrengthDetector();

            List<FileInfo> matchingFileList = _myStrengthDetector.RuleMatches(this, allData);
            StartEstimation = MatchingFiles.Count;

            List<string> MatchingFilesStrings = matchingFileList.Select(file => file.FullName).ToList();
            MatchingFiles = MatchingFilesStrings;
        }

        public List<Param> Paramaters { get; set; } = new();
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

            Debug.WriteLine("Rulepath: " + Path.Combine(EndPath, fileInfo.Name));
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
                    if(param?.MyApproximateMatch() == true) { continue; }
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
    }
}
