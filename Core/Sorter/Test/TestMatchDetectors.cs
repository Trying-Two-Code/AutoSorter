using Core.Sorter.Creator;
using Core.Sorter.MatchDetectors;

namespace Core.Sorter.Test
{
    internal class TestMatchDetectors
    {
        internal class TestRule : ITestAutoSorter
        {
            public List<AutoSorterTestMethod> TestMethods { set; get; }
            public static readonly string MyDirectory = Path.Combine(AppContext.BaseDirectory, "\\test\\testrule\\");
            public static readonly string MyOldDirectory = Path.Combine(AppContext.BaseDirectory, "\\test\\testoldrule\\");

            TestRule()
            {
                TestMethods.Add(TestRuleInit);
                Directory.CreateDirectory(MyDirectory);
                Directory.CreateDirectory(MyOldDirectory);
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
                List<FileInfo> TestAllData = new();
                List<FileMoveInfo> TestUserActions = new();
                List<Param> TestParameters = new();
                string TestStartPath = MyOldDirectory;
                string TestEndPath = MyDirectory;

                Rule TestRule = new Rule(
                    TestAllData, TestUserActions, TestParameters, TestStartPath, TestEndPath);

                return TestRule.MatchingFiles.Count == 0;
            };
        }
    }
}
