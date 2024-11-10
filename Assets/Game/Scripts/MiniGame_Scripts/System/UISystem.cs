using System;
using System.Collections.Generic;
using System.Linq;
using MOYV;
using QFramework;
using UnityEngine;
using Object = UnityEngine.Object;

public class UISystem : AbstractUISystem
{
    public RectTransform UIRootRect { get; protected set; }
    protected override void SetupEnvironment()
    {
        GameObject go = Object.Instantiate(Resources.Load<GameObject>("UI/Prefab/UiRoot"));
        go.name = "UiRoot";
        Object.DontDestroyOnLoad(go);
        UIRootRect = go.GetComponent<RectTransform>();
        UIRoot = UIRootRect.transform;
        UICamera = UIRoot.Find("Camera").GetComponent<Camera>();
    }

    protected override void RegisterPanelsLoadPath()
    {
        AddPanelLoadPath<LoadingView>("LoadingView");
    }

    protected override Object LoadAsset(string path)
    {
        return this.GetUtility<YooassetUtility>().LoadAssetSync<GameObject>(path);
    }

}
