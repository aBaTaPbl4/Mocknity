﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using Mocknity;
using Mocknity.Strategies.Rhino;
using NUnit.Framework;
using Rhino.Mocks;
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

    public class SecondObjectImpl : ISecondObject
    {
        public int MyProperty { get; set; }
        public string HelloWorld()
        {
            throw new NotImplementedException();
        }
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


    public class ObjDependsOnIFirstObj
    {
        public IFirstObject Obj { get; }

        public ObjDependsOnIFirstObj(IFirstObject obj)
        {
            Obj = obj;
        }
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

    public interface IObjectWithPropDependency
    {
        int SetWasCalledCount { get; }
    }

    public class ObjectWithPropDependency : IObjectWithPropDependency
    {
        private int _setWasCalledCount = 0;
        private IFirstObject _obj;

        public int SetWasCalledCount
        {
            get { return _setWasCalledCount; }
        }

        [Dependency]
        public IFirstObject Obj
        {
            get { return _obj; }
            set
            {
                _obj = value;
                _setWasCalledCount++;
            }
        }

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

    public interface IClassWithInjectionMethod
    {
        bool IsInjectionMethodCalled { get; }
    }

    public class ClassWithInjectionMethod : IClassWithInjectionMethod
    {
        private bool _isInjectionMethodCalled;

        public ClassWithInjectionMethod()
        {
            _isInjectionMethodCalled = false;
        }

        [InjectionMethod]
        public void Init()
        {
            _isInjectionMethodCalled = true;
        }

        public bool IsInjectionMethodCalled
        {
            get { return _isInjectionMethodCalled; }
        }
    }

    public class BallPoco
    {
        public string Type { get; set; }
        public int SizeNumber { get; set; }
    }

    public class Football
    {
        public Football(BallPoco ball)
        {
            Ball = ball;
        }

        public BallPoco Ball { get; private set; }
        

    }

    #endregion

    [TestFixture]
    public class MocknityTests
    {
        private IUnityContainer _ioc;
        private MocknityContainerExtension _mocknity;
        private MockRepository _mocks;

        [SetUp]
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

        [TearDown]
        public void TestCleanup()
        {
            _ioc.Dispose();
        }

        [Test]
        public void ResolveObject__MockDependenciesInitiated()
        {
            var od = _ioc.Resolve<ObjectWithDependencies>();

            Assert.IsNotNull(od.firstObject);
            Assert.IsNotNull(od.secondObject);
        }

        [Test]
        public void ResolveObjectWithSpecificImpl__OneDependencyIsNotMocked()
        {
            _ioc.RegisterType<IFirstObject, FirstObjectImpl>();
            var od = _ioc.Resolve<ObjectWithDependencies>();
            
            Assert.IsInstanceOf(typeof (FirstObjectImpl), od.firstObject);
            Assert.IsNotNull(od.secondObject);
        }

        [Test]
        public void ResolveObjectWithExpectedCall__GetDesiredResult()
        {            
            var od = _ioc.Resolve<ObjectWithDependencies>();
            using (_mocks.Record())
            {
                Expect.Call(od.secondObject.HelloWorld()).Return("I'm the first of none");    
            }
            Assert.AreEqual("I'm the first of none", od.PokeSecond());
            _mocks.VerifyAll();
        }

        [Test]
        public void SecondObjectIsAStrictMock__ExceptionThrownOnCallingNotExpectedMethod()
        {
            Assert.Throws<ExpectationViolationException>(() =>
            {
                _mocknity.SetStrategy<StrictRhinoMocksBuilderStrategy>(typeof(ISecondObject));
                var od = _ioc.Resolve<ObjectWithDependencies>();

                _mocknity.getRepository().ReplayAll();
                od.secondObject.HelloWorld();
                _mocknity.getRepository().VerifyAll();
            });

        }

        [Test]
        public void SecondObjectIsAStub__CanChangeProperties()
        {
            _mocknity.SetStrategy<StubRhinoMocksBuilderStrategy>(typeof (ISecondObject));
            var od = _ioc.Resolve<ObjectWithDependencies>();

            _mocknity.getRepository().ReplayAll();
            od.secondObject.MyProperty = 42;
            Assert.AreEqual(42, od.secondObject.MyProperty);
            _mocknity.getRepository().VerifyAll();
        }

        [Test]
        public void SecondObjectIsADynamicCall__AcceptsUnexpectedCall()
        {
            // dynamic mocking is a default strategy in mocknity
            var od = _ioc.Resolve<ObjectWithDependencies>();

            _mocknity.getRepository().ReplayAll();
            // doesn't raise exceptions
            od.secondObject.HelloWorld();
            _mocknity.getRepository().VerifyAll();
        }

        [Test]
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

        [Test]
        public void IfMockingUnregegisteredInterfacesIs_ON_Work_If_Interfaces_Unregistered()
        {
            _mocknity.RegisterStub<ISecondObject>();
            _mocknity.RegisterStub<IThirdObject>();
            var obj = _ioc.Resolve<ObjectWithDependencies2>();
        }

        [Test]
        public void IfMockingUnregegisteredInterfacesIs_OFF_NOT_Work_If_Interfaces_Unregistered()
        {
            Assert.Throws<ResolutionFailedException>(() =>
            {
                InitPrivateMembers(false);
                _mocknity.RegisterStub<ISecondObject>();
                _mocknity.RegisterStub<IThirdObject>();
                var obj = _ioc.Resolve<ObjectWithDependencies2>();
            });
        }

        [Test]
        public void IfMockingUnregegisteredInterfacesIs_OFF_Work_If_Interfaces_Registered()
        {
            InitPrivateMembers(false);
            _mocknity.RegisterStub<IFirstObject>();
            _mocknity.RegisterStub<ISecondObject>();
            var obj = _ioc.Resolve<ObjectWithDependencies>();
            Assert.IsNotNull(obj);
        }

        [Test]
        public void IfMockingUnregegisteredInterfacesIs_OFF_Work_If_Class_Registered()
        {
            InitPrivateMembers(false);
            _mocknity.RegisterPartialMock<FirstObjectImpl>();
            var obj = _ioc.Resolve<FirstObjectImpl>();
            CheckObjectIsPartialMock(obj);
        }

        [Test]
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

        [Test]
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

        [Test]
        public void ResolveObjectThanWasNotRegistered_ShouldWorks_WhenHaveDependcies()
        {
            _mocknity.RegisterDynamicMock<IFirstObject>();
            _mocknity.RegisterDynamicMock<ISecondObject>();
            var obj = _ioc.Resolve<ObjectWithDependencies>();
            Assert.IsNotNull(obj);
        }

        [Test]
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

        [Test]
        public void ResolvePartialMock_Works_WhenRequestingBaseType()
        {
            _mocknity.RegisterPartialMock<IObjectWithDependencies, ObjectWithDependencies>();
            var obj = _ioc.Resolve<IObjectWithDependencies>();
            Assert.IsNotNull(obj);
        }

        [Test]
        public void ResolvePartialMock_IfCallTwiceForInterface__MocksShouldBeSame()
        {
            _mocknity.RegisterPartialMock<IObjectWithDependencies, ObjectWithDependencies>();

            var obj1 = _ioc.Resolve<IObjectWithDependencies>();
            var obj2 = _ioc.Resolve<IObjectWithDependencies>();
            Assert.AreEqual(obj1, obj2);
        }

        [Test]
        public void ResolvePartialMock_IfCallTwiceForInterfaceAndImpl__MocksShouldBeSame()
        {
            _mocknity.RegisterPartialMock<IObjectWithDependencies, ObjectWithDependencies>();

            var obj1 = _ioc.Resolve<IObjectWithDependencies>();
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreEqual(obj1, obj2);
        }

        [Test]
        public void ResolvePartialMock_IfCallTwiceForInterfaceAndImpl_ReverseOrder__MocksShouldBeSame()
        {
            _mocknity.RegisterPartialMock<IObjectWithDependencies, ObjectWithDependencies>();
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            var obj1 = _ioc.Resolve<IObjectWithDependencies>();
            Assert.AreEqual(obj1, obj2);
        }

        [Test]
        public void ResolvePartialMock_IfCallTwiceForImpl__MocksShouldBeSame()
        {
            _mocknity.RegisterPartialMock<IObjectWithDependencies, ObjectWithDependencies>();

            var obj1 = _ioc.Resolve<ObjectWithDependencies>();
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreEqual(obj1, obj2);
        }

        //DYNAMIC
        [Test]
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

        [Test]
        public void ResolveDynamicMock_Works_WhenRequestingBaseType()
        {
            _mocknity.RegisterDynamicMock<IObjectWithDependencies, ObjectWithDependencies>();
            var obj = _ioc.Resolve<IObjectWithDependencies>();
            Assert.IsNotNull(obj);
        }

        [Test]
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
        [Test]
        public void ResolveStrictMock_Works_WhenHaveDependcies()
        {
            Assert.Throws<ExpectationViolationException>(() =>
            {
                _mocknity.RegisterStrictMock<IFirstObject>();
                _mocknity.RegisterStrictMock<ISecondObject>();
                _mocknity.RegisterStrictMock<ObjectWithDependencies>();
                var obj = _ioc.Resolve<ObjectWithDependencies>();
                _mocks.ReplayAll();
                string result = obj.Test();
            });
        }

        [Test]
        public void ResolveStrictMock_Works_WhenRequestingBaseType()
        {
            _mocknity.RegisterStrictMock<IObjectWithDependencies, ObjectWithDependencies>();
            var obj = _ioc.Resolve<IObjectWithDependencies>();
            Assert.IsNotNull(obj);
        }

        [Test]
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

        [Test]
        public void AutoReplay_If_True__Test()
        {
            _mocknity.RegisterPartialMock<ObjectWithDependencies>();
            var obj = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreEqual("foo", obj.Foo());
        }

        [Test]
        public void AutoReplay_If_False__Test()
        {
            _mocknity.AutoReplayPartialMocks = false;
            _mocknity.RegisterPartialMock<ObjectWithDependencies>();
            var obj = _ioc.Resolve<ObjectWithDependencies>();
            Assert.IsNull(obj.Foo());
        }

        [Test]
        public void RegisterPartialMockType_ImplVSImpl__Test()
        {
            _mocknity.RegisterPartialMockType<ObjectWithDependencies>();
            var obj1 = _ioc.Resolve<ObjectWithDependencies>();
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);  
        }

        [Test]
        public void RegisterPartialMockType_ImplVSInterface__Test()
        {
            _mocknity.RegisterPartialMockType<IObjectWithDependencies, ObjectWithDependencies>();
            var obj1 = _ioc.Resolve<IObjectWithDependencies>();
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [Test]
        public void RegisterStubType_ImplVSImpl__Test()
        {
            _mocknity.RegisterStubType<ObjectWithDependencies>();
            var obj1 = _ioc.Resolve<ObjectWithDependencies>();
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [Test]
        public void RegisterStubType_ImplVSInterface__Test()
        {
            _mocknity.RegisterStubType<IObjectWithDependencies, ObjectWithDependencies>();
            var obj1 = _ioc.Resolve<IObjectWithDependencies>();
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [Test]
        public void RegisterDynamicMockType_ImplVSImpl__Test()
        {
            _mocknity.RegisterDynamicMockType<ObjectWithDependencies>();
            var obj1 = _ioc.Resolve<ObjectWithDependencies>();
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [Test]
        public void RegisterDynamicMockType_ImplVSInterface__Test()
        {
            _mocknity.RegisterDynamicMockType<IObjectWithDependencies, ObjectWithDependencies>();
            var obj1 = _ioc.Resolve<IObjectWithDependencies>();
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [Test]
        public void RegisterStrictMockType_ImplVSImpl__Test()
        {
            _mocknity.RegisterStrictMockType<ObjectWithDependencies>();
            var obj1 = _ioc.Resolve<ObjectWithDependencies>();
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [Test]
        public void RegisterStrictMockType_ImplVSInterface__Test()
        {
            _mocknity.RegisterStrictMockType<IObjectWithDependencies, ObjectWithDependencies>();
            var obj1 = _ioc.Resolve<IObjectWithDependencies>();
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [Test]
        public void RegisterPartialNamedMock_NamedAndDefaultInstances__AreDifferent()
        {
            _mocknity.RegisterPartialMock<ObjectWithDependencies>();
            _mocknity.RegisterPartialMock<ObjectWithDependencies>("test");
            var obj1 = _ioc.Resolve<ObjectWithDependencies>("test");
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [Test]
        public void RegisterPartialNamedMock_DefaultResolveShouldFailed__Test()
        {
            Assert.Throws<ResolutionFailedException>(() =>
            {
                _mocknity.MockUnregisteredInterfaces = false;
                //object will be created by unity, but fail will occured on dependency resolve;
                _mocknity.RegisterPartialMock<ObjectWithDependencies>("test");
                _ioc.Resolve<ObjectWithDependencies>();
            });
        }

        [Test]
        public void RegisterDynamicNamedMock_NamedAndDefaultInstances__AreDifferent()
        {
            _mocknity.RegisterDynamicMock<ObjectWithDependencies>();
            _mocknity.RegisterDynamicMock<ObjectWithDependencies>("test");
            var obj1 = _ioc.Resolve<ObjectWithDependencies>("test");
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [Test]
        public void RegisterDynamicNamedMock_DefaultResolveShouldFailed__Test()
        {
            Assert.Throws<ResolutionFailedException>(() =>
            {
                _mocknity.MockUnregisteredInterfaces = false;
                //object will be created by unity, but fail will occured on dependency resolve;
                _mocknity.RegisterDynamicMock<ObjectWithDependencies>("test");
                _ioc.Resolve<ObjectWithDependencies>();
            });

        }

        [Test]
        public void RegisterStrictNamedMock_NamedAndDefaultInstances__AreDifferent()
        {
            _mocknity.RegisterStrictMock<ObjectWithDependencies>();
            _mocknity.RegisterStrictMock<ObjectWithDependencies>("test");
            var obj1 = _ioc.Resolve<ObjectWithDependencies>("test");
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [Test]
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

        [Test]
        public void RegisterStrictNamedMock_DefaultResolveShouldFailed__Test()
        {
            Assert.Throws<ResolutionFailedException>(() =>
            {
                _mocknity.MockUnregisteredInterfaces = false;
                //object will be created by unity, but fail will occured on dependency resolve;
                _mocknity.RegisterStrictMock<ObjectWithDependencies>("test");
                _ioc.Resolve<ObjectWithDependencies>();
            });
        }

        [Test]
        public void RegisterNamedStub_NamedAndDefaultInstances__AreDifferent()
        {
            _mocknity.RegisterStub<ObjectWithDependencies>();
            _mocknity.RegisterStub<ObjectWithDependencies>("test");
            var obj1 = _ioc.Resolve<ObjectWithDependencies>("test");
            var obj2 = _ioc.Resolve<ObjectWithDependencies>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [Test]
        public void RegisterNamedStub_DefaultResolveShouldFailed__Test()
        {
            Assert.Throws<ResolutionFailedException>(() =>
            {
                _mocknity.MockUnregisteredInterfaces = false;
                //object will be created by unity, but fail will occured on dependency resolve;
                _mocknity.RegisterStub<ObjectWithDependencies>("test");
                _ioc.Resolve<ObjectWithDependencies>();
            });
        }

        [Test]
        public void DependencyProperties_ShouldBeInitialized_ByMockResolve()
        {
            _ioc.RegisterType<IFirstObject, FirstObjectImpl>();
            _ioc.RegisterType<IFirstObject, FirstObjectImpl3>("FirstObjectImpl3");
            _mocknity.RegisterPartialMock<ObjectWithPropDependency>();
            var mock = _ioc.Resolve<ObjectWithPropDependency>();
            Assert.IsNotNull(mock.Obj);
        }

        [Test]
        public void DependencyProperties_ShouldBeInitializedOnes_ByMockResolve()
        {
            _ioc.RegisterType<IFirstObject, FirstObjectImpl>();
            _ioc.RegisterType<IFirstObject, FirstObjectImpl3>("FirstObjectImpl3");
            _mocknity.RegisterPartialMock<ObjectWithPropDependency>();
            var mock = _ioc.Resolve<ObjectWithPropDependency>();
            Assert.AreEqual(1, mock.SetWasCalledCount, "Property was init {0} times!", mock.SetWasCalledCount);
        }

        [Test]
        public void DependencyProperties_ShouldBeInitializedOnes_ByMockInterfaceResolve_WhenMockUnregisteredInterfacesOff()
        {
            _mocknity.MockUnregisteredInterfaces = false;
            DependencyProperties_ShouldBeInitializedOnes_ByMockInterfaceResolve();
        }

        [Test]
        public void DependencyProperties_ShouldBeInitializedOnes_ByMockInterfaceResolve_WhenMockUnregisteredInterfacesOn()
        {
            _mocknity.MockUnregisteredInterfaces = true;
            DependencyProperties_ShouldBeInitializedOnes_ByMockInterfaceResolve();
        }

        public void DependencyProperties_ShouldBeInitializedOnes_ByMockInterfaceResolve()
        {
            _ioc.RegisterType<IFirstObject, FirstObjectImpl>();
            _ioc.RegisterType<IFirstObject, FirstObjectImpl3>("FirstObjectImpl3");
            _mocknity.RegisterPartialMock<IObjectWithPropDependency, ObjectWithPropDependency>();
            var mock = _ioc.Resolve<IObjectWithPropDependency>();
            Assert.AreEqual(1, mock.SetWasCalledCount, "Property was called wrong times count!");            
        }

        [Test]
        public void NamedDependencyProperties_ShouldBeInitialized_ByMockResolve()
        {
            _ioc.RegisterType<IFirstObject, FirstObjectImpl>();
            _ioc.RegisterType<IFirstObject, FirstObjectImpl3>("FirstObjectImpl3");         
            _mocknity.RegisterPartialMock<ObjectWithPropDependency>();

            var mock = _ioc.Resolve<ObjectWithPropDependency>();
            Assert.IsInstanceOf(typeof(FirstObjectImpl3), mock.ObjNamed);
        }

        [Test]
        public void RegisterMockTypeWithName_WhenDefaultInstnceIsDefined_ShouldFail()
        {
            _ioc.RegisterType<FirstObjectImpl>();
            var name = "nm";
            _mocknity.RegisterPartialMockType<FirstObjectImpl>(name);
            var obj = _ioc.Resolve<FirstObjectImpl>(name);
            CheckObjectIsReal(obj);
        }

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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
            Assert.IsInstanceOf(typeof(FirstObjectImpl3), mock.FirstObj);
            Assert.IsNotNull(mock.SecondObj);
        }

        [Test]
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

        [Test]
        public void AfterType_WasResolved_FromParentUnity_MocksClassRegistration_ShouldWork_InChildUnity()
        {
            AfterType_WasResolved_FromParentUnity_MocksRegistration_ShouldWork_InChildUnity<FirstObjectImpl>();
        }

        [Test]
        public void AfterType_WasResolved_FromParentUnity_MocksInterfaceRegistration_ShouldWork_InChildUnity()
        {
            AfterType_WasResolved_FromParentUnity_MocksRegistration_ShouldWork_InChildUnity<IFirstObject>();
        }

        public void AfterType_WasResolved_FromParentUnity_MocksRegistration_ShouldWork_InChildUnity<T>() where T : class, IFirstObject
        {
            _ioc.RegisterType<EmptyType>();
            var objImpl = _ioc.Resolve<EmptyType>();
            MocknityContainerExtension childMocknity;
            IUnityContainer childContainer;
            InitChildContainer(out childContainer, out childMocknity);             

            childMocknity.RegisterDynamicMock<T>();
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
        [Test]
        public void First_ShouldResolveType_From_ChildIoc()
        {
            IUnityContainer rootContainer = _ioc;
            MocknityContainerExtension rootMocknity = _mocknity;
            MocknityContainerExtension childMocknity;
            IUnityContainer childContainer;
            InitChildContainer(out childContainer, out childMocknity);             

            childContainer.RegisterType<IFirstObject, FirstObjectImpl>(); //only checked container contains real object
            childMocknity.RegisterDynamicMock<IFirstObject>();
            rootMocknity.RegisterDynamicMock<IFirstObject>();
            var mock = rootContainer.Resolve<IFirstObject>();
            rootContainer.RegisterInstance(mock);

            var obj = childContainer.Resolve<IFirstObject>();
            CheckObjectIsReal(obj);
        }

        [Test]
        public void Second_ShouldResolveType_From_ChildMocknity()
        {
            IUnityContainer rootContainer = _ioc;
            MocknityContainerExtension rootMocknity = _mocknity;
            MocknityContainerExtension childMocknity;
            IUnityContainer childContainer;
            InitChildContainer(out childContainer, out childMocknity);             

            childMocknity.RegisterPartialMock<IFirstObject, FirstObjectImpl>(); //only that one contains partial mock
            rootMocknity.RegisterDynamicMock<IFirstObject>();
            rootContainer.RegisterInstance<IFirstObject>(new FirstObjectImpl());

            var obj = childContainer.Resolve<IFirstObject>();
            CheckObjectIsPartialMock(obj);
        }

        [Test, Description("https://github.com/aBaTaPbl4/Mocknity/issues/6")]
        public void Test_By_Issue_6()
        {
            IUnityContainer rootContainer = _ioc;
            MocknityContainerExtension rootMocknity = _mocknity;
            MocknityContainerExtension childMocknity;
            IUnityContainer childContainer;
            InitChildContainer(out childContainer, out childMocknity);

            rootContainer.RegisterType<IFirstObject, FirstObjectImpl>();
            childMocknity.RegisterDynamicMock<IFirstObject>();
            var dependant = childContainer.Resolve<ObjDependsOnIFirstObj>(); 
            CheckObjectIsMock(dependant.Obj);
        }

        [Test]
        public void Third_ShouldResolveType_From_RootContainer()
        {
            IUnityContainer rootContainer = _ioc;
            MocknityContainerExtension rootMocknity = _mocknity;
            MocknityContainerExtension childMocknity;
            IUnityContainer childContainer;
            InitChildContainer(out childContainer, out childMocknity);             

            rootMocknity.RegisterDynamicMock<IFirstObject>();
            rootContainer.RegisterType<IFirstObject, FirstObjectImpl>();

            var obj = childContainer.Resolve<IFirstObject>();
            CheckObjectIsReal(obj);
        }

        [Test]
        public void Fourth_ShouldResolveType_From_RootMocknity()
        {
            IUnityContainer rootContainer = _ioc;
            MocknityContainerExtension rootMocknity = _mocknity;
            MocknityContainerExtension childMocknity;
            IUnityContainer childContainer;
            InitChildContainer(out childContainer, out childMocknity);             

            rootMocknity.RegisterPartialMock<IFirstObject, FirstObjectImpl>();
            var obj = childContainer.Resolve<IFirstObject>();
            CheckObjectIsPartialMock(obj);
        }

        //ParentIoc(-) ----> ParentMocknity(+)
        //    |
        //ChildIoc(+) -----> ChildMocknity(-)
        [Test]
        public void If_TypeRegisetered_In_RootMocknity_And_In_Child_Ioc_ShouldReturnType_FromIoc()
        {
            IUnityContainer rootContainer = _ioc;
            MocknityContainerExtension rootMocknity = _mocknity;
            MocknityContainerExtension childMocknity;
            IUnityContainer childContainer;
            InitChildContainer(out childContainer, out childMocknity);

            rootMocknity.RegisterPartialMock<IFirstObject, FirstObjectImpl>();
            childContainer.RegisterType<IFirstObject, FirstObjectImpl>();
            var obj = childContainer.Resolve<IFirstObject>();
            CheckObjectIsReal(obj);
        }

        //ParentIoc(-) ----> ParentMocknity(+)
        //    |
        //ChildIoc1(+) -----> ChildMocknity1(-)
        //
        //ChildIoc2(-) -----> ChildMocknity2(-)
        [Test]
        public void If_TypeRegisetered_In_RootMocknity_And_In_Middle_Child_Ioc_ShouldReturnType_FromMiddleIoc()
        {
            IUnityContainer rootContainer = _ioc;
            MocknityContainerExtension rootMocknity = _mocknity;
            MocknityContainerExtension childMocknity1;
            IUnityContainer childContainer1;
            InitChildContainer(out childContainer1, out childMocknity1);

            MocknityContainerExtension childMocknity2;
            IUnityContainer childContainer2;
            InitChildContainer(out childContainer2, out childMocknity2, childContainer1);

            rootMocknity.RegisterPartialMock<IFirstObject, FirstObjectImpl>();
            childContainer1.RegisterType<IFirstObject, FirstObjectImpl>();
            var obj = childContainer2.Resolve<IFirstObject>();
            CheckObjectIsReal(obj);
        }

        //ParentIoc(+) ----> ParentMocknity(-)
        //    |
        //ChildIoc(-) -----> ChildMocknity(+)
        [Test]
        public void If_TypeRegisetered_In_ChildMocknity_And_In_RootIoc_ShouldReturnType_FromMocknity()
        {
            IUnityContainer rootContainer = _ioc;
            MocknityContainerExtension rootMocknity = _mocknity;
            MocknityContainerExtension childMocknity;
            IUnityContainer childContainer;
            InitChildContainer(out childContainer, out childMocknity);

            rootContainer.RegisterType<IFirstObject, FirstObjectImpl>();
            childMocknity.RegisterPartialMock<IFirstObject, FirstObjectImpl>();
            var obj = childContainer.Resolve<IFirstObject>();
            CheckObjectIsPartialMock(obj);
        }

        //ParentIoc(-) ----> ParentMocknity(PartialMock)
        //    |
        //ChildIoc(-) -----> ChildMocknity(DynamicMock)
        [Test]
        public void PartialMockIn_TopMocknity_ShouldNotBrokeResolve_DynamicMock_FromLowLevlelMocknity()
        {
            IUnityContainer rootContainer = _ioc;
            MocknityContainerExtension rootMocknity = _mocknity;
            MocknityContainerExtension childMocknity;
            IUnityContainer childContainer;
            InitChildContainer(out childContainer, out childMocknity);

            rootMocknity.RegisterPartialMock<IFirstObject, FirstObjectImpl>();
            childMocknity.RegisterDynamicMock<IFirstObject>();

            var obj = childContainer.Resolve<IFirstObject>();
            Assert.IsNotNull(obj);
            Assert.IsNull(obj.IntroduceYourself()); //is dynamic mock
        }

        [Test]
        public void AllowTwoRegistration_InDifferentExtensions_Test()
        {
            IUnityContainer rootContainer = _ioc;
            MocknityContainerExtension rootMocknity = _mocknity;
            MocknityContainerExtension childMocknity;
            IUnityContainer childContainer;
            InitChildContainer(out childContainer, out childMocknity);             

            rootMocknity.RegisterPartialMock<IFirstObject, FirstObjectImpl>();
            var obj1 = rootContainer.Resolve<IFirstObject>();
            childMocknity.RegisterPartialMock<IFirstObject, FirstObjectImpl2>();
            var obj2 = childContainer.Resolve<IFirstObject>();
            Assert.AreNotEqual(obj1, obj2);
        }

        [Test]
        public void AllowFreeRegistration_InDifferentExtensions_Test()
        {
            IUnityContainer rootContainer = _ioc;
            MocknityContainerExtension rootMocknity = _mocknity;

            MocknityContainerExtension childMocknity;
            IUnityContainer childContainer;
            InitChildContainer(out childContainer, out childMocknity, rootContainer);

            MocknityContainerExtension childChildMocknity;
            IUnityContainer childChildContainer;
            InitChildContainer(out childChildContainer, out childChildMocknity, childContainer);
            rootMocknity.RegisterPartialMock<IFirstObject, FirstObjectImpl>();
            var obj1 = rootContainer.Resolve<IFirstObject>();
            childMocknity.RegisterPartialMock<IFirstObject, FirstObjectImpl2>();
            var obj2 = childContainer.Resolve<IFirstObject>();
            Assert.AreNotEqual(obj1, obj2);
            childChildMocknity.RegisterPartialMock<IFirstObject, FirstObjectImpl>();
            var obj3 = childChildContainer.Resolve<IFirstObject>();
            Assert.AreNotEqual(obj1, obj3);
            Assert.AreNotEqual(obj2, obj3);
        }

        private void InitChildContainer(out IUnityContainer childContainer, out MocknityContainerExtension mocknity, IUnityContainer parentContainer = null)
        {
            parentContainer = parentContainer ?? _ioc;
            childContainer = parentContainer.CreateChildContainer();
            var mocks = new MockRepository();
            mocknity = new MocknityContainerExtension(mocks, false);
            childContainer.AddExtension(mocknity);
        }


        #endregion

        #region Unity Specifics

        [Test]
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


        [Test, Description("Added to unity property PrivateRegistrations test")]
        public void Unity_PrivateRegistrations_Specific_Test()
        {
            IUnityContainer rootContainer = new UnityContainer();
            IUnityContainer childContainer = rootContainer.CreateChildContainer();

            rootContainer.RegisterType<IFirstObject, FirstObjectImpl>();
            Assert.AreEqual(1, childContainer.PrivateRegistrations.Count());

            rootContainer.RegisterType<EmptyType>();
            Assert.AreEqual(1, childContainer.PrivateRegistrations.Count());
        }

        [Test, Description("Added to unity method ClearCache")]
        public void AfterType_WasResolved_FROMUnity_MocksRegistration_Works()
        {
            _ioc.RegisterType<EmptyType>();
            var objImpl = _ioc.Resolve<EmptyType>();
            _mocknity.RegisterPartialMock<FirstObjectImpl>();
            var obj = _ioc.Resolve<FirstObjectImpl>(); //ioc returns real type!!!
            obj.Stub(x => x.IntroduceYourself()).Return("t");//exception occured
        }

        #endregion

        #region unity befaviour tests

        [Test]
        public void UnityShouldTryResolveDependency_FromChildContainer_First_WhenResolvedTypeNotRegistered()
        {
            IUnityContainer rootContainer = new UnityContainer();
            IUnityContainer childContainer = rootContainer.CreateChildContainer();

            rootContainer.RegisterInstance<IFirstObject>(new FirstObjectImpl());
            rootContainer.RegisterType<ISecondObject, SecondObjectImpl>();
            childContainer.RegisterInstance<IFirstObject>(MockRepository.GenerateMock<IFirstObject>());

            var obj = childContainer.Resolve<ObjectWithDependencies>();

            CheckObjectIsMock(obj.firstObject);//should return mock
        }

        [Test]
        public void UnityShouldTryResolveDependency_FromChildContainer_First_WhenResolvedTypeRegisteredInRootContainer()
        {
            IUnityContainer rootContainer = new UnityContainer();
            IUnityContainer childContainer = rootContainer.CreateChildContainer();

            rootContainer.RegisterInstance<IFirstObject>(new FirstObjectImpl());
            rootContainer.RegisterType<ISecondObject, SecondObjectImpl>();
            childContainer.RegisterInstance<IFirstObject>(MockRepository.GenerateMock<IFirstObject>());

            rootContainer.RegisterType<ObjectWithDependencies>();

            var obj = childContainer.Resolve<ObjectWithDependencies>();

            CheckObjectIsMock(obj.firstObject);//should return mock
        }

        [Test]
        public void UnityShouldTryResolveDependency_FromChildContainer_First_WhenResolvedTypeRegisteredInChildContainer()
        {
            IUnityContainer rootContainer = new UnityContainer();
            IUnityContainer childContainer = rootContainer.CreateChildContainer();

            rootContainer.RegisterInstance<IFirstObject>(new FirstObjectImpl());
            rootContainer.RegisterType<ISecondObject, SecondObjectImpl>();
            childContainer.RegisterInstance<IFirstObject>(MockRepository.GenerateMock<IFirstObject>());

            childContainer.RegisterType<ObjectWithDependencies>();

            var obj = childContainer.Resolve<ObjectWithDependencies>();

            CheckObjectIsMock(obj.firstObject);//should return mock
        }

        #endregion


        [Test]
        public void RegisterPartialMockType_WithDefinedStubBehaviour_Should_Work()
        {
            _mocknity.RegisterPartialMockType<FirstObjectImpl>(x => x.Stub(y => y.IntroduceYourself()).Return("t"));
            var obj = _ioc.Resolve<FirstObjectImpl>();
            Assert.AreEqual("t", obj.IntroduceYourself());
        }

        [Test]
        public void RegisterStrictMockType_WithDefinedStubBehaviour_Should_Work()
        {
            _mocknity.RegisterStrictMockType<IFirstObject>(x => x.Stub(y => y.IntroduceYourself()).Return("t"));
            var obj = _ioc.Resolve<IFirstObject>();
            Assert.AreEqual("t", obj.IntroduceYourself());
        }

        [Test]
        public void RegisterDynamicMockType_WithDefinedStubBehaviour_Should_Work()
        {
            _mocknity.RegisterDynamicMockType<FirstObjectImpl>(x => x.Stub(y => y.IntroduceYourself()).Return("t"));
            var obj = _ioc.Resolve<FirstObjectImpl>();
            Assert.AreEqual("t", obj.IntroduceYourself());
        }

        [Test]
        public void RegisterStubType_WithDefinedStubBehaviour_Should_Work()
        {
            _mocknity.RegisterStubType<FirstObjectImpl>(x => x.Stub(y => y.IntroduceYourself()).Return("t"));
            var obj = _ioc.Resolve<FirstObjectImpl>();
            Assert.AreEqual("t", obj.IntroduceYourself());
        }

        [Test]
        public void RegisterPartialMockType_WithDefinedStubBehaviour_Should_Work_ImplType()
        {
            _mocknity.RegisterPartialMockType<IFirstObject, FirstObjectImpl>(x => x.Stub(y => y.IntroduceYourself()).Return("t"));
            var obj = _ioc.Resolve<FirstObjectImpl>();
            Assert.AreEqual("t", obj.IntroduceYourself());
        }

        [Test]
        public void RegisterPartialMockType_WithDefinedStubBehaviour_Should_Work_WithName()
        {
            _mocknity.RegisterPartialMockType<FirstObjectImpl>("test", x => x.Stub(y => y.IntroduceYourself()).Return("t"));
            var obj = _ioc.Resolve<FirstObjectImpl>("test");
            Assert.AreEqual("t", obj.IntroduceYourself());
        }

        [Test]
        public void WhenOverridenDependencySuplied_MocknityShouldConfigureTheMockByIt()
        {
            
            _mocknity.RegisterPartialMockType<ObjectWithDependencies>();
            _mocknity.RegisterDynamicMock<ISecondObject>();
            _ioc.RegisterType<IFirstObject, FirstObjectImpl>();

            var obj = _ioc.Resolve<ObjectWithDependencies>(new DependencyOverride<IFirstObject>(_mocks.DynamicMock<IFirstObject>()));
            CheckObjectIsMock(obj.firstObject);
            CheckObjectIsMock(obj.secondObject);
        }

        [Test]
        public void WhenOverridenParametersSuplied_MocknityShouldConfigureTheMockByIt_ViaResolvedParameterUsing()
        {
            _ioc.RegisterType<ObjectWithDependencies>(
                new InjectionConstructor(new ResolvedParameter<IFirstObject>(),
                new ResolvedParameter<ISecondObject>()));

            WhenOverridenParametersSuplied_MocknityShouldConfigureTheMockByIt();
        }

        [Test]
        public void WhenOverridenParametersSuplied_MocknityShouldConfigureTheMockByIt_ViaInjectionParameterUsing()
        {
            _ioc.RegisterType<ObjectWithDependencies>(
                new InjectionConstructor(new InjectionParameter<IFirstObject>(null),
                new InjectionParameter<ISecondObject>(null)));

            WhenOverridenParametersSuplied_MocknityShouldConfigureTheMockByIt();
        }

        [Test]
        public void WhenMockRegesteredUnderBaseAndDerivedType_ResolveShouldReturnSameInstances()
        {
            _mocknity.RegisterDynamicMock<IFirstObject>();
            _mocknity.RegisterDynamicMock<IFirstObject, FirstObjectImpl>();
            var mock1 = _ioc.Resolve<IFirstObject>();
            var mock2 = _ioc.Resolve<FirstObjectImpl>();
            Assert.AreNotEqual(mock1, mock2, "Mocks Are The Same! mock1 type:{0}, mock2 type:{1}", 
                mock1.GetType().Name, mock2.GetType().Name);
        }

        [Test]
        public void DuringPartialMockResolving_ByImpType_InjectionMethodShouldBeCalled()
        {
            _mocknity.RegisterPartialMock<ClassWithInjectionMethod>();
            var instance = _ioc.Resolve<ClassWithInjectionMethod>();
            CheckObjectIsMock(instance);
            Assert.IsTrue(instance.IsInjectionMethodCalled, "Injection method was not called!");
        }

        [Test]
        public void DuringPartialMockResolving_ByInteface_InjectionMethodShouldBeCalled()
        {
            _mocknity.RegisterPartialMock<IClassWithInjectionMethod, ClassWithInjectionMethod>();
            var instance = _ioc.Resolve<IClassWithInjectionMethod>();
            CheckObjectIsMock(instance);
            Assert.IsTrue(instance.IsInjectionMethodCalled, "Injection method was not called!");
        }

        [Test]
        public void DuringTypeResolving_ByInteface_InjectionMethodShouldBeCalled()
        {
            _ioc.RegisterType<IClassWithInjectionMethod, ClassWithInjectionMethod>();
            var instance = _ioc.Resolve<IClassWithInjectionMethod>();
            CheckObjectIsReal(instance);
            Assert.IsTrue(instance.IsInjectionMethodCalled, "Injection method was not called!");
        }

        [Test]
        public void param_override_should_work_when_resolving_partial_mock_from_mocknity_Test()
        {
            _mocknity.RegisterPartialMock<Football>();
            TestOverrideDependency("football ball", 4);
            //TestOverrideDependency("volleyball ball", 3);      //it would-not work, because mock create only once      
        }

        private void TestOverrideDependency(string ballType, int ballSize)
        {
            var ball = new BallPoco();
            ball.Type = ballType;
            ball.SizeNumber = ballSize;
            var game = _ioc.Resolve<Football>(new DependencyOverride<BallPoco>(ball));
            Assert.AreEqual(ballSize, game.Ball.SizeNumber);
            CheckObjectIsMock(game);
        }


        private void WhenOverridenParametersSuplied_MocknityShouldConfigureTheMockByIt()
        {
            _ioc.RegisterType<ISecondObject, SecondObjectImpl>();
            _ioc.RegisterType<IFirstObject, FirstObjectImpl>();

            var obj = _ioc.Resolve<ObjectWithDependencies>(
                new ParameterOverride("firstObject", _mocks.DynamicMock<IFirstObject>()),
                new ParameterOverride("secondObject", _mocks.DynamicMock<ISecondObject>()));
            CheckObjectIsMock(obj.firstObject);
            CheckObjectIsMock(obj.secondObject);
        }

        private void CheckObjectIsReal(object obj)
        {
            MockObjectCheck(obj, false);
        }

        private void CheckObjectIsMock(object obj)
        {
            MockObjectCheck(obj, true);
        }

        private void MockObjectCheck(object obj, bool expectedMock)
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