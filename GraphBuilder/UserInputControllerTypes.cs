using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphBuilder
{
    //Define type of user workspace controller
    public enum UserInputController
    {
        Default = 0,
        NodeCreating = 1,
        EdgeCreating = 2,
        NodeDelete = 3
    }

    //Define edge way types
    public enum EdgeTypes 
    {
        Oneway = 0,
        Doubleway = 1
    }
}
