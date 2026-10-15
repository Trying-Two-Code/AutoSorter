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
        /// Returns the power set of all possible param combinations. Keep in mind
        /// this method is a time sinker: O((2^AllParameters) * AllParameters)
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public List<List<Param>> AllParams(List<FileInfo> allData, FileInfo forFile, int limit)
        {
            List<List<Param>> AllParamList = new();

            int possibleSubsetCount = (2 ^ Param.AllParamaters.Count);

            for (int subsetI = 0; subsetI < Math.Min(possibleSubsetCount, limit); subsetI += 1)
            {
                List<Param> tempList = new();

                for (int i = 0; i < Param.AllParamaters.Count; i += 1)
                {
                    int mask = (1 << i);
                    bool subsetInMask = (subsetI & mask) != 0;

                    if (subsetInMask)
                    {
                        Param.ParamOptions defaultParam = Param.AllParamaters[i];
                        string PropertyName = defaultParam.name;

                        Param newParameter = new(PropertyName)
                        {
                            FileProperty = PropertyName
                        };

                        object? value = typeof(FileInfo)?.GetProperty(defaultParam.name)?.GetValue(forFile);
                        if (value?.GetType() == defaultParam.ValueType)
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
        public List<Rule> AllRules(
            List<FileInfo> allData, 
            FileInfo newData, 
            string oldPath, 
            int limit = 1000)
        {
            List<List<Param>> allParameters = AllParams(allData, newData, limit);
            List<Rule> allRules = RuleParamConverter.ConvertParamsToRules(
                MyStrengthDetector, allData, oldPath, newData.DirectoryName, allParameters);

            return allRules;
        }

        /// <summary>
        /// Creates a list of many possible Rules that could be made based on the paramaters
        /// given.
        /// </summary>
        /// <returns>The created list of Rules.</returns>
        public List<Rule> GenerateRules(List<FileInfo> allData, FileInfo newData, string oldPath, int limit)
        {
            const int RequiredStrength = 10;
            const int MaximumRules = 1000;

            List<Rule>? newRules = new();

            List<Rule> allRules = AllRules(allData, newData, oldPath);
            //for every type of paramater
            List<Rule> allPositiveRules = FilterData.StrongRules(allRules, RequiredStrength, limit);
            newRules.AddRange(allPositiveRules);

            Debug.WriteLine("Generated Rules Finished. Count is: " + newRules.Count);
            return newRules;
        }
    }
}

static class RuleParamConverter
{

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
            Rule newRule = new(allData)
            {
                Paramaters = paramList,
                StartPath = startPath,
                EndPath = endPath,
            };

            allRules.Add(newRule);
        }
        return allRules;
    }
}