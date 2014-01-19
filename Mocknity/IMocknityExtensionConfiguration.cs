using System;
using Microsoft.Practices.Unity;
using Mocknity.Strategies.Structure;
using Rhino.Mocks;

namespace Mocknity
{
    public interface IMocknityExtensionConfiguration
    {
        bool MockUnregisteredInterfaces { get; }
        /// <summary>
        /// always replay partial mocks during resolving
        /// </summary>
        bool AutoReplayPartialMocks { get; set; }
        /// <summary>
        /// Replay mock during resolving, if stub behiviour is set during registration
        /// </summary>
        bool AutoReplayStubbedMocks { get; set; }
        void SetStrategy<T>(Type type, bool oneMockCreate = true, string name = "", StubAction stubAction = null, params TypedInjectionValue[] resolveParams);

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