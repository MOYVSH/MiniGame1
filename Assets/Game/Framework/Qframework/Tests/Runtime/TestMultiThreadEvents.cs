#if ENABLE_MULTITHREAD_EVENT
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace QFramework.Tests
{
    public class TestEventToMainThread
    {
        public readonly int sleepTime;

        public TestEventToMainThread(int sleepTime)
        {
            this.sleepTime = sleepTime;
        }
    }

    [TestFixture]
    public class TestMultiThreadEvents
    {
        protected int eventCount = 10;


        private bool isRunning;
        protected int mCurrentEvent;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var architecture = TestArchitecture.Interface;
            architecture.ClearContainer();
            architecture.RegisterSystem(new MonoBehaviourSystem());
            architecture.RegisterSystem(new TimerSystem());
            architecture.RegisterEvent<TestEventToMainThread>(OnEventReceived);
        }
        
        [UnitySetUp]
        public virtual IEnumerator SetUp()
        {
            yield return null;
        }

        protected virtual void OnEventReceived(TestEventToMainThread e)
        {
            mCurrentEvent++;
            new GameObject(mCurrentEvent.ToString());
            Debug.Log(e.sleepTime);
            Debug.Log($"{mCurrentEvent}:mCurrentEvent");
        }

        protected IEnumerator SendEvents()
        {
            for (int i = 0; i < eventCount; i++)
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    TestArchitecture.Interface.SendEventToMainThread(new TestEventToMainThread(0));

                    var sleepTime = Math.Abs((int) (DateTime.Now.Millisecond % 10) * 100);
                    Thread.Sleep(sleepTime);
                    TestArchitecture.Interface.SendEventToMainThread(new TestEventToMainThread(sleepTime));
                });
                yield return null;
            }
        }

        [UnityTest]
        public virtual IEnumerator TestSendEventToMainThread()
        {
            mCurrentEvent = 0;

            yield return SendEvents();

            yield return new WaitForSeconds(2);
            Debug.Log($"Finished: {mCurrentEvent}");
            LogAssert.Expect(LogType.Log, $"{eventCount * 2}:mCurrentEvent");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            TestArchitecture.Interface.ClearContainer();
        }
        
        [UnityTearDown]
        public virtual IEnumerator TearDown()
        {
            isRunning = false;
            yield return null;
        }
    }
}
#endif