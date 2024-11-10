using System;
using NUnit.Framework;
using NUnit.Framework.Internal;
using QFramework;
using QFramework.Tests;

namespace Tests.Editor
{
    public interface ICommonTest: ISystem
    {
    }

    public interface ITestSystem1 : ISystem
    {
        void Test();
    }

    public class TestClassSystem1 : AbstractSystem, ITestSystem1
    {
        public void Test()
        {
        }

        protected override void OnInit()
        {
        }
    }

    public interface ITestSystem2 : ISystem
    {
        void Test();
    }

    public class TestClassSystem2 : AbstractSystem, ITestSystem2
    {
        public void Test()
        {
        }

        protected override void OnInit()
        {
        }
    }

    public interface ITestSystem3 : ISystem
    {
        void Test();
    }

    public class TestClassSystem3 : AbstractSystem, ITestSystem3, ICommonTest
    {
        public void Test()
        {
        }

        protected override void OnInit()
        {
        }
    }

    public interface ITestSystem4 : ISystem
    {
        void Test();
    }

    public class TestClassSystem4 : AbstractSystem, ITestSystem4, ICommonTest
    {
        public void Test()
        {
        }

        protected override void OnInit()
        {
        }
    }

    public class TestMultipleKeysContainer
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            TestArchitecture.Interface.ClearContainer();
        }


        [Test]
        public void TestRegisterAndGet()
        {
            var beforeSys = TestArchitecture.Interface.GetAllSystems();

            TestArchitecture.Interface.RegisterSystem(new TestClassSystem1());
            TestArchitecture.Interface.RegisterSystem<ITestSystem1>(new TestClassSystem1());

            TestArchitecture.Interface.RegisterSystem<ITestSystem2>(new TestClassSystem2());
            TestArchitecture.Interface.RegisterSystem(new TestClassSystem2());


            var itf1 = TestArchitecture.Interface.GetSystem<ITestSystem1>();
            var kls1 = TestArchitecture.Interface.GetSystem<TestClassSystem1>();

            var itf2 = TestArchitecture.Interface.GetSystem<ITestSystem2>();
            var kls2 = TestArchitecture.Interface.GetSystem<TestClassSystem2>();

            Assert.That(itf1, Is.EqualTo(kls1));
            Assert.That(itf2, Is.EqualTo(kls2));

            var allsys = TestArchitecture.Interface.GetAllSystems();
            Assume.That(allsys, Is.Not.Empty);
            Assume.That(allsys.Count - beforeSys.Count, Is.EqualTo(2));

            CollectionAssert.Contains(allsys, itf1);
            CollectionAssert.Contains(allsys, itf2);
            CollectionAssert.Contains(allsys, kls1);
            CollectionAssert.Contains(allsys, kls2);
        }

        [Test]
        public void TestRegisterSameInterfaceClasses()
        {
            var s3 = new TestClassSystem3();
            var s4 = new TestClassSystem4();


            TestArchitecture.Interface.RegisterSystem(s3);
            TestArchitecture.Interface.RegisterSystem<ITestSystem4>(s4);
            
            Assert.That(TestArchitecture.Interface.GetSystem<ITestSystem3>(),Is.EqualTo(s3));
            Assert.That(TestArchitecture.Interface.GetSystem<TestClassSystem4>(),Is.EqualTo(s4));
            Assert.That(TestArchitecture.Interface.GetSystem<ICommonTest>(),Is.EqualTo(s4));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
        }
    }
}