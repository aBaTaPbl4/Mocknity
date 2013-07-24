using System;
using Microsoft.Practices.Unity;
using Rhino.Mocks;

namespace Mocknity
{
    public interface IMocknityExtensionConfiguration
    {
        bool MockUnregisteredInterfaces { get; }
        bool AutoReplayPartialMocks { get; set; }
        void SetStrategy<T>(Type type, bool oneMockCreate = true, string name = "");

        bool CheckStrategyMapping<T>(Type type, string name);

        bool IsTypeMapped(Type type, string name);

        object GetMock<T>(string name = null);

        void AddMock(Type type, object mock, string name);

        MockRepository getRepository();

        IUnityContainer getContainer();

        bool ContainsMock<T>(string name = null);
        bool ContainsMock(Type tp, string name);
        object GetMock(Type key, string name);
        bool ContainsMapping(Type key, string name);
    }
}