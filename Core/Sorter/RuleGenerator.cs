using Core.Sorter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Core.Sorter
{
    internal class RuleGenerator
    {
        public StrengthDetector MyStrengthDetector {get; set;}

        public RuleGenerator(StrengthDetector myStrengthDetector)
        {
            MyStrengthDetector = myStrengthDetector;
        }

        /// <summary>
        /// Creates a potential rule based on the params given.
        /// </summary>
        /// <param name="allData">All the old files that represent what the user wants</param>
        /// <param name="newData">The new file that is being tested for rules</param>
        /// <param name="requiredStrength">The required amount of files in all data that must match new data</param>
        /// <returns>Returns the created rule.</returns>
        public Rule? GenerateRule(
            List<FileInfo> allData,
            FileInfo newData,
            string oldPath,
            int requiredStrength = 0)
        {
            //There is no possibility for a rule that matches the strength required
            if (requiredStrength > allData.Count)
                return null;

            Rule? newRule = new();

            string? oldDirectoryName = Path.GetDirectoryName(oldPath);
            newRule.StartPath = oldDirectoryName == null ? "null" : oldDirectoryName;
            newRule.EndPath = newData?.DirectoryName == null ? "null" : newData.DirectoryName;

            foreach ((string name, Type type) in Param.AllParamaters)
            {
                int? StrengthOfCorralation = MyStrengthDetector.CorralationStrength(name, allData, newData);
                if (StrengthOfCorralation != null && StrengthOfCorralation >= requiredStrength)
                {
                    string? mainFileParamater = (string?)typeof(FileInfo)
                                                ?.GetProperty(name)
                                                ?.GetValue(newData)
                                                ?.ToString();

                    Param newParameter = new(name)
                    {
                        PropertyContains = mainFileParamater,
                    };

                    newRule.Strength += (int)StrengthOfCorralation;

                    newRule.Paramaters.Add(newParameter);
                }
            }

            return newRule;
        }

        /// <summary>
        /// Returns the power set of all possible param combinations. Keep in mind
        /// this method is a time sinker: O((2^AllParameters) * AllParameters)
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public List<List<Param>> AllParams(List<FileInfo> allData, FileInfo forFile, int limit)
        {
            List<List<Param>> AllParamList = new();

            List<Param> paramList = RuleParamConverter.ConvertDictToParamList(Param.AllParamaters);

            int possibleSubsetCount = (1 << Param.AllParamaters.Count);
            Dictionary<Param, int> ParamToStrength = MyStrengthDetector.CorralationStrengths(paramList, allData, forFile);

            for (int subsetI = 0; subsetI < Math.Min(possibleSubsetCount, limit); subsetI += 1)
            {
                List<Param> tempList = new();

                for (int i = 0; i < Param.AllParamaters.Count; i += 1)
                {
                    int mask = (1 << i);
                    bool subsetInMask = (subsetI & mask) != 0;

                    if (subsetInMask)
                    {
                        KeyValuePair<string, Type> keyVal = Param.AllParamaters.ElementAt(i);
                        string PropertyName = keyVal.Key;

                        Param newParameter = new(PropertyName)
                        {
                            FileProperty = PropertyName,
                            strength = MyStrengthDetector.MatchingStrength(ParamToStrength, PropertyName)
                        };

                        object? value = typeof(FileInfo)?.GetProperty(keyVal.Key)?.GetValue(forFile);
                        if (value?.GetType() == keyVal.Value)
                        {
                            newParameter.PropertyContains = value.ToString();
                        }

                        tempList.Add(newParameter);
                    }
                }

                AllParamList.Add(tempList);
            }

            return AllParamList;
        }

        /// <summary>
        /// Returns the power set of possible rule paramaters as rules.
        /// </summary>
        /// <param name="allData"></param>
        /// <param name="newData"></param>
        /// <param name="oldPath"></param>
        /// <param name="limit"></param>
        /// <returns>The list of rules that can be created for a given file</returns>
        public List<Rule> AllRules(List<FileInfo> allData, FileInfo newData, string oldPath, int limit = 1000)
        {
            List<List<Param>> allParameters = AllParams(allData, newData, limit);
            Debug.WriteLine("all params made, count is: " + allParameters.Count);
            List<Rule> allRules = RuleParamConverter.ConvertParamsToRules(MyStrengthDetector, allData, oldPath, newData.DirectoryName, allParameters);
            Debug.WriteLine("all rules made, count is: " + allRules.Count);

            return allRules;
        }

        /// <summary>
        /// Creates a list of many possible Rules that could be made based on the paramaters
        /// given.
        /// </summary>
        /// <returns>The created list of Rules.</returns>
        public List<Rule?>? GenerateRules(List<FileInfo> allData, FileInfo newData, string oldPath, int limit)
        {
            const int RequiredStrength = 10;
            const int MaximumRules = 1000;

            List<Rule>? newRules = new();

            List<Rule> allRules = AllRules(allData, newData, oldPath);
            //for every type of paramater
            List<Rule> allPositiveRules = FilterData.StrongRules(allRules, RequiredStrength, limit);
            newRules.AddRange(allPositiveRules);

            Rule? newRule = GenerateRule(
                allData: allData,
                newData: newData,
                oldPath: oldPath,
                requiredStrength: RequiredStrength);

            if (newRule != null)
                newRules.Add(newRule);

            Debug.WriteLine("Generated Rules Finished. Count is: " + newRules.Count);
            return newRules;
        }
    }
}

static class RuleParamConverter
{
    public static List<Param> ConvertDictToParamList(Dictionary<string, Type> dict)
    {
        List<Param> allParams = new();
        foreach (KeyValuePair<string, Type> kvp in dict)
        {
            Param temp = new(kvp.Key);
            allParams.Add(temp);
        }
        return allParams;
    }

    public static List<Rule> ConvertParamsToRules(
        StrengthDetector strengthDetector, 
        List<FileInfo> allData,
        string startPath,
        string endPath, 
        List<List<Param>> paramLists)
    {

        List<Rule> allRules = new();
        foreach (List<Param> paramList in paramLists)
        {
            Rule newRule = new()
            {
                Paramaters = paramList,
                StartPath = startPath,
                EndPath = endPath,
            };

            List<FileInfo> MatchingFiles = strengthDetector.CorralationFiles(newRule, allData);
            newRule.Strength = MatchingFiles.Count;
            newRule.MatchingFiles = MatchingFiles;

            allRules.Add(newRule);
        }
        return allRules;
    }
}
