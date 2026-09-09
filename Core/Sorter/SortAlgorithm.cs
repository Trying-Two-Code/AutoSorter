//OVERVIEW: contains an algorithm that detects if an automation rule can and should be made.

//example:
//File moves from C://Downloads/A -> C://Downloads/B. It had a name of "BOB.txt". It was created 01-01-2026.
//Potential rules are created with paramaters focused on name, extension, and creation date.
//It is found that everytime user moves from A -> B, it is a .txt.
//Rule with:
// - A param of ({FileProperty = "extension", PropertyContains = ".txt", PropertyNotContains = null})
// - StartPath == C://Downloads/A && EndPath == C://Downloads/B
//is returned
namespace Core.Sorter
{

    /// <summary>
    /// Contains one paramater for a file, and what the parameter should or should not be.
    /// </summary>
    struct Param()
    {
        string FileProperty { get; set; } = "name";
        string? PropertyContains { get; set; }
        string? PropertyNotContains { get; set; }
    }

    /// <summary>
    /// Contains many paramaters, and a Method for detecting if those paramaters
    /// all match the paramater of a given file.
    /// </summary>
    class Rule()
    {
        List<Param> Paramaters { get; set; } = [];
        string StartPath { get; set; }
        string EndPath { get; set; }

        bool ShouldMove(FileInfo fileInfo, string currentPath)
        {
            if(currentPath != StartPath) return false;

            return false;
        }
    }
    internal class SortAlgorithm
    {
        /// <summary>
        /// Tests a potential rule to see if it is worth prompting the user.
        /// </summary>
        /// <returns>How many files fit the rule.</returns>
        static int TestRule(Rule rule)
        {
            return 0;
        }

        /// <summary>
        /// Creates a potential rule based on the params given.
        /// </summary>
        /// <returns>Returns the created rule.</returns>
        static Rule? GenerateRule(List<Param> parameters)
        {
            return null;
        }

        /// <summary>
        /// Creates a list of every possible Rule that could be made based on the paramaters
        /// given.
        /// </summary>
        /// <returns>The created list of Rules.</returns>
        static List<Rule?> GenerateRules()
        {
            return [null];
        }
        /// <summary>
        /// Generates a list of all possible rules given the old data and new file data.
        /// </summary>
        /// <returns>The best possible rule, only if it is worth prompting the user.</returns>
        static Rule? ManageRules(/*old data*/)
        {
            //sort old data for only those files that match the start folder and end destination paths

            //generate a list of rules

            //remove any rules if they already exist

            //return best rule if applicable

            return null;
        }

        //Called when, for example, a data entry is added to userAction.json
        static void OnDataGained()
        {
            ManageRules();
        }
    }
}
