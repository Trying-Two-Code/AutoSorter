using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Sorter.Creator
{
    /// <summary>
    /// A struct that is made to easily and without much data hold
    /// important data pertaining to file movements. 
    /// </summary>
    public struct FileMoveInfo()
    {
        public string OldPath { get; set; } = "";
        public string NewPath { get; set; } = "";
    }
}
