#if ENABLE_MULTITHREAD_EVENT
using System;
using System.Collections;
using System.Threading;
using QFramework.Tests;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.Editor
{
    public class TestMultiThreadEventsInEditor : TestMultiThreadEvents
    {
        void OnUpdate()
        {
            TestArchitecture.Interface.HandleMainThreadEvents();
        }

        protected override void OnEventReceived(TestEventToMainThread e)
        {
            mCurrentEvent++;
            Debug.Log(e.sleepTime);
            new GameObject(mCurrentEvent.ToString());
            Debug.Log($"{mCurrentEvent}:mCurrentEvent");
        }

        [UnitySetUp]
        public override IEnumerator SetUp()
        {
            yield return base.SetUp();

            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
            mCurrentEvent = 0;
        }

        [UnityTest]
        public override IEnumerator TestSendEventToMainThread()
        {
            mCurrentEvent = 0;

            yield return SendEvents();

            while ((eventCount * 2) != mCurrentEvent)
            {
                yield return null;
            }

            Debug.Log($"Finished: {mCurrentEvent}");
            LogAssert.Expect(LogType.Log, $"{eventCount * 2}:mCurrentEvent");
        }

        [UnityTearDown]
        public override IEnumerator TearDown()
        {
            yield return base.TearDown();
            EditorApplication.update -= OnUpdate;
        }
    }
}
#endif