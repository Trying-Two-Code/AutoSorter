using Core.Sorter.Executor;
using Core.Sorter.MatchDetectors;
using Core.Sorter.Test.Helpers;
using System.Diagnostics;

namespace Core.Sorter.Test
{
    class TestExecutor : ITestAutoSorter
    {
        public List<AutoSorterTestMethod> TestMethods { get; set; } = new();

        public TestExecutor()
        {
            Directory.CreateDirectory(MyDirectory);
            Directory.CreateDirectory(MyNewDirectory);
            TestMethods.Add(TestExecuteRule);
            TestMethods.Add(TestExecuteRuleOnFolder);
        }

        public bool TestsAllSucceed()
        {
            return !TestMethods.Any(method =>
            {
                return method() == false;
            });
        }

        public static RuleExecuter RuleExecuterTest = new();
        public static readonly string MyDirectory = Path.Combine(AppContext.BaseDirectory, "test\\testexec\\");
        public static readonly string MyNewDirectory = Path.Combine(AppContext.BaseDirectory, "test\\testexecnew\\");
        public static MassFileCreator MyFileCreator { get; set; } = new(MyDirectory);

        readonly AutoSorterTestMethod TestExecuteRule = () =>
        {
            //it moves when it should move
            bool MovesWhenItShould = false;

            AutoSorterTestMethod CheckMovesWhenItShould = () =>
            {
                string oldFilePath = Path.Combine(MyDirectory, "ExampleFile.example");
                string newDirectoryPath = MyNewDirectory;
                string newFilePath = Path.Combine(newDirectoryPath, "ExampleFile.example");

                var MoveFile = File.Create(oldFilePath);
                var Destination = Directory.CreateDirectory(newDirectoryPath);

                Rule TestRule = new()
                {
                    StartPath = MyDirectory,
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
        readonly AutoSorterTestMethod TestExecuteRuleOnFolder = () =>
        {
            List<string> TestFiles = new()
            {
                "File1.txt",
                "File2.txt",
                "File3.txt",
                "File4.different"
            };

            MyFileCreator.CreateAndForget(TestFiles);
            string TestDirectoryName = MyDirectory;
            string TestNewDirectoryName = MyNewDirectory;

            Rule TestRule = new()
            {
                StartPath = TestDirectoryName,
                EndPath = TestNewDirectoryName,
                Paramaters = new() { new("Extension")
                {
                    PropertyContains = ".txt"
                } }
            };

            RuleExecuterTest.ExecuteRuleOnFolder(TestRule, TestDirectoryName);

            bool txtInRightPlace = !File.Exists(Path.Combine(MyDirectory, TestFiles[0])) && 
                File.Exists(Path.Combine(MyNewDirectory, TestFiles[0]));

            bool difInRightPlace = File.Exists(Path.Combine(MyDirectory, TestFiles[^1])) &&
                !File.Exists(Path.Combine(MyNewDirectory, TestFiles[^1]));

            MyFileCreator.Delete();

            foreach(string testFile in TestFiles)
                File.Delete(Path.Combine(MyNewDirectory, testFile));

            return txtInRightPlace && difInRightPlace;
        };
    }
}
