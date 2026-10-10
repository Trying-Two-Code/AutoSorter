//OVERVIEW: contains an algorithm that detects if an automation rule can and should be made.

//example:
//File moves from C://Downloads/A -> C://Downloads/B. It had a name of "BOB.txt". It was created 01-01-2026.
//Potential rules are created with paramaters focused on name, extension, and creation date.
//It is found that everytime user moves from A -> B, it is a .txt.
//Rule with:
// - A param of ({FileProperty = "extension", PropertyContains = ".txt", PropertyNotContains = null})
// - StartPath == C://Downloads/A && EndPath == C://Downloads/B
//is returned
using Core.FileSystem;
using System.Diagnostics;
using System.Xml.Linq;
using static Core.FileSystem.AutoSorter;

namespace Core.Sorter;

internal class SortAlgorithm
{
    private PromptUser _myPromptUser { get; set; }
    public SortAlgorithm(PromptUser _promptUser)
    {
        _myPromptUser = _promptUser;
    }

    /// <summary>
    /// Creates a potential rule based on the params given.
    /// </summary>
    /// <param name="allData">All the old files that represent what the user wants</param>
    /// <param name="newData">The new file that is being tested for rules</param>
    /// <param name="requiredStrength">The required amount of files in all data that must match new data</param>
    /// <returns>Returns the created rule.</returns>
    static Rule? GenerateRule(
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
            int? StrengthOfCorralation = CorralationStrength(name, allData, newData);
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
    /// Tests a potential parameter to see if it is worth prompting the user.
    /// </summary>
    /// <returns>How many files fit the parameter.</returns>
    static private int? CorralationStrength(
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

    /// <summary>
    /// returns the power set of all possible param combinations. 
    /// </summary>
    /// <param name="limit"></param>
    /// <returns></returns>
    static List<List<Param>> AllParams(int limit, FileInfo forFile)
    {
        List<List<Param>> AllParamList = new();

        int possibleSubsetCount = (1 << Param.AllParamaters.Count);

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

                    Param newParameter = new(keyVal.Key)
                    {
                        FileProperty = keyVal.Key,
                    };

                    object? value = typeof(FileInfo)?.GetProperty(keyVal.Key)?.GetValue(forFile);
                    if(value?.GetType() == keyVal.Value)
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

    static List<Rule> convertParamsToRules(int strength, string startPath, string endPath, List<List<Param>> paramLists)
    {
        List<Rule> allRules = new();
        foreach (List<Param> paramList in paramLists)
        {
            Rule newRule = new()
            {
                Paramaters = paramList,
                StartPath = startPath,
                EndPath = endPath,
                Strength = strength
            };

            allRules.Add(newRule);
        }
        return allRules;
    }

    /// <summary>
    /// Returns the power set of possible rule paramaters as rules.
    /// </summary>
    /// <param name="allData"></param>
    /// <param name="newData"></param>
    /// <param name="oldPath"></param>
    /// <param name="limit"></param>
    /// <returns>The list of rules that can be created for a given file</returns>
    static List<Rule> AllRules(List<FileInfo> allData, FileInfo newData, string oldPath, int limit = 1000)
    {
        List<List<Param>> allParameters = AllParams(limit, newData);
        List<Rule> allRules = convertParamsToRules(-1, oldPath, newData.DirectoryName, allParameters);

        return allRules;
    }

    /// <summary>
    /// Creates a list of many possible Rules that could be made based on the paramaters
    /// given.
    /// </summary>
    /// <returns>The created list of Rules.</returns>
    static List<Rule?> GenerateRules(List<FileInfo> allData, FileInfo newData, string oldPath, int limit)
    {   
        const int RequiredStrength = 10;
        const int MaximumRules = 1000;

        List<Rule>? newRules = new();

        List<Rule> allRules = AllRules(allData, newData, oldPath);
        //for every type of paramater
        List<Rule> allPositiveRules = FilterData.StrongRules(allRules, RequiredStrength, limit);

        Rule? newRule = GenerateRule(
            allData: allData, 
            newData: newData,
            oldPath: oldPath,
            requiredStrength: RequiredStrength);

        if(newRule != null)
            newRules.Add(newRule);

        return newRules;
    }

    static object oldData = new();

    /// <summary>
    /// Generates a list of all possible Rules given the old data and new file data.
    /// </summary>
    /// <returns>The best possible Rule, only if it is worth prompting the user.</returns>
    static Rule? ManageRules(List<FileInfo>? allData, FileInfo newData, string oldPath)
    {
        const int limit = 1000;

        //sort old data for only those files that match the start folder and end destination paths
        List<FileInfo> FilteredFiles = FilterData.Filter(allData, newData);

        //generate a list of rules
        List<Rule>? GeneratedRules = GenerateRules(FilteredFiles, newData, oldPath, limit);

        //remove any rules if they already exist
        GeneratedRules = FilterData.DetectDuplicateRules(GeneratedRules);

        //find the strongest rule
        Rule StrongestRule = FilterData.StrongestRule(GeneratedRules);

        //return strongest rule if applicable
        return StrongestRule;
    }

    //Called when, for example, a data entry is added to userAction.json
    public void OnDataGained(object sender, OnFileMoveEventArgs e)
    {
        FileMoveData[] AllData = e.AllData;
        List<FileInfo> AllFileInfo = MultiConvertFileMoveData(AllData);

        Rule? newRule = ManageRules(AllFileInfo, e.NewFile, e.OldPath);

        if (newRule != null)
            if(_myPromptUser.Prompt())
                RuleExecuter.OnRuleMade(newRule);
    }

    /// <UTILITY>
    public FileInfo ConvertFileMoveData(FileMoveData fileMoveData)
    {
        FileInfo fileInfo = new(fileMoveData.NewPath);
        return fileInfo;
    }
    public List<FileInfo> MultiConvertFileMoveData(FileMoveData[] fileMoveDataPoints)
    {
        List<FileInfo> allData = new();
        foreach (FileMoveData fileMoveData in fileMoveDataPoints)
        {
            allData.Add(ConvertFileMoveData(fileMoveData));
        }
        return allData;
    }
    /// </UTILITY>
}
