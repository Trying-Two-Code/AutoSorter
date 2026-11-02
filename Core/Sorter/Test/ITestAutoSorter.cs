using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Sorter.Test
{
    internal interface ITestAutoSorter
    {
        public List<AutoSorterTestMethod> TestMethods { set; get; }

        public bool TestsAllSucceed();
        /*
        Example:
        public bool TestsAllSucceed()
        {
            return !TestMethods.Any(method =>
            {
                return method() == false;
            });
        }*/
    }
}
