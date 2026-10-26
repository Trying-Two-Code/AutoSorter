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
using Core.Sorter.Executor;
using Core.Sorter.MatchDetectors;
using System.Data;
using System.Diagnostics;

namespace Core.Sorter.Creator;

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

    private const int RuleLimit = 10000;
    private int MinimumParams = 3;
    private int MinimumFileMatches = 2;

    public SortAlgorithm(
        string myfolder,
        PromptUser _promptUser, 
        GetSettingsTask myGetSettingsTask, 
        GetUserActionTask myGetUserActionTask,
        RuleExecuter myRuleExecutor)
    {
        _myFolder = myfolder;
        _myPromptUser = _promptUser;
        _myGetSettingsTask = myGetSettingsTask;
        _myGetUserActionTask = myGetUserActionTask;
        _myStrengthDetector = new StrengthDetector();
        _myRuleGenerator = new RuleGenerator(_myStrengthDetector);
        _myRuleExecuter = myRuleExecutor;
        _myFilterData = new FilterData();

        SetRequiredStrength();
    }

    public bool IsEnabled { get; set; } = false;

    public void Start()
    {
        IsEnabled = true;
    }

    public void Stop()
    {
        IsEnabled = false;
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

    public MatchDetectors.Rule? GenerateStrongRule(List<FileInfo> allData, List<FileMoveInfo> userActions, FileInfo newData, string oldPath)
    {
        //generate a list of rules
        List<MatchDetectors.Rule> GeneratedRules = _myRuleGenerator.AllRules(
            allData, userActions,
            newData, oldPath,
            RuleLimit,
            MinimumParams);

        //remove any weak rules
        GeneratedRules = _myFilterData.StrongRules(
            allRules: GeneratedRules,
            requiredStrength: RequiredStrength,
            requiredParams: MinimumParams,
            requiredMatches: MinimumFileMatches,
            limit: RuleLimit);

        //remove any rules if they already exist
        List<MatchDetectors.Rule> OldRules = RuleEditor.GetRules();
        GeneratedRules = _myFilterData.DetectDuplicateRules(GeneratedRules, OldRules);

        //find the strongest rule
        List<MatchDetectors.Rule>? StrongestRules = _myStrengthDetector.StrongestRules(GeneratedRules);

        MatchDetectors.Rule? newRule = null;

        //arbitrary, since all the strongest rules are equal in their effectiveness
        //TODO: make rules slightly stronger depending on if param has an approximate
        if(StrongestRules.Count > 0)
            newRule = StrongestRules[0];

        return newRule;
    }

    /// <summary>
    /// Generates an appropriate rule and saves said rule.
    /// </summary>
    public void ManageRules(List<FileInfo> allData, FileInfo newData, string oldPath)
    {
        allData = allData.Distinct().ToList();

        //sort old data for only those files that match the start folder and end destination paths
        List <FileInfo> FilteredFiles = _myFilterData.Filter(allData, newData);

        List<FileMoveInfo> userActions = GetUserActions();

        MatchDetectors.Rule? newRule = GenerateStrongRule(FilteredFiles, userActions, newData, oldPath);

        if (newRule != null)
        {
            if (_myPromptUser.Prompt(newRule))
            {
                _myRuleExecuter.OnRuleMade(newRule);
            }
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


    //Called when, for example, a data entry is added to userAction.json
    public void OnDataGained(object? sender, OnFileMoveEventArgs? e)
    {
        if(IsEnabled)
            HandleData(sender, e);
    }

    private void HandleData(object? sender, OnFileMoveEventArgs? e)
    {
        if(sender == null || e == null || e.NewFile == null || e.NewFile.DirectoryName == null)
        {
            Debug.WriteLine("smth is null bro");
            return;
        }

        bool isInCorrectFolder = e.NewFile.DirectoryName.Contains(_myFolder);
        bool isRedundantData = _myFilterData.RedundantData(e.NewFile);

        Debug.WriteLine($"isCorrectFolder:{isInCorrectFolder}|| isRedundant:{isRedundantData}||");

        if (isInCorrectFolder && !isRedundantData)
        {
            SetRequiredStrength();

            FileMoveData[]? AllData = e.AllData;
            if(AllData != null)
            {
                List<FileInfo> AllFileInfo = FileUtility.ConvertFileMoveData(AllData);

                List<FileInfo> FileInfoOfFolder = FileUtility.GetFileInfoOfFolder(e.NewFile.DirectoryName, RuleLimit);
                AllFileInfo.AddRange(FileInfoOfFolder);

                string? oldPath = Path.GetDirectoryName(e.OldPath);

                _myFilterData.AddOldData(e.NewFile.FullName, true);

                if (oldPath != null)
                    ManageRules(AllFileInfo, e.NewFile, oldPath);
            }
        } else if (isRedundantData)
        {
            _myFilterData.AddOldData(e.NewFile.FullName, false);
        }
    }
}

//Todo: make this less hacky
static class FileUtility
{
    /// <UTILITY>
    public static List<FileInfo> ConvertFileMoveData(FileMoveData[] fileMoveDataPoints)
    {
        List<FileInfo> allData = new();
        foreach (FileMoveData fileMoveData in fileMoveDataPoints)
        {
            FileInfo fileInfo = new(fileMoveData.NewPath);
            allData.Add(fileInfo);
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

