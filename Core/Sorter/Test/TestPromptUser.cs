using Core.Sorter.Creator;
using Core.Sorter.MatchDetectors;
using System.Diagnostics;
using static Core.Sorter.Test.TestData;

namespace Core.Sorter.Test
{
    internal class TestPromptUser : ITestAutoSorter
    {
        public List<AutoSorterTestMethod> TestMethods { set; get; } = new();
        public UITask myUITask { get; init; }

        public TestPromptUser(UITask uiTask)
        {
            myUITask = uiTask;
            TestMethods.Add(TestPrompt);
        }

        public bool TestsAllSucceed()
        {
            return !TestMethods.Any(method =>
            {
                return method() == false;
            });
        }

        bool TestPrompt()
        {
            Debug.WriteLine("starting prompt tests");
            PromptData nullData = PromptData.nullPrompt;

            PromptData TestTrueData = nullData.defaultPrompt;
            TestTrueData.TestData = new TestData()
            {
                actions = new List<TestAction>()
                {
                    TestAction.ClickYes
                }
            };
            bool TestTrueDataResult = myUITask(this, TestTrueData).Result;

            PromptData TestFalseData = nullData.defaultPrompt;
            TestFalseData.TestData = new TestData()
            {
                actions = new List<TestAction>()
                {
                    TestAction.ClickNo
                }
            };
            bool TestFalseDataResult = myUITask(this, TestFalseData).Result;

            Debug.WriteLine($"Results for TestPromptUser.cs: \ntruedata = {TestTrueDataResult}\nfalseData = {TestFalseDataResult}");

            return !TestFalseDataResult && TestTrueDataResult;
        }
    }

    /// <summary>
    /// Holds data for testing.
    /// </summary>
    public class TestData()
    {
        public enum TestAction
        {
            ClickYes,
            ClickNo,
            ClickNewRuleInfo,
            ClickFileInfo,
            ClickOldRuleInfo,
            ClickSettings,
            CloseActiveWindow
        };

        public List<TestAction> actions = new List<TestAction>();
    }
}
