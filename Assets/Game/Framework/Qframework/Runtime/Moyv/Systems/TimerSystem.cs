using System;
using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class BaseTask : IUnRegister
{
    public Action taskAction;
    public float startSeconds;

    public bool busyRun;

    public bool isCanceled;

    public virtual bool Execute(bool isBusy)
    {
        return false;
    }


    public void UnRegister()
    {
        isCanceled = true;
    }
}

public class DelayTask : BaseTask
{
    public float seconds;

    public float finishSeconds;

    public TaskState state;


    public override bool Execute(bool isBusy)
    {
        if (isCanceled)
            return true;
        if (!busyRun && isBusy)
        {
            return false;
        }

        state = TaskState.Finished;
        taskAction?.Invoke();

        taskAction = null;
        return true;
    }
}

public class RepeatTask : BaseTask
{
    public float peroidSeconds;

    public long executedTimes;

    public long maxExecuteTimes;

    public float NextPeriodSeconds => startSeconds + (executedTimes + 1) * peroidSeconds;

    public bool IsExceedLimitTimes
    {
        get
        {
            if (maxExecuteTimes <= 0)
            {
                return false;
            }

            return maxExecuteTimes <= executedTimes + 1;
        }
    }

    public override bool Execute(bool isBusy)
    {
        if (isCanceled)
            return true;

        if (!busyRun && isBusy)
        {
            return false;
        }

        taskAction?.Invoke();
        executedTimes++;
        return true;
    }
}

public class FrameTask : BaseTask
{
    public override bool Execute(bool isBusy)
    {
        if (isCanceled)
            return true;

        if (!busyRun && isBusy)
        {
            return false;
        }

        taskAction?.Invoke();
        return true;
    }
}

public enum TaskState
{
    NotStart,
    Started,
    Finished
}


public class TimerSystem : AbstractSystem
{
    private LinkedList<DelayTask> mDelayTasks = new LinkedList<DelayTask>();
    private LinkedList<RepeatTask> mRepeatTasks = new LinkedList<RepeatTask>();
    private Queue<FrameTask> mFrameTasks = new Queue<FrameTask>(64);

    public float CurrentSeconds { get; private set; }

    private MonoBehaviourSystem _monoBehaviour;
    
    protected override void OnInit()
    {
        _monoBehaviour = this.GetSystem<MonoBehaviourSystem>();

        CurrentSeconds = 0;

        this.RegisterEvent<UpdateEvent>(OnUpdate);
    }
    
    protected override void OnReset()
    {
        CurrentSeconds = 0;
        
        mDelayTasks.Clear();
        mRepeatTasks.Clear();
        mFrameTasks.Clear();
        
        this.UnRegisterEvent<UpdateEvent>(OnUpdate);
    }

    private void OnUpdate(UpdateEvent e)
    {
        CurrentSeconds += Time.deltaTime;

        while (mFrameTasks.Count > 0)
        {
            var task = mFrameTasks.Peek();
            if (task != null && task.Execute(_monoBehaviour.IsBusy))
            {
                mFrameTasks.Dequeue();
            }
        }

        if (mDelayTasks.Count > 0)
        {
            var currentNode = mDelayTasks.First;

            while (currentNode != null)
            {
                var nextNode = currentNode.Next;
                var delayTask = currentNode.Value;

                if (delayTask.state == TaskState.NotStart)
                {
                    delayTask.state = TaskState.Started;
                    delayTask.startSeconds = CurrentSeconds;
                    delayTask.finishSeconds = CurrentSeconds + delayTask.seconds;
                }
                else if (delayTask.state == TaskState.Started)
                {
                    if (CurrentSeconds >= delayTask.finishSeconds)
                    {
                        if (delayTask.Execute(_monoBehaviour.IsBusy))
                            mDelayTasks.Remove(currentNode);
                    }
                }

                currentNode = nextNode;
            }
        }

        if (mRepeatTasks.Count > 0)
        {
            var currentNode = mRepeatTasks.First;

            while (currentNode != null)
            {
                var nextNode = currentNode.Next;
                var repeatTask = currentNode.Value;

                if (CurrentSeconds >= repeatTask.NextPeriodSeconds)
                {
                    if (repeatTask.Execute(_monoBehaviour.IsBusy) && repeatTask.IsExceedLimitTimes)
                    {
                        mRepeatTasks.Remove(repeatTask);
                    }
                }

                currentNode = nextNode;
            }
        }
    }

    public IEnumerator ExecuteBySlice<T>(IEnumerable<T> enumerator, Action<T> action, float sliceTime = .1f,
        Action onSkipFrame = null)
    {
        if (action == null)
        {
            Debug.LogError("Null action used by ExecuteBySlice");
            yield break;
        }

        var e = enumerator.GetEnumerator();

        while (e.MoveNext())
        {
            if (_monoBehaviour.ElapsedTime >= sliceTime)
            {
                yield return null;
                if (onSkipFrame != null)
                {
                    onSkipFrame();
                }
            }

            action.Invoke(e.Current);
        }

        e.Dispose();
    }

    public IEnumerator ExecuteBySlice(IEnumerable<Action> enumerator, float sliceTime = .1f, Action onSkipFrame = null)
    {
        var e = enumerator.GetEnumerator();
        while (e.MoveNext())
        {
            if (_monoBehaviour.ElapsedTime >= sliceTime)
            {
                yield return null;
                if (onSkipFrame != null)
                {
                    onSkipFrame();
                }
            }


            if (e.Current != null) e.Current.Invoke();
        }

        e.Dispose();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="action"></param>
    /// <param name="busyRun">是否在该帧繁忙时执行，默认 false</param>
    public DelayTask AddDelayTask(float delay, Action action, bool busyRun = false)
    {
        var delayTask = new DelayTask()
        {
            seconds = delay,
            taskAction = action,
            state = TaskState.NotStart,
            busyRun = busyRun
        };

        mDelayTasks.AddLast(delayTask);
        return delayTask;
    }

    public bool RemoveDelayTask(DelayTask delayTask)
    {
        if (!mDelayTasks.Contains(delayTask)) return false;
        mDelayTasks.Remove(delayTask);
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="period"></param>
    /// <param name="action"></param>
    /// <param name="busyRun">是否在该帧繁忙时执行，默认 false</param>
    /// <param name="maxTimes"></param>
    public RepeatTask AddRepeatTask(float period, Action action, bool busyRun = false, int maxTimes = 0)
    {
        var repeatTask = new RepeatTask()
        {
            peroidSeconds = period,
            taskAction = action,
            executedTimes = 0,
            maxExecuteTimes = maxTimes,
            startSeconds = CurrentSeconds,
            busyRun = busyRun
        };
        mRepeatTasks.AddLast(repeatTask);
        return repeatTask;
    }

    public bool RemoveRepeatTask(RepeatTask repeatTask)
    {
        if (!mRepeatTasks.Contains(repeatTask)) return false;
        mRepeatTasks.Remove(repeatTask);
        return true;
    }

    public void AddFrameTask(Action action, bool busyRun = false)
    {
        var frameTask = new FrameTask()
        {
            taskAction = action,
            startSeconds = CurrentSeconds,
            busyRun = busyRun
        };
        mFrameTasks.Enqueue(frameTask);
    }

    public void AddFrameTask(Action[] actions, bool busyRun = false)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            var a = actions[i];
            AddFrameTask(a, busyRun);
        }
    }
}