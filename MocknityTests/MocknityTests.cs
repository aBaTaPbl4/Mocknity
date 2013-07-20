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

  public class ObjectWithDependencies
  {
    public IFirstObject firstObject;
    public ISecondObject secondObject;

    public ObjectWithDependencies(IFirstObject firstObject, ISecondObject secondObject)
    {
      this.firstObject = firstObject;
      this.secondObject = secondObject;
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
      container = new UnityContainer();
      mr = new MockRepository();
      mocknity = new MocknityContainerExtension(mr, true);

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
    public void SetSameStrategyTwiceForDiffernetTypes__ExpectedWorks2()
    {
        mocknity.SetStrategy<StubRhinoMocksBuilderStrategy>(typeof (ISecondObject));
        mocknity.SetStrategy<StubRhinoMocksBuilderStrategy>(typeof (IThirdObject));
        var obj = container.Resolve<ObjectWithDependencies2>();
    }
  }
}
