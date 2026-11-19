using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static Core.Sorter.Test.TestCreator;
using Core.Sorter.Test.Helpers;

namespace Core.Sorter.Test
{
    internal class TestAll
    {
        public List<ITestAutoSorter> AllTests = new();
        private Mutex mut = new Mutex(false, "Global\\_TestAllMutex_");

        public TestAll(UITask uiTask)
        {
                TestExecutor MyTestExecutor = new TestExecutor();
                AllTests.Add(MyTestExecutor);
                TestRuleGenerator MyTestCreator = new TestRuleGenerator();
                AllTests.Add(MyTestCreator);
                TestMatchDetectors.TestRule MyTestRule = new TestMatchDetectors.TestRule();
                AllTests.Add(MyTestRule);
                TestSortAlgorithm MyTestSortAlgorithm = new TestSortAlgorithm();
                AllTests.Add(MyTestSortAlgorithm);
                TestStrengthDetector MyTestStrengthDetector = new TestStrengthDetector();
                AllTests.Add(MyTestStrengthDetector);
                TestFilter MyTestFilter = new TestFilter();
                AllTests.Add(MyTestFilter);
                TestPromptUser MyTestPromptUser = new TestPromptUser(uiTask);
                AllTests.Add(MyTestPromptUser);
        }

        private TimeLog _TimeLog = new("TestAll");

        public void Start()
        {
            if (mut.WaitOne())
            {
                int index = 0;
                Debug.WriteLine(":::Starting Tests:::");
                _TimeLog.Start();
                List<bool> results = new();
                foreach (ITestAutoSorter test in AllTests)
                {
                    bool result = test.TestsAllSucceed();
                    Debug.WriteLine($"{index} test: {test}, result: {result}");
                    _TimeLog.Log(test.ToString() + " test");
                    results.Add(result);
                    index++;
                }
                Debug.WriteLine("!!! FINAL RESULT !!!");
                if (results.Any(result => result == false))
                    Debug.WriteLine(":::Tests Failed:::");
                else
                    Debug.WriteLine(":::Tests Passed:::");
                mut.ReleaseMutex();
            }
        }
    }
}
