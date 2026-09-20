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

/// <summary>
/// Contains one paramater for a file, and what the parameter should or should not be.
/// </summary>
public class Param(string FileProperty)
{
    public static readonly Dictionary<string, Type> AllParamaters = 
        new Dictionary<string, Type>()
        {
            { "CreationTime", typeof(DateTime) },
            { "LastAccessTime", typeof(DateTime) },
            { "Extension", typeof(string)},
            { "Name" , typeof(string) },
            { "IsReadOnly", typeof(bool) },
            { "Length" , typeof(long) },
        };

    public string FileProperty { get; set; } = FileProperty;
    public string? PropertyContains { get; set; }
    public string? PropertyNotContains { get; set; }

    /// <summary>
    /// Detects if a file matches the paramater.
    /// </summary>
    /// <param name="fileInfo">The file detected.</param>
    public bool? Matches(FileInfo fileInfo)
    {
        string? value = (string?)typeof(FileInfo)?.GetProperty(FileProperty)?.GetValue(fileInfo);

        if(value == null) return null;
         
        bool HasProperContents =PropertyContains    == null ? true :  value.Contains(PropertyContains);
        bool HasNoBadContents = PropertyNotContains == null ? true : !value.Contains(PropertyNotContains);

        return HasProperContents && HasNoBadContents;
    }
}

/// <summary>
/// Contains many paramaters, and a Method for detecting if those paramaters
/// all match the paramater of a given file.
/// </summary>

public class OnRuleMadeEventArgs : EventArgs
{
    required public Rule rule { get; set; }
}

internal class SortAlgorithm
{
    /// <summary>
    /// Tests a potential rule to see if it is worth prompting the user.
    /// </summary>
    /// <returns>How many files fit the rule.</returns>
    static int TestRule(Rule rule)
    {
        return 0;
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
        int requiredStrength = 10)
    {
        //There is no possibility for a rule that matches the strength required
        if (requiredStrength > allData.Count)
            return null;

        Rule? newRule = new();

        foreach ((string name, Type type) in Param.AllParamaters)
        {
            int? StrengthOfCorralation = CorralationStrength(name, allData, newData);
            if (StrengthOfCorralation != null && StrengthOfCorralation >= requiredStrength)
            {
                string? mainFileParamater = (string?)typeof(FileInfo)
                                            ?.GetProperty(name)
                                            ?.GetValue(newData);

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
    /// Creates a list of many possible Rules that could be made based on the paramaters
    /// given.
    /// </summary>
    /// <returns>The created list of Rules.</returns>
    static List<Rule?> GenerateRules(List<FileInfo> allData, FileInfo newData)
    {   
        const int RequiredStrength = 10;
        List<Rule>? newRules = new();

        //TODO: generate more rules

        Rule? newRule = GenerateRule(
            allData: allData, 
            newData: newData, 
            requiredStrength: 10);

        if(newRule != null)
            newRules.Add(newRule);

        return newRules;
    }

    static object oldData = new();

    /// <summary>
    /// Generates a list of all possible Rules given the old data and new file data.
    /// </summary>
    /// <returns>The best possible Rule, only if it is worth prompting the user.</returns>
    static Rule? ManageRules(List<FileInfo>? allData, FileInfo newData)
    {
        //sort old data for only those files that match the start folder and end destination paths
        List<FileInfo> FilteredFiles = FilterData.Filter(allData, newData);

        //generate a list of rules
        List<Rule>? GeneratedRules = GenerateRules(FilteredFiles, newData);

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

        Rule? newRule = ManageRules(AllFileInfo, e.NewFile);

        if(newRule != null)
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
