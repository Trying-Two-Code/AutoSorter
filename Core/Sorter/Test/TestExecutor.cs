using Core.Sorter.Executor;
using Core.Sorter.MatchDetectors;

namespace Core.Sorter.Test
{
    class TestExecutor : ITestAutoSorter
    {
        public List<AutoSorterTestMethod> TestMethods { get; set; } = new();

        public TestExecutor()
        {
            Directory.CreateDirectory(MyDirectory);
            TestMethods.Add(TestExecuteRule);
        }

        public bool TestsAllSucceed()
        {
            return !TestMethods.Any(method =>
            {
                return method() == false;
            });
        }

        public static RuleExecuter RuleExecuterTest = new();
        public static readonly string MyDirectory = Path.Combine(AppContext.BaseDirectory, "\\test\\testexec\\");

        AutoSorterTestMethod TestExecuteRule = () =>
        {
            //it moves when it should move
            bool MovesWhenItShould = false;

            AutoSorterTestMethod CheckMovesWhenItShould = () =>
            {
                string oldFilePath = Path.Combine(MyDirectory, "ExampleFile.example");
                string newDirectoryPath = Path.Combine(MyDirectory, "/NewDestination/");
                string newFilePath = Path.Combine(newDirectoryPath, "ExampleFile.example");

                var MoveFile = File.Create(oldFilePath);
                var Destination = Directory.CreateDirectory(newDirectoryPath);

                Rule TestRule = new()
                {
                    StartPath = Path.GetDirectoryName(MoveFile.Name),
                    EndPath = Destination.FullName,
                    Paramaters = new(),
                };

                MoveFile.Dispose();

                RuleExecuterTest.ExecuteRule(TestRule, new FileInfo(oldFilePath));

                bool result = !File.Exists(oldFilePath) && File.Exists(newFilePath);

                File.Delete(newFilePath);

                return result;
            };

            MovesWhenItShould = CheckMovesWhenItShould();

            //return result
            return MovesWhenItShould;
        };
    }
}
