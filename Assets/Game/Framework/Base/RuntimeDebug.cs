using System;
using MOYV;

namespace UnityEngine
{
    public class RuntimeDebug : MonoBehaviour
    {
        public enum CommandPosEnum
        {
            LeftTop,
            Top,
            RightTop,
            Right,
            RightBottom,
            Bottom,
            LeftBottom,
            Left
        }

        [Range(0.1f, 0.5f)] public float areaRatio = 0.2f;
        public CommandPosEnum[] CommandPositions;
        public event Action<bool> OnEnableStateChanged;

        private int areaInPixel => (int) (Screen.width * areaRatio);
        private int currentInx;

        [HideInInspector] public bool IsSomethingEnabled = false;

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (CommandPositions.IsEmpty())
            {
                return;
            }

            if (currentInx >= CommandPositions.Length)
            {
                currentInx = 0;
                IsSomethingEnabled = !IsSomethingEnabled;
                if (OnEnableStateChanged != null)
                    OnEnableStateChanged(IsSomethingEnabled);
                Debug.LogWarning("Succeed: " + name + "enabled " + IsSomethingEnabled);
            }

            if (Input.GetMouseButtonDown(0))
            {
                Check();
            }
        }

        private void Check()
        {
            if (ValidateClickPos(CommandPositions[currentInx]))
            {
                Debug.Log("Passed: " + name + "     " + CommandPositions[currentInx] + "    <----" + currentInx);
                currentInx++;
            }
            else
            {
                if (currentInx > 1)
                    Debug.Log("failed: " + CommandPositions[currentInx] + "    <----" + currentInx);
                currentInx = 0;
                if (ValidateClickPos(CommandPositions[currentInx]))
                {
                    Debug.Log("Passed: " + name + "     " + CommandPositions[currentInx] + "    <----" + currentInx);
                    currentInx++;
                }
            }
        }

        private bool ValidateClickPos(CommandPosEnum targetPosEnum)
        {
            var p = Input.mousePosition;
            switch (targetPosEnum)
            {
                case CommandPosEnum.LeftTop:
                    return Vector2.Distance(p, new Vector2(0, Screen.height)) <= areaInPixel;
                case CommandPosEnum.Top:
                    return Vector2.Distance(p, new Vector2(Screen.width * 0.5f, Screen.height)) <= areaInPixel;
                    break;
                case CommandPosEnum.RightTop:
                    return Vector2.Distance(p, new Vector2(Screen.width, Screen.height)) <= areaInPixel;
                    break;
                case CommandPosEnum.Right:
                    return Vector2.Distance(p, new Vector2(Screen.width, Screen.height * 0.5f)) <= areaInPixel;
                    break;
                case CommandPosEnum.RightBottom:
                    return Vector2.Distance(p, new Vector2(Screen.width, 0)) <= areaInPixel;
                    break;
                case CommandPosEnum.Bottom:
                    return Vector2.Distance(p, new Vector2(Screen.width * .5f, 0)) <= areaInPixel;
                    break;
                case CommandPosEnum.LeftBottom:
                    return Vector2.Distance(p, new Vector2(0, 0)) <= areaInPixel;
                    break;
                case CommandPosEnum.Left:
                    return Vector2.Distance(p, new Vector2(0, Screen.height * 0.5f)) <= areaInPixel;
                    break;
            }


            return false;
        }

        public static RuntimeDebug CreateRuntimeCommand(string cmdName, CommandPosEnum[] cmds,
            Action<bool> onEnableStateChanged)
        {
            var runtimeScript = new GameObject(cmdName).AddComponent<RuntimeDebug>();
            runtimeScript.CommandPositions = cmds;
            runtimeScript.OnEnableStateChanged += onEnableStateChanged;
            return runtimeScript;
        }


        public void SetOnEnableStateChangedCallback(Action<bool> callback)
        {
            OnEnableStateChanged += callback;
        }

#if UNITY_EDITOR
        public bool showGizmos = false;

        private GUIStyle _myStyle;

        private GUIStyle myStyle
        {
            get
            {
                if (_myStyle != null) return _myStyle;

                if (!tex)
                    tex = CreateCircle(128);
                return _myStyle = new GUIStyle(GUI.skin.textField)
                {
                    alignment = TextAnchor.MiddleCenter,

                    fontSize = 200,
                    normal = new GUIStyleState
                    {
                        background = tex
                    },
                    active = new GUIStyleState
                    {
                        background = tex
                    },
                    hover = new GUIStyleState
                    {
                        background = tex
                    },
                    focused = new GUIStyleState
                    {
                        background = tex
                    }
                };
            }
        }

        [SerializeField]
        private Texture2D tex;

        private void OnGUI()
        {
            if (!showGizmos)
            {
                return;
            }

            var size = new Vector2(areaInPixel, areaInPixel) * 2f;
            for (var i = CommandPositions.Length - 1; i >= 0; i--)
            {
                var cp = CommandPositions[i];
                var pos = Vector2.zero;

                switch (cp)
                {
                    case CommandPosEnum.LeftTop:
                        myStyle.alignment = TextAnchor.LowerRight;
                        pos = new Vector2(0, 0);
                        break;
                    case CommandPosEnum.Top:
                        myStyle.alignment = TextAnchor.LowerCenter;
                        pos = new Vector2(Screen.width * .5f, 0);

                        break;
                    case CommandPosEnum.RightTop:
                        myStyle.alignment = TextAnchor.LowerLeft;
                        pos = new Vector2(Screen.width, 0);

                        break;
                    case CommandPosEnum.Right:
                        myStyle.alignment = TextAnchor.MiddleLeft;
                        pos = new Vector2(Screen.width, Screen.height * .5f);

                        break;
                    case CommandPosEnum.RightBottom:
                        myStyle.alignment = TextAnchor.UpperLeft;
                        pos = new Vector2(Screen.width, Screen.height);

                        break;
                    case CommandPosEnum.Bottom:
                        myStyle.alignment = TextAnchor.UpperCenter;
                        pos = new Vector2(Screen.width * .5f, Screen.height);

                        break;
                    case CommandPosEnum.LeftBottom:
                        myStyle.alignment = TextAnchor.UpperRight;
                        pos = new Vector2(0, Screen.height);

                        break;
                    case CommandPosEnum.Left:
                        myStyle.alignment = TextAnchor.MiddleRight;
                        pos = new Vector2(0, Screen.height * .5f);
                        break;
                }

                pos -= size * 0.5f;
                GUI.color = new Color(0, 0.8f, 0.6f, 0.05f);
                GUI.TextField(new Rect(pos, size), i.ToString(), myStyle);
            }
        }


        public static Texture2D CreateCircle(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    var d = Vector2.Distance(new Vector2(i, j), new Vector2(size * 0.5f, size * 0.5f));
                    d = (size - 2 * d) / size;
                    texture.SetPixel(i, j,
                        new Color(1, 1, 1,
                            d > 0.05f ? 1 : d));
                }
            }

            texture.Apply();
            return texture;
        }

#endif
    }
}