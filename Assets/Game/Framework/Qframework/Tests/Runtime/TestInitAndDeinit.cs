using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace QFramework.Tests
{
    public class TestInitAndDeinit
    {
        private static bool _isTriggerTestEvent = false;
        
        public class TestEvent
        {
        }
        
        public class TestProxy : ArchitectureProxy<TestProxy>
        {
            public override void Init()
            {
                RegisterModel(new TestModel());
                RegisterModel(new TestLazyModel());
                
                RegisterSystem(new TestSystem());
                RegisterSystem(new TestLazySystem());
            }
        }
        
        public class TestSystem : AbstractSystem
        {
            public string testString;
            
            public TestModel cacheModel { get; private set; }
            
            protected override void OnInit()
            {
                cacheModel = this.GetModel<TestModel>();
                
                testString = cacheModel.testInt.ToString();
                
                this.RegisterEvent<TestEvent>(OnTestEvent);
            }
            
            protected override void OnReset()
            {
                base.OnReset();
                testString = string.Empty;
                this.UnRegisterEvent<TestEvent>(OnTestEvent);
            }
            
            private void OnTestEvent(TestEvent e)
            {
                _isTriggerTestEvent = true;
            }
        }
        
        public class TestModel : AbstractModel
        {
            public int testInt;
            public List<int> testList;
            
            protected override void OnInit()
            {
                testInt = 1;
                testList = new List<int>(2) {2, 3};
            }
            
            protected override void OnReset()
            {
                base.OnReset();
                testInt = 0;
                testList.Clear();
            }
        }
        
        public class TestLazySystem : AbstractSystem
        {
            public string testString;
            
            public override bool LazyInit => true;
            
            public TestLazyModel cacheModel { get; private set; }
            
            protected override void OnInit()
            {
                cacheModel = this.GetModel<TestLazyModel>();
                
                testString = cacheModel.testInt.ToString();
            }
            
            protected override void OnReset()
            {
                base.OnReset();
                testString = string.Empty;
            }
        }
        
        public class TestLazyModel : AbstractModel
        {
            public override bool LazyInit => true;
            
            public int testInt;
            public List<int> testList;
            
            protected override void OnInit()
            {
                testInt = 2;
                testList = new List<int>(3) {2, 3, 4};
            }
            
            protected override void OnReset()
            {
                base.OnReset();
                testInt = 0;
                testList.Clear();
            }
        }
        
        [SetUp]
        public void SetUp()
        {
            GameArchitecture.Interface.ClearAll();
            _isTriggerTestEvent = false;
        }
        
        [Test]
        public void TestDeinit()
        {
            var architecture = TestProxy.Interface;
            var architectureInstance = GameArchitecture.Instance;
            var preSystem = architectureInstance.mContainer.Get<TestSystem>(false);
            var preModel = architectureInstance.mContainer.Get<TestModel>();
            var preLazySystem = architectureInstance.mContainer.Get<TestLazySystem>();
            var preLazyModel = architectureInstance.mContainer.Get<TestLazyModel>();
            Assert.AreEqual(1, preModel.testInt);
            Assert.AreEqual(2, preModel.testList.Count);
            Assert.AreEqual(preModel.testInt.ToString(), preSystem.testString);
            Assert.AreEqual(0, preLazyModel.testInt);
            Assert.AreEqual(null, preLazyModel.testList);
            Assert.AreEqual(null, preLazySystem.testString);
            preLazySystem = architecture.GetSystem<TestLazySystem>();
            preLazySystem = architecture.GetSystem<TestLazySystem>();
            preLazyModel = architecture.GetModel<TestLazyModel>();
            preLazyModel = architecture.GetModel<TestLazyModel>();
            Assert.AreEqual(2, preLazyModel.testInt);
            Assert.AreEqual(3, preLazyModel.testList.Count);
            Assert.AreEqual(preLazyModel.testInt.ToString(), preLazySystem.testString);
            architecture.Reset();
            architecture.SendEvent(new TestEvent());
            
            Assert.AreEqual(0, preModel.testInt);
            Assert.AreEqual(0, preModel.testList.Count);
            Assert.AreEqual(string.Empty, preSystem.testString);
            Assert.AreEqual(0, preLazyModel.testInt);
            Assert.AreEqual(0, preLazyModel.testList.Count);
            Assert.AreEqual(string.Empty, preLazySystem.testString);
            Assert.IsFalse(_isTriggerTestEvent);
        }
        
        [Test]
        public void TestReinit()
        {
            var architecture = TestProxy.Interface;
            var architectureInstance = GameArchitecture.Instance;
            var preSystem = architectureInstance.mContainer.Get<TestSystem>(false);
            var preModel = architectureInstance.mContainer.Get<TestModel>();
            var preLazySystem = architectureInstance.mContainer.Get<TestLazySystem>();
            var preLazyModel = architectureInstance.mContainer.Get<TestLazyModel>();
            Assert.AreEqual(1, preModel.testInt);
            Assert.AreEqual(2, preModel.testList.Count);
            Assert.AreEqual(preModel.testInt.ToString(), preSystem.testString);
            Assert.AreEqual(0, preLazyModel.testInt);
            Assert.AreEqual(null, preLazyModel.testList);
            Assert.AreEqual(null, preLazySystem.testString);
            preLazySystem = architecture.GetSystem<TestLazySystem>();
            preLazySystem.testString = "10";
            preLazySystem = architecture.GetSystem<TestLazySystem>();
            preLazyModel = architecture.GetModel<TestLazyModel>();
            preLazyModel.testInt = 20;
            preLazyModel.testList = new List<int>() {30, 40};
            preLazyModel = architecture.GetModel<TestLazyModel>();
            Assert.AreEqual(20, preLazyModel.testInt);
            Assert.AreEqual(2, preLazyModel.testList.Count);
            Assert.AreEqual("10", preLazySystem.testString);
            architecture.Reset();
            architecture.Reinit();
            TestProxy.Interface.SendEvent(new TestEvent());
            
            Assert.AreEqual(1, preModel.testInt);
            Assert.AreEqual(2, preModel.testList.Count);
            Assert.AreEqual(preModel.testInt.ToString(), preSystem.testString);
            Assert.AreEqual(2, preLazyModel.testInt);
            Assert.AreEqual(3, preLazyModel.testList.Count);
            Assert.AreEqual(preLazyModel.testInt.ToString(), preLazySystem.testString);
            Assert.IsTrue(_isTriggerTestEvent);
        }
        
        [TearDown]
        public void TearDown()
        {
            GameArchitecture.Interface.ClearAll();
            _isTriggerTestEvent = false;
        }
    }
}