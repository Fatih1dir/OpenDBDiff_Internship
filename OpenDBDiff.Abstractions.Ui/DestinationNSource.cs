using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDBDiff.Abstractions.Ui
{
    public class DestinationNSource 
    {
        public IFront LeftDatabaseSelector;
        public IFront RightDatabaseSelector;
        public static string leftconString;
        public static string rigthconString;

        public static DestinationNSource destinationNSource=new DestinationNSource();
    }
}
