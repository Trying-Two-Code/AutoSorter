using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Sorter
{
    public class Rule()
    {
        public List<Param> Paramaters { get; set; } = [];
        public string StartPath { get; set; }
        public string EndPath { get; set; }
        /// <summary>
        /// the amount of times the rule has been executed
        /// (includes Autosorter and human intervention)
        /// </summary>
        public int Strength { get; set; } = 0;

        public bool ShouldMove(FileInfo fileInfo)
        {
            if (fileInfo.DirectoryName != StartPath)
            {
                return false;
            };

            if (Paramaters.Any((parameter) =>
            {
                return parameter.Matches(fileInfo) == false;
            }))
            {
                //A parameter didn't match the fileInfo.
            }
            else
            {
                //All parameters matched.
                return true;
            }

            return false;
        }
    }
}
