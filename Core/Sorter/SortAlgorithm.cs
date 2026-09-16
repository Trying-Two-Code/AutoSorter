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
public struct Param()
{
    string FileProperty { get; set; } = "name";
    string? PropertyContains { get; set; }
    string? PropertyNotContains { get; set; }
}

/// <summary>
/// Contains many paramaters, and a Method for detecting if those paramaters
/// all match the paramater of a given file.
/// </summary>
public class Rule()
{
    List<Param> Paramaters { get; set; } = [];
    public string StartPath { get; set; }
    public string EndPath { get; set; }
    /// <summary>
    /// the ammount of times the rule has been executed
    /// </summary>
    public int Strength { get; set; } = 0; 

    public bool ShouldMove(FileInfo fileInfo, string currentPath)
    {
        if (currentPath != StartPath) return false;

        return false;
    }
}

public class OnRuleMadeEventArgs : EventArgs
{
    required public Rule rule { get; set; }
}


/// <summary>
/// Class to get rid of useless data.
/// </summary>
class FilterData()
{
    /// <summary>
    /// Filters for only files that moved to and from the same path
    /// as datapoint.
    /// </summary>
    /// <param name="data">all old data</param>
    /// <param name="datapoint">just the newest datapoint</param>
    /// <returns>The dataset minus extra datapoints.</returns>
    public static object Filter(object data, object datapoint)
    {
        return new object();
    }
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
    static Rule? GenerateRule(List<Param> parameters)
    {
        return null;
    }

    /// <summary>
    /// Creates a list of every possible Rule that could be made based on the paramaters
    /// given.
    /// </summary>
    /// <returns>The created list of Rules.</returns>
    static List<Rule?> GenerateRules()
    {
        return [null];
    }

    static object oldData = new();

    /// <summary>
    /// Generates a list of all possible Rules given the old data and new file data.
    /// </summary>
    /// <returns>The best possible Rule, only if it is worth prompting the user.</returns>
    static Rule? ManageRules(object oldData, OnFileMoveEventArgs newData)
    {
        //sort old data for only those files that match the start folder and end destination paths
        object FilterFiles = FilterData.Filter(oldData, newData);

        //generate a list of rules

        //remove any rules if they already exist

        //return best rule if applicable

        return null;
        oldData = newData;
    }

    //Called when, for example, a data entry is added to userAction.json
    public void OnDataGained(object sender, OnFileMoveEventArgs e)
    {
        Debug.WriteLine("recieved data:");
        int i = 0;
        foreach (var datapoint in e.AllData)
        {
            i++;
            Debug.WriteLine(i);
            Debug.WriteLine("old path of datapoint:");
            Debug.WriteLine(datapoint.OldPath);
            Debug.WriteLine("new path of datapoint:");
            Debug.WriteLine(datapoint.NewPath);
        }
        Debug.WriteLine(e);
        //ManageRules(e);
    }
}

