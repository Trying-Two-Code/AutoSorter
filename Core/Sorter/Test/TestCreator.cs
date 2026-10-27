using Core.Sorter.Creator;
using Core.Sorter.MatchDetectors;
using System.Diagnostics;

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

            public TestRuleGenerator()
            {
                lock (_lock)
                {
                    Directory.CreateDirectory(MyDirectory);
                    Directory.CreateDirectory(MyOldDirectory);
                    MyRuleGenerator = new RuleGenerator(MyStrengthDetector);
                    TestMethods.Add(TestAllRules);
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

                foreach (string file in TestFiles)
                {
                    string _path = Path.Combine(MyDirectory, file);
                    File.Create(_path).Close();
                    TestAllData.Add(new FileInfo(file));
                }

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

                return TestNoRules.Count == 0 && TestAllRules.Count > 0;
            };
        }
        internal class TestFilter : ITestAutoSorter
        {
            public List<AutoSorterTestMethod> TestMethods { set; get; } = new();
            public static FilterData MyFilterData { set; get; } = new();

            public TestFilter()
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

                TestAllRules.Add(TestRule1);

                int TestRequiredStrength = 10;
                int TestRequiredParams = 3;
                int TestRequiredMatches = 3;
                int TestLimit = 100;

                List<Rule> StrongRules = MyFilterData.StrongRules(
                    TestAllRules, TestRequiredStrength,
                    TestRequiredParams, TestRequiredMatches,
                    TestLimit);

                return StrongRules.Count == 1;
            };
        }
    }
}
