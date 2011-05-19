using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mocknity.Strategies.Structure
{
  public interface IAutoMockBuilderStrategy
  {
    object MockObject(Type type);
  }
}
