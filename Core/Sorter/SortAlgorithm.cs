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
    private string _myFolder { get; set;  }
    private GetUserActionTask _myGetUserActionTask {  get; set; }
    public GetSettingsTask _myGetSettingsTask { get; set; }
    private PromptUser _myPromptUser { get; set; }
    private StrengthDetector _myStrengthDetector { get; set; }
    private RuleGenerator _myRuleGenerator { get; set; }
    private RuleExecuter _myRuleExecuter { get; set; }
    private FilterData _myFilterData { get; set; }

    //how many files should sort algorithm be looking through maximum?
    const int limit = 1000; 

    public SortAlgorithm(
        string myfolder,
        PromptUser _promptUser, 
        GetSettingsTask myGetSettingsTask, 
        GetUserActionTask myGetUserActionTask)
    {
        _myFolder = myfolder;
        _myPromptUser = _promptUser;
        _myGetSettingsTask = myGetSettingsTask;
        _myGetUserActionTask = myGetUserActionTask;
        _myStrengthDetector = new StrengthDetector();
        _myRuleGenerator = new RuleGenerator(_myStrengthDetector);
        _myRuleExecuter = new RuleExecuter();
        _myFilterData = new FilterData();

        SetRequiredStrength();
    }

    public int RequiredStrength = 10;

    public void SetRequiredStrength()
    {
        object? data = _myGetSettingsTask?.Invoke(this, new());

        if (data != null)
        {
            int? dataStrength = data.GetType()?.GetProperty("promptUserAmm")?.GetValue(data) as int?;
            if(dataStrength != null)
            {
                RequiredStrength = (int)dataStrength;
            }
        }
    }

    /// <summary>
    /// Generates a list of all possible Rules given the old data and new file data.
    /// </summary>
    public void ManageRules(List<FileInfo> allData, FileInfo newData, string oldPath)
    {
        allData = allData.Distinct().ToList();

        //sort old data for only those files that match the start folder and end destination paths
        List <FileInfo> FilteredFiles = _myFilterData.Filter(allData, newData);

        List<FileMoveInfo> userActions = GetUserActions();
        //generate a list of rules
        List<Rule> GeneratedRules = _myRuleGenerator.AllRules(FilteredFiles, userActions, newData, oldPath, limit);
        //remove any weak rules
        GeneratedRules = _myFilterData.StrongRules(
            allRules: GeneratedRules, 
            requiredStrength: RequiredStrength,
            requiredParams: 2,
            requiredMatches: 2,
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

        Debug.WriteLine($"made rules, only |{newRule}| got through.");

        if (newRule != null)
        {
            if (_myPromptUser.Prompt(newRule))
            {
                _myRuleExecuter.OnRuleMade(newRule);
            }
            previusDatapoints.Add(newData);
        }
    }

    public List<FileMoveInfo> GetUserActions()
    {
        List<object> userActions = _myGetUserActionTask(this, new());
        List<FileMoveInfo> userActionResult = new();

        foreach (object action in userActions)
        {
            string? oldFileLocation = (string?)action?.GetType()?.GetProperty("From")?.GetValue(action);
            string? newFileLocation = (string?)action?.GetType()?.GetProperty("To")?.GetValue(action);

            oldFileLocation ??= "";
            newFileLocation ??= "";

            FileMoveInfo newInfo = new()
            {
                OldFileLocation = oldFileLocation,
                CurrentFile = newFileLocation
            };
            userActionResult.Add(newInfo);
        }

        return userActionResult;
    }

    private List<FileInfo> previusDatapoints { get; set; } = new();
    private bool RedundantData(FileInfo newDatapoint)
    {
        //TODO: make it send data to json file to allow persistancy
        //TODO?: move to FilterData
        if (previusDatapoints.Contains(newDatapoint))
        {
            return true;
        }

        return false;
    }

    //Called when, for example, a data entry is added to userAction.json
    public void OnDataGained(object sender, OnFileMoveEventArgs e)
    {
        bool isInCorrectFolder = e.NewFile.DirectoryName.Contains(_myFolder);
        bool isNotRedundantData = !RedundantData(e.NewFile);

        if (isInCorrectFolder && isNotRedundantData)
        {
            SetRequiredStrength();

            FileMoveData[] AllData = e.AllData;
            List<FileInfo> AllFileInfo = FileUtility.MultiConvertFileMoveData(AllData);

            List<FileInfo> FileInfoOfFolder = FileUtility.GetFileInfoOfFolder(e.NewFile.DirectoryName, limit);
            AllFileInfo.AddRange(FileInfoOfFolder);

            string oldPath = Path.GetDirectoryName(e.OldPath);

            ManageRules(AllFileInfo, e.NewFile, oldPath);
        }
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

