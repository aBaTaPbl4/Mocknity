using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.Unity;
using Mocknity;
using Rhino.Mocks;
using Mocknity.Strategies.Rhino;
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
    public string IntroduceYourself()
    {
      return "I'm the first of all";
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

  public interface  IObjectWithDependencies
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

    public virtual string Test()
    {
        return "test";
    }

    public virtual string Foo()
    {
        return "foo";
    }

    public string PokeSecond()
    {
      return this.secondObject.HelloWorld();
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
          return this.fourthObject.HelloWorld();
      }
  }

#endregion

  [TestClass]
  public class MocknityTests
  {
    IUnityContainer container;
    MocknityContainerExtension mocknity;
    MockRepository mr;

    [TestInitialize()]
    public void TestInitialize()
    {
        InitPrivateMembers(true);
    }

    private void InitPrivateMembers(bool mockUnregisteredInterfaces)
    {
        container = new UnityContainer();
        mr = new MockRepository();
        mocknity = new MocknityContainerExtension(mr, mockUnregisteredInterfaces);
        container.AddExtension(mocknity);        
    }

    [TestCleanup()]
    public void TestCleanup()
    {
      container.Dispose();
    }

    [TestMethod]
    public void ResolveObject__MockDependenciesInitiated()
    {
      ObjectWithDependencies od = container.Resolve<ObjectWithDependencies>();

      Assert.IsNotNull(od.firstObject);
      Assert.IsNotNull(od.secondObject);
    }

    [TestMethod]
    public void ResolveObjectWithSpecificImpl__OneDependencyIsNotMocked()
    {
      container.RegisterType<IFirstObject, FirstObjectImpl>();
      ObjectWithDependencies od = container.Resolve<ObjectWithDependencies>();

      Assert.IsInstanceOfType(od.firstObject, typeof(FirstObjectImpl));
      Assert.IsNotNull(od.secondObject);
    }

    [TestMethod]
    public void ResolveObjectWithExpectedCall__GetDesiredResult()
    {
      ObjectWithDependencies od = container.Resolve<ObjectWithDependencies>();

      Expect.Call(od.secondObject.HelloWorld()).Return("I'm the first of none");
      mocknity.getRepository().ReplayAll();
      Assert.AreEqual("I'm the first of none", od.PokeSecond());
      mocknity.getRepository().VerifyAll();
    }

    [TestMethod]
    [ExpectedException(typeof(ExpectationViolationException))]
    public void SecondObjectIsAStrictMock__ExceptionThrownOnCallingNotExpectedMethod()
    {
      mocknity.SetStrategy<StrictRhinoMocksBuilderStrategy>(typeof(ISecondObject));
      ObjectWithDependencies od = container.Resolve<ObjectWithDependencies>();

      mocknity.getRepository().ReplayAll();
      od.secondObject.HelloWorld();
      mocknity.getRepository().VerifyAll();
    }

    [TestMethod]
    public void SecondObjectIsAStub__CanChangeProperties()
    {
      mocknity.SetStrategy<StubRhinoMocksBuilderStrategy>(typeof(ISecondObject));
      ObjectWithDependencies od = container.Resolve<ObjectWithDependencies>();

      mocknity.getRepository().ReplayAll();
      od.secondObject.MyProperty = 42;
      Assert.AreEqual(42, od.secondObject.MyProperty);
      mocknity.getRepository().VerifyAll();
    }

    [TestMethod]
    public void SecondObjectIsADynamicCall__AcceptsUnexpectedCall()
    {
      // dynamic mocking is a default strategy in mocknity
      ObjectWithDependencies od = container.Resolve<ObjectWithDependencies>();

      mocknity.getRepository().ReplayAll();
      // doesn't raise exceptions
      od.secondObject.HelloWorld();
      mocknity.getRepository().VerifyAll();
    }

    [TestMethod]
    public void SetSameStrategyTwiceForDiffernetTypes__ExpectedWorks()
    {
        mocknity.SetStrategy<StubRhinoMocksBuilderStrategy>(typeof(ISecondObject));
        mocknity.SetStrategy<StubRhinoMocksBuilderStrategy>(typeof(IThirdObject));
        
        var obj = container.Resolve<IThirdObject>();
        Assert.IsNotNull(obj);
        obj.MyProperty = 42;
        Assert.AreEqual(42, obj.MyProperty);

        var obj2 = container.Resolve<ISecondObject>();
        Assert.IsNotNull(obj2);
        obj2.MyProperty = 42;
        Assert.AreEqual(42, obj2.MyProperty);
    }

    [TestMethod]
    public void IfMockingUnregegisteredInterfacesIs_ON_Work_If_Interfaces_Unregistered()
    {
        mocknity.SetStrategy<StubRhinoMocksBuilderStrategy>(typeof (ISecondObject));
        mocknity.SetStrategy<StubRhinoMocksBuilderStrategy>(typeof (IThirdObject));
        var obj = container.Resolve<ObjectWithDependencies2>();
    }

    [TestMethod, ExpectedException(typeof(Microsoft.Practices.Unity.ResolutionFailedException))]
    public void IfMockingUnregegisteredInterfacesIs_OFF_NOT_Work_If_Interfaces_Unregistered()
    {
        InitPrivateMembers(false);
        mocknity.SetStrategy<StubRhinoMocksBuilderStrategy>(typeof(ISecondObject));
        mocknity.SetStrategy<StubRhinoMocksBuilderStrategy>(typeof(IThirdObject));
        var obj = container.Resolve<ObjectWithDependencies2>();
    }

    public void CheckPartialMock(IFirstObject obj)
    {
        Assert.IsNotNull(obj);
        var str = obj.IntroduceYourself();
        Assert.IsFalse(String.IsNullOrEmpty(str));
    }

    [TestMethod]
    public void ResolvePartialMock_HaveNoException()
    {
        mocknity.RegisterPartialMock<IFirstObject, FirstObjectImpl>();
        var obj = container.Resolve<IFirstObject>();;
        obj.Replay();
        CheckPartialMock(obj);
        var obj2 = container.Resolve<FirstObjectImpl>();
        obj2.Replay();
        CheckPartialMock(obj2);
    }


    [TestMethod]
    public void ResolvePartialMock_Works_WhenHaveDependcies()
    {
        mocknity.RegisterStrictMock<IFirstObject>();
        mocknity.RegisterStrictMock<ISecondObject>();
        mocknity.RegisterPartialMock<ObjectWithDependencies>();

        var obj = container.Resolve<ObjectWithDependencies>();
        obj.Replay();
        Assert.AreEqual("foo", obj.Foo());
        obj.Stub(x => x.Foo()).Return("f").Repeat.Any();
        obj.Replay();
        Assert.AreEqual("f", obj.Foo());
    }

    [TestMethod]
    public void ResolvePartialMock_Works_WhenRequestingBaseType()
    {
        mocknity.RegisterPartialMock<IObjectWithDependencies, ObjectWithDependencies>();
        var obj = container.Resolve<IObjectWithDependencies>();
        Assert.IsNotNull(obj);
    }

    [TestMethod]
    public void ResolvePartialMock_IfCallTwiceForInterface__MocksShouldBeSame()
    {
        mocknity.RegisterPartialMock<IObjectWithDependencies, ObjectWithDependencies>();

        var obj1 = container.Resolve<IObjectWithDependencies>();
        var obj2 = container.Resolve<IObjectWithDependencies>();
        Assert.AreEqual(obj1, obj2);
    }

    [TestMethod]
    public void ResolvePartialMock_IfCallTwiceForInterfaceAndImpl__MocksShouldBeSame()
    {
        mocknity.RegisterPartialMock<IObjectWithDependencies, ObjectWithDependencies>();

        var obj1 = container.Resolve<IObjectWithDependencies>();
        var obj2 = container.Resolve<ObjectWithDependencies>();
        Assert.AreEqual(obj1, obj2);
    }

    [TestMethod]
    public void ResolvePartialMock_IfCallTwiceForInterfaceAndImpl_ReverseOrder__MocksShouldBeSame()
    {
        mocknity.RegisterPartialMock<IObjectWithDependencies, ObjectWithDependencies>();
        var obj2 = container.Resolve<ObjectWithDependencies>();
        var obj1 = container.Resolve<IObjectWithDependencies>();
        Assert.AreEqual(obj1, obj2);
    }

    [TestMethod]
    public void ResolvePartialMock_IfCallTwiceForImpl__MocksShouldBeSame()
    {
        mocknity.RegisterPartialMock<IObjectWithDependencies, ObjectWithDependencies>();

        var obj1 = container.Resolve<ObjectWithDependencies>();
        var obj2 = container.Resolve<ObjectWithDependencies>();
        Assert.AreEqual(obj1, obj2);
    }
      //DYNAMIC
    [TestMethod]
    public void ResolveDynamicMock_Works_WhenHaveDependcies()
    {
        mocknity.RegisterDynamicMock<IFirstObject>();
        mocknity.RegisterDynamicMock<ISecondObject>();
        mocknity.RegisterDynamicMock<ObjectWithDependencies>();

        var obj = container.Resolve<ObjectWithDependencies>();
        Assert.AreEqual(null, obj.Test());
        obj.Stub(x => x.Test()).Return("t").Repeat.Any(); 
        obj.Replay();
        Assert.AreEqual("t", obj.Test());            
    }

    [TestMethod]
    public void ResolveDynamicMock_Works_WhenRequestingBaseType()
    {
        mocknity.RegisterDynamicMock<IObjectWithDependencies, ObjectWithDependencies>();
        var obj = container.Resolve<IObjectWithDependencies>();
        Assert.IsNotNull(obj);
    }

    [TestMethod]
    public void ResolveDynamicMock_IfCallTwice__MocksShouldBeSame()
    {
        mocknity.RegisterDynamicMock<IObjectWithDependencies, ObjectWithDependencies>();

        var obj1 = container.Resolve<IObjectWithDependencies>();
        var obj2 = container.Resolve<IObjectWithDependencies>();
        Assert.AreEqual(obj1, obj2);

        obj1 = container.Resolve<ObjectWithDependencies>();
        obj2 = container.Resolve<ObjectWithDependencies>();
        Assert.AreEqual(obj1, obj2);
    }
      //STRICT
    [TestMethod, ExpectedException(typeof(Rhino.Mocks.Exceptions.ExpectationViolationException))]
    public void ResolveStrictMock_Works_WhenHaveDependcies()
    {
        mocknity.RegisterStrictMock<IFirstObject>();
        mocknity.RegisterStrictMock<ISecondObject>();
        mocknity.RegisterStrictMock<ObjectWithDependencies>();
        var obj = container.Resolve<ObjectWithDependencies>();
        mr.ReplayAll();
        var result = obj.Test();
    }

    [TestMethod]
    public void ResolveStrictMock_Works_WhenRequestingBaseType()
    {
        mocknity.RegisterStrictMock<IObjectWithDependencies, ObjectWithDependencies>();
        var obj = container.Resolve<IObjectWithDependencies>();
        Assert.IsNotNull(obj);
    }

    [TestMethod]
    public void ResolveStrictMock_IfCallTwice__MocksShouldBeSame()
    {
        mocknity.RegisterStrictMock<IObjectWithDependencies, ObjectWithDependencies>();

        var obj1 = container.Resolve<IObjectWithDependencies>();
        var obj2 = container.Resolve<IObjectWithDependencies>();
        Assert.AreEqual(obj1, obj2);

        obj1 = container.Resolve<ObjectWithDependencies>();
        obj2 = container.Resolve<ObjectWithDependencies>();
        Assert.AreEqual(obj1, obj2);
    }
  }
}
