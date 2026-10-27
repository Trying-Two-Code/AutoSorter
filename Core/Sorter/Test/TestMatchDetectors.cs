using Core.Sorter.Creator;
using Core.Sorter.MatchDetectors;
using Core.Sorter.Test.Helpers;

namespace Core.Sorter.Test
{
    public class TestMatchDetectors
    {
        public class TestRule : ITestAutoSorter
        {
            public List<AutoSorterTestMethod> TestMethods { set; get; } = new();
            public static readonly string MyDirectory = Path.Combine(AppContext.BaseDirectory, "\\test\\testrule\\");
            public static readonly string MyOldDirectory = Path.Combine(AppContext.BaseDirectory, "\\test\\testoldrule\\");
            public static MassFileCreator MyFileCreator;

            public TestRule()
            {
                TestMethods.Add(TestRuleInit);
                Directory.CreateDirectory(MyDirectory);
                Directory.CreateDirectory(MyOldDirectory);
                MyFileCreator = new(MyDirectory);
            }

            public bool TestsAllSucceed()
            {
                return !TestMethods.Any(method =>
                {
                    return method() == false;
                });
            }

            AutoSorterTestMethod TestRuleInit = () =>
            {
                List<string> NewFiles = new List<string>()
                {
                    "File1.txt", "File2.txt", "File3.txt", "File4.txt"
                };

                AutoSorterTestMethod TestStrength0 = () =>
                {

                    MyFileCreator.CreateAndForget(NewFiles);

                    List<FileInfo> TestAllData = MyFileCreator.MyFileInfos;
                    List<FileMoveInfo> TestUserActions = new();
                    List<Param> TestParameters = new();
                    string TestStartPath = MyOldDirectory;
                    string TestEndPath = MyDirectory;

                    Rule MyTestRule = new Rule(
                        TestAllData, TestUserActions, TestParameters, TestStartPath, TestEndPath);

                    MyFileCreator.Delete();

                    return MyTestRule.Strength == 0 && MyTestRule.ExecutionCount == 0 && MyTestRule.StartEstimation != 0;
                };

                return TestStrength0();
            };
        }
    }
}
