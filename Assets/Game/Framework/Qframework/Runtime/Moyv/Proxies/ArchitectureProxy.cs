using System;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using UnityEngine;

public interface IArchitectureProxy
{
    void RegisterSystem<T>(T system) where T : class, ISystem;

    void RegisterModel<T>(T model) where T : class, IModel;

    void RegisterUtility<T>(T utility) where T : class, IUtility;

    T GetSystem<T>() where T : class, ISystem;
    List<ISystem> GetAllSystems();

    T GetModel<T>() where T : class, IModel;
    IModel GetModel(Type type);
    List<IModel> GetAllModels();

    T GetUtility<T>() where T : class, IUtility;

    void SendCommand<T>() where T : ICommand, new();
    void SendCommand<T>(T command) where T : ICommand;

    TResult SendCommand<TResult>(ICommand<TResult> command);

    TResult SendQuery<TResult>(IQuery<TResult> query);

    void SendEvent<T>() where T : new();
    void SendEvent<T>(T e);

    void SendEventToMainThread<T>() where T : new();
    void SendEventToMainThread<T>(T e);

    IUnRegister RegisterEvent<T>(Action<T> onEvent);
    void UnRegisterEvent<T>(Action<T> onEvent);

    void ClearEvent();

    void ClearContainer();

    void Init();

    void Reinit();

    void Reset();

    void ClearAll();
}

public abstract class ArchitectureProxy<T> : IArchitectureProxy where T : ArchitectureProxy<T>, new()
{
    private static T mProxy;

    public static IArchitecture Interface
    {
        get
        {
            var architecture = GameArchitecture.Interface;
            architecture.RegisterProxy<T>();
            return architecture;
        }
    }

    public abstract void Init();

    public void RegisterSystem<TSystem>(TSystem system) where TSystem : class, ISystem
    {
        GameArchitecture.Interface.RegisterSystem(system);
    }

    public void RegisterModel<TModel>(TModel model) where TModel : class, IModel
    {
        GameArchitecture.Interface.RegisterModel(model);
    }

    public void RegisterUtility<TUtility>(TUtility utility) where TUtility : class, IUtility
    {
        GameArchitecture.Interface.RegisterUtility(utility);
    }

    public T1 GetSystem<T1>() where T1 : class, ISystem
    {
        return GameArchitecture.Interface.GetSystem<T1>();
    }

    public List<ISystem> GetAllSystems()
    {
        return GameArchitecture.Interface.GetAllSystems();
    }

    public T1 GetModel<T1>() where T1 : class, IModel
    {
        return GameArchitecture.Interface.GetModel<T1>();
    }

    public IModel GetModel(Type type)
    {
        return GameArchitecture.Interface.GetModel(type);
    }

    public List<IModel> GetAllModels()
    {
        return GameArchitecture.Interface.GetAllModels();
    }

    public T1 GetUtility<T1>() where T1 : class, IUtility
    {
        return GameArchitecture.Interface.GetUtility<T1>();
    }

    public void SendCommand<T1>() where T1 : ICommand, new()
    {
        GameArchitecture.Interface.SendCommand<T1>();
    }

    public void SendCommand<T1>(T1 command) where T1 : ICommand
    {
        GameArchitecture.Interface.SendCommand(command);
    }

    public TResult SendCommand<TResult>(ICommand<TResult> command)
    {
        return GameArchitecture.Interface.SendCommand(command);
    }

    public TResult SendQuery<TResult>(IQuery<TResult> query)
    {
        return GameArchitecture.Interface.SendQuery(query);
    }

    public void SendEvent<T1>() where T1 : new()
    {
        GameArchitecture.Interface.SendEvent<T1>();
    }

    public void SendEvent<T1>(T1 e)
    {
        GameArchitecture.Interface.SendEvent(e);
    }

    public void SendEventToMainThread<T1>() where T1 : new()
    {
        GameArchitecture.Interface.SendEventToMainThread<T1>();
    }

    public void SendEventToMainThread<T1>(T1 e)
    {
        GameArchitecture.Interface.SendEventToMainThread(e);
    }

    public IUnRegister RegisterEvent<T1>(Action<T1> onEvent)
    {
        return GameArchitecture.Interface.RegisterEvent(onEvent);
    }

    public void UnRegisterEvent<T1>(Action<T1> onEvent)
    {
        GameArchitecture.Interface.UnRegisterEvent(onEvent);
    }

    public void ClearEvent()
    {
        GameArchitecture.Interface.ClearEvent();
    }

    public void ClearContainer()
    {
        GameArchitecture.Interface.ClearContainer();
    }
    
    /// <summary>
    /// Reset后使用此方法重启所有的System和Model
    /// </summary>
    public void Reinit()
    {
        GameArchitecture.Interface.Reinit();
    }
    
    /// <summary>
    /// 调用每个System和Model的Reset方法
    /// </summary>
    public void Reset()
    {
        GameArchitecture.Interface.Reset();
    }
    
    /// <summary>
    /// 会清空所有的事件和实例
    /// </summary>
    public void ClearAll()
    {
        GameArchitecture.Interface.ClearAll();
    }
}