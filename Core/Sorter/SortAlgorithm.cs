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
using static Core.FileSystem.AutoSorter;

namespace Core.Sorter;

/// <summary>
/// Contains one paramater for a file, and what the parameter should or should not be.
/// </summary>
public struct Param(string FileProperty)
{
    public static readonly string[] AllParamaters = [
        "CreationTime",
        "Extension",
        "IsReadOnly",
        "LastAccessTime",
        "Length",
        "Name"
    ];

    public string FileProperty { get; set; } = FileProperty;
    public string? PropertyContains { get; set; }
    public string? PropertyNotContains { get; set; }
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
    /// <returns>Returns the created rule.</returns>
    static Rule? GenerateRule(List<Param> parameters, Param mainParamater)
    {
        return null;
    }

    static T? GenerateRule<T>(List<Param> paramaters, T mainParamater)
    {
        return default;
    }


    /// <summary>
    /// Creates a list of every possible Rule that could be made based on the paramaters
    /// given.
    /// </summary>
    /// <returns>The created list of Rules.</returns>
    static List<Rule?> GenerateRules(List<FileInfo> allData, FileInfo newData)
    {
        return [null];
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
        Debug.WriteLine("recieved data:");
        FileMoveData[] AllData = e.AllData;
        List<FileInfo> AllFileInfo = MultiConvertFileMoveData(AllData);
        ManageRules(AllFileInfo, e.NewFile);
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
