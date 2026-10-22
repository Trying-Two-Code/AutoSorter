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
        /// Returns the power set of all possible param combinations for a file. Keep in mind
        /// this method is a time sinker: O((2^AllParameters) * AllParameters)
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public List<List<Param>> AllParams(
            List<FileInfo> allData,
            FileInfo forFile, 
            int limit)
        {
            List<List<Param>> AllParamList = new();

            int possibleSubsetCount = (int)Math.Pow((double)2, (double)ParamOptions.AllParamaters.Count);

            for (int subsetI = 0; subsetI < Math.Min(possibleSubsetCount, limit); subsetI += 1)
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
                            FileProperty = PropertyName,
                            MyApproximateMatchName = defaultParam.ApproximateMatchMethodName
                        };

                        object? value = 
                            typeof(FileInfo)
                            ?.GetProperty(defaultParam.name)
                            ?.GetValue(forFile);

                        //Important:
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
            List<FileMoveInfo> userActions, 
            FileInfo newData,
            string oldPath, 
            int limit = 1000)
        {
            List<List<Param>> allParameters = AllParams(allData, newData, limit);
            List<Rule> allRules = Rule.ConvertParamsToRules(
                allData, userActions, oldPath, newData.DirectoryName, allParameters);

            return allRules;
        }
    }
}