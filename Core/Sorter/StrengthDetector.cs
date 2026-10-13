using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Sorter
{
    internal class StrengthDetector
    {
        /// <summary>
        /// Tests a potential parameter to see if it is worth prompting the user.
        /// </summary>
        /// <returns>How many files fit the parameter.</returns>
        public int? CorralationStrength(
                string ParamaterTypeName,
                List<FileInfo> allData,
                FileInfo newData)
        {
            int CorralationStrength = 0;
            object? newFileParamater = typeof(FileInfo)
                                        ?.GetProperty(ParamaterTypeName)
                                        ?.GetValue(newData);

            if (newFileParamater == null) return null;

            foreach (FileInfo paramater in allData)
            {
                object? secondaryParamater = typeof(Param)
                                            ?.GetProperty(ParamaterTypeName)
                                            ?.GetValue(paramater);

                if (secondaryParamater == null) { continue; }

                if (secondaryParamater == newFileParamater)
                {
                    CorralationStrength++;
                }
                else
                {
                    CorralationStrength -= 10;
                }
            }

            return CorralationStrength;
        }

        public int? CorralationStrength(
                IDetectMatches detector,
                List<FileInfo> allData
            )
        {
            const int positiveMultiplier = 1;
            const int negativeMultiplier = 1;
            int positiveCount = 0;
            int negativeCount = 0;

            foreach (FileInfo datapoint in allData)
            {
                if (detector.IsMatch(datapoint))
                {
                    positiveCount++;
                    detector.MatchingFiles.Add(datapoint);
                }
                else
                {
                    negativeCount++;
                }
            }

            int positiveResult = positiveCount * positiveMultiplier;
            int negativeResult = negativeCount * negativeMultiplier;
            int result = positiveResult - negativeResult;

            return result;
        }

        public List<FileInfo> CorralationFiles(Rule rule, List<FileInfo> corralationFiles)
        {
            foreach (Param param in rule.Paramaters)
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

        public Dictionary<Param, int> CorralationStrengths(
            List<Param> Params,
            List<FileInfo> allData,
            FileInfo forFile)
        {
            Dictionary<Param, int> allStrengths = new();

            foreach (Param param in Params)
            {
                int? strength = CorralationStrength(param, allData);
                int safeStrength = (strength == null) ? 0 : (int)strength;
                allStrengths.Add(param, safeStrength);
            }

            return allStrengths;
        }
        public int MatchingStrength(Dictionary<Param, int> ParamToStrength, string paramName)
        {
            foreach (KeyValuePair<Param, int> KV in ParamToStrength)
            {
                if (KV.Key.FileProperty == paramName)
                {
                    return KV.Value;
                }
            }
            return 0;
        }
    }
}
