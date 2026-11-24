using Core.Sorter.MatchDetectors;
using Core.Sorter.Test.Helpers;
using System.Diagnostics;

namespace Core.Sorter.Creator
{
    /// <summary>
    /// A class that creates rules.
    /// </summary>
    internal class RuleGenerator
    {
        public StrengthDetector MyStrengthDetector {get; set;}

        public RuleGenerator(StrengthDetector myStrengthDetector)
        {
            MyStrengthDetector = myStrengthDetector;
        }

        /// <summary>
        /// Generates the power set of all possible param combinations for a file. Keep in mind
        /// this method is a time sinker: ((2^AllParameters) * AllParameters)
        /// </summary>
        /// <param name="limit"></param>
        /// <returns>
        /// The power set of all possible param combinations for a file.
        /// Minus all the param combinations that have less params than
        /// minParams.
        /// </returns>
        public List<List<Param>> AllParams(
            FileInfo forFile, 
            int limit,
            int minParams = 2)
        {
            List<List<Param>> AllParamList = new();

            int possibleSubsetCount = (int)Math.Pow((double)2, (double)ParamOptions.AllParamaters.Count);

            int SubsetCount = Math.Min(possibleSubsetCount, limit);

            for (int SubsetIndex = 0; SubsetIndex < SubsetCount; SubsetIndex++)
            {
                int ExpectedCount = GetSubsetCount(SubsetIndex);
                if(ExpectedCount >= minParams)
                {
                    List<Param> SubsetParamList = CreateParamList(SubsetIndex, forFile);

                    Debug.Assert(SubsetParamList.Count == ExpectedCount);

                    AllParamList.Add(SubsetParamList);
                }
            }

            return AllParamList;
        }

        /// <summary>
        /// returns the length of the list of params that a subset int would contain
        /// </summary>
        public int GetSubsetCount(
            int subsetI)
        {
            return int.PopCount(subsetI);
        }

        /// <summary>
        /// Creates a list of paramaters. 
        /// It uses an int called subsetI
        /// </summary>
        /// <param name="ParamaterMask">An integer that effeciently stores the bits
        /// that decide which paramaters belong in the param list</param>
        /// <returns>A list of params that matches the paramater mask</returns>
        public List<Param> CreateParamList(
            int ParamaterMask,
            FileInfo forFile,
            bool canContainNullParams = false
            )
        {
            List<Param> ResultList = new();

            for (int i = 0; i < ParamOptions.AllParamaters.Count; i += 1)
            {
                int bitMask = (1 << i);
                bool BitMaskInParamaterMask = (ParamaterMask & bitMask) != 0;

                if (BitMaskInParamaterMask)
                {
                    ParamOptions defaultParam = ParamOptions.AllParamaters[i];
                    string PropertyName = defaultParam.name;

                    Param newParameter = new(PropertyName)
                    {
                        MyApproximateMatchName = defaultParam.ApproximateMatchMethodName
                    };

                    object? value = null;

                    if (forFile.Exists)
                    {
                        value =
                            typeof(FileInfo)
                            ?.GetProperty(defaultParam.name)
                            ?.GetValue(forFile)!;
                    }

                    Debug.Assert(value?.GetType() == defaultParam.ValueType, 
                        $"Value of file metadata {value} dost not match expected value type {defaultParam.ValueType}.");
                    newParameter.PropertyContains = value.ToString();

                    ResultList.Add(newParameter);
                }
            }

            return ResultList;
        }

        /// <summary>
        /// Generates the power set of possible rule paramaters as rules.
        /// </summary>
        /// <returns>The list of rules that can be created for a given file (limited in Count by limit)</returns>
        public List<Rule> AllRules(
            HashSet<FileInfo> allData,
            List<FileMoveInfo> userActions,
            FileInfo newData,
            string oldPath,
            int limit,
            int minParams)
        {
            List<List<Param>> allParameters = AllParams(newData, limit, minParams);
            List<Rule> allRules = FileUtility.ConvertParamsToRules(
                allData, userActions, oldPath, newData.DirectoryName, allParameters);

            return allRules;
        }

        /// <summary>
        /// Calls all the functions necessary to generate a strong rule
        /// that matches a dataset
        /// </summary>
        /// <param name="allData"></param>
        /// <param name="userActions"></param>
        /// <param name="newData"></param>
        /// <param name="oldPath"></param>
        /// <param name="MinimumParams"></param>
        /// <param name="MinimumFileMatches"></param>
        /// <param name="RequiredStrength"></param>
        /// <param name="RuleLimit"></param>
        /// <returns></returns>
        public Rule? GenerateStrongRule(
            HashSet<FileInfo> allData, 
            List<FileMoveInfo> userActions, 
            FileInfo newData, 
            string oldPath,
            int MinimumParams,
            int MinimumFileMatches,
            int RequiredStrength,
            int RuleLimit
            )
        {
            allData.RemoveWhere(file => !file.Exists);

            userActions = userActions.Where(useraction =>
            {
                return File.Exists(useraction.NewPath);
            }).ToList();

            FilterData myFilterData = new();
            StrengthDetector myStrengthDetector = new();
            //generate a list of rules
            List<Rule> GeneratedRules = AllRules(
                allData, userActions,
                newData, oldPath,
                RuleLimit,
                MinimumParams);

            //remove any weak rules
            List<Rule> StrongGeneratedRules = myStrengthDetector.StrongRules(
                allRules: GeneratedRules,
                requiredStrength: RequiredStrength,
                requiredParams: MinimumParams,
                requiredMatches: MinimumFileMatches,
                limit: RuleLimit);

            //remove any rules if they already exist
            List<Rule> OldRules = RuleEditor.GetRules();
            StrongGeneratedRules = myFilterData.DetectDuplicateRules(StrongGeneratedRules, OldRules);

            //find the strongest rule
            List<Rule>? Strongest = myStrengthDetector.StrongestRules(StrongGeneratedRules);

             Rule? newRule = null;

            //arbitrary, since all the strongest rules are equal in their effectiveness
            //TODO: make rules slightly stronger depending on if param has an approximate
            if (Strongest.Count > 0)
                newRule = Strongest[0];

            return newRule;
        }
    }
}