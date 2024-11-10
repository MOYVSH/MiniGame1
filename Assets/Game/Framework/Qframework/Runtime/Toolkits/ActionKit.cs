/****************************************************************************
 * Copyright (c) 2018 ~ 2022 liangxiegame UNDER MIT LICENSE
 * 
 * https://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 *
 * Latest Update: 2022.4.24 16:01 Add new API
 ****************************************************************************/

using QFramework.ActionKitSingleFile.Dependency.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using System.Reflection;


namespace QFramework
{
#if UNITY_EDITOR
    // v1 No.164
    [ClassAPI("4.ActionKit", "ActionKit", 0, "ActionKit")]
    [APIDescriptionCN("Action 时序动作序列（组合模式 + 命令模式 + 建造者模式）")]
    [APIDescriptionEN("Action Sequence (composite pattern + command pattern + builder pattern)")]
#endif
    public class ActionKit : Architecture<ActionKit>
    {
#if UNITY_EDITOR
        [MethodAPI]
        [APIDescriptionCN("延时回调")]
        [APIDescriptionEN("delay callback")]
        [APIExampleCode(@"
Debug.Log(""Start Time:"" + Time.time);
 
ActionKit.Delay(1.0f, () =>
{
    Debug.Log(""End Time:"" + Time.time);
             
}).Start(this); // update driven
 
// Start Time: 0.000000
---- after 1 seconds ----
---- 一秒后 ----
// End Time: 1.000728
")]
#endif
        public static IAction Delay(float seconds, Action callback)
        {
            return QFramework.Delay.Allocate(seconds, callback);
        }


#if UNITY_EDITOR
        [MethodAPI]
        [APIDescriptionCN("动作序列")]
        [APIDescriptionEN("action sequence")]
        [APIExampleCode(@"
Debug.Log(""Sequence Start:"" + Time.time);
 
ActionKit.Sequence()
    .Callback(() => Debug.Log(""Delay Start:"" + Time.time))
    .Delay(1.0f)
    .Callback(() => Debug.Log(""Delay Finish:"" + Time.time))
    .Start(this, _ => { Debug.Log(""Sequence Finish:"" + Time.time); });
 
// Sequence Start: 0
// Delay Start: 0
------ after 1 seconds ------
------ 1 秒后 ------
// Delay Finish: 1.01012
// Sequence Finish: 1.01012
")]
#endif
        public static ISequence Sequence()
        {
            return QFramework.Sequence.Allocate();
        }

#if UNITY_EDITOR
        [MethodAPI]
        [APIDescriptionCN("延时帧")]
        [APIDescriptionEN("delay by frameCount")]
        [APIExampleCode(@"
Debug.Log(""Delay Frame Start FrameCount:"" + Time.frameCount);
 
ActionKit.DelayFrame(1, () => { Debug.Log(""Delay Frame Finish FrameCount:"" + Time.frameCount); })
        .Start(this);
 
ActionKit.Sequence()
        .DelayFrame(10)
        .Callback(() => Debug.Log(""Sequence Delay FrameCount:"" + Time.frameCount))
        .Start(this);

// Delay Frame Start FrameCount:1
// Delay Frame Finish FrameCount:2
// Sequence Delay FrameCount:11
 
// --- also support nextFrame
// --- 还可以用 NextFrame  
// ActionKit.Sequence()
//      .NextFrame()
//      .Start(this);
//
// ActionKit.NextFrame(() => { }).Start(this);
")]
#endif
        public static IAction DelayFrame(int frameCount, Action onDelayFinish)
        {
            return QFramework.DelayFrame.Allocate(frameCount, onDelayFinish);
        }

        public static IAction NextFrame(Action onNextFrame)
        {
            return QFramework.DelayFrame.Allocate(1, onNextFrame);
        }


#if UNITY_EDITOR
        [MethodAPI]
        [APIDescriptionCN("条件")]
        [APIDescriptionEN("condition action")]
        [APIExampleCode(@"
ActionKit.Sequence()
        .Callback(() => Debug.Log(""Before Condition""))
        .Condition(() => Input.GetMouseButtonDown(0))
        .Callback(() => Debug.Log(""Mouse Clicked""))
        .Start(this);

// Before Condition
// ---- after left mouse click ----
// ---- 鼠标左键点击之后 ----
// Mouse Clicked
")]
#endif
        void ConditionAPI()
        {
        }

#if UNITY_EDITOR
        [MethodAPI]
        [APIDescriptionCN("重复动作")]
        [APIDescriptionEN("repeat action")]
        [APIExampleCode(@"
ActionKit.Repeat()
        .Condition(() => Input.GetMouseButtonDown(0))
        .Callback(() => Debug.Log(""Mouse Clicked""))
        .Start(this);
// always Log Mouse Clicked when click left mouse
// 鼠标左键点击时，每次都会输出 Mouse Clicked

ActionKit.Repeat(5) // -1、0 means forever 1 means once  2 means twice
        .Condition(() => Input.GetMouseButtonDown(1))
        .Callback(() => Debug.Log(""Mouse right clicked""))
        .Start(this, () =>
        {
            Debug.Log(""Right click finished"");
        });
// Mouse right clicked
// Mouse right clicked
// Mouse right clicked
// Mouse right clicked
// Mouse right clicked
// Right click finished
    ")]
#endif
        public static IRepeat Repeat(int repeatCount = -1)
        {
            return QFramework.Repeat.Allocate(repeatCount);
        }


#if UNITY_EDITOR
        [MethodAPI]
        [APIDescriptionCN("并行动作")]
        [APIDescriptionEN("parallel action")]
        [APIExampleCode(@"
Debug.Log(""Parallel Start:"" + Time.time);
 
ActionKit.Parallel()
        .Delay(1.0f, () => { Debug.Log(Time.time); })
        .Delay(2.0f, () => { Debug.Log(Time.time); })
        .Delay(3.0f, () => { Debug.Log(Time.time); })
        .Start(this, () =>
        {
            Debug.Log(""Parallel Finish:"" + Time.time);
        });
// Parallel Start:0
// 1.01
// 2.01
// 3.02
// Parallel Finish:3.02
")]
#endif
        public static IParallel Parallel()
        {
            return QFramework.Parallel.Allocate();
        }

#if UNITY_EDITOR
        [MethodAPI]
        [APIDescriptionCN("复合动作示例")]
        [APIDescriptionEN("Complex action example")]
        [APIExampleCode(@"
ActionKit.Sequence()
        .Callback(() => Debug.Log(""Sequence Start""))
        .Callback(() => Debug.Log(""Parallel Start""))
        .Parallel(p =>
        {
            p.Delay(1.0f, () => Debug.Log(""Delay 1s Finished""))
                .Delay(2.0f, () => Debug.Log(""Delay 2s Finished""));
        })
        .Callback(() => Debug.Log(""Parallel Finished""))
        .Callback(() => Debug.Log(""Check Mouse Clicked""))
        .Sequence(s =>
        {
            s.Condition(() => Input.GetMouseButton(0))
                .Callback(() => Debug.Log(""Mouse Clicked""));
        })
        .Start(this, () =>
        {
            Debug.Log(""Finish"");
        });
// 
// Sequence Start
// Parallel Start
// Delay 1s Finished
// Delay 2s Finished
// Parallel Finished
// Check Mouse Clicked
// ------ After Left Mouse Clicked ------
// ------ 鼠标左键点击后 ------
// Mouse Clicked
// Finish

")]
#endif
        public void ComplexAPI()
        {
        }


#if UNITY_EDITOR
        [MethodAPI]
        [APIDescriptionCN("自定义动作")]
        [APIDescriptionEN("Custom action example")]
        [APIExampleCode(@" 
ActionKit.Custom(a =>
{
    a
        .OnStart(() => { Debug.Log(""OnStart""); })
        .OnExecute(dt =>
        {
            Debug.Log(""OnExecute"");
 
            a.Finish();
        })
        .OnFinish(() => { Debug.Log(""OnFinish""); });
}).Start(this);
             
// OnStart
// OnExecute
// OnFinish
 
class SomeData
{
    public int ExecuteCount = 0;
}
 
ActionKit.Custom<SomeData>(a =>
{
    a
        .OnStart(() =>
        {
            a.Data = new SomeData()
            {
                ExecuteCount = 0
            };
        })
        .OnExecute(dt =>
        {
            Debug.Log(a.Data.ExecuteCount);
            a.Data.ExecuteCount++;
 
            if (a.Data.ExecuteCount >= 5)
            {
                a.Finish();
            }
        }).OnFinish(() => { Debug.Log(""Finished""); });
}).Start(this);
         
// 0
// 1
// 2
// 3
// 4
// Finished
 
// 还支持 Sequence、Repeat、Parallel 等
// Also support sequence repeat Parallel
// ActionKit.Sequence()
//     .Custom(c =>
//     {
//         c.OnStart(() => c.Finish());
//     }).Start(this);
")]
#endif
        public static IAction Custom(Action<ICustomAPI<object>> customSetting)
        {
            var action = QFramework.Custom.Allocate();
            customSetting(action);
            return action;
        }

        public static IAction Custom<TData>(Action<ICustomAPI<TData>> customSetting)
        {
            var action = QFramework.Custom<TData>.Allocate();
            customSetting(action);
            return action;
        }


#if UNITY_EDITOR
        [MethodAPI]
        [APIDescriptionCN("协程支持")]
        [APIDescriptionEN("coroutine action example")]
        [APIExampleCode(@"
IEnumerator SomeCoroutine()
{
    yield return new WaitForSeconds(1.0f);
    Debug.Log(""Hello:"" + Time.time);
}
 
ActionKit.Coroutine(SomeCoroutine).Start(this);
// Hello:1.0039           
SomeCoroutine().ToAction().Start(this);
// Hello:1.0039
ActionKit.Sequence()
    .Coroutine(SomeCoroutine)
    .Start(this);
// Hello:1.0039
")]
#endif
        public static IAction Coroutine(Func<IEnumerator> coroutineGetter)
        {
            return CoroutineAction.Allocate(coroutineGetter);
        }


        #region Events

        public static EasyEvent OnUpdate => ActionKitMonoBehaviourEvents.Instance.OnUpdate;
        public static EasyEvent OnFixedUpdate => ActionKitMonoBehaviourEvents.Instance.OnFixedUpdate;
        public static EasyEvent OnLateUpdate => ActionKitMonoBehaviourEvents.Instance.OnLateUpdate;
        public static EasyEvent OnGUI => ActionKitMonoBehaviourEvents.Instance.OnGUIEvent;
        public static EasyEvent OnApplicationQuit => ActionKitMonoBehaviourEvents.Instance.OnApplicationQuitEvent;

        protected override void Init()
        {
        }

        #endregion
    }
}


namespace QFramework
{
    public class DeprecateActionKit
    {
        [RuntimeInitializeOnLoadMethod]
        private static void InitNodeSystem()
        {
            // cache list			

            // cache node
            SafeObjectPool<DelayAction>.Instance.Init(50, 50);
            SafeObjectPool<EventAction>.Instance.Init(50, 50);
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// 延时执行节点
    /// </summary>
    [Serializable]
    [ActionGroup("ActionKit")]
    public class DelayAction : ActionKitAction, IPoolable, IResetable
    {
        [SerializeField] public float DelayTime;

        public System.Action OnDelayFinish { get; set; }

        public float CurrentSeconds { get; set; }

        public static DelayAction Allocate(float delayTime, System.Action onDelayFinish = null)
        {
            var retNode = SafeObjectPool<DelayAction>.Instance.Allocate();
            retNode.DelayTime = delayTime;
            retNode.OnDelayFinish = onDelayFinish;
            retNode.CurrentSeconds = 0.0f;
            return retNode;
        }

        protected override void OnReset()
        {
            CurrentSeconds = 0.0f;
        }

        protected override void OnExecute(float dt)
        {
            CurrentSeconds += dt;
            Finished = CurrentSeconds >= DelayTime;
            if (Finished && OnDelayFinish != null)
            {
                OnDelayFinish();
            }
        }

        protected override void OnDispose()
        {
            SafeObjectPool<DelayAction>.Instance.Recycle(this);
        }

        public void OnRecycled()
        {
            OnDelayFinish = null;
            DelayTime = 0.0f;
            Reset();
        }

        public bool IsRecycled { get; set; }
    }

    public static class DelayActionExtensions
    {
        public static IDeprecateAction Delay<T>(this T selfBehaviour, float seconds, System.Action delayEvent)
            where T : MonoBehaviour
        {
            var delayAction = DelayAction.Allocate(seconds, delayEvent);
            selfBehaviour.ExecuteNode(delayAction);
            return delayAction;
        }

        public static IActionChain Delay(this IActionChain selfChain, float seconds)
        {
            return selfChain.Append(DelayAction.Allocate(seconds));
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// 安枕执行节点
    /// </summary>
    [Serializable]
    [ActionGroup("ActionKit")]
    public class DelayFrameAction : ActionKitAction, IPoolable, IResetable
    {
        [SerializeField] public int FrameCount;

        public System.Action OnDelayFrameFinish { get; set; }

        public float CurrentSeconds { get; set; }

        public static DelayFrameAction Allocate(int frameCount, System.Action onDelayFrameFinish = null)
        {
            var retNode = SafeObjectPool<DelayFrameAction>.Instance.Allocate();
            retNode.FrameCount = frameCount;
            retNode.OnDelayFrameFinish = onDelayFrameFinish;
            retNode.CurrentSeconds = 0.0f;
            return retNode;
        }

        protected override void OnReset()
        {
            CurrentSeconds = 0.0f;
        }

        private int StartFrame;

        protected override void OnBegin()
        {
            base.OnBegin();

            StartFrame = Time.frameCount;
        }

        protected override void OnExecute(float dt)
        {
            Finished = Time.frameCount - StartFrame >= FrameCount;

            if (Finished && OnDelayFrameFinish != null)
            {
                OnDelayFrameFinish();
            }
        }

        protected override void OnDispose()
        {
            SafeObjectPool<DelayFrameAction>.Instance.Recycle(this);
        }

        public void OnRecycled()
        {
            OnDelayFrameFinish = null;
            FrameCount = 0;
            Reset();
        }

        public bool IsRecycled { get; set; }
    }

    public static class DelayFrameActionExtensions
    {
        public static void DelayFrame<T>(this T selfBehaviour, int frameCount, System.Action delayFrameEvent)
            where T : MonoBehaviour
        {
            selfBehaviour.ExecuteNode(DelayFrameAction.Allocate(frameCount, delayFrameEvent));
        }

        public static void NextFrame<T>(this T selfBehaviour, System.Action nextFrameEvent)
            where T : MonoBehaviour
        {
            selfBehaviour.ExecuteNode(DelayFrameAction.Allocate(1, nextFrameEvent));
        }

        public static IActionChain DelayFrame(this IActionChain selfChain, int frameCount)
        {
            return selfChain.Append(DelayFrameAction.Allocate(frameCount));
        }

        public static IActionChain NextFrame(this IActionChain selfChain)
        {
            return selfChain.Append(DelayFrameAction.Allocate(1));
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// 延时执行节点
    /// </summary>
    [OnlyUsedByCode]
    public class EventAction : ActionKitAction, IPoolable
    {
        private System.Action mOnExecuteEvent;

        /// <summary>
        /// TODO:这里填可变参数会有问题
        /// </summary>
        /// <param name="onExecuteEvents"></param>
        /// <returns></returns>
        public static EventAction Allocate(params System.Action[] onExecuteEvents)
        {
            var retNode = SafeObjectPool<EventAction>.Instance.Allocate();
            Array.ForEach(onExecuteEvents, onExecuteEvent => retNode.mOnExecuteEvent += onExecuteEvent);
            return retNode;
        }

        /// <summary>
        /// finished
        /// </summary>
        protected override void OnExecute(float dt)
        {
            if (mOnExecuteEvent != null)
            {
                mOnExecuteEvent.Invoke();
            }

            Finished = true;
        }

        protected override void OnDispose()
        {
            SafeObjectPool<EventAction>.Instance.Recycle(this);
        }

        public void OnRecycled()
        {
            Reset();
            mOnExecuteEvent = null;
        }

        public bool IsRecycled { get; set; }
    }

    [OnlyUsedByCode]
    public class OnlyBeginAction : ActionKitAction, IPoolable, IPoolType
    {
        private Action<OnlyBeginAction> mBeginAction;

        public static OnlyBeginAction Allocate(Action<OnlyBeginAction> beginAction)
        {
            var retSimpleAction = SafeObjectPool<OnlyBeginAction>.Instance.Allocate();

            retSimpleAction.mBeginAction = beginAction;

            return retSimpleAction;
        }

        public void OnRecycled()
        {
            mBeginAction = null;
        }

        protected override void OnBegin()
        {
            if (mBeginAction != null)
            {
                mBeginAction.Invoke(this);
            }
        }

        public bool IsRecycled { get; set; }

        public void Recycle2Cache()
        {
            SafeObjectPool<OnlyBeginAction>.Instance.Recycle(this);
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// like filter, add condition
    /// </summary>
    [OnlyUsedByCode]
    public class UntilAction : ActionKitAction, IPoolable
    {
        private Func<bool> mCondition;

        public static UntilAction Allocate(Func<bool> condition)
        {
            var retNode = SafeObjectPool<UntilAction>.Instance.Allocate();
            retNode.mCondition = condition;
            return retNode;
        }

        protected override void OnExecute(float dt)
        {
            Finished = mCondition.Invoke();
        }

        protected override void OnDispose()
        {
            SafeObjectPool<UntilAction>.Instance.Recycle(this);
        }

        void IPoolable.OnRecycled()
        {
            Reset();
            mCondition = null;
        }

        bool IPoolable.IsRecycled { get; set; }
    }

    internal class OnDestroyDisposeTrigger : MonoBehaviour
    {
        HashSet<IDisposable> mDisposables = new HashSet<IDisposable>();

        public void AddDispose(IDisposable disposable)
        {
            if (!mDisposables.Contains(disposable))
            {
                mDisposables.Add(disposable);
            }
        }

        private void OnDestroy()
        {
            if (Application.isPlaying)
            {
                foreach (var disposable in mDisposables)
                {
                    disposable.Dispose();
                }

                mDisposables.Clear();
                mDisposables = null;
            }
        }
    }

    internal class OnDisableDisposeTrigger : MonoBehaviour
    {
        HashSet<IDisposable> mDisposables = new HashSet<IDisposable>();

        public void AddDispose(IDisposable disposable)
        {
            if (!mDisposables.Contains(disposable))
            {
                mDisposables.Add(disposable);
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                foreach (var disposable in mDisposables)
                {
                    disposable.Dispose();
                }

                mDisposables.Clear();
                mDisposables = null;
            }
        }
    }

    internal static class IDisposableExtensions
    {
        /// <summary>
        /// 与 GameObject 绑定销毁
        /// </summary>
        /// <param name="self"></param>
        /// <param name="component"></param>
        public static void DisposeWhenGameObjectDestroyed(this IDisposable self, Component component)
        {
            var onDestroyDisposeTrigger = component.gameObject.GetComponent<OnDestroyDisposeTrigger>();
            if (!onDestroyDisposeTrigger)
            {
                onDestroyDisposeTrigger = component.gameObject.AddComponent<OnDestroyDisposeTrigger>();
            }

            onDestroyDisposeTrigger.AddDispose(self);
        }
    }

    /// <summary>
    /// 时间轴执行节点
    /// </summary>
    public class Timeline : ActionKitAction
    {
        private float mCurTime = 0;

        public System.Action OnTimelineBeganCallback
        {
            get { return OnBeganCallback; }
            set { OnBeganCallback = value; }
        }

        public System.Action OnTimelineEndedCallback
        {
            get { return OnEndedCallback; }
            set { OnEndedCallback = value; }
        }

        public Action<string> OnKeyEventsReceivedCallback = null;

        public class TimelinePair
        {
            public float Time;
            public IDeprecateAction Node;

            public TimelinePair(float time, IDeprecateAction node)
            {
                Time = time;
                Node = node;
            }
        }

        /// <summary>
        /// refator 2 one list? all in one list;
        /// </summary>
        public Queue<TimelinePair> TimelineQueue = new Queue<TimelinePair>();

        protected override void OnReset()
        {
            mCurTime = 0.0f;

            foreach (var timelinePair in TimelineQueue)
            {
                timelinePair.Node.Reset();
            }
        }

        protected override void OnExecute(float dt)
        {
            mCurTime += dt;

            foreach (var pair in TimelineQueue.Where(pair => pair.Time < mCurTime && !pair.Node.Finished))
            {
                if (pair.Node.Execute(dt))
                {
                    Finished = TimelineQueue.Count(timelinePair => !timelinePair.Node.Finished) == 0;
                }
            }
        }

        public Timeline(params TimelinePair[] pairs)
        {
            foreach (var pair in pairs)
            {
                TimelineQueue.Enqueue(pair);
            }
        }

        public void Append(TimelinePair pair)
        {
            TimelineQueue.Enqueue(pair);
        }

        public void Append(float time, IDeprecateAction node)
        {
            TimelineQueue.Enqueue(new TimelinePair(time, node));
        }

        protected override void OnDispose()
        {
            foreach (var timelinePair in TimelineQueue)
            {
                timelinePair.Node.Dispose();
            }

            TimelineQueue.Clear();
            TimelineQueue = null;
        }
    }

    public class KeyEventAction : ActionKitAction, IPoolable
    {
        private Timeline mTimeline;
        private string mEventName;

        public static KeyEventAction Allocate(string eventName, Timeline timeline)
        {
            var keyEventAction = SafeObjectPool<KeyEventAction>.Instance.Allocate();

            keyEventAction.mEventName = eventName;
            keyEventAction.mTimeline = timeline;

            return keyEventAction;
        }

        protected override void OnBegin()
        {
            base.OnBegin();

            mTimeline.OnKeyEventsReceivedCallback?.Invoke(mEventName);

            Finish();
        }

        protected override void OnDispose()
        {
            SafeObjectPool<KeyEventAction>.Instance.Recycle(this);
        }

        public void OnRecycled()
        {
            mTimeline = null;
            mEventName = null;
        }

        public bool IsRecycled { get; set; }
    }

    public class AsyncNode : ActionKitAction
    {
        public HashSet<IDeprecateAction> mActions = new HashSet<IDeprecateAction>();

        public void Add(IDeprecateAction action)
        {
            mActions.Add(action);
        }

        protected override void OnExecute(float dt)
        {
            foreach (var action in mActions.Where(action => action.Execute(dt)))
            {
                mActions.Remove(action);
                action.Dispose();
            }
        }
    }

    public class QueueNode : ActionKitAction
    {
        private Queue<IDeprecateAction> mQueue = new Queue<IDeprecateAction>(20);

        public void Enqueue(IDeprecateAction action)
        {
            mQueue.Enqueue(action);
        }

        protected override void OnExecute(float dt)
        {
            if (mQueue.Count != 0 && mQueue.Peek().Execute(dt))
            {
                mQueue.Dequeue().Dispose();
            }
        }
    }

    public interface IResetable
    {
        void Reset();
    }

    [OnlyUsedByCode]
    public class RepeatNode : ActionKitAction, INode
    {
        public RepeatNode(IDeprecateAction node, int repeatCount = -1)
        {
            RepeatCount = repeatCount;
            mNode = node;
        }

        public IDeprecateAction CurrentExecutingNode
        {
            get
            {
                var currentNode = mNode;
                var node = currentNode as INode;
                return node == null ? currentNode : node.CurrentExecutingNode;
            }
        }

        private IDeprecateAction mNode;

        public int RepeatCount = 1;

        private int mCurRepeatCount = 0;

        protected override void OnReset()
        {
            if (null != mNode)
            {
                mNode.Reset();
            }

            mCurRepeatCount = 0;
            Finished = false;
        }

        protected override void OnExecute(float dt)
        {
            if (RepeatCount == -1)
            {
                if (mNode.Execute(dt))
                {
                    mNode.Reset();
                }

                return;
            }

            if (mNode.Execute(dt))
            {
                mNode.Reset();
                mCurRepeatCount++;
            }

            if (mCurRepeatCount == RepeatCount)
            {
                Finished = true;
            }
        }

        protected override void OnDispose()
        {
            if (null != mNode)
            {
                mNode.Dispose();
                mNode = null;
            }
        }
    }

    /// <summary>
    /// 序列执行节点
    /// </summary>
    [OnlyUsedByCode]
    public class SequenceNode : ActionKitAction, INode, IResetable
    {
        protected List<IDeprecateAction> mNodes = ListPool<IDeprecateAction>.Get();
        protected List<IDeprecateAction> mExcutingNodes = ListPool<IDeprecateAction>.Get();

        public int TotalCount
        {
            get { return mExcutingNodes.Count; }
        }

        public IDeprecateAction CurrentExecutingNode
        {
            get
            {
                var currentNode = mExcutingNodes[0];
                var node = currentNode as INode;
                return node == null ? currentNode : node.CurrentExecutingNode;
            }
        }

        protected override void OnReset()
        {
            mExcutingNodes.Clear();
            foreach (var node in mNodes)
            {
                node.Reset();
                mExcutingNodes.Add(node);
            }
        }

        protected override void OnExecute(float dt)
        {
            if (mExcutingNodes.Count > 0)
            {
                // 如果有异常，则进行销毁，不再进行下边的操作
                if (mExcutingNodes[0].Disposed && !mExcutingNodes[0].Finished)
                {
                    Dispose();
                    return;
                }

                while (mExcutingNodes[0].Execute(dt))
                {
                    mExcutingNodes.RemoveAt(0);

                    OnCurrentActionFinished();

                    if (mExcutingNodes.Count == 0)
                    {
                        break;
                    }
                }
            }

            Finished = mExcutingNodes.Count == 0;
        }

        protected virtual void OnCurrentActionFinished()
        {
        }

        public SequenceNode(params IDeprecateAction[] nodes)
        {
            foreach (var node in nodes)
            {
                mNodes.Add(node);
                mExcutingNodes.Add(node);
            }
        }

        public SequenceNode Append(IDeprecateAction appendedNode)
        {
            mNodes.Add(appendedNode);
            mExcutingNodes.Add(appendedNode);
            return this;
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            if (null != mNodes)
            {
                mNodes.ForEach(node => node.Dispose());
                mNodes.Clear();
                mNodes.Release2Pool();
                mNodes = null;
            }

            if (null != mExcutingNodes)
            {
                mExcutingNodes.Release2Pool();
                mExcutingNodes = null;
            }
        }
    }

    /// <summary>
    /// 并发执行的协程
    /// </summary>
    [OnlyUsedByCode]
    public class SpawnNode : ActionKitAction
    {
        protected List<ActionKitAction> mNodes = ListPool<ActionKitAction>.Get();

        protected override void OnReset()
        {
            mNodes.ForEach(node => node.Reset());
            mFinishCount = 0;
        }

        public override void Finish()
        {
            for (var i = mNodes.Count - 1; i >= 0; i--)
            {
                mNodes[i].Finish();
            }

            base.Finish();
        }

        protected override void OnExecute(float dt)
        {
            for (var i = mNodes.Count - 1; i >= 0; i--)
            {
                var node = mNodes[i];
                if (!node.Finished && node.Execute(dt))
                    Finished = mNodes.Count == mFinishCount;
            }
        }

        private int mFinishCount = 0;

        private void IncreaseFinishCount()
        {
            mFinishCount++;
        }

        public SpawnNode(params ActionKitAction[] nodes)
        {
            mNodes.AddRange(nodes);

            foreach (var nodeAction in nodes)
            {
                nodeAction.OnEndedCallback += IncreaseFinishCount;
            }
        }

        public void Add(params ActionKitAction[] nodes)
        {
            mNodes.AddRange(nodes);

            foreach (var nodeAction in nodes)
            {
                nodeAction.OnEndedCallback += IncreaseFinishCount;
            }
        }

        protected override void OnDispose()
        {
            foreach (var node in mNodes)
            {
                node.OnEndedCallback -= IncreaseFinishCount;
                node.Dispose();
            }

            mNodes.Release2Pool();
            mNodes = null;
        }
    }

    public class ActionKitFSM
    {
        public ActionKitFSMState CurrentState { get; private set; }
        public Type PreviousStateType { get; private set; }

        public void Update()
        {
            if (CurrentState != null)
            {
                CurrentState.Update();
            }
        }

        public void FixedUpdate()
        {
            if (CurrentState != null)
            {
                CurrentState.FixedUpdate();
            }
        }

        private Dictionary<Type, ActionKitFSMState> mStates = new Dictionary<Type, ActionKitFSMState>();
        private ActionKitFSMTransitionTable mTrasitionTable = new ActionKitFSMTransitionTable();


        public void AddState(ActionKitFSMState state)
        {
            mStates.Add(state.GetType(), state);
        }

        public void AddTransition(ActionKitFSMTransition transition)
        {
            mTrasitionTable.Add(transition);
        }

        public bool HandleEvent<TTransition>() where TTransition : ActionKitFSMTransition
        {
            foreach (var transition in mTrasitionTable.TypeIndex.Get(typeof(TTransition)))
            {
                if (transition.SrcStateTypes.Contains(CurrentState.GetType()))
                {
                    var currentState = CurrentState;
                    var nextState = mStates[transition.DstStateType];
                    CurrentState.Exit();
                    transition.OnTransition(currentState, nextState);
                    CurrentState = nextState;
                    CurrentState.Enter();
                    return true;
                }
            }

            return false;
        }

        public void ChangeState<TState>() where TState : ActionKitFSMState
        {
            ChangeState(typeof(TState));
        }

        public void ChangeState(Type stateType)
        {
            if (CurrentState.GetType() != stateType)
            {
                PreviousStateType = CurrentState.GetType();
                CurrentState.Exit();
                CurrentState = mStates[stateType];
                CurrentState.Enter();
            }
        }

        public void BackToPreviousState()
        {
            ChangeState(PreviousStateType);
        }

        public void StartState<T>()
        {
            CurrentState = mStates[typeof(T)];
            CurrentState.Enter();
        }
    }

    public class ActionKitFSMState
    {
        public void Enter()
        {
            OnEnter();
        }

        public void Update()
        {
            OnUpdate();
        }

        public void FixedUpdate()
        {
            OnFixedUpdate();
        }

        public void Exit()
        {
            OnExit();
        }

        protected virtual void OnEnter()
        {
        }

        protected virtual void OnUpdate()
        {
        }

        protected virtual void OnFixedUpdate()
        {
        }

        protected virtual void OnExit()
        {
        }
    }

    public class ActionKitFSMState<T> : ActionKitFSMState
    {
        public ActionKitFSMState(T target)
        {
            mTarget = target;
        }

        protected T mTarget;

        public T Target
        {
            get { return mTarget; }
        }
    }

    public class ActionKitFSMTransition
    {
        public virtual HashSet<Type> SrcStateTypes { get; set; }

        public virtual Type DstStateType { get; set; }

        public virtual void OnTransition(ActionKitFSMState srcState, ActionKitFSMState dstState)
        {
        }

        public ActionKitFSMTransition AddSrcState<T>() where T : ActionKitFSMState
        {
            if (SrcStateTypes == null)
            {
                SrcStateTypes = new HashSet<Type>()
                {
                    typeof(T)
                };
            }
            else
            {
                SrcStateTypes.Add(typeof(T));
            }

            return this;
        }

        public ActionKitFSMTransition WithDstState<T>() where T : ActionKitFSMState
        {
            DstStateType = typeof(T);
            return this;
        }
    }


    public class ActionKitFSMTransition<TSrcState, TDstState> : ActionKitFSMTransition
    {
        private Type mSrcStateType = typeof(TSrcState);
        private Type mDstStateType = typeof(TDstState);

        public override HashSet<Type> SrcStateTypes
        {
            get { return new HashSet<Type>() {mSrcStateType}; }
        }

        public override Type DstStateType
        {
            get { return mDstStateType; }
        }
    }

    public struct EnumStateChangeEvent<T> where T : struct, IComparable, IConvertible, IFormattable
    {
        public GameObject Target;
        public EnumStateMachine<T> TargetStateMachine;
        public T NewState;
        public T PreviousState;

        public EnumStateChangeEvent(EnumStateMachine<T> stateMachine)
        {
            Target = stateMachine.Target;
            TargetStateMachine = stateMachine;
            NewState = stateMachine.CurrentState;
            PreviousState = stateMachine.PreviousState;
        }
    }

    public interface IEnumStateMachine
    {
        bool TriggerEvents { get; set; }
    }

    public class EnumStateMachine<T> : IEnumStateMachine where T : struct, IComparable, IConvertible, IFormattable
    {
        /// If you set TriggerEvents to true, the state machine will trigger events when entering and exiting a state. 
        /// Additionnally, if you also use a StateMachineProcessor, it'll trigger events for the current state on FixedUpdate, LateUpdate, but also
        /// on Update (separated in EarlyUpdate, Update and EndOfUpdate, triggered in this order at Update()
        /// To listen to these events, from any class, in its Start() method (or wherever you prefer), use MMEventManager.StartListening(gameObject.GetInstanceID().ToString()+"XXXEnter",OnXXXEnter);
        /// where XXX is the name of the state you're listening to, and OnXXXEnter is the method you want to call when that event is triggered.
        /// MMEventManager.StartListening(gameObject.GetInstanceID().ToString()+"CrouchingEarlyUpdate",OnCrouchingEarlyUpdate); for example will listen to the Early Update event of the Crouching state, and 
        /// will trigger the OnCrouchingEarlyUpdate() method. 
        public bool TriggerEvents { get; set; }

        /// the name of the target gameobject
        public GameObject Target;

        /// the current character's movement state
        public T CurrentState { get; protected set; }

        /// the character's movement state before entering the current one
        public T PreviousState { get; protected set; }

        /// <summary>
        /// Creates a new StateMachine, with a targetName (used for events, usually use GetInstanceID()), and whether you want to use events with it or not
        /// </summary>
        /// <param name="targetName">Target name.</param>
        /// <param name="triggerEvents">If set to <c>true</c> trigger events.</param>
        public EnumStateMachine(GameObject target, bool triggerEvents)
        {
            this.Target = target;
            this.TriggerEvents = triggerEvents;
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="newState">New state.</param>
        public virtual void ChangeState(T newState)
        {
            // if the "new state" is the current one, we do nothing and exit
            if (newState.Equals(CurrentState))
            {
                return;
            }

            // we store our previous character movement state
            PreviousState = CurrentState;
            CurrentState = newState;

            if (TriggerEvents)
            {
                TypeEventSystem.Global.Send(new EnumStateChangeEvent<T>(this));
            }
        }

        /// <summary>
        /// 返回上一个状态
        /// </summary>
        public virtual void RestorePreviousState()
        {
            CurrentState = PreviousState;

            if (TriggerEvents)
            {
                TypeEventSystem.Global.Send(new EnumStateChangeEvent<T>(this));
            }
        }
    }

    public class ActionKitFSMTransitionTable : Table<ActionKitFSMTransition>
    {
        public TableIndex<Type, ActionKitFSMTransition> TypeIndex =
            new TableIndex<Type, ActionKitFSMTransition>(t => t.GetType());

        protected override void OnAdd(ActionKitFSMTransition item)
        {
            TypeIndex.Add(item);
        }

        protected override void OnRemove(ActionKitFSMTransition item)
        {
            TypeIndex.Remove(item);
        }

        protected override void OnClear()
        {
            TypeIndex.Clear();
        }

        public override IEnumerator<ActionKitFSMTransition> GetEnumerator()
        {
            return TypeIndex.Dictionary.SelectMany(src => src.Value).GetEnumerator();
        }

        protected override void OnDispose()
        {
            TypeIndex.Dispose();
        }
    }

    /// <summary>
    /// FSM 基于枚举的状态机
    /// </summary>
    public class FSM<TStateEnum, TEventEnum> : IDisposable
    {
        private Action<TStateEnum, TStateEnum> mOnStateChanged = null;

        public FSM(Action<TStateEnum, TStateEnum> onStateChanged = null)
        {
            mOnStateChanged = onStateChanged;
        }

        /// <summary>
        /// FSM onStateChagned.
        /// </summary>
        public delegate void FSMOnStateChanged(params object[] param);

        /// <summary>
        /// QFSM state.
        /// </summary>
        public class FSMState<TName>
        {
            public TName Name;

            public FSMState(TName name)
            {
                Name = name;
            }

            /// <summary>
            /// The translation dict.
            /// </summary>
            public readonly Dictionary<TEventEnum, FSMTranslation<TName, TEventEnum>> TranslationDict =
                new Dictionary<TEventEnum, FSMTranslation<TName, TEventEnum>>();
        }

        /// <summary>
        /// Translation 
        /// </summary>
        public class FSMTranslation<TStateName, KEventName>
        {
            public TStateName FromState;
            public KEventName Name;
            public TStateName ToState;
            public Action<object[]> OnTranslationCallback; // 回调函数

            public FSMTranslation(TStateName fromState, KEventName name, TStateName toState,
                Action<object[]> onStateChagned)
            {
                FromState = fromState;
                ToState = toState;
                Name = name;
                OnTranslationCallback = onStateChagned;
            }
        }

        /// <summary>
        /// The state of the m current.
        /// </summary>
        TStateEnum mCurState;

        public TStateEnum State
        {
            get { return mCurState; }
        }

        /// <summary>
        /// The m state dict.
        /// </summary>
        Dictionary<TStateEnum, FSMState<TStateEnum>>
            mStateDict = new Dictionary<TStateEnum, FSMState<TStateEnum>>();

        /// <summary>
        /// Adds the state.
        /// </summary>
        /// <param name="name">Name.</param>
        private void AddState(TStateEnum name)
        {
            mStateDict[name] = new FSMState<TStateEnum>(name);
        }

        /// <summary>
        /// Adds the translation.
        /// </summary>
        /// <param name="fromState">From state.</param>
        /// <param name="name">Name.</param>
        /// <param name="toState">To state.</param>
        /// <param name="onStateChagned">Callfunc.</param>
        public void AddTransition(TStateEnum fromState, TEventEnum name, TStateEnum toState,
            Action<object[]> onStateChagned = null)
        {
            if (!mStateDict.ContainsKey(fromState))
            {
                AddState(fromState);
            }

            if (!mStateDict.ContainsKey(toState))
            {
                AddState(toState);
            }

            mStateDict[fromState].TranslationDict[name] =
                new FSMTranslation<TStateEnum, TEventEnum>(fromState, name, toState, onStateChagned);
        }

        /// <summary>
        /// Start the specified name.
        /// </summary>
        /// <param name="name">Name.</param>
        public void Start(TStateEnum name)
        {
            mCurState = name;
        }

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="param">Parameter.</param>
        public void HandleEvent(TEventEnum name, params object[] param)
        {
            if (mCurState != null && mStateDict[mCurState].TranslationDict.ContainsKey(name))
            {
                var tempTranslation = mStateDict[mCurState].TranslationDict[name];

                if (tempTranslation.OnTranslationCallback != null)
                {
                    tempTranslation.OnTranslationCallback.Invoke(param);
                }

                if (mOnStateChanged != null)
                {
                    mOnStateChanged.Invoke(mCurState, tempTranslation.ToState);
                }

                mCurState = tempTranslation.ToState;
            }
        }

        /// <summary>
        /// Clear this instance.
        /// </summary>
        public void Clear()
        {
            foreach (var keyValuePair in mStateDict)
            {
                foreach (var translationDictValue in keyValuePair.Value.TranslationDict.Values)
                {
                    translationDictValue.OnTranslationCallback = null;
                }

                keyValuePair.Value.TranslationDict.Clear();
            }

            mStateDict.Clear();
        }

        public void Dispose()
        {
            Clear();
        }
    }

    /// <summary>
    /// QFSM lite.
    /// 基于字符串的状态机
    /// </summary>
    public class QFSMLite
    {
        /// <summary>
        /// FSM callfunc.
        /// </summary>
        public delegate void FSMCallfunc(params object[] param);

        /// <summary>
        /// QFSM state.
        /// </summary>
        class QFSMState
        {
            public string Name;

            public QFSMState(string name)
            {
                Name = name;
            }

            /// <summary>
            /// The translation dict.
            /// </summary>
            public readonly Dictionary<string, QFSMTranslation> TranslationDict =
                new Dictionary<string, QFSMTranslation>();
        }

        /// <summary>
        /// Translation 
        /// </summary>
        public class QFSMTranslation
        {
            public string FromState;
            public string Name;
            public string ToState;
            public FSMCallfunc OnTranslationCallback; // 回调函数

            public QFSMTranslation(string fromState, string name, string toState, FSMCallfunc onTranslationCallback)
            {
                FromState = fromState;
                ToState = toState;
                Name = name;
                OnTranslationCallback = onTranslationCallback;
            }
        }

        /// <summary>
        /// The state of the m current.
        /// </summary>
        string mCurState;

        public string State
        {
            get { return mCurState; }
        }

        /// <summary>
        /// The m state dict.
        /// </summary>
        Dictionary<string, QFSMState> mStateDict = new Dictionary<string, QFSMState>();

        /// <summary>
        /// Adds the state.
        /// </summary>
        /// <param name="name">Name.</param>
        public void AddState(string name)
        {
            mStateDict[name] = new QFSMState(name);
        }

        /// <summary>
        /// Adds the translation.
        /// </summary>
        /// <param name="fromState">From state.</param>
        /// <param name="name">Name.</param>
        /// <param name="toState">To state.</param>
        /// <param name="callfunc">Callfunc.</param>
        public void AddTranslation(string fromState, string name, string toState, FSMCallfunc callfunc)
        {
            mStateDict[fromState].TranslationDict[name] = new QFSMTranslation(fromState, name, toState, callfunc);
        }

        /// <summary>
        /// Start the specified name.
        /// </summary>
        /// <param name="name">Name.</param>
        public void Start(string name)
        {
            mCurState = name;
        }

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="param">Parameter.</param>
        public void HandleEvent(string name, params object[] param)
        {
            if (mCurState != null && mStateDict[mCurState].TranslationDict.ContainsKey(name))
            {
                var tempTranslation = mStateDict[mCurState].TranslationDict[name];
                tempTranslation.OnTranslationCallback(param);
                mCurState = tempTranslation.ToState;
            }
        }

        /// <summary>
        /// Clear this instance.
        /// </summary>
        public void Clear()
        {
            mStateDict.Clear();
        }
    }

    public interface IDeprecateAction : IDisposable
    {
        bool Disposed { get; }

        bool Execute(float delta);

        void Reset();

        void Finish();

        bool Finished { get; }
    }

    [System.Serializable]
    public class ActionData
    {
        [SerializeField] public string ActionName;
        [SerializeField] public string AcitonData;
    }

    public interface INode
    {
        IDeprecateAction CurrentExecutingNode { get; }
    }

    [Serializable]
    public abstract class ActionKitAction : IDeprecateAction
    {
        public System.Action OnBeganCallback = null;
        public System.Action OnEndedCallback = null;
        public System.Action OnDisposedCallback = null;

        protected bool mOnBeginCalled = false;

        #region IAction Support

        bool IDeprecateAction.Disposed
        {
            get { return mDisposed; }
        }

        protected bool mDisposed = false;

        public bool Finished { get; protected set; }

        public virtual void Finish()
        {
            Finished = true;
        }

        public void Break()
        {
            Finished = true;
        }

        #endregion

        #region ResetableSupport

        public void Reset()
        {
            Finished = false;
            mOnBeginCalled = false;
            mDisposed = false;
            OnReset();
        }

        #endregion


        #region IExecutable Support

        public bool Execute(float dt)
        {
            if (mDisposed) return true;

            // 有可能被别的地方调用
            if (Finished)
            {
                return Finished;
            }

            if (!mOnBeginCalled)
            {
                mOnBeginCalled = true;
                OnBegin();

                if (OnBeganCallback != null)
                {
                    OnBeganCallback.Invoke();
                }
            }

            if (!Finished)
            {
                OnExecute(dt);
            }

            if (Finished)
            {
                if (OnEndedCallback != null)
                {
                    OnEndedCallback.Invoke();
                }

                OnEnd();
            }

            return Finished || mDisposed;
        }

        #endregion

        protected virtual void OnReset()
        {
        }

        protected virtual void OnBegin()
        {
        }

        /// <summary>
        /// finished
        /// </summary>
        protected virtual void OnExecute(float dt)
        {
        }

        protected virtual void OnEnd()
        {
        }

        protected virtual void OnDispose()
        {
        }

        #region IDisposable Support

        public void Dispose()
        {
            if (mDisposed) return;
            mDisposed = true;

            OnBeganCallback = null;
            OnEndedCallback = null;

            if (OnDisposedCallback != null)
            {
                OnDisposedCallback.Invoke();
            }

            OnDisposedCallback = null;
            OnDispose();
        }

        #endregion
    }

    public class OnlyUsedByCodeAttribute : Attribute
    {
    }

    public class ActionGroupAttribute : Attribute
    {
        public readonly string GroupName;

        public ActionGroupAttribute(string groupName)
        {
            GroupName = groupName;
        }
    }

    /// <summary>
    /// 支持链式方法
    /// </summary>
    public class SequenceNodeChain : ActionChain
    {
        protected override ActionKitAction mNode
        {
            get { return mSequenceNode; }
        }

        private SequenceNode mSequenceNode;

        public SequenceNodeChain()
        {
            mSequenceNode = new SequenceNode();
        }

        public override IActionChain Append(IDeprecateAction node)
        {
            mSequenceNode.Append(node);
            return this;
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            mSequenceNode.Dispose();
            mSequenceNode = null;
        }
    }

    public class RepeatNodeChain : ActionChain
    {
        protected override ActionKitAction mNode
        {
            get { return mRepeatNodeAction; }
        }

        private RepeatNode mRepeatNodeAction;

        private SequenceNode mSequenceNode;

        public RepeatNodeChain(int repeatCount)
        {
            mSequenceNode = new SequenceNode();
            mRepeatNodeAction = new RepeatNode(mSequenceNode, repeatCount);
        }

        public override IActionChain Append(IDeprecateAction node)
        {
            mSequenceNode.Append(node);
            return this;
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            if (null != mRepeatNodeAction)
            {
                mRepeatNodeAction.Dispose();
            }

            mRepeatNodeAction = null;

            mSequenceNode.Dispose();
            mSequenceNode = null;
        }
    }

    public interface IDisposeWhen : IDisposeEventRegister
    {
        IDisposeEventRegister DisposeWhen(Func<bool> condition);
    }

    public interface IDisposeEventRegister
    {
        void OnDisposed(System.Action onDisposedEvent);

        IDisposeEventRegister OnFinished(System.Action onFinishedEvent);
    }

    public static class IActionExtension
    {
        public static T ExecuteNode<T>(this T selBehaviour, IDeprecateAction commandNode) where T : MonoBehaviour
        {
            selBehaviour.StartCoroutine(commandNode.Execute());
            return selBehaviour;
        }

        private static WaitForEndOfFrame mEndOfFrame = new WaitForEndOfFrame();

        public static IEnumerator Execute(this IDeprecateAction selfNode)
        {
            if (selfNode.Finished) selfNode.Reset();

            while (!selfNode.Execute(Time.deltaTime))
            {
                yield return mEndOfFrame;
            }
        }
    }

    public static partial class IActionChainExtention
    {
        public static IActionChain Repeat<T>(this T selfbehaviour, int count = -1) where T : MonoBehaviour
        {
            var retNodeChain = new RepeatNodeChain(count) {Executer = selfbehaviour};
            retNodeChain.DisposeWhenGameObjectDestroyed(selfbehaviour);
            return retNodeChain;
        }

        public static IActionChain Sequence<T>(this T selfbehaviour) where T : MonoBehaviour
        {
            var retNodeChain = new SequenceNodeChain {Executer = selfbehaviour};
            retNodeChain.DisposeWhenGameObjectDestroyed(selfbehaviour);
            return retNodeChain;
        }

        public static IActionChain OnlyBegin(this IActionChain selfChain, Action<OnlyBeginAction> onBegin)
        {
            return selfChain.Append(OnlyBeginAction.Allocate(onBegin));
        }


        /// <summary>
        /// Same as Delayw
        /// </summary>
        /// <param name="senfChain"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static IActionChain Wait(this IActionChain senfChain, float seconds)
        {
            return senfChain.Append(DelayAction.Allocate(seconds));
        }

        public static IActionChain Event(this IActionChain selfChain, params System.Action[] onEvents)
        {
            return selfChain.Append(EventAction.Allocate(onEvents));
        }


        public static IActionChain Until(this IActionChain selfChain, Func<bool> condition)
        {
            return selfChain.Append(UntilAction.Allocate(condition));
        }
    }

    public interface IActionChain : IDeprecateAction
    {
        MonoBehaviour Executer { get; set; }

        IActionChain Append(IDeprecateAction node);

        IDisposeWhen Begin();
    }

    public abstract class ActionChain : ActionKitAction, IActionChain, IDisposeWhen
    {
        public MonoBehaviour Executer { get; set; }

        protected abstract ActionKitAction mNode { get; }

        public abstract IActionChain Append(IDeprecateAction node);

        protected override void OnExecute(float dt)
        {
            if (mDisposeWhenCondition && mDisposeCondition != null && mDisposeCondition.Invoke())
            {
                Finish();
            }
            else
            {
                Finished = mNode.Execute(dt);
            }
        }

        protected override void OnEnd()
        {
            base.OnEnd();
            Dispose();
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            Executer = null;
            mDisposeWhenCondition = false;
            mDisposeCondition = null;

            if (mOnDisposedEvent != null)
            {
                mOnDisposedEvent.Invoke();
            }

            mOnDisposedEvent = null;
        }

        public IDisposeWhen Begin()
        {
            Executer.ExecuteNode(this);
            return this;
        }

        private bool mDisposeWhenCondition = false;
        private Func<bool> mDisposeCondition;
        private System.Action mOnDisposedEvent = null;

        public IDisposeEventRegister DisposeWhen(Func<bool> condition)
        {
            mDisposeWhenCondition = true;
            mDisposeCondition = condition;
            return this;
        }

        IDisposeEventRegister IDisposeEventRegister.OnFinished(System.Action onFinishedEvent)
        {
            OnEndedCallback += onFinishedEvent;
            return this;
        }

        public void OnDisposed(System.Action onDisposedEvent)
        {
            mOnDisposedEvent = onDisposedEvent;
        }
    }

    /// <summary>
    /// 事件注入,和 NodeSystem 配套使用
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EventInjector<T>
    {
        public delegate bool InjectEventTrigger(T lastValue, T newValue);

        public delegate T InjectEventGetter();

        private T mCahedLastValue;

        public readonly Func<T> mGetter;

        public T Value
        {
            get { return mCahedLastValue; }
        }

        public EventInjector(Func<T> getter)
        {
            mGetter = getter;
        }

        public bool GetOn(InjectEventTrigger triggerConditionWithOldAndNewValue)
        {
            var value = mGetter();
            var trig = triggerConditionWithOldAndNewValue(mCahedLastValue, value);
            mCahedLastValue = value;
            return trig;
        }

        public bool GetOnValueChanged(Func<T, bool> triggerConditionWithNewValue = null)
        {
            return GetOn((lastValue, newValue) =>
                lastValue.Equals(newValue) &&
                (triggerConditionWithNewValue == null || triggerConditionWithNewValue(newValue)));
        }
    }

    [MonoSingletonPath("[ActionKit]/ActionQueue")]
    public class ActionQueue : MonoBehaviour, ISingleton
    {
        private List<IDeprecateAction> mActions = new List<IDeprecateAction>();

        public static void Append(IDeprecateAction action)
        {
            mInstance.mActions.Add(action);
        }

        // Update is called once per frame
        private void Update()
        {
            if (mActions.Count != 0 && mActions[0].Execute(Time.deltaTime))
            {
                mActions.RemoveAt(0);
            }
        }

        void ISingleton.OnSingletonInit()
        {
        }

        private static ActionQueue mInstance
        {
            get { return MonoSingletonProperty<ActionQueue>.Instance; }
        }
    }


    #region ECA

    public class EmptyEventData
    {
        public static EmptyEventData Default;
    }

    public interface IECAEvent<T>
    {
        void Register(UnityAction<T> onEvent);
        void UnRegister(UnityAction<T> onEvent);
        void Trigger(T t);
    }

    public interface IECACondition<T>
    {
        bool Match(T t);
    }

    public interface IECAAction<T>
    {
        void Execute(T t);
    }

    public interface IECARule<T> : IDisposable
    {
        IECAEvent<T> E { get; set; }
        IECACondition<T> C { get; set; }
        IECAAction<T> A { get; set; }

        IECARule<T> Build();
    }

    public class ECARule<T> : IECARule<T>
    {
        public IECAEvent<T> E { get; set; }
        public IECACondition<T> C { get; set; }
        public IECAAction<T> A { get; set; }


        void OnEvent(T t)
        {
            if (C.Match(t))
            {
                A.Execute(t);
            }
        }

        public IECARule<T> Build()
        {
            E.Register(OnEvent);
            return this;
        }

        public void Dispose()
        {
            E.UnRegister(OnEvent);
            E = null;
            C = null;
            A = null;
        }
    }

    public class CustomECACondition<T> : IECACondition<T>
    {
        private readonly Func<T, bool> mCondition;

        public CustomECACondition(Func<T, bool> condition)
        {
            mCondition = condition;
        }


        public bool Match(T t)
        {
            return mCondition.Invoke(t);
        }
    }

    public class CustomECAAction<T> : IECAAction<T>
    {
        private readonly UnityAction<T> mAction;

        public CustomECAAction(UnityAction<T> action)
        {
            mAction = action;
        }


        public void Execute(T t)
        {
            mAction?.Invoke(t);
        }
    }

    public static class ECARuleExtension
    {
        public static IECARule<T> Condition<T>(this IECARule<T> self, Func<T, bool> condition)
        {
            self.C = new CustomECACondition<T>(condition);
            return self;
        }

        public static IECARule<T> Condition<T>(this IECARule<T> self, Func<bool> condition)
        {
            self.C = new CustomECACondition<T>(_ => condition());
            return self;
        }

        public static IECARule<T> Action<T>(this IECARule<T> self, UnityAction<T> action)
        {
            self.A = new CustomECAAction<T>(action);
            return self;
        }

        public static IECARule<T> Action<T>(this IECARule<T> self, UnityAction action)
        {
            self.A = new CustomECAAction<T>(_ => action());
            return self;
        }
    }

    public class UnityEventT<T> : UnityEvent<T>
    {
    }

    public class MonoEventTrigger<T> : MonoBehaviour, IECAEvent<T>
    {
        public UnityEvent<T> Event = new UnityEventT<T>();

        public void Register(UnityAction<T> onEvent)
        {
            Event.AddListener(onEvent);
        }

        public void UnRegister(UnityAction<T> onEvent)
        {
            Event.RemoveListener(onEvent);
        }

        public void Trigger(T t)
        {
            Event?.Invoke(t);
        }
    }


    public class UpdateTrigger : MonoEventTrigger<EmptyEventData>
    {
        private void Update()
        {
            Trigger(EmptyEventData.Default);
        }
    }

    public class FixedUpdateTrigger : MonoEventTrigger<EmptyEventData>
    {
        private void FixedUpdate()
        {
            Trigger(EmptyEventData.Default);
        }
    }

    public class LateUpdateTrigger : MonoEventTrigger<EmptyEventData>
    {
        private void LateUpdate()
        {
            Trigger(EmptyEventData.Default);
        }
    }

    public class OnTriggerEnter2DTrigger : MonoEventTrigger<Collider2D>
    {
        private void OnTriggerEnter2D(Collider2D col)
        {
            Trigger(col);
        }
    }

    public static class ECARuleTriggerExtension
    {
        public static void OnUpdate(this Component self, UnityAction onUpdate)
        {
            self.UpdateEvent().Condition(_ => true).Action(_ => onUpdate()).Build();
        }

        public static void OnFixedUpdate(this Component self, UnityAction onFixedUpdate)
        {
            self.FixedUpdateEvent().Condition(_ => true).Action(_ => onFixedUpdate()).Build();
        }

        public static void OnLateUpdate(this Component self, UnityAction onLateUpdate)
        {
            self.LateUpdateEvent().Condition(_ => true).Action(_ => onLateUpdate()).Build();
        }

        public static IECARule<EmptyEventData> UpdateEvent(this Component self)
        {
            return new ECARule<EmptyEventData>
            {
                E = self.gameObject.GetOrAddComponent<UpdateTrigger>()
            };
        }

        public static IECARule<EmptyEventData> FixedUpdateEvent(this Component self)
        {
            return new ECARule<EmptyEventData>
            {
                E = self.gameObject.GetOrAddComponent<FixedUpdateTrigger>()
            };
        }

        public static IECARule<EmptyEventData> LateUpdateEvent(this Component self)
        {
            return new ECARule<EmptyEventData>
            {
                E = self.gameObject.GetOrAddComponent<LateUpdateTrigger>()
            };
        }
    }

    #endregion
}


namespace QFramework
{
    public enum ActionStatus
    {
        NotStart,
        Started,
        Finished,
    }

    public interface IActionController
    {
        bool Paused { get; set; }
        void Reset();
        void Deinit();
    }

    public interface IAction<TStatus> : IActionController
    {
        TStatus Status { get; set; }
        void OnStart();
        void OnExecute(float dt);
        void OnFinish();

        bool Deinited { get; set; }
    }


    public interface IAction : IAction<ActionStatus>
    {
    }


    public static class IActionExtensions
    {
        public static IActionController Start(this IAction self, MonoBehaviour monoBehaviour,
            Action<IAction> onFinish = null)
        {
            return monoBehaviour.ExecuteByUpdate(self, onFinish);
        }

        public static IActionController Start(this IAction self, MonoBehaviour monoBehaviour,
            Action onFinish)
        {
            return monoBehaviour.ExecuteByUpdate(self, _ => onFinish());
        }

        public static IActionController StartGlobal(this IAction self, Action<IAction> onFinish = null)
        {
            IActionExecutor executor = null;
            if (executor.UpdateAction(self, 0, onFinish)) return self;

            void Update()
            {
                if (executor.UpdateAction(self, Time.deltaTime, onFinish))
                {
                    ActionKit.OnUpdate.UnRegister(Update);
                }
            }

            ActionKit.OnUpdate.Register(Update);


            return self;
        }


        public static void Pause(this IActionController self)
        {
            self.As<IAction>().Paused = true;
        }

        public static void Resume(this IActionController self)
        {
            self.As<IAction>().Paused = false;
        }

        public static void Finish(this IAction self)
        {
            self.Status = ActionStatus.Finished;
        }

        public static bool Execute(this IAction self, float dt)
        {
            if (self.Status == ActionStatus.NotStart)
            {
                self.OnStart();

                if (self.Status == ActionStatus.Finished)
                {
                    self.OnFinish();
                    return true;
                }

                self.Status = ActionStatus.Started;
            }
            else if (self.Status == ActionStatus.Started)
            {
                self.OnExecute(dt);

                if (self.Status == ActionStatus.Finished)
                {
                    self.OnFinish();
                    return true;
                }
            }
            else if (self.Status == ActionStatus.Finished)
            {
                self.OnFinish();
                return true;
            }

            return false;
        }
    }
}


namespace QFramework
{
    public interface IActionExecutor
    {
        void Execute(IAction action, Action<IAction> onFinish = null);
    }


    public static class IActionExecutorExtensions
    {
        public static bool UpdateAction(this IActionExecutor self, IAction action, float dt,
            Action<IAction> onFinish = null)
        {
            if (action.Execute(dt))
            {
                onFinish?.Invoke(action);
                action.Deinit();
            }

            if (action.Deinited)
            {
                return true;
            }

            return false;
        }
    }
}


namespace QFramework
{
    internal class Callback : IAction
    {
        private Callback()
        {
        }

        private Action mCallback;

        private static SimpleObjectPool<Callback> mSimpleObjectPool =
            new SimpleObjectPool<Callback>(() => new Callback(), null, 10);

        public static Callback Allocate(Action callback)
        {
            var callbackAction = mSimpleObjectPool.Allocate();
            callbackAction.Reset();
            callbackAction.Deinited = false;
            callbackAction.mCallback = callback;
            return callbackAction;
        }

        public bool Paused { get; set; }
        public bool Deinited { get; set; }
        public ActionStatus Status { get; set; }

        public void OnStart()
        {
            mCallback?.Invoke();
            this.Finish();
        }

        public void OnExecute(float dt)
        {
        }

        public void OnFinish()
        {
        }

        public void Deinit()
        {
            if (!Deinited)
            {
                Deinited = true;
                mCallback = null;
                mSimpleObjectPool.Recycle(this);
            }
        }

        public void Reset()
        {
            Status = ActionStatus.NotStart;
        }
    }

    public static class CallbackExtension
    {
        public static ISequence Callback(this ISequence self, Action callback)
        {
            return self.Append(QFramework.Callback.Allocate(callback));
        }
    }
}


namespace QFramework
{
    public class Condition : IAction
    {
        private Func<bool> mCondition;

        private static SimpleObjectPool<Condition> mSimpleObjectPool =
            new SimpleObjectPool<Condition>(() => new Condition(), null, 10);

        private Condition()
        {
        }

        public static Condition Allocate(Func<bool> condition)
        {
            var conditionAction = mSimpleObjectPool.Allocate();
            conditionAction.Deinited = false;
            conditionAction.Reset();
            conditionAction.mCondition = condition;
            return conditionAction;
        }

        public bool Paused { get; set; }
        public bool Deinited { get; set; }
        public ActionStatus Status { get; set; }

        public void OnStart()
        {
        }

        public void OnExecute(float dt)
        {
            if (mCondition.Invoke())
            {
                this.Finish();
            }
        }

        public void OnFinish()
        {
        }

        public void Deinit()
        {
            if (!Deinited)
            {
                Deinited = true;
                mCondition = null;
                mSimpleObjectPool.Recycle(this);
            }
        }

        public void Reset()
        {
            Status = ActionStatus.NotStart;
        }
    }

    public static class ConditionExtension
    {
        public static ISequence Condition(this ISequence self, Func<bool> condition)
        {
            return self.Append(QFramework.Condition.Allocate(condition));
        }
    }
}


namespace QFramework
{
    internal class CoroutineAction : IAction
    {
        private static SimpleObjectPool<CoroutineAction> mPool =
            new SimpleObjectPool<CoroutineAction>(() => new CoroutineAction(), null, 10);

        private Func<IEnumerator> mCoroutineGetter = null;

        private CoroutineAction()
        {
        }

        public static CoroutineAction Allocate(Func<IEnumerator> coroutineGetter)
        {
            var coroutineAction = mPool.Allocate();
            coroutineAction.Deinited = false;
            coroutineAction.Reset();
            coroutineAction.mCoroutineGetter = coroutineGetter;
            return coroutineAction;
        }

        public bool Paused { get; set; }

        public void Deinit()
        {
            if (!Deinited)
            {
                Deinited = true;

                mCoroutineGetter = null;

                mPool.Recycle(this);
            }
        }

        public void Reset()
        {
            Status = ActionStatus.NotStart;
        }

        public bool Deinited { get; set; }
        public ActionStatus Status { get; set; }

        public void OnStart()
        {
            ActionKitMonoBehaviourEvents.Instance.ExecuteCoroutine(mCoroutineGetter(),
                () => { Status = ActionStatus.Finished; });
        }

        public void OnExecute(float dt)
        {
        }

        public void OnFinish()
        {
        }
    }

    public static class CoroutineExtension
    {
        public static ISequence Coroutine(this ISequence self, Func<IEnumerator> coroutineGetter)
        {
            return self.Append(CoroutineAction.Allocate(coroutineGetter));
        }

        public static IAction ToAction(this IEnumerator self)
        {
            return CoroutineAction.Allocate(() => self);
        }
    }
}


namespace QFramework
{
    public interface ICustomAPI<TData>
    {
        TData Data { get; set; }

        ICustomAPI<TData> OnStart(Action onStart);
        ICustomAPI<TData> OnExecute(Action<float> onExecute);
        ICustomAPI<TData> OnFinish(Action onFinish);

        void Finish();
    }

    internal class Custom<TData> : IAction, ICustomAPI<TData>
    {
        public TData Data { get; set; }

        protected Action mOnStart;
        protected Action<float> mOnExecute;
        protected Action mOnFinish;

        private static SimpleObjectPool<Custom<TData>> mSimpleObjectPool =
            new SimpleObjectPool<Custom<TData>>(() => new Custom<TData>(), null, 10);

        protected Custom()
        {
        }

        public static Custom<TData> Allocate()
        {
            var custom = mSimpleObjectPool.Allocate();
            custom.Deinited = false;
            custom.Reset();
            return custom;
        }

        public bool Paused { get; set; }

        public virtual void Deinit()
        {
            if (!Deinited)
            {
                Deinited = true;

                mOnStart = null;
                mOnExecute = null;
                mOnFinish = null;

                mSimpleObjectPool.Recycle(this);
            }
        }

        public void Reset()
        {
            Status = ActionStatus.NotStart;
        }

        public bool Deinited { get; set; }
        public ActionStatus Status { get; set; }

        public void OnStart()
        {
            mOnStart?.Invoke();
        }

        public void OnExecute(float dt)
        {
            mOnExecute?.Invoke(dt);
        }

        public void OnFinish()
        {
            mOnFinish?.Invoke();
        }

        public ICustomAPI<TData> OnStart(Action onStart)
        {
            mOnStart = onStart;
            return this;
        }

        public ICustomAPI<TData> OnExecute(Action<float> onExecute)
        {
            mOnExecute = onExecute;
            return this;
        }

        public ICustomAPI<TData> OnFinish(Action onFinish)
        {
            mOnFinish = onFinish;
            return this;
        }

        public void Finish()
        {
            Status = ActionStatus.Finished;
        }
    }

    internal class Custom : Custom<object>
    {
        private static SimpleObjectPool<Custom> mSimpleObjectPool =
            new SimpleObjectPool<Custom>(() => new Custom(), null, 10);

        protected Custom()
        {
        }

        public new static Custom Allocate()
        {
            var custom = mSimpleObjectPool.Allocate();
            custom.Deinited = false;
            custom.Reset();
            return custom;
        }

        public override void Deinit()
        {
            if (!Deinited)
            {
                Deinited = true;

                mOnStart = null;
                mOnExecute = null;
                mOnFinish = null;

                mSimpleObjectPool.Recycle(this);
            }
        }
    }

    public static class CustomExtension
    {
        public static ISequence Custom(this ISequence self, Action<ICustomAPI<object>> onCustomSetting)
        {
            var custom = ActionKit.Custom(onCustomSetting);
            return self.Append(custom);
        }


        public static ISequence Custom<TData>(this ISequence self, Action<ICustomAPI<TData>> onCustomSetting)
        {
            var custom = ActionKit.Custom(onCustomSetting);
            return self.Append(custom);
        }
    }
}


namespace QFramework
{
    internal class Delay : IAction
    {
        public float DelayTime;

        public System.Action OnDelayFinish { get; set; }

        public float CurrentSeconds { get; set; }

        private Delay()
        {
        }

        private static readonly SimpleObjectPool<Delay> mPool =
            new SimpleObjectPool<Delay>(() => new Delay(), null, 10);

        public static Delay Allocate(float delayTime, System.Action onDelayFinish = null)
        {
            var retNode = mPool.Allocate();
            retNode.Deinited = false;
            retNode.Reset();
            retNode.DelayTime = delayTime;
            retNode.OnDelayFinish = onDelayFinish;
            retNode.CurrentSeconds = 0.0f;
            return retNode;
        }


        public ActionStatus Status { get; set; }

        public void OnStart()
        {
        }

        public void OnExecute(float dt)
        {
            if (CurrentSeconds >= DelayTime)
            {
                this.Finish();
                OnDelayFinish?.Invoke();
            }

            CurrentSeconds += dt;
        }

        public void OnFinish()
        {
        }

        public void Reset()
        {
            Status = ActionStatus.NotStart;
            CurrentSeconds = 0.0f;
        }

        public bool Paused { get; set; }

        public void Deinit()
        {
            if (!Deinited)
            {
                OnDelayFinish = null;
                Deinited = true;
                mPool.Recycle(this);
            }
        }

        public bool Deinited { get; set; }
    }

    public static class DelayExtension
    {
        public static ISequence Delay(this ISequence self, float seconds, Action onDelayFinish = null)
        {
            return self.Append(QFramework.Delay.Allocate(seconds, onDelayFinish));
        }
    }
}


namespace QFramework
{
    internal class DelayFrame : IAction
    {
        public bool Paused { get; set; }
        public bool Deinited { get; set; }
        public ActionStatus Status { get; set; }

        private static SimpleObjectPool<DelayFrame> mSimpleObjectPool =
            new SimpleObjectPool<DelayFrame>(() => new DelayFrame(), null, 10);

        private Action mOnDelayFinish;

        public static DelayFrame Allocate(int frameCount, Action onDelayFinish = null)
        {
            var delayFrame = mSimpleObjectPool.Allocate();
            delayFrame.Reset();
            delayFrame.Deinited = false;
            delayFrame.mDelayedFrameCount = frameCount;
            delayFrame.mOnDelayFinish = onDelayFinish;

            return delayFrame;
        }

        private int mStartFrameCount;
        private int mDelayedFrameCount;

        public void OnStart()
        {
            mStartFrameCount = Time.frameCount;
        }

        public void OnExecute(float dt)
        {
            if (Time.frameCount >= mStartFrameCount + mDelayedFrameCount)
            {
                mOnDelayFinish?.Invoke();
                this.Finish();
            }
        }

        public void OnFinish()
        {
        }

        public void Deinit()
        {
            if (!Deinited)
            {
                Deinited = true;
                mOnDelayFinish = null;
                mSimpleObjectPool.Recycle(this);
            }
        }

        public void Reset()
        {
            Status = ActionStatus.NotStart;
            mStartFrameCount = 0;
        }
    }

    public static class DelayFrameExtension
    {
        public static ISequence DelayFrame(this ISequence self, int frameCount)
        {
            return self.Append(QFramework.DelayFrame.Allocate(frameCount));
        }

        public static ISequence NextFrame(this ISequence self)
        {
            return self.Append(QFramework.DelayFrame.Allocate(1));
        }
    }
}


namespace QFramework
{
    public interface IParallel : ISequence
    {
    }

    internal class Parallel : IParallel
    {
        private Parallel()
        {
        }

        private static SimpleObjectPool<Parallel> mSimpleObjectPool =
            new SimpleObjectPool<Parallel>(() => new Parallel(), null, 5);

        private List<IAction> mActions = ListPool<IAction>.Get();

        private int mFinishedCount = 0;

        public static Parallel Allocate()
        {
            var parallel = mSimpleObjectPool.Allocate();
            parallel.Deinited = false;
            parallel.Reset();
            return parallel;
        }

        public bool Paused { get; set; }
        public bool Deinited { get; set; }
        public ActionStatus Status { get; set; }

        public void OnStart()
        {
        }

        public void OnExecute(float dt)
        {
            for (var i = mFinishedCount; i < mActions.Count; i++)
            {
                if (!mActions[i].Execute(dt)) continue;

                mFinishedCount++;

                if (mFinishedCount >= mActions.Count)
                {
                    this.Finish();
                }
                else
                {
                    // swap
                    (mActions[i], mActions[mFinishedCount - 1]) = (mActions[mFinishedCount - 1], mActions[i]);
                }
            }
        }

        public void OnFinish()
        {
        }

        public ISequence Append(IAction action)
        {
            mActions.Add(action);
            return this;
        }

        public void Deinit()
        {
            if (!Deinited)
            {
                Deinited = true;

                foreach (var action in mActions)
                {
                    action.Deinit();
                }


                mActions.Clear();

                mSimpleObjectPool.Recycle(this);
            }
        }

        public void Reset()
        {
            Status = ActionStatus.NotStart;

            foreach (var action in mActions)
            {
                action.Reset();
            }

            mFinishedCount = 0;
        }
    }

    public static class ParallelExtension
    {
        public static ISequence Parallel(this ISequence self, Action<ISequence> parallelSetting)
        {
            var parallel = QFramework.Parallel.Allocate();
            parallelSetting(parallel);
            return self.Append(parallel);
        }
    }
}


namespace QFramework
{
    public interface ISequence : IAction
    {
        ISequence Append(IAction action);
    }

    internal class Sequence : ISequence
    {
        private IAction mCurrentAction = null;
        private int mCurrentActionIndex = 0;
        private List<IAction> mActions = ListPool<IAction>.Get();

        private Sequence()
        {
        }

        private static SimpleObjectPool<Sequence> mSimpleObjectPool =
            new SimpleObjectPool<Sequence>(() => new Sequence(), null, 10);

        public static Sequence Allocate()
        {
            var sequence = mSimpleObjectPool.Allocate();
            sequence.Reset();
            sequence.Deinited = false;
            return sequence;
        }

        public bool Paused { get; set; }

        public bool Deinited { get; set; }

        public ActionStatus Status { get; set; }

        public void OnStart()
        {
            if (mActions.Count > 0)
            {
                mCurrentActionIndex = 0;
                mCurrentAction = mActions[mCurrentActionIndex];
                mCurrentAction.Reset();
                TryExecuteUntilNextNotFinished();
            }
            else
            {
                this.Finish();
            }
        }

        void TryExecuteUntilNextNotFinished()
        {
            while (mCurrentAction != null && mCurrentAction.Execute(0))
            {
                mCurrentActionIndex++;

                if (mCurrentActionIndex < mActions.Count)
                {
                    mCurrentAction = mActions[mCurrentActionIndex];
                    mCurrentAction.Reset();
                }
                else
                {
                    mCurrentAction = null;
                    this.Finish();
                }
            }
        }

        public void OnExecute(float dt)
        {
            if (mCurrentAction != null)
            {
                if (mCurrentAction.Execute(dt))
                {
                    mCurrentActionIndex++;

                    if (mCurrentActionIndex < mActions.Count)
                    {
                        mCurrentAction = mActions[mCurrentActionIndex];
                        mCurrentAction.Reset();

                        TryExecuteUntilNextNotFinished();
                    }
                    else
                    {
                        this.Finish();
                    }
                }
            }
            else
            {
                this.Finish();
            }
        }

        public void OnFinish()
        {
        }

        public ISequence Append(IAction action)
        {
            mActions.Add(action);
            return this;
        }

        public void Deinit()
        {
            if (!Deinited)
            {
                Deinited = true;

                mActions.Clear();

                foreach (var action in mActions)
                {
                    action.Deinit();
                }

                mSimpleObjectPool.Recycle(this);
            }
        }

        public void Reset()
        {
            mCurrentActionIndex = 0;
            Status = ActionStatus.NotStart;
            foreach (var action in mActions)
            {
                action.Reset();
            }
        }
    }

    public static class SequenceExtension
    {
        public static ISequence Sequence(this ISequence self, Action<ISequence> sequenceSetting)
        {
            var repeat = QFramework.Sequence.Allocate();
            sequenceSetting(repeat);
            return self.Append(repeat);
        }
    }
}


namespace QFramework
{
    [MonoSingletonPath("QFramework/ActionKit/GlobalMonoBehaviourEvents")]
    internal class ActionKitMonoBehaviourEvents : MonoSingleton<ActionKitMonoBehaviourEvents>
    {
        internal readonly EasyEvent OnUpdate = new EasyEvent();
        internal readonly EasyEvent OnFixedUpdate = new EasyEvent();
        internal readonly EasyEvent OnLateUpdate = new EasyEvent();
        internal readonly EasyEvent OnGUIEvent = new EasyEvent();
        internal readonly EasyEvent<bool> OnApplicationFocusEvent = new EasyEvent<bool>();
        internal readonly EasyEvent<bool> OnApplicationPauseEvent = new EasyEvent<bool>();
        internal readonly EasyEvent OnApplicationQuitEvent = new EasyEvent();

        private void Awake()
        {
            hideFlags = HideFlags.HideInHierarchy;
        }

        private void Update()
        {
            OnUpdate?.Trigger();
        }

        private void OnGUI()
        {
            OnGUIEvent?.Trigger();
        }

        private void FixedUpdate()
        {
            OnFixedUpdate?.Trigger();
        }

        private void LateUpdate()
        {
            OnLateUpdate?.Trigger();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            OnApplicationFocusEvent?.Trigger(hasFocus);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            OnApplicationPauseEvent?.Trigger(pauseStatus);
        }

        protected override void OnApplicationQuit()
        {
            OnApplicationQuitEvent?.Trigger();
            base.OnApplicationQuit();
        }

        public void ExecuteCoroutine(IEnumerator coroutine, Action onFinish)
        {
            StartCoroutine(DoExecuteCoroutine(coroutine, onFinish));
        }

        IEnumerator DoExecuteCoroutine(IEnumerator coroutine, Action onFinish)
        {
            yield return coroutine;
            onFinish?.Invoke();
        }
    }
}


namespace QFramework
{
    internal class MonoUpdateActionExecutor : MonoBehaviour, IActionExecutor
    {
        private Action mOnUpdate = () => { };

        public void Execute(IAction action, Action<IAction> onFinish = null)
        {
            if (action.Status == ActionStatus.Finished) action.Reset();
            if (this.UpdateAction(action, 0, onFinish)) return;

            void OnUpdate()
            {
                if (this.UpdateAction(action, Time.deltaTime, onFinish))
                {
                    mOnUpdate -= OnUpdate;
                }
            }

            mOnUpdate += OnUpdate;
        }

        private void Update()
        {
            mOnUpdate?.Invoke();
        }
    }

    public static class MonoUpdateActionExecutorExtension
    {
        public static IAction ExecuteByUpdate<T>(this T self, IAction action, Action<IAction> onFinish = null)
            where T : MonoBehaviour
        {
            if (action.Status == ActionStatus.Finished) action.Reset();
            self.gameObject.GetOrAddComponent<MonoUpdateActionExecutor>().Execute(action, onFinish);
            return action;
        }
    }
}


namespace QFramework
{
    public interface IRepeat : ISequence
    {
    }

    public class Repeat : IRepeat
    {
        private Sequence mSequence = Sequence.Allocate();

        private int mRepeatCount = -1;
        private int mCurrentRepeatCount = 0;

        private static SimpleObjectPool<Repeat> mSimpleObjectPool =
            new SimpleObjectPool<Repeat>(() => new Repeat(), null, 5);

        private Repeat()
        {
        }

        public static Repeat Allocate(int repeatCount = -1)
        {
            var repeat = mSimpleObjectPool.Allocate();
            repeat.Deinited = false;
            repeat.Reset();
            repeat.mRepeatCount = repeatCount;
            return repeat;
        }

        public bool Paused { get; set; }
        public bool Deinited { get; set; }
        public ActionStatus Status { get; set; }

        public void OnStart()
        {
            mCurrentRepeatCount = 0;
        }

        public void OnExecute(float dt)
        {
            if (mRepeatCount == -1 || mRepeatCount == 0)
            {
                if (mSequence.Execute(dt))
                {
                    mSequence.Reset();
                }
            }
            else if (mCurrentRepeatCount < mRepeatCount)
            {
                if (mSequence.Execute(dt))
                {
                    mCurrentRepeatCount++;

                    if (mCurrentRepeatCount >= mRepeatCount)
                    {
                        this.Finish();
                    }
                    else
                    {
                        mSequence.Reset();
                    }
                }
            }
        }

        public void OnFinish()
        {
        }

        public ISequence Append(IAction action)
        {
            mSequence.Append(action);
            return this;
        }

        public void Deinit()
        {
            if (!Deinited)
            {
                Deinited = true;

                mSimpleObjectPool.Recycle(this);
            }
        }

        public void Reset()
        {
            mCurrentRepeatCount = 0;
            Status = ActionStatus.NotStart;
            mSequence.Reset();
        }
    }

    public static class RepeatExtension
    {
        public static ISequence Repeat(this ISequence self, Action<IRepeat> repeatSetting)
        {
            var repeat = QFramework.Repeat.Allocate();
            repeatSetting(repeat);
            return self.Append(repeat);
        }

        public static ISequence Repeat(this ISequence self, int repeatCount, Action<IRepeat> repeatSetting)
        {
            var repeat = QFramework.Repeat.Allocate(repeatCount);
            repeatSetting(repeat);
            return self.Append(repeat);
        }
    }
}


namespace QFramework.ActionKitSingleFile.Dependency.Internal
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class ClassAPIAttribute : Attribute
    {
        public string DisplayMenuName { get; private set; }
        public string GroupName { get; private set; }

        public int RenderOrder { get; private set; }

        public string DisplayClassName { get; private set; }

        public ClassAPIAttribute(string groupName, string displayMenuName, int renderOrder,
            string displayClassName = null)
        {
            GroupName = groupName;
            DisplayMenuName = displayMenuName;
            RenderOrder = renderOrder;
            DisplayClassName = displayClassName;
        }
    }
}


namespace QFramework.ActionKitSingleFile.Dependency.Internal
{
    internal class APIDescriptionCNAttribute : Attribute
    {
        public string Description { get; private set; }

        public APIDescriptionCNAttribute(string description)
        {
            Description = description;
        }
    }
}


namespace QFramework.ActionKitSingleFile.Dependency.Internal
{
    internal class APIDescriptionENAttribute : Attribute
    {
        public string Description { get; private set; }

        public APIDescriptionENAttribute(string description)
        {
            Description = description;
        }
    }
}

namespace QFramework.ActionKitSingleFile.Dependency.Internal
{
    internal class APIExampleCodeAttribute : Attribute
    {
        public string Code { get; private set; }

        public APIExampleCodeAttribute(string code)
        {
            Code = code;
        }
    }
}


namespace QFramework.ActionKitSingleFile.Dependency.Internal
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class MethodAPIAttribute : Attribute
    {
        
    }
}