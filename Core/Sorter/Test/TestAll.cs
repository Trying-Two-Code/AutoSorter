using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static Core.Sorter.Test.TestCreator;

namespace Core.Sorter.Test
{
    internal class TestAll
    {
        public List<ITestAutoSorter> AllTests = new()
        {

        };

        public TestAll()
        {
            TestExecutor MyTestExecutor = new TestExecutor();
            AllTests.Add(MyTestExecutor);
            TestRuleGenerator MyTestCreator = new TestRuleGenerator();
            AllTests.Add(MyTestCreator);
        }

        public void Start()
        {
            int index = 0;
            Debug.WriteLine(":::Starting Tests:::");
            List<bool> results = new();
            foreach (ITestAutoSorter test in AllTests)
            {
                bool result = test.TestsAllSucceed();
                Debug.WriteLine($"{index} test: {test}, result: {result}");
                results.Add(result);
                index++;
            }
            Debug.WriteLine("!!! FINAL RESULT !!!");
            if (results.Any(result => !result))
                Debug.WriteLine(":::Tests Failed:::");
            else
                Debug.WriteLine(":::Tests Passed:::");
        }
    }
}
