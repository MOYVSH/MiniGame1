using System;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.TestTools;

namespace QFramework.Tests
{
    public class TestArchitectureProxy
    {
        public class TestSystem1 : AbstractSystem
        {
            public int a;

            protected override void OnInit()
            {
                a = 2;
            }
        }
        
        public class TestSystem2 : AbstractSystem
        {
            public int b;

            protected override void OnInit()
            {
                b = this.GetSystem<TestSystem1>().a + 1;
            }
        }
        
        public class TestModel1 : AbstractModel
        {
            protected override void OnInit()
            {
            }
        }
        
        public class TestModel2 : AbstractModel
        {
            protected override void OnInit()
            {
            }
        }
        
        public class Proxy1 : ArchitectureProxy<Proxy1>
        {
            public override void Init()
            {
                RegisterSystem(new TestSystem1());
                RegisterModel(new TestModel1());
            }
        }
        
        public class Proxy2 : ArchitectureProxy<Proxy2>
        {
            public override void Init()
            {
                RegisterSystem(new TestSystem2());
                RegisterModel(new TestModel2());
            }
        }

        public class Proxy3 : ArchitectureProxy<Proxy3>
        {
            public override void Init()
            {
                RegisterSystem(new TestSystem2());
                RegisterSystem(new TestSystem1());
            }
        }

        [SetUp]
        public void Setup()
        {
            GameArchitecture.Interface.ClearAll();
        }
        
        [Test]
        public void TestNormalRegister()
        {
            var proxy1 = Proxy1.Interface;
            proxy1 = Proxy1.Interface;
            proxy1 = Proxy1.Interface;
            Assert.IsNotNull(proxy1.GetSystem<TestSystem1>());
            Assert.IsNotNull(proxy1.GetModel<TestModel1>());
            proxy1.GetSystem<TestSystem2>();
            proxy1.GetModel<TestModel2>();
            Assert.IsNotNull(proxy1.GetSystem<TestSystem2>());
            LogAssert.Expect(LogType.Error,
                "Get QFramework.Tests.TestArchitectureProxy+TestModel2 from container failed");
            
            var proxy2 = Proxy2.Interface;
            proxy2 = Proxy2.Interface;
            proxy2 = Proxy2.Interface;
            Assert.IsNotNull(proxy2.GetSystem<TestSystem1>());
            Assert.IsNotNull(proxy2.GetModel<TestModel1>());
            Assert.IsNotNull(proxy2.GetSystem<TestSystem2>());
            Assert.IsNotNull(proxy2.GetModel<TestModel2>());
        }

        [Test]
        public void TestInvertedRegister()
        {
            var proxy3 = Proxy3.Interface;
            Assert.IsNotNull(proxy3.GetSystem<TestSystem1>());
            Assert.IsNotNull(proxy3.GetSystem<TestSystem2>());
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            GameArchitecture.Interface.ClearAll();
        }
    }
}