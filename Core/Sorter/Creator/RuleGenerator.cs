using Core.Sorter.MatchDetectors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Tar;
using System.Numerics;
using System.Text;

namespace Core.Sorter.Creator
{
    internal class RuleGenerator
    {
        public StrengthDetector MyStrengthDetector {get; set;}

        public RuleGenerator(StrengthDetector myStrengthDetector)
        {
            MyStrengthDetector = myStrengthDetector;
        }

        /// <summary>
        /// Returns the power set of all possible param combinations for a file. Keep in mind
        /// this method is a time sinker: O((2^AllParameters) * AllParameters)
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public List<List<Param>> AllParams(
            List<FileInfo> allData,
            FileInfo forFile, 
            int limit,
            int minParams)
        {
            List<List<Param>> AllParamList = new();

            int possibleSubsetCount = (int)Math.Pow((double)2, (double)ParamOptions.AllParamaters.Count);

            for (int subsetI = 0; subsetI < Math.Min(possibleSubsetCount, limit); subsetI += 1)
            {
                int Count = GetSubsetCount(subsetI);
                if (Count >= minParams)
                {
                    List<Param> paramList = CreateParamList(subsetI, forFile);
                    //Debug.Assert(paramList.Count == Count);
                    if(paramList.Count != Count)
                    {
                        continue;
                    }
                    AllParamList.Add(paramList);
                }
            }
            Debug.WriteLine($"AllParamList.Count={AllParamList.Count}");

            return AllParamList;
        }

        /// <summary>
        /// returns the amount of params that a subsetI would generate
        /// </summary>
        public int GetSubsetCount(
            int subsetI)
        {
            int count = int.PopCount(subsetI);
            return count;
        }

        public List<Param> CreateParamList(
            int subsetI,
            FileInfo forFile,
            bool canContainNullParams = false
            )
        {
            List<Param> tempList = new();

            for (int i = 0; i < ParamOptions.AllParamaters.Count; i += 1)
            {
                int mask = (1 << i);
                bool subsetInMask = (subsetI & mask) != 0;

                if (subsetInMask)
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

                    //Important:
                    if (value?.GetType() == defaultParam.ValueType)
                    {
                        newParameter.PropertyContains = value.ToString();
                    }
                    else
                    {
                        if(!canContainNullParams)
                            break;
                    }

                    tempList.Add(newParameter);
                }
            }

            return tempList;
        }

        /// <summary>
        /// Returns the power set of possible rule paramaters as rules.
        /// </summary>
        /// <param name="allData"></param>
        /// <param name="newData"></param>
        /// <param name="oldPath"></param>
        /// <param name="limit"></param>
        /// <returns>The list of rules that can be created for a given file</returns>
        public List<Rule> AllRules(
            List<FileInfo> allData,
            List<FileMoveInfo> userActions,
            FileInfo newData,
            string oldPath,
            int limit,
            int minParams)
        {
            List<List<Param>> allParameters = AllParams(allData, newData, limit, minParams);
            List<Rule> allRules = FileUtility.ConvertParamsToRules(
                allData, userActions, oldPath, newData.DirectoryName, allParameters);

            return allRules;
        }

        public Rule? GenerateStrongRule(
            List<FileInfo> allData, 
            List<FileMoveInfo> userActions, 
            FileInfo newData, 
            string oldPath,
            int MinimumParams,
            int MinimumFileMatches,
            int RequiredStrength,
            int RuleLimit
            )
        {
            FilterData myFilterData = new();
            StrengthDetector myStrengthDetector = new();

            //generate a list of rules
            List<MatchDetectors.Rule> GeneratedRules = AllRules(
                allData, userActions,
                newData, oldPath,
                RuleLimit,
                MinimumParams);

            //remove any weak rules
            List<MatchDetectors.Rule> StrongGeneratedRules = myStrengthDetector.StrongRules(
                allRules: GeneratedRules,
                requiredStrength: RequiredStrength,
                requiredParams: MinimumParams,
                requiredMatches: MinimumFileMatches,
                limit: RuleLimit);

            //remove any rules if they already exist
            List<MatchDetectors.Rule> OldRules = RuleEditor.GetRules();
            StrongGeneratedRules = myFilterData.DetectDuplicateRules(StrongGeneratedRules, OldRules);

            //find the strongest rule
            List<MatchDetectors.Rule>? Strongest = myStrengthDetector.StrongestRules(StrongGeneratedRules);

            MatchDetectors.Rule? newRule = null;

            //arbitrary, since all the strongest rules are equal in their effectiveness
            //TODO: make rules slightly stronger depending on if param has an approximate
            if (Strongest.Count > 0)
                newRule = Strongest[0];

            return newRule;
        }
    }
}