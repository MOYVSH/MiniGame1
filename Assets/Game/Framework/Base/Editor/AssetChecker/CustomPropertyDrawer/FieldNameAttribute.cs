using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MOYV.AssetsCheckerBase
{
    public class FieldNameAttribute : PropertyAttribute
    {
        /// <summary> 枚举名称 </summary>
        public string name = "";

        /// <summary> 文本颜色 </summary>
        public string htmlColor = "#000000";

        /// <summary> 重命名属性 </summary>
        /// <param name="name">新名称</param>
        public FieldNameAttribute(string name)
        {
            this.name = name;
        }

        public FieldNameAttribute(string name, bool randomColor)
        {
            this.name = name;
            if (randomColor)
            {
                var c = ColorUtility.ToHtmlStringRGB(Random.ColorHSV(0.01f, .11f, 1, 1, 1, 1));
                htmlColor = $"#{c}";
            }
        }

        /// <summary> 重命名属性 </summary>
        /// <param name="name">新名称</param>
        /// <param name="htmlColor">文本颜色 例如："#FFFFFF" 或 "black"</param>
        public FieldNameAttribute(string name, string htmlColor)
        {
            this.name = name;
            this.htmlColor = htmlColor;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(FieldNameAttribute))]
    public class FieldNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 替换属性名称
            var rename = (FieldNameAttribute) attribute;
            label.text = rename.name;

            // 重绘GUI
            var defaultColor = EditorStyles.label.normal.textColor;
            EditorStyles.label.fontStyle = FontStyle.Bold;
            EditorStyles.label.normal.textColor = htmlToColor(rename.htmlColor);
            var isElement = Regex.IsMatch(property.displayName, "Element \\d+");
            if (isElement) label.text = property.displayName;
            if (property.propertyType == SerializedPropertyType.Enum)
            {
                DrawEnum(position, property, label);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }

            EditorStyles.label.normal.textColor = defaultColor;
        }

        // 绘制枚举类型
        private void DrawEnum(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();

            // 获取枚举相关属性
            var type = fieldInfo.FieldType;
            var names = property.enumNames;
            var values = new string[names.Length];
            Array.Copy(names, values, names.Length);
            while (type.IsArray) type = type.GetElementType();

            // 获取枚举所对应的RenameAttribute
            for (var i = 0; i < names.Length; i++)
            {
                var info = type.GetField(names[i]);
                var atts =
                    (FieldNameAttribute[]) info.GetCustomAttributes(typeof(FieldNameAttribute), true);
                if (atts.Length != 0) values[i] = atts[0].name;
            }

            // 重绘GUI
            var index = EditorGUI.Popup(position, label.text, property.enumValueIndex, values);
            if (EditorGUI.EndChangeCheck() && index != -1) property.enumValueIndex = index;
        }

        /// <summary> Html颜色转换为Color </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static Color htmlToColor(string hex)
        {
            // 编辑器默认颜色
            if (string.IsNullOrEmpty(hex)) return new Color(0.705f, 0.705f, 0.705f);

#if UNITY_EDITOR
            // 转换颜色
            hex = hex.ToLower();
            if (hex.IndexOf("#") == 0 && hex.Length == 7)
            {
                var r = Convert.ToInt32(hex.Substring(1, 2), 16);
                var g = Convert.ToInt32(hex.Substring(3, 2), 16);
                var b = Convert.ToInt32(hex.Substring(5, 2), 16);
                return new Color(r / 255f, g / 255f, b / 255f);
            }
            else if (hex == "red")
            {
                return Color.red;
            }
            else if (hex == "green")
            {
                return Color.green;
            }
            else if (hex == "blue")
            {
                return Color.blue;
            }
            else if (hex == "yellow")
            {
                return Color.yellow;
            }
            else if (hex == "black")
            {
                return Color.black;
            }
            else if (hex == "white")
            {
                return Color.white;
            }
            else if (hex == "cyan")
            {
                return Color.cyan;
            }
            else if (hex == "gray")
            {
                return Color.gray;
            }
            else if (hex == "grey")
            {
                return Color.grey;
            }
            else if (hex == "magenta")
            {
                return Color.magenta;
            }
            else if (hex == "orange")
            {
                return new Color(1, 165 / 255f, 0);
            }
#endif

            return new Color(0.705f, 0.705f, 0.705f);
        }
    }
#endif
}