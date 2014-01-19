using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mocknity.Strategies.Structure
{
    public abstract class StubAction
    {
        public abstract void Execute(object mock);
    }
}
