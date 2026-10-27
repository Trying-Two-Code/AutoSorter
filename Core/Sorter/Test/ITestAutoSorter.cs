using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Sorter.Test
{
    internal interface ITestAutoSorter
    {
        public bool TestsAllSucceed()
        {
            return !TestMethods.Any(method =>
            {
                return method() == false;
            });
        }

        public List<AutoSorterTestMethod> TestMethods { set; get; }
    }
}
