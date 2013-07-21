using System;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mocknity;
using Mocknity.Strategies.Rhino;
using Rhino.Mocks;
using Rhino.Mocks.Exceptions;

namespace MocknityTests
{

    #region test classes

    public interface IFirstObject
    {
        string IntroduceYourself();
    }

    public interface ISecondObject
    {
        int MyProperty { get; set; }

        string HelloWorld();
    }

    public class FirstObjectImpl : IFirstObject
    {
        #region IFirstObject Members

        public string IntroduceYourself()
        {
            return "I'm the first of all";
        }

        #endregion
    }

    public interface IThirdObject
    {
        int MyProperty { get; set; }
        string HelloWorld3();
    }

    public interface IFourthObject
    {
        void Foo();
    }

    public interface IObjectWithDependencies
    {
        string Test();
    }

    public class ObjectWithDependencies : IObjectWithDependencies
    {
        public IFirstObject firstObject;
        public ISecondObject secondObject;

        public ObjectWithDependencies(IFirstObject firstObject, ISecondObject secondObject)
        {
            this.firstObject = firstObject;
            this.secondObject = secondObject;
        }

        #region IObjectWithDependencies Members

        public virtual string Test()
        {
            return "test";
        }

        #endregion

        public virtual string Foo()
        {
            return "foo";
        }

        public string PokeSecond()
        {
            return secondObject.HelloWorld();
        }
    }

    public class ObjectWithDependencies2
    {
        public IFirstObject firstObject;
        public ISecondObject fourthObject;

        public ObjectWithDependencies2(IFirstObject firstObject, IFourthObject fourthObject)
        {
            this.firstObject = firstObject;
            this.fourthObject = this.fourthObject;
        }

        public string PokeSecond()
        {
            return fourthObject.HelloWorld();
        }
    }

    #endregion

    [TestClass]
    public class MocknityTests
    {
        private IUnityContainer _ioc;
        private MocknityContainerExtension _mocknity;
        private MockRepository _mocks;

        [TestInitialize]
        public void TestInitialize()
        {
            InitPrivateMembers(true);
        }

        private void InitPrivateMembers(bool mockUnregisteredInterfaces)
        {
            _ioc = new UnityContainer();
            _mocks = new MockRepository();
            _mocknity = new MocknityContainerExtension(_mocks, mockUnregisteredInterfaces);
            _ioc.AddExtension(_mocknity);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _ioc.Dispose();
        }

        [TestMethod]
        public void ResolveObject__MockDependenciesInitiated()
        {
            var od = _ioc.Resolve<ObjectWithDependencies>();

            Assert.IsNotNull(od.firstObject);
            Assert.IsNotNull(od.secondObject);
        }

        [TestMethod]
        public void ResolveObjectWithSpecificImpl__OneDependencyIsNotMocked()
        {
            _ioc.RegisterType<IFirstObject, FirstObjectImpl>();
            var od = _ioc.Resolve<ObjectWithDependencies>();

            Assert.IsInstanceOfType(od.firstObject, typeof (FirstObjectImpl));
            Assert.IsNotNull(od.secondObject);
        }

        [TestMethod]
        public void ResolveObjectWithExpectedCall__GetDesiredResult()
        {
            var od = _ioc.Resolve<ObjectWithDependencies>();

            Expect.Call(od.secondObject.HelloWorld()).Return("I'm the first of none");
            _mocknity.getRepository().ReplayAll();
            Assert.AreEqual("I'm the first of none", od.PokeSecond());
            _mocknity.getRepository().VerifyAll();
        }

        [TestMethod]
        [ExpectedException(typeof (ExpectationViolationException))]
        public void SecondObjectIsAStrictMock__ExceptionThrownOnCallingNotExpectedMethod()
        {
            _mocknity.SetStrategy<StrictRhinoMocksBuilderStrategy>(typeof (ISecondObject));
            var od = _ioc.Resolve<ObjectWithDependencies>();

            _mocknity.getRepository().ReplayAll();
            od.secondObject.HelloWorld();
            _mocknity.getRepository().VerifyAll();
        }

        [TestMethod]
        public void SecondObjectIsAStub__CanChangeProperties()
        {
            _mocknity.SetStrategy<StubRhinoMocksBuilderStrategy>(typeof (ISecondObject));
            var od = _ioc.Resolve<ObjectWithDependencies>();

            _mocknity.getRepository().ReplayAll();
            od.secondObject.MyProperty = 42;
            Assert.AreEqual(42, od.secondObject.MyProperty);
            _mocknity.getRepository().VerifyAll();
        }

        [TestMethod]
        public void SecondObjectIsADynamicCall__AcceptsUnexpectedCall()
        {
            // dynamic mocking is a default strategy in mocknity
            var od = _ioc.Resolve<ObjectWithDependencies>();

            _mocknity.getRepository().ReplayAll();
            // doesn't raise exceptions
            od.secondObject.HelloWorld();
            _mocknity.getRepository().VerifyAll();
        }

        [TestMethod]
        public void SetSameStrategyTwiceForDiffernetTypes__ExpectedWorks()
        {
            _mocknity.SetStrategy<StubRhinoMocksBuilderStrategy>(typeof (ISecondObject));
            _mocknity.SetStrategy<StubRhinoMocksBuilderStrategy>(typeof (IThirdObject));

            var obj = _ioc.Resolve<IThirdObject>();
            Assert.IsNotNull(obj);
            obj.MyProperty = 42;
            Assert.AreEqual(42, obj.MyProperty);

            var obj2 = _ioc.Resolve<ISecondObject>();
            Assert.IsNotNull(obj2);
            obj2.MyProperty = 42;
            Assert.AreEqual(42, obj2.MyProperty);
        }

        [TestMethod]
        public void IfMockingUnregegisteredInterfacesIs_ON_Work_If_Interfaces_Unregistered()
        {
            _mocknity.SetStrategy<StubRhinoMocksBuilderStrategy>(typeof (ISecondObject));
            _mocknity.SetStrategy<StubRhinoMocksBuilderStrategy>(typeof (IThirdObject));
            var obj = _ioc.Resolve<ObjectWithDependencies2>();
        }

        [TestMethod, ExpectedException(typeof (ResolutionFailedException))]
        public void IfMockingUnregegisteredInterfacesIs_OFF_NOT_Work_If_Interfaces_Unregistered()
        {
            InitPrivateMembers(false);
            _mocknity.SetStrategy<StubRhinoMocksBuilderStrategy>(typeof (ISecondObject));
            _mocknity.SetStrategy<StubRhinoMocksBuilderStrategy>(typeof (IThirdObject));
            var obj = _ioc.Resolve<ObjectWithDependencies2>();
        }

        public void CheckPartialMock(IFirstObject obj)
        {
            Assert.IsNotNull(obj);
            string str = obj.IntroduceYourself();
            Assert.IsFalse(String.IsNullOrEmpty(str));
        }

        [TestMethod]
        public void ResolvePartialMock_HaveNoException()
        {
            _mocknity.RegisterPartialMock<IFirstObject, FirstObjectImpl>();
            var obj = _ioc.Resolve<IFirstObject>();
            ;
            obj.Replay();
            CheckPartialMock(obj);
            var obj2 = _ioc.Resolve<FirstObjectImpl>();
            obj2.Replay();
            CheckPartialMock(obj2);
        }

        [TestMethod]
        public void ResolveObjectThanWasNotRegistered_ShouldWorks_WhenHaveDependcies()
        {
            _mocknity.RegisterDynamicMock<IFirstObject>();
            _mocknity.RegisterDynamicMock<ISecondObject>();
            var obj = _ioc.Resolve<ObjectWithDependencies>();
            Assert.IsNotNull(obj);
        }

        [TestMethod]
        public void ResolvePartialMock_Works_WhenHaveDependcies()
        {
            _mocknity.RegisterStrictMock<IFirstObject>();
            _mocknity.RegisterStrictMock<ISecondObject>();
            _mocknity.RegisterPartialMock<ObjectWithDependencies>();

            var obj = _ioc.Resolve<ObjectWithDependencies>();
            obj.Replay();
            Assert.AreEqual("foo", obj.Foo());
            obj.Stub(x => x.Foo()).Return("f").Repeat.Any();
            obj.Replay();
            Assert.AreEqual("f", obj.Foo());
        }

        [TestMethod]
        public void ResolvePartialMock_Works_WhenRequestingBaseType()
        {
            _mocknity.RegisterPartialMock<IObjectWithDependencies, ObjectWithDependencies>();
            var obj = _ioc.Resolve<IObjectWithDependencies>();
            Assert.IsNotNull(obj);
        }

        [TestMethod]
        public void ResolvePartialMock_IfCallTwiceForInterface__MocksShouldBeSame()
        {
            _mocknity.RegisterPartialMock<IObjectWithDependencies, ObjectWithDependencies>();

            var obj1 = _ioc.Resolve<IObjectWithDependencies>();
            var obj2 = _ioc.Resolve<IObjectWithDependencies>();
            Assert.AreEqual(obj1, obj2);
        }

        [TestMethod]
        public void ResolvePartialMock_IfCallTwiceForInterfaceAndImpl__MocksShouldBeSame()
        {
            _mocknity.RegisterPartialMock<IObjectWithDependencies, ObjectWithDependencies>();

            var obj1 = _ioc.Resolve<IObjectWithDependencies>();
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreEqual(obj1, obj2);
        }

        [TestMethod]
        public void ResolvePartialMock_IfCallTwiceForInterfaceAndImpl_ReverseOrder__MocksShouldBeSame()
        {
            _mocknity.RegisterPartialMock<IObjectWithDependencies, ObjectWithDependencies>();
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            var obj1 = _ioc.Resolve<IObjectWithDependencies>();
            Assert.AreEqual(obj1, obj2);
        }

        [TestMethod]
        public void ResolvePartialMock_IfCallTwiceForImpl__MocksShouldBeSame()
        {
            _mocknity.RegisterPartialMock<IObjectWithDependencies, ObjectWithDependencies>();

            var obj1 = _ioc.Resolve<ObjectWithDependencies>();
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreEqual(obj1, obj2);
        }

        //DYNAMIC
        [TestMethod]
        public void ResolveDynamicMock_Works_WhenHaveDependcies()
        {
            _mocknity.RegisterDynamicMock<IFirstObject>();
            _mocknity.RegisterDynamicMock<ISecondObject>();
            _mocknity.RegisterDynamicMock<ObjectWithDependencies>();

            var obj = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreEqual(null, obj.Test());
            obj.Stub(x => x.Test()).Return("t").Repeat.Any();
            obj.Replay();
            Assert.AreEqual("t", obj.Test());
        }

        [TestMethod]
        public void ResolveDynamicMock_Works_WhenRequestingBaseType()
        {
            _mocknity.RegisterDynamicMock<IObjectWithDependencies, ObjectWithDependencies>();
            var obj = _ioc.Resolve<IObjectWithDependencies>();
            Assert.IsNotNull(obj);
        }

        [TestMethod]
        public void ResolveDynamicMock_IfCallTwice__MocksShouldBeSame()
        {
            _mocknity.RegisterDynamicMock<IObjectWithDependencies, ObjectWithDependencies>();

            var obj1 = _ioc.Resolve<IObjectWithDependencies>();
            var obj2 = _ioc.Resolve<IObjectWithDependencies>();
            Assert.AreEqual(obj1, obj2);

            obj1 = _ioc.Resolve<ObjectWithDependencies>();
            obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreEqual(obj1, obj2);
        }

        //STRICT
        [TestMethod, ExpectedException(typeof (ExpectationViolationException))]
        public void ResolveStrictMock_Works_WhenHaveDependcies()
        {
            _mocknity.RegisterStrictMock<IFirstObject>();
            _mocknity.RegisterStrictMock<ISecondObject>();
            _mocknity.RegisterStrictMock<ObjectWithDependencies>();
            var obj = _ioc.Resolve<ObjectWithDependencies>();
            _mocks.ReplayAll();
            string result = obj.Test();
        }

        [TestMethod]
        public void ResolveStrictMock_Works_WhenRequestingBaseType()
        {
            _mocknity.RegisterStrictMock<IObjectWithDependencies, ObjectWithDependencies>();
            var obj = _ioc.Resolve<IObjectWithDependencies>();
            Assert.IsNotNull(obj);
        }

        [TestMethod]
        public void ResolveStrictMock_IfCallTwice__MocksShouldBeSame()
        {
            _mocknity.RegisterStrictMock<IObjectWithDependencies, ObjectWithDependencies>();

            var obj1 = _ioc.Resolve<IObjectWithDependencies>();
            var obj2 = _ioc.Resolve<IObjectWithDependencies>();
            Assert.AreEqual(obj1, obj2);

            obj1 = _ioc.Resolve<ObjectWithDependencies>();
            obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreEqual(obj1, obj2);
        }
    }
}