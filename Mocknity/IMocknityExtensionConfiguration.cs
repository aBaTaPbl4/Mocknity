using System;
using Microsoft.Practices.Unity;
using Rhino.Mocks;

namespace Mocknity
{
    public interface IMocknityExtensionConfiguration
    {
        bool MockUnregisteredInterfaces { get; }
        bool AutoReplayPartialMocks { get; set; }
        void SetStrategy<T>(Type type, bool oneMockCreate = true);

        bool CheckStrategyMapping<T>(Type type);

        bool IsTypeMapped(Type type);

        object Get<T>();

        void AddMock(Type type, object mock);

        MockRepository getRepository();

        IUnityContainer getContainer();

        bool ContainsMock<T>();
        bool ContainsMock(Type tp);
        object Get(Type key);
        bool ContainsMapping(Type key);
    }
}