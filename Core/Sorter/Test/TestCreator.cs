using Core.FileSystem;
using Core.Sorter.Creator;
using Core.Sorter.MatchDetectors;
using Core.Sorter.Test.Helpers;
using Helper.DataGathering;
using System.Diagnostics;
using System.Text.Json;

namespace Core.Sorter.Test
{
    internal class TestCreator
    {
        internal class TestRuleGenerator : ITestAutoSorter
        {
            public List<AutoSorterTestMethod> TestMethods { set; get; } = new();
            public static readonly string MyDirectory = Path.Combine(AppContext.BaseDirectory, "test/testcreate/");
            public static readonly string MyOldDirectory = Path.Combine(AppContext.BaseDirectory, "test/testoldcreate/");
            public static StrengthDetector MyStrengthDetector = new StrengthDetector();
            public static RuleGenerator MyRuleGenerator;
            public static object _lock = new();
            public static readonly MassFileCreator MyFileCreator = new(MyDirectory);

            public TestRuleGenerator()
            {
                lock (_lock)
                {
                    Directory.CreateDirectory(MyDirectory);
                    Directory.CreateDirectory(MyOldDirectory);

                    MyRuleGenerator = new RuleGenerator(MyStrengthDetector);

                    TestMethods.Add(TestAllRules);
                    TestMethods.Add(TestCreateParamList);
                    TestMethods.Add(TestGenerateStrongRule);
                }
            }

            public bool TestsAllSucceed()
            {
                lock (_lock)
                {
                    return !TestMethods.Any(method =>
                    {
                        return method() == false;
                    });
                }
            }

            AutoSorterTestMethod TestAllRules = () =>
            {
                List<string> TestFiles = new() { "smth.txt", "smth2.txt", "smth3.txt" };
                List<FileInfo> TestAllData = new();

                MyFileCreator.CreateAndForget(TestFiles);
                TestAllData.AddRange(MyFileCreator.MyFileInfos);

                List<FileMoveInfo> TestFileMoveInfos = new List<FileMoveInfo>() { 
                    new FileMoveInfo()
                    {
                        OldPath = MyOldDirectory,
                        NewPath = TestFiles[0]
                    },
                    new FileMoveInfo()
                    {
                        OldPath = MyOldDirectory,
                        NewPath = TestFiles[2]
                    },
                };

                FileInfo TestNewData = TestAllData[1];

                int TestLimit = 100;

                int TestMinParams = 0;

                List<Rule> TestNoRules = MyRuleGenerator.AllRules(
                    TestAllData,
                    TestFileMoveInfos,
                    TestNewData,
                    MyOldDirectory,
                    0,
                    TestMinParams);

                List<Rule> TestAllRules = MyRuleGenerator.AllRules(
                    TestAllData,
                    TestFileMoveInfos,
                    TestNewData,
                    MyOldDirectory,
                    TestLimit,
                    TestMinParams);

                MyFileCreator.Delete();

                return TestNoRules.Count == 0 && TestAllRules.Count > 0;
            };
            AutoSorterTestMethod TestCreateParamList = () =>
            {
                List<Param> TestNull = MyRuleGenerator.CreateParamList(0, null);
                List<Param> TestNonExistant = MyRuleGenerator.CreateParamList(0, new FileInfo("C://ThisFileDoesNotExist.N0EXISTING"));

                FileStream TestFile = File.Create(Path.Combine(MyDirectory, "test.txt"));
                List<Param> TestOne = MyRuleGenerator.CreateParamList(1, new FileInfo(TestFile.Name));

                Debug.WriteLine(TestOne.Count);

                TestFile.Dispose();
                File.Delete(TestFile.Name);

                return TestOne.Count == 1 && TestNull.Count == 0 && TestNonExistant.Count == 0;
            };
            AutoSorterTestMethod TestGenerateStrongRule = () =>
            {
                List<string> NewFiles = new List<string>()
                {
                    "pass1.txt","pass2.txt","pass3.txt","pass4.txt","pass5.txt",
                    "pass6.txt","pass7.txt","pass8.txt","pass9.txt","pass10.txt",
                    "fail1.fail","fail2.fail","fail3.fail","fail4.fail","fail5.fail"
                };

                MyFileCreator.CreateAndForget(NewFiles);

                List<FileInfo> FileInfos = MyFileCreator.MyFileInfos;

                List<FileInfo> allData = new();
                List<FileMoveInfo> fileMoveInfo = new();
                FileInfo newData = FileInfos[0];
                string newDataOldPath = Path.Combine(MyOldDirectory, newData.Name);

                Rule? NoRule = MyRuleGenerator.GenerateStrongRule(
                    allData,
                    fileMoveInfo,
                    newData,
                    newDataOldPath,
                    2,2,2,1);

                foreach (FileInfo file in FileInfos)
                {
                    //Debug.WriteLine($"filepath: {file.FullName}");
                    fileMoveInfo.Add(new()
                    {
                        OldPath = Path.Combine(MyOldDirectory, file.Name),
                        NewPath = Path.Combine(file.FullName)
                    });
                    allData.Add(file);
                }

                Rule? GoodRule = MyRuleGenerator.GenerateStrongRule(
                    allData,
                    fileMoveInfo,
                    newData,
                    MyOldDirectory,
                    2, 2, 2, 500);

                MyFileCreator.Delete();

                return NoRule == null && GoodRule != null;
            };
        }
        internal class TestStrengthDetector : ITestAutoSorter
        {
            public List<AutoSorterTestMethod> TestMethods { set; get; } = new();
            public static StrengthDetector MyStrengthDetector { get; } = new();
            public TestStrengthDetector()
            {
                TestMethods.Add(TestStrongRules);
            }

            public bool TestsAllSucceed()
            {
                return !TestMethods.Any(method =>
                {
                    return method() == false;
                });
            }

            AutoSorterTestMethod TestStrongRules = () =>
            {
                List<Rule> TestAllRules = new();

                Rule TestRule1 = new();
                TestRule1.ExecutionCount = 10;
                TestRule1.Paramaters = new()
                {
                    new Param("Example"),
                    new Param("Example2"),
                    new Param("Example3")
                };
                TestRule1.StartCertain = 10;

                Rule WeakTestRule2 = new();
                WeakTestRule2.ExecutionCount = 2;
                WeakTestRule2.Paramaters = new();
                WeakTestRule2.StartCertain = 2;

                TestAllRules.Add(TestRule1);
                TestAllRules.Add(WeakTestRule2);

                int TestRequiredStrength = 10;
                int TestRequiredParams = 3;
                int TestRequiredMatches = 3;
                int TestLimit = 100;

                List<Rule> StrongRules = MyStrengthDetector.StrongRules(
                    TestAllRules, TestRequiredStrength,
                    TestRequiredParams, TestRequiredMatches,
                    TestLimit);

                Debug.WriteLine(StrongRules.Count);

                return StrongRules.Contains(TestRule1) && !StrongRules.Contains(WeakTestRule2);
            };
        }
        internal class TestFilter : ITestAutoSorter
        {
            public static readonly string MyDirectory = Path.Combine(AppContext.BaseDirectory, "test\\creator\\filter");

            public List<AutoSorterTestMethod> TestMethods { set; get; } = new();
            public static FilterData MyFilterData { set; get; } = new();

            public TestFilter()
            {
                TestMethods.Add(TestRedundantData);
            }

            public bool TestsAllSucceed()
            {
                return !TestMethods.Any(method =>
                {
                    return method() == false;
                });
            }

            
            AutoSorterTestMethod TestRedundantData = () =>
            {
                FileStream file = File.Create(Path.Combine(MyDirectory, "example.txt"));

                MyFilterData.promptAmmount = 2;

                bool take1 = MyFilterData.RedundantData(new FileInfo(file.Name));
                bool take2 = MyFilterData.RedundantData(new FileInfo(file.Name));
                bool take3 = MyFilterData.RedundantData(new FileInfo(file.Name));
                bool take4 = MyFilterData.RedundantData(new FileInfo(file.Name));

                Debug.WriteLine($"1:{take1}2:{take2}3:{take3}4:{take4}");

                return !take1 && take2 && !take3 && take4;
            };
        }
        internal class TestSortAlgorithm : ITestAutoSorter
        {
            private static AutoSorter MyAutoSorter;
            private static SortAlgorithm MySortAlgorithm;

            public static readonly string MyDirectory = Path.Combine(AppContext.BaseDirectory, "test\\creator\\root\\somefolder");
            public static readonly string MyRootDirectory = Path.Combine(AppContext.BaseDirectory, "test\\creator\\root\\");
            public static readonly string UserSettingFilePath = Path.Combine(AppContext.BaseDirectory, "Ui\\UserSettings\\userSettings.json");
            public static readonly string UserMoveFilePath = Path.Combine(AppContext.BaseDirectory, "data\\userAction.json");
            private static Mutex useractionmut = new Mutex(false, "Global\\UserActionMutex");


            public TestSortAlgorithm()
            {
                Directory.CreateDirectory(MyRootDirectory);
                Directory.CreateDirectory(MyDirectory);

                TestMethods.Add(TestGetUserActions);

                MyAutoSorter = new(MyDirectory, MyRootDirectory);
                MySortAlgorithm = new(MyDirectory);
            }

            public bool TestsAllSucceed()
            {
                return !TestMethods.Any(method =>
                {
                    return method() == false;
                });
            }

            public List<AutoSorterTestMethod> TestMethods { set; get; } = new();

            AutoSorterTestMethod TestGetUserActions = () =>
            {
                List<object> testResult = MyAutoSorter.SendAllDataInstance.GetUserActions(MyAutoSorter, new());

                UserActionGatherFile? myResult = null;

                if (useractionmut.WaitOne(1000))
                {
                    myResult = JsonSerializer.Deserialize<UserActionGatherFile>(File.ReadAllText(UserMoveFilePath));
                }
                useractionmut.ReleaseMutex();

                List<UserActionGatherLine>? myResultList = myResult?.Lines;



                if (testResult.Count == 0 || myResultList.Count == 0)
                    return testResult.Count == myResultList.Count;
                else
                    return ((UserActionGatherLine)testResult[0]).To == myResultList[0].To;
            };
            
        }
    }
}
