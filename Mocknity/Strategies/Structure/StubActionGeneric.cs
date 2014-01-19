using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mocknity.Strategies.Structure
{
    public class StubActionGeneric<TMock> : StubAction
    {
        private readonly Action<TMock> _stubAction;
        public StubActionGeneric(Action<TMock> stubAction)
        {
            _stubAction = stubAction;
        }

        public override void Execute(object mock)
        {
            _stubAction((TMock)mock);
        }
    }
}
