using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOYV
{
    public class CoroutineRunner : MonoBehaviour
    {
        private readonly Dictionary<string, Queue<Action>> mQueuedActions = new Dictionary<string, Queue<Action>>();
        private const string IMMEDIATE_TASK = "immediate";
        private Action mInvokeAction;

        private void Update()
        {
            if (mQueuedActions.ContainsKey(IMMEDIATE_TASK))
            {
                var actions = mQueuedActions[IMMEDIATE_TASK];
                while (actions.Count>0)
                {
                    actions.Dequeue()();
                }
            }

            foreach (Queue<Action> actions in mQueuedActions.Values)
            {
                if (actions.Count > 0)
                {
                    mInvokeAction = actions.Dequeue();
                    if (mInvokeAction != null)
                    {
                        mInvokeAction();
                    }
                }
            }
        }

        public void AddActionToQueue(string taskName, Action action)
        {
            if (!mQueuedActions.ContainsKey(taskName))
            {
                mQueuedActions[taskName] = new Queue<Action>(128);
            }

            mQueuedActions[taskName].Enqueue(action);
        }

        public void AddActionToImmediate(Action action)
        {
            if (!mQueuedActions.ContainsKey(IMMEDIATE_TASK))
            {
                mQueuedActions[IMMEDIATE_TASK] = new Queue<Action>(128);
            }

            mQueuedActions[IMMEDIATE_TASK].Enqueue(action);
        }

        public void ClearQueue(string taskName)
        {
            if (string.IsNullOrEmpty(taskName) || !mQueuedActions.ContainsKey(taskName))
            {
                return;
            }

            mQueuedActions[taskName].Clear();
        }

        #region helper methods

        public static IEnumerator ActionOneByOne(List<Action> actions)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                action();
                yield return null;
            }
        }

        public static IEnumerator ActionOneByOneWithDelay(List<Action> actions, float delayTime)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                action();
                yield return new WaitForSeconds(delayTime);
            }
        }

        /// <summary>
        /// some action do after delay
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delayTime"></param>
        /// <returns></returns>
        public static IEnumerator ActionAfterDelay(Action action, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            action();
        }

        /// <summary>
        /// Generic version
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="delayTime"></param>
        /// <param name="para"></param>
        /// <returns></returns>
        public static IEnumerator ActionAfterDelay<T>(Action<T> action, float delayTime, T para)
        {
            yield return new WaitForSeconds(delayTime);
            action(para);
        }

        public static IEnumerator DelayBetweenActions(Action a, float delayTime, Action b)
        {
            a();
            yield return new WaitForSeconds(delayTime);
            b();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action">return a bool decide the loop whether need to be breaked.</param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static IEnumerator InfiniteDoOneAction(Func<bool> action, float interval)
        {
            while (true)
            {
                yield return new WaitForSeconds(interval);
                var isKeeping = action();
                if (!isKeeping)
                {
                    yield break;
                }
            }
        }

        /// <summary>
        /// Every frame do an action untill the action return false
        /// To be remind is as each frame time may be not the same, so you'd better multiply Time.deltaTime at your delta value
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IEnumerator EveryFrameDoOneAction(Func<bool> action)
        {
            while (true)
            {
                yield return null;
                var isKeeping = action();
                if (!isKeeping)
                {
                    yield break;
                }
            }
        }

        /// <summary>
        /// Remember dispose the www.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="onSucceed"></param>
        /// <returns></returns>
        public static IEnumerator WWWLoader(string path, Action<WWW> onSucceed)
        {
            WWW www = new WWW(path);
            yield return www;
            onSucceed(www);
        }

        #endregion
    }

    // <summary>
    /// background coroutine manger, for some tasks like large count of files to download or load...
    /// 
    /// </summary>
    public class CoroutineManager : Singleton<CoroutineManager>
    {
        /// <summary>
        /// Max running coroutines num 
        /// </summary>
        public const int MaxRunningCoroutinesCount = 5;

        private CoroutineRunner m_Hoster;


        public CoroutineManager()
        {
            m_Hoster = new GameObject("CoroutineRunner").AddComponent<CoroutineRunner>();
            GameObject.DontDestroyOnLoad(m_Hoster);
        }


        public void DoAction(List<Action> actionList)
        {
            m_Hoster.StartCoroutine(CoroutineRunner.ActionOneByOne(actionList));
        }

        public void DoActionWithDelay(List<Action> actionList, float delay)
        {
            m_Hoster.StartCoroutine(CoroutineRunner.ActionOneByOneWithDelay(actionList, delay));
        }

        public void QueueAction(string taskName, Action action)
        {
            m_Hoster.AddActionToQueue(taskName, action);
        }

        public void ImmediateAction(Action action)
        {
            m_Hoster.AddActionToImmediate(action);
        }

        public void ClearQueue(string taskName)
        {
            m_Hoster.ClearQueue(taskName);
        }

        public void DoAction(IEnumerator coroutine)
        {
            m_Hoster.StartCoroutine(coroutine);
        }

        public override void Dispose()
        {
            GameObject.Destroy(m_Hoster);
        }
    }
}