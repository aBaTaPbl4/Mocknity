using System;

namespace Mocknity.Strategies.Structure
{
    public interface IAutoMockBuilderStrategy
    {
        object CreateMockByInterface(Type type);
    }
}