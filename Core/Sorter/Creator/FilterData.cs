using Core.Sorter.MatchDetectors;
using System.Diagnostics;

namespace Core.Sorter.Creator
{
    /// <summary>
    /// Class to get rid of useless rule and fileinfo data.
    /// </summary>
    public class FilterData
    {
        private JSONDataMinipulator<FilePromptDatapoint> _DatapointFile;
        public FilterData()
        {
            _DatapointFile = new JSONDataMinipulator<FilePromptDatapoint>("PreviousDatapoints.json", "DatapointFileMutex");
            PreviousDatapoints = _DatapointFile.Get();
        }

        /// <summary>
        /// Filters for only files that moved to and from the same path
        /// as datapoint.
        /// </summary>
        /// <param name="data">all old data</param>
        /// <param name="datapoint">just the newest datapoint</param>
        /// <returns>The dataset minus extra datapoints.</returns>
        public HashSet<FileInfo> Filter(HashSet<FileInfo> data, FileInfo datapoint)
        {
            HashSet<FileInfo> filtered = new HashSet<FileInfo>();

            foreach (FileInfo filedatapoint in data)
            {
                if (!filedatapoint.Exists)
                { continue; }
                if (filedatapoint.DirectoryName != datapoint.DirectoryName)
                { continue; }

                filtered.Add(filedatapoint);
            }
            return filtered;
        }

        /// <summary>
        /// Makes sure that the rules are distinct and that none of them exist
        /// in the rule file already.
        /// </summary>
        /// <param name="Rules"></param>
        /// <returns>Rule list without duplicates.</returns>
        public List<Rule> DetectDuplicateRules(
            List<Rule> NewRules, 
            List<Rule> OldRules)
        {
            IEnumerable<Rule> DistinctRules = NewRules.Distinct();

            DistinctRules = DistinctRules.DistinctBy(rule => new
                {
                    rule.Parameters,
                    rule.Strength
                }
            ).ToList();
            Debug.Assert(NewRules.Count <= DistinctRules.Count());

            return DistinctRules.ToList();
        }

        /// <summary>
        /// How many prompts to skip
        /// before prompting the user about a folder.
        /// </summary>
        public int PromptAmmount { get; set; } = 10;
        private List<FilePromptDatapoint> PreviousDatapoints { get; set; }

        /// <returns>Wether a fileinfo has been prompted about too much or not</returns>
        public bool RedundantPrompt(FileInfo newDatapoint)
        {
            List<string> PreviousPrompts = FilePromptDatapoint.GetPromptedFiles(PreviousDatapoints);

            if (PreviousPrompts.Count == 0)
                return PromptAmmount >= 0;

            List<FilePromptDatapoint> Matches = PreviousDatapoints.Where(match =>
            {
                return Path.GetDirectoryName(match.File) == newDatapoint.DirectoryName;
            }).ToList();

            for (int i = PreviousDatapoints.Count - 1; i >= 0; i--)
            {
                FilePromptDatapoint data = PreviousDatapoints[i];
                if (data.WasPrompted)
                {
                    //aka: how many prompts didn't happen before now
                    int NegativeIndex = PreviousDatapoints.Count - i;
                    return NegativeIndex >= PromptAmmount;
                }
            }
            
            Debug.Assert(Matches.Count == 0);
            return PromptAmmount >= 0;
        }

        /// <summary>
        /// stores when a file was considered for prompting
        /// </summary>
        public class FilePromptDatapoint
        {
            public FilePromptDatapoint(string file, bool wasPrompted)
            {
                File = file;
                WasPrompted = wasPrompted;
            }

            public string File { get; set; }
            public bool WasPrompted { get; set; }

            /// <returns>A list of files that were previously prompted for</returns>
            public static List<string> GetPromptedFiles(List<FilePromptDatapoint> FromFilePrompts)
            {
                return FromFilePrompts.Where(filePrompt =>
                {
                    if (filePrompt.WasPrompted)
                    {
                        return true;
                    }
                    return false;
                }).Select(filePrompt =>
                {
                    return filePrompt.File;
                }).ToList();
            }
        }

        /// <summary>
        /// Adds a datapoint to the prompt history
        /// </summary>
        public void AddPromptHistory(string newDatapoint, bool prompted)
        {
            FilePromptDatapoint NewData = new FilePromptDatapoint(newDatapoint, prompted);
            PreviousDatapoints.Add(NewData);
            _DatapointFile.Set(NewData);
        }
    }
}
