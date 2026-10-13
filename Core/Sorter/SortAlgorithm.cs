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
using System.Data;
using System.Diagnostics;

namespace Core.Sorter;

internal class SortAlgorithm
{
    private PromptUser _myPromptUser { get; set; }
    private StrengthDetector _myStrengthDetector { get; set; }
    private RuleGenerator _myRuleGeneretor { get; set; }
    public SortAlgorithm(PromptUser _promptUser)
    {
        _myPromptUser = _promptUser;
        _myStrengthDetector = new StrengthDetector();
        _myRuleGeneretor = new RuleGenerator(_myStrengthDetector);
    }

    /// <summary>
    /// Generates a list of all possible Rules given the old data and new file data.
    /// </summary>
    /// <returns>The best possible Rule, only if it is worth prompting the user.</returns>
    public Rule? ManageRules(List<FileInfo>? allData, FileInfo newData, string oldPath)
    {
        const int limit = 1000;

        //sort old data for only those files that match the start folder and end destination paths
        List<FileInfo> FilteredFiles = FilterData.Filter(allData, newData);

        //generate a list of rules
        List<Rule?>? GeneratedRules = _myRuleGeneretor.GenerateRules(FilteredFiles, newData, oldPath, limit);
        if (GeneratedRules == null) return null;

        //remove any rules if they already exist
        GeneratedRules = FilterData.DetectDuplicateRules(GeneratedRules);
        if (GeneratedRules == null) return null;

        GeneratedRules.Reverse();

        //find the strongest rule
        Rule? StrongestRule = FilterData.StrongestRule(GeneratedRules);

        Debug.WriteLine(StrongestRule?.Strength);

        //return strongest rule if applicable
        return StrongestRule;
    }

    //Called when, for example, a data entry is added to userAction.json
    public void OnDataGained(object sender, OnFileMoveEventArgs e)
    {
        FileMoveData[] AllData = e.AllData;
        List<FileInfo> AllFileInfo = MultiConvertFileMoveData(AllData);

        const int limit = 1000;
        List<FileInfo> FileInfoOfFolder = RuleExecuter.GetFileInfoOfFolder(e.NewFile.DirectoryName, limit);
        AllFileInfo.AddRange(FileInfoOfFolder);

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
