using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mocknity;
using Mocknity.Strategies.Rhino;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using Rhino.Mocks.Exceptions;
using Rhino.Mocks.Interfaces;

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

        public virtual string IntroduceYourself()
        {
            return GetType().Name;
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

    public class FirstObjectImpl3 : IFirstObject
    {
        public string IntroduceYourself()
        {
            throw new NotImplementedException();
        }
    }

    public class FirstObjectImpl2 : IFirstObject
    {
        private ISecondObject _obj;

        public FirstObjectImpl2(ISecondObject obj)
        {
            _obj = obj;
        }

        public virtual string IntroduceYourself()
        {
            return GetType().Name;
        }

    }

    public class ObjectWithPropDependency
    {
        [Dependency]
        public IFirstObject Obj { get; set; }

        [Dependency("FirstObjectImpl3")]
        public IFirstObject ObjNamed { get; set; }
    }

    public class EmptyType
    {
        
    }


    public class  MyClass 
    {
        public IFirstObject FirstObj;
        public Guid Id1;
        public Guid Id2;
        public ISecondObject SecondObj;

        public MyClass(IFirstObject firstObj, Guid id1, ISecondObject secondObj, Guid id2)
        {
            FirstObj = firstObj;
            Id1 = id1;
            SecondObj = secondObj;
            Id2 = id2;
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
            _mocknity.RegisterStub<ISecondObject>();
            _mocknity.RegisterStub<IThirdObject>();
            var obj = _ioc.Resolve<ObjectWithDependencies2>();
        }

        [TestMethod, ExpectedException(typeof (ResolutionFailedException))]
        public void IfMockingUnregegisteredInterfacesIs_OFF_NOT_Work_If_Interfaces_Unregistered()
        {
            InitPrivateMembers(false);
            _mocknity.RegisterStub<ISecondObject>();
            _mocknity.RegisterStub<IThirdObject>();
            var obj = _ioc.Resolve<ObjectWithDependencies2>();
        }

        [TestMethod]
        public void IfMockingUnregegisteredInterfacesIs_OFF_Work_If_Interfaces_Registered()
        {
            InitPrivateMembers(false);
            _mocknity.RegisterStub<IFirstObject>();
            _mocknity.RegisterStub<ISecondObject>();
            var obj = _ioc.Resolve<ObjectWithDependencies>();
            Assert.IsNotNull(obj);
        }

        [TestMethod]
        public void IfMockingUnregegisteredInterfacesIs_OFF_Work_If_Class_Registered()
        {
            InitPrivateMembers(false);
            _mocknity.RegisterPartialMock<FirstObjectImpl>();
            var obj = _ioc.Resolve<FirstObjectImpl>();
            CheckObjectIsPartialMock(obj);
        }

        [TestMethod]
        public void IfMockingUnregegisteredInterfacesIs_OFF_Work_If_ClassWithArgument_Registered()
        {
            InitPrivateMembers(false);
            _mocknity.RegisterPartialMock<FirstObjectImpl2>();
            _mocknity.RegisterStrictMock<ISecondObject>();
            var obj = _ioc.Resolve<FirstObjectImpl2>();
            CheckObjectIsPartialMock(obj);
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
        [TestMethod, ExpectedException(typeof(ExpectationViolationException))]
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

        [TestMethod]
        public void AutoReplay_If_True__Test()
        {
            _mocknity.RegisterPartialMock<ObjectWithDependencies>();
            var obj = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreEqual("foo", obj.Foo());
        }

        [TestMethod]
        public void AutoReplay_If_False__Test()
        {
            _mocknity.AutoReplayPartialMocks = false;
            _mocknity.RegisterPartialMock<ObjectWithDependencies>();
            var obj = _ioc.Resolve<ObjectWithDependencies>();
            Assert.IsNull(obj.Foo());
        }

        [TestMethod]
        public void RegisterPartialMockType_ImplVSImpl__Test()
        {
            _mocknity.RegisterPartialMockType<ObjectWithDependencies>();
            var obj1 = _ioc.Resolve<ObjectWithDependencies>();
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);  
        }

        [TestMethod]
        public void RegisterPartialMockType_ImplVSInterface__Test()
        {
            _mocknity.RegisterPartialMockType<IObjectWithDependencies, ObjectWithDependencies>();
            var obj1 = _ioc.Resolve<IObjectWithDependencies>();
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [TestMethod]
        public void RegisterStubType_ImplVSImpl__Test()
        {
            _mocknity.RegisterStubType<ObjectWithDependencies>();
            var obj1 = _ioc.Resolve<ObjectWithDependencies>();
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [TestMethod]
        public void RegisterStubType_ImplVSInterface__Test()
        {
            _mocknity.RegisterStubType<IObjectWithDependencies, ObjectWithDependencies>();
            var obj1 = _ioc.Resolve<IObjectWithDependencies>();
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [TestMethod]
        public void RegisterDynamicMockType_ImplVSImpl__Test()
        {
            _mocknity.RegisterDynamicMockType<ObjectWithDependencies>();
            var obj1 = _ioc.Resolve<ObjectWithDependencies>();
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [TestMethod]
        public void RegisterDynamicMockType_ImplVSInterface__Test()
        {
            _mocknity.RegisterDynamicMockType<IObjectWithDependencies, ObjectWithDependencies>();
            var obj1 = _ioc.Resolve<IObjectWithDependencies>();
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [TestMethod]
        public void RegisterStrictMockType_ImplVSImpl__Test()
        {
            _mocknity.RegisterStrictMockType<ObjectWithDependencies>();
            var obj1 = _ioc.Resolve<ObjectWithDependencies>();
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [TestMethod]
        public void RegisterStrictMockType_ImplVSInterface__Test()
        {
            _mocknity.RegisterStrictMockType<IObjectWithDependencies, ObjectWithDependencies>();
            var obj1 = _ioc.Resolve<IObjectWithDependencies>();
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [TestMethod]
        public void RegisterPartialNamedMock_NamedAndDefaultInstances__AreDifferent()
        {
            _mocknity.RegisterPartialMock<ObjectWithDependencies>();
            _mocknity.RegisterPartialMock<ObjectWithDependencies>("test");
            var obj1 = _ioc.Resolve<ObjectWithDependencies>("test");
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [TestMethod, ExpectedException(typeof(ResolutionFailedException))]
        public void RegisterPartialNamedMock_DefaultResolveShouldFailed__Test()
        {
            _mocknity.MockUnregisteredInterfaces = false;
            //object will be created by unity, but fail will occured on dependency resolve;
            _mocknity.RegisterPartialMock<ObjectWithDependencies>("test");
            _ioc.Resolve<ObjectWithDependencies>();
        }

        [TestMethod]
        public void RegisterDynamicNamedMock_NamedAndDefaultInstances__AreDifferent()
        {
            _mocknity.RegisterDynamicMock<ObjectWithDependencies>();
            _mocknity.RegisterDynamicMock<ObjectWithDependencies>("test");
            var obj1 = _ioc.Resolve<ObjectWithDependencies>("test");
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [TestMethod, ExpectedException(typeof(ResolutionFailedException))]
        public void RegisterDynamicNamedMock_DefaultResolveShouldFailed__Test()
        {
            _mocknity.MockUnregisteredInterfaces = false;
            //object will be created by unity, but fail will occured on dependency resolve;
            _mocknity.RegisterDynamicMock<ObjectWithDependencies>("test");
            _ioc.Resolve<ObjectWithDependencies>();
        }

        [TestMethod]
        public void RegisterStrictNamedMock_NamedAndDefaultInstances__AreDifferent()
        {
            _mocknity.RegisterStrictMock<ObjectWithDependencies>();
            _mocknity.RegisterStrictMock<ObjectWithDependencies>("test");
            var obj1 = _ioc.Resolve<ObjectWithDependencies>("test");
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [TestMethod]
        public void RegisterPartial_NamedTypes__InstancesAreDifferent()
        {
            _mocknity.RegisterPartialMockType<IFirstObject, FirstObjectImpl>("Impl1");
            _mocknity.RegisterPartialMockType<IFirstObject, FirstObjectImpl2>("Impl2");
            var obj1 = _ioc.Resolve<IFirstObject>("Impl1");
            var obj2 = _ioc.Resolve<IFirstObject>("Impl2");
            Assert.AreNotEqual(obj1, obj2);
            Assert.AreEqual(obj1.GetType().Name, obj1.IntroduceYourself());
            Assert.AreEqual(obj2.GetType().Name, obj2.IntroduceYourself());
        }

        [TestMethod, ExpectedException(typeof(ResolutionFailedException))]
        public void RegisterStrictNamedMock_DefaultResolveShouldFailed__Test()
        {
            _mocknity.MockUnregisteredInterfaces = false;
            //object will be created by unity, but fail will occured on dependency resolve;
            _mocknity.RegisterStrictMock<ObjectWithDependencies>("test");
            _ioc.Resolve<ObjectWithDependencies>();
        }

        [TestMethod]
        public void RegisterNamedStub_NamedAndDefaultInstances__AreDifferent()
        {
            _mocknity.RegisterStub<ObjectWithDependencies>();
            _mocknity.RegisterStub<ObjectWithDependencies>("test");
            var obj1 = _ioc.Resolve<ObjectWithDependencies>("test");
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [TestMethod, ExpectedException(typeof(ResolutionFailedException))]
        public void RegisterNamedStub_DefaultResolveShouldFailed__Test()
        {
            _mocknity.MockUnregisteredInterfaces = false;
            //object will be created by unity, but fail will occured on dependency resolve;
            _mocknity.RegisterStub<ObjectWithDependencies>("test");
            _ioc.Resolve<ObjectWithDependencies>();
        }

        [TestMethod]
        public void DependencyProperties_ShouldBeInitialized_ByMockResolve()
        {
            _ioc.RegisterType<IFirstObject, FirstObjectImpl>();
            _ioc.RegisterType<IFirstObject, FirstObjectImpl3>("FirstObjectImpl3");
            _mocknity.RegisterPartialMock<ObjectWithPropDependency>();
            var mock = _ioc.Resolve<ObjectWithPropDependency>();
            Assert.IsNotNull(mock.Obj);
        }

        [TestMethod]
        public void NamedDependencyProperties_ShouldBeInitialized_ByMockResolve()
        {
            _ioc.RegisterType<IFirstObject, FirstObjectImpl>();
            _ioc.RegisterType<IFirstObject, FirstObjectImpl3>("FirstObjectImpl3");         
            _mocknity.RegisterPartialMock<ObjectWithPropDependency>();

            var mock = _ioc.Resolve<ObjectWithPropDependency>();
            Assert.IsInstanceOfType(mock.ObjNamed, typeof(FirstObjectImpl3));
        }

        [TestMethod]
        public void RegisterMockTypeWithName_WhenDefaultInstnceIsDefined_ShouldFail()
        {
            _ioc.RegisterType<FirstObjectImpl>();
            var name = "nm";
            _mocknity.RegisterPartialMockType<FirstObjectImpl>(name);
            var obj = _ioc.Resolve<FirstObjectImpl>(name);
            CheckObjectIsReal(obj);
        }

        [TestMethod]
        public void RegisterMockTypeWithName_ShouldWork()
        {
            var name = "nm";
            _mocknity.RegisterPartialMockType<FirstObjectImpl>(name);
            var obj1 = _ioc.Resolve<FirstObjectImpl>(name);
            CheckObjectIsPartialMock(obj1);
            var obj2 = _ioc.Resolve<FirstObjectImpl>(name);
            CheckObjectIsPartialMock(obj2);
            Assert.AreNotEqual(obj1, obj2);
        }

        [TestMethod]
        public void Constructor_Parameters_OverridingTest()
        {
            _ioc.RegisterType<IFirstObject, FirstObjectImpl>();
            _mocknity.RegisterDynamicMock<ISecondObject>();
            var id = Guid.NewGuid();
            _mocknity.RegisterPartialMock<MyClass>(new InjectionParameter<Guid>(id));

            var mock = _ioc.Resolve<MyClass>();
            Assert.AreEqual(id, mock.Id1);
            Assert.AreEqual(id, mock.Id2);
            Assert.IsNotNull(mock.FirstObj);
            Assert.IsNotNull(mock.SecondObj);
        }

        [TestMethod]
        public void Constructor_Overriding_SimilarType_Params_Test()
        {
            _ioc.RegisterType<IFirstObject, FirstObjectImpl>();
            _mocknity.RegisterDynamicMock<ISecondObject>();
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            _mocknity.RegisterPartialMock<MyClass>(new InjectionParameter<Guid>(id1), new InjectionParameter<Guid>(id2));

            var mock = _ioc.Resolve<MyClass>();
            Assert.AreEqual(id1, mock.Id1);
            Assert.AreEqual(id2, mock.Id2);
            Assert.IsNotNull(mock.FirstObj);
            Assert.IsNotNull(mock.SecondObj);
        }

        [TestMethod]
        public void Constructor_Named_Parameters_OverridingTest()
        {
            _ioc.RegisterType<IFirstObject, FirstObjectImpl>();
            _ioc.RegisterType<IFirstObject, FirstObjectImpl3>("FirstObjectImpl3");
            _mocknity.RegisterDynamicMock<ISecondObject>();
            var id = Guid.NewGuid();
            _mocknity.RegisterPartialMock<MyClass>(
                new ResolvedParameter<IFirstObject>("FirstObjectImpl3"), new InjectionParameter<Guid>(id));

            var mock = _ioc.Resolve<MyClass>();
            Assert.AreEqual(id, mock.Id1);
            Assert.IsInstanceOfType(mock.FirstObj, typeof(FirstObjectImpl3));
            Assert.IsNotNull(mock.SecondObj);
        }

        [TestMethod]
        public void Resolve_Mock_Array_Test()
        {
            _ioc.RegisterType<IFirstObject, FirstObjectImpl>();
            _ioc.RegisterType<IFirstObject, FirstObjectImpl3>("FirstObjectImpl3");
            _mocknity.RegisterPartialMockType<ObjectWithPropDependency>("one");
            _mocknity.RegisterPartialMock<ObjectWithPropDependency>("two");
            _mocknity.RegisterPartialMockType<ObjectWithPropDependency>("free");
            IEnumerable<ObjectWithPropDependency> mocks = _ioc.ResolveAll<ObjectWithPropDependency>();
            Assert.AreEqual(3, mocks.Count());
        }

        [TestMethod]
        public void AfterType_WasResolved_FromParentUnity_MocksClassRegistration_ShouldWork_InChildUnity()
        {
            AfterType_WasResolved_FromParentUnity_MocksRegistration_ShouldWork_InChildUnity<FirstObjectImpl>();
        }

        [TestMethod]
        public void AfterType_WasResolved_FromParentUnity_MocksInterfaceRegistration_ShouldWork_InChildUnity()
        {
            AfterType_WasResolved_FromParentUnity_MocksRegistration_ShouldWork_InChildUnity<IFirstObject>();
        }

        public void AfterType_WasResolved_FromParentUnity_MocksRegistration_ShouldWork_InChildUnity<T>() where T : class, IFirstObject
        {
            _ioc.RegisterType<EmptyType>();
            var objImpl = _ioc.Resolve<EmptyType>();
            var mocknity = CreateMocknityLocatedInNewChildContainer();
            IUnityContainer childContainer = mocknity.Container;

            mocknity.RegisterDynamicMock<T>();
            var obj = childContainer.Resolve<T>(); //ioc returns fake type
            obj.Stub(x => x.IntroduceYourself()).Return("t");
            obj.Replay();
            Assert.AreEqual("t", obj.IntroduceYourself());

        }

        #region Resolve Order Tests
        /// <summary>
        /// Correct resolve order:
        /// TypeToFind ==> childIoc =NotFound=> childMocknity =NotFound=> rootIoc =NotFound=> rootMocknity =NotFound=> Resolve Error
        /// </summary>
        [TestMethod]
        public void First_ShouldResolveType_From_ChildIoc()
        {
            IUnityContainer rootContainer = _ioc;
            MocknityContainerExtension rootMocknity = _mocknity;
            MocknityContainerExtension childMocknity = CreateMocknityLocatedInNewChildContainer();
            IUnityContainer childContainer = childMocknity.Container;

            childContainer.RegisterType<IFirstObject, FirstObjectImpl>(); //only checked container contains real object
            childMocknity.RegisterDynamicMock<IFirstObject>();
            rootMocknity.RegisterDynamicMock<IFirstObject>();
            var mock = rootContainer.Resolve<IFirstObject>();
            rootContainer.RegisterInstance(mock);

            var obj = childContainer.Resolve<IFirstObject>();
            CheckObjectIsReal(obj);
        }

        [TestMethod]
        public void Second_ShouldResolveType_From_ChildMocknity()
        {
            IUnityContainer rootContainer = _ioc;
            MocknityContainerExtension rootMocknity = _mocknity;
            MocknityContainerExtension childMocknity = CreateMocknityLocatedInNewChildContainer();
            IUnityContainer childContainer = childMocknity.Container;

            childMocknity.RegisterPartialMock<IFirstObject, FirstObjectImpl>(); //only that one contains partial mock
            rootMocknity.RegisterDynamicMock<IFirstObject>();
            rootContainer.RegisterInstance<IFirstObject>(new FirstObjectImpl());

            var obj = childContainer.Resolve<IFirstObject>();
            CheckObjectIsPartialMock(obj);
        }

        [TestMethod]
        public void Third_ShouldResolveType_From_RootContainer()
        {
            IUnityContainer rootContainer = _ioc;
            MocknityContainerExtension rootMocknity = _mocknity;
            MocknityContainerExtension childMocknity = CreateMocknityLocatedInNewChildContainer();
            IUnityContainer childContainer = childMocknity.Container;

            rootMocknity.RegisterDynamicMock<IFirstObject>();
            rootContainer.RegisterType<IFirstObject, FirstObjectImpl>();

            var obj = childContainer.Resolve<IFirstObject>();
            CheckObjectIsReal(obj);
        }

        [TestMethod]
        public void Fourth_ShouldResolveType_From_RootMocknity()
        {
            IUnityContainer rootContainer = _ioc;
            MocknityContainerExtension rootMocknity = _mocknity;
            MocknityContainerExtension childMocknity = CreateMocknityLocatedInNewChildContainer();
            IUnityContainer childContainer = childMocknity.Container;

            rootMocknity.RegisterPartialMock<IFirstObject, FirstObjectImpl>();
            var obj = childContainer.Resolve<IFirstObject>();
            CheckObjectIsPartialMock(obj);
        }


        #endregion

        #region Unity Specifics

        [TestMethod]
        public void Unity_Standard_Registrations_Specific_Test()
        {
            IUnityContainer rootContainer = new UnityContainer();
            IUnityContainer childContainer = rootContainer.CreateChildContainer();

            rootContainer.RegisterType<IFirstObject, FirstObjectImpl>();
            Assert.AreEqual(2, childContainer.Registrations.Count());
            rootContainer.RegisterType<EmptyType>();
            Assert.AreEqual(3, childContainer.Registrations.Count());
            //rootContainer.RegisterType<EmptyType>();
            //rootContainer.RegisterType<FirstObjectImpl>();
            //childContainer.RegisterInstance<IFirstObject>(_mocks.DynamicMock<IFirstObject>());

            //var obj = childContainer.Resolve<IFirstObject>();
            //CheckObjectIsMock(obj);
        }


        [TestMethod, Description("Added to unity property PrivateRegistrations test")]
        public void Unity_PrivateRegistrations_Specific_Test()
        {
            IUnityContainer rootContainer = new UnityContainer();
            IUnityContainer childContainer = rootContainer.CreateChildContainer();

            rootContainer.RegisterType<IFirstObject, FirstObjectImpl>();
            Assert.AreEqual(1, childContainer.PrivateRegistrations.Count());

            rootContainer.RegisterType<EmptyType>();
            Assert.AreEqual(1, childContainer.PrivateRegistrations.Count());
        }

        [TestMethod, Description("Added to unity method ClearCache")]
        public void AfterType_WasResolved_FROMUnity_MocksRegistration_Works()
        {
            _ioc.RegisterType<EmptyType>();
            var objImpl = _ioc.Resolve<EmptyType>();
            _mocknity.RegisterPartialMock<FirstObjectImpl>();
            var obj = _ioc.Resolve<FirstObjectImpl>(); //ioc returns real type!!!
            obj.Stub(x => x.IntroduceYourself()).Return("t");//exception occured
        }

        #endregion

        private MocknityContainerExtension CreateMocknityLocatedInNewChildContainer()
        {
            IUnityContainer childContainer = _ioc.CreateChildContainer();
            var mocks = new MockRepository();
            var mocknity = new MocknityContainerExtension(mocks, false);
            childContainer.AddExtension(mocknity);
            return mocknity;
        }

        private void CheckObjectIsReal(IFirstObject obj)
        {
            MockObjectCheck(obj, false);
        }

        private void CheckObjectIsMock(IFirstObject obj)
        {
            MockObjectCheck(obj, true);
        }

        private void MockObjectCheck(IFirstObject obj, bool expectedMock)
        {
            bool isMocked = obj is IMockedObject;
            string errMessage;
            if (expectedMock)
            {
                errMessage = "Expected mock object but it was not";
            }
            else
            {
                errMessage = "Expected real object but it was not";
            }
            Assert.AreEqual(expectedMock, isMocked, errMessage);
        }


        private void CheckObjectIsPartialMock(IFirstObject obj)
        {
            try
            {
                obj.Replay();
                Assert.AreEqual(obj.GetType().Name, obj.IntroduceYourself());
                obj.Stub(x => x.IntroduceYourself()).Return("t");
                obj.Replay();
                Assert.AreEqual("t", obj.IntroduceYourself());
            }
            catch (Exception)
            {
                string errMessage;
                bool isMocked = obj is IMockedObject;
                if (isMocked)
                {
                    errMessage = "dynamic or partial mock!";
                }
                else
                {
                    errMessage = "real type!";
                }
                Assert.Fail("Expected partaial mock, but was {0}", errMessage);
            }
        }
    }
}