using System;
using System.Collections;
using QFramework;
using UnityEngine;
using Object = UnityEngine.Object;

public class MonoBehaviourSystem : AbstractSystem
{
    [DisallowMultipleComponent]
    public class MonoBehaviourScript : MonoBehaviour
    {
        public event Action onUpdate;
        public event Action onLateUpdate;
        public event Action onDestroy;
        public event Action<bool> onApplicationPause;
        public event Action<bool> onApplicationFocus;
        public event Action onApplicationQuit;
        
        private int _awakeAtFrameCount;
        
        private float _lastTime;
        
        public float ElapsedTime => Time.realtimeSinceStartup - _lastTime;
        
        public bool IsBusy => ElapsedTime > 0.032f;
        
        private void Awake()
        {
            _awakeAtFrameCount = Time.frameCount;
            _lastTime = Time.realtimeSinceStartup;
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        
        private void Update()
        {
            _lastTime = Time.realtimeSinceStartup;
            onUpdate?.Invoke();
        }
        
        private void LateUpdate()
        {
            onLateUpdate?.Invoke();
        }
        
        private void OnDestroy()
        {
            onDestroy?.Invoke();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (_awakeAtFrameCount != Time.frameCount)
                onApplicationPause?.Invoke(pauseStatus);
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (_awakeAtFrameCount != Time.frameCount)
                onApplicationFocus?.Invoke(hasFocus);
        }
        
        private void OnApplicationQuit()
        {
            onApplicationQuit?.Invoke();
        }
    }
    
    private MonoBehaviourScript _monoBehaviour;
    
    public bool IsBusy => _monoBehaviour.IsBusy;
    
    public float ElapsedTime => _monoBehaviour.ElapsedTime;
    
    public static WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();
    
    protected override void OnInit()
    {
        _monoBehaviour = Object.FindObjectOfType<MonoBehaviourScript>();
        
        if (!_monoBehaviour)
        {
            var gameObject = GameObject.Find(nameof(MonoBehaviourScript));
            if (gameObject)
            {
                UnregisterUnityEvents();
                
                if (!Application.isPlaying)
                {
                    Object.DestroyImmediate(gameObject);
                }
                else
                {
                    Object.Destroy(gameObject);
                }
            }
            
            gameObject = new GameObject
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _monoBehaviour = gameObject.AddComponent<MonoBehaviourScript>();
            
            UnregisterUnityEvents();
            RegisterUnityEvents();
            
            StartCoroutine(MonitorEndOfFrame());
        }
    }
    
    protected override void OnReset()
    {
        UnregisterUnityEvents();
    }
    
    private void RegisterUnityEvents()
    {
        _monoBehaviour.onUpdate += OnUpdate;
        _monoBehaviour.onLateUpdate += OnLateUpdate;
        _monoBehaviour.onDestroy += OnDestroy;
        _monoBehaviour.onApplicationPause += OnApplicationPause;
        _monoBehaviour.onApplicationFocus += OnApplicationFocus;
        _monoBehaviour.onApplicationQuit += OnApplicationQuit;
    }
    
    private void UnregisterUnityEvents()
    {
        _monoBehaviour.onUpdate -= OnUpdate;
        _monoBehaviour.onLateUpdate -= OnLateUpdate;
        _monoBehaviour.onDestroy -= OnDestroy;
        _monoBehaviour.onApplicationPause -= OnApplicationPause;
        _monoBehaviour.onApplicationFocus -= OnApplicationFocus;
        _monoBehaviour.onApplicationQuit -= OnApplicationQuit;
    }
    
    private void OnUpdate()
    {
        this.SendEvent(new UpdateEvent());
    }
    
    private void OnLateUpdate()
    {
        this.SendEvent(new LateUpdateEvent());
    }
    
    private void OnDestroy()
    {
        this.SendEvent(new DestroyEvent());
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        this.SendEvent(new ApplicationPauseEvent(pauseStatus));
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        this.SendEvent(new ApplicationFocusEvent(hasFocus));
    }
    
    private void OnApplicationQuit()
    {
        this.SendEvent(new ApplicationQuitEvent());
    }
    
    public Coroutine StartCoroutine(IEnumerator enumerator)
    {
        return _monoBehaviour.StartCoroutine(enumerator);
    }
    
    public void StopCoroutine(Coroutine coroutine)
    {
        _monoBehaviour.StopCoroutine(coroutine);
    }
    
    private IEnumerator MonitorEndOfFrame()
    {
        while (true)
        {
            yield return _waitForEndOfFrame;
            this.SendEvent(new EndOfFrameEvent());
        }
    }
}