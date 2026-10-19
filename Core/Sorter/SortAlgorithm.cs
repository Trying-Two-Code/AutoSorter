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
    private RuleExecuter _myRuleExecuter { get; set; }
    private FilterData _myFilterData { get; set; }

    //how many files should sort algorithm be looking through maximum?
    const int limit = 1000; 

    public SortAlgorithm(PromptUser _promptUser)
    {
        _myPromptUser = _promptUser;
        _myStrengthDetector = new StrengthDetector();
        _myRuleGeneretor = new RuleGenerator(_myStrengthDetector);
        _myRuleExecuter = new RuleExecuter();
        _myFilterData = new FilterData();
    }

    /// <summary>
    /// Generates a list of all possible Rules given the old data and new file data.
    /// </summary>
    public void ManageRules(List<FileInfo> allData, FileInfo newData, string oldPath)
    {
        allData = allData.Distinct().ToList();
        const int RequiredStrength = 10;

        //sort old data for only those files that match the start folder and end destination paths
        List <FileInfo> FilteredFiles = _myFilterData.Filter(allData, newData);

        //generate a list of rules
        List<Rule> GeneratedRules = _myRuleGeneretor.AllRules(FilteredFiles, newData, oldPath, limit);

        //remove any weak rules
        GeneratedRules = _myFilterData.StrongRules(
            allRules: GeneratedRules, 
            requiredStrength: RequiredStrength,
            requiredParams: 2,
            requiredMatches: 10,
            limit: limit);

        //remove any rules if they already exist
        List<Rule> OldRules = RuleEditor.GetRules();
        GeneratedRules = _myFilterData.DetectDuplicateRules(GeneratedRules, OldRules);

        //find the strongest rule
        List<Rule>? StrongestRules = _myStrengthDetector.StrongestRules(GeneratedRules);

        Rule? newRule = null;

        if (StrongestRules.Count == 1)
        {
            newRule = StrongestRules[0];
        }
        else if(StrongestRules.Count > 1)
        {
            //newRule = DecideOnRuleSomehow;
            newRule = StrongestRules[0];
            //throw new NotImplementedException();
        }

        if (newRule != null)
            if (_myPromptUser.Prompt())
                _myRuleExecuter.OnRuleMade(newRule);
    }

    //Called when, for example, a data entry is added to userAction.json
    public void OnDataGained(object sender, OnFileMoveEventArgs e)
    {
        FileMoveData[] AllData = e.AllData;
        List<FileInfo> AllFileInfo = FileUtility.MultiConvertFileMoveData(AllData);

        List<FileInfo> FileInfoOfFolder = FileUtility.GetFileInfoOfFolder(e.NewFile.DirectoryName, limit);
        AllFileInfo.AddRange(FileInfoOfFolder);

        ManageRules(AllFileInfo, e.NewFile, e.OldPath);
    }

}

//Todo: make this less hacky
static class FileUtility
{
    /// <UTILITY>
    public static FileInfo ConvertFileMoveData(FileMoveData fileMoveData)
    {
        FileInfo fileInfo = new(fileMoveData.NewPath);
        return fileInfo;
    }
    public static List<FileInfo> MultiConvertFileMoveData(FileMoveData[] fileMoveDataPoints)
    {
        List<FileInfo> allData = new();
        foreach (FileMoveData fileMoveData in fileMoveDataPoints)
        {
            allData.Add(ConvertFileMoveData(fileMoveData));
        }
        return allData;
    }
    public static List<FileInfo> GetFileInfoOfFolder(string folderName, int limit = 1000)
    {
        List<string> filesList =
            Directory.EnumerateFiles(folderName)
            .Take(limit)
            .ToList();

        List<FileInfo> result = new();

        foreach (string file in filesList)
        {
            FileInfo newFileInfo = new(file);
            result.Add(newFileInfo);
        }

        return result;
    }
    /// </UTILITY>

}

