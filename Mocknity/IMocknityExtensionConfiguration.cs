using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Mocks;
using Microsoft.Practices.Unity;

namespace Mocknity
{
  public interface IMocknityExtensionConfiguration
  {
    void SetStrategy<T>(Type type);

    bool CheckStrategyMapping<T>(Type type);

    bool IsTypeMapped(Type type);

    object Get<T>();

    void AddMock(Type type, object mock);

    MockRepository getRepository();

    IUnityContainer getContainer();

    bool MockUnregisteredInterfaces { get; }
      bool ContainsMock<T>();
      bool ContainsMock(Type tp);
      object Get(Type key);
      bool ContainsMapping(Type key);
  }
}
