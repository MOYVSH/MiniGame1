using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[InitializeOnLoad]
public static class CustomEditorToolbar
{
    #region TimeScale Slider

    private static FloatField _timeScaleField;

    private static double _timeScaleLabelClickTime;


    private static void InitializeTimeScaleSlider()
    {
        // Memo Unity Bug: https://issuetracker.unity3d.com/product/unity/issues/guid/UUM-40353
        // The text input field may not display correctly.
        _timeScaleField = new FloatField("Time Scale")
        {
            value = Time.timeScale,
            isDelayed = true,
            style =
            {
                width = 110,
            },
        };
        _timeScaleField.labelElement.style.minWidth = 0;
        _timeScaleField.labelElement.style.flexShrink = 1;
        _timeScaleField.labelElement.tooltip = "Double-click to reset time scale to 1.\n" +
                                               "Right-click to display shortcut options.";
        _timeScaleField.labelElement.RegisterCallback<ClickEvent>(OnClickTimeScaleLabel);
        _timeScaleField.labelElement.RegisterCallback<ContextClickEvent>(OnContextClickTimeScaleLabel);
        // Workaround for Unity Bug UUM-40353
        _timeScaleField.Q("unity-text-input").style.overflow = Overflow.Visible;
        _timeScaleField.RegisterValueChangedCallback(evt =>
        {
            var timeScale = evt.newValue;
            if (timeScale < 0) timeScale = 0;
            var timeMgrAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TimeManager.asset")[0];
            Undo.RecordObject(timeMgrAsset, "Change Time Scale");
            Time.timeScale = timeScale;
        });
        _customToolbarRight.Add(_timeScaleField);
    }

    private static void UpdateTimeScaleField()
    {
        if (!Mathf.Approximately(Time.timeScale, _timeScaleField.value))
        {
            _timeScaleField.SetValueWithoutNotify(Time.timeScale);
        }
    }

    private static void OnClickTimeScaleLabel(ClickEvent evt)
    {
        evt.StopPropagation();

        // Reset TimeScale to 1 on double click
        var time = EditorApplication.timeSinceStartup;
        if (time - _timeScaleLabelClickTime < 0.3f)
        {
            _timeScaleLabelClickTime = 0;
            _timeScaleField.value = 1;
        }
        else
        {
            _timeScaleLabelClickTime = time;
        }
    }

    private static void OnContextClickTimeScaleLabel(ContextClickEvent evt)
    {
        evt.StopPropagation();

        float[] timeScaleOptions = { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 1.5f, 2f, };
        TimeScaleSelectWindow.Popup(GUIUtility.GUIToScreenPoint(evt.mousePosition),
            timeScaleOptions, newTimeScale => _timeScaleField.value = newTimeScale);
    }


    class TimeScaleSelectWindow : EditorWindow
    {
        public static void Popup(Vector2 screenPosition, float[] options, Action<float> onSubmit)
        {
            var window = CreateInstance<TimeScaleSelectWindow>();
            window.position = new Rect(screenPosition.x, screenPosition.y, 80, GetWindowHeight(options.Length));
            window._onSubmit = onSubmit;
            window.ShowPopup();
            window.SetOptions(options);
        }

        static float GetWindowHeight(int optionCount)
        {
            return optionCount * (_BUTTON_HEIGHT + _BUTTON_MARGIN_V * 2) + _WINDOW_PADDING_V * 2;
        }

        private const float _WINDOW_PADDING_H = 1;
        private const float _WINDOW_PADDING_V = 2;
        private const float _BUTTON_MARGIN_V = 1;
        private const float _BUTTON_HEIGHT = 20;

        private float[] _options;
        private Action<float> _onSubmit;


        private void SetOptions(float[] options)
        {
            _options = options;

            for (int i = 0; i < _options.Length; i++)
            {
                int index = i;
                var button = new Button(() => Submit(index))
                {
                    text = $"{_options[i]:F2}x",
                    style =
                    {
                        marginTop = _BUTTON_MARGIN_V,
                        marginBottom = _BUTTON_MARGIN_V,
                        height = _BUTTON_HEIGHT,
                    }
                };
                rootVisualElement.Add(button);
            }
        }

        private void Submit(int index)
        {
            _onSubmit(_options[index]);
            Close();
        }

        private void OnEnable()
        {
            rootVisualElement.style.paddingLeft = _WINDOW_PADDING_H;
            rootVisualElement.style.paddingRight = _WINDOW_PADDING_H;
            rootVisualElement.style.paddingTop = _WINDOW_PADDING_V;
            rootVisualElement.style.paddingBottom = _WINDOW_PADDING_V;
        }

        private void OnLostFocus() => Close();
    }

    #endregion

    #region Buttons
    private static Button _refreshAssetButton;

    private static void InitializeRefreshBtn()
    {
        _refreshAssetButton = new Button(() =>
        {
            Debug.Log("Refresh Asset Button Clicked!");
            AssetDatabase.Refresh(); // 刷新资源数据库
        })
        {
            text = "资源刷新", // 按钮文本
        };

        // 设置按钮样式
        _refreshAssetButton.style.minWidth = 40; // 设置宽度
        _refreshAssetButton.style.height = 30; // 设置高度
        _refreshAssetButton.style.marginLeft = 5; // 设置左边距
        _refreshAssetButton.style.marginRight = 5; // 设置右边距
        _refreshAssetButton.style.overflow = Overflow.Visible;

        // 将按钮添加到自定义工具栏的左侧容器中
        _customToolbarLeft.Add(_refreshAssetButton);
    }



    private static Button _enterPlaymodeButton;

    private static void InitializeEnterPlaymodeBtn()
    {

        _enterPlaymodeButton = new Button(() =>
        {
            Debug.Log("启动场景!");
            EditorSceneManager.OpenScene(SceneUtility.GetScenePathByBuildIndex(0));
            EditorApplication.EnterPlaymode();
        })
        {
            text = "启动场景", // 按钮文本
        };

        // 设置按钮样式
        _enterPlaymodeButton.style.minWidth = 40; // 设置宽度
        _enterPlaymodeButton.style.height = 30; // 设置高度
        _enterPlaymodeButton.style.marginLeft = 5; // 设置左边距
        _enterPlaymodeButton.style.marginRight = 5; // 设置右边距
        _enterPlaymodeButton.style.overflow = Overflow.Visible;

        // 将按钮添加到自定义工具栏的左侧容器中
        _customToolbarLeft.Add(_enterPlaymodeButton);
    }

    #endregion


    #region Custom Left UI

    private static void InitializeCustomLeftUI() 
    {
        // RefreshBtn
        InitializeRefreshBtn();
        // EnterPlaymodeBtn
        InitializeEnterPlaymodeBtn();
    }

    private static void UpdateCustomLeftUI() { }

    #endregion


    #region Custom Right UI

    private static void InitializeCustomRightUI()
    {
        // Time Scale
        InitializeTimeScaleSlider();

    }

    private static void UpdateCustomRightUI()
    {
        // Time Scale
        UpdateTimeScaleField();
    }

    #endregion


    #region Lifecycle

    private static VisualElement _toolbarRoot;
    private static VisualElement _toolbarLeft;
    private static VisualElement _toolbarRight;
    private static VisualElement _customToolbarLeft;
    private static VisualElement _customToolbarRight;


    static CustomEditorToolbar()
    {
        // EditorApplication.delayCall += TryInitialize;
        EditorApplication.update -= OnUpdate;
        EditorApplication.update += OnUpdate;
    }

    private static void TryInitialize()
    {
        if (_toolbarRoot != null)
        {
            return;
        }

        var toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
        var toolbarObj = toolbarType.GetField("get").GetValue(null);
        _toolbarRoot = (VisualElement)toolbarType.GetField("m_Root",
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(toolbarObj);
        _toolbarLeft = _toolbarRoot.Q("ToolbarZoneLeftAlign");
        _toolbarRight = _toolbarRoot.Q("ToolbarZoneRightAlign");

        _customToolbarLeft = new VisualElement
        {
            name = "custom-toolbar-left",
            style =
            {
                flexGrow = 1,
                flexDirection = FlexDirection.RowReverse,
                overflow = Overflow.Visible,
                alignItems = Align.Center, // 添加垂直居中对齐
            },
        };
        _toolbarLeft.Add(_customToolbarLeft);

        _customToolbarRight = new VisualElement
        {
            name = "custom-toolbar-right",
            style =
            {
                flexGrow = 1,
                flexDirection = FlexDirection.Row,
                overflow = Overflow.Visible,
                alignItems = Align.Center, // 添加垂直居中对齐
            },
        };
        _toolbarRight.Add(_customToolbarRight);

        InitializeCustomLeftUI();
        InitializeCustomRightUI();
    }

    private static void OnUpdate()
    {
        TryInitialize();
        UpdateCustomLeftUI();
        UpdateCustomRightUI();
    }

    #endregion
}