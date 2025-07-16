using System.Reflection;
using TriInspector;
using TriInspector.Drawers;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(DynamicSliderDrawer), TriDrawerOrder.Decorator)]

namespace TriInspector.Drawers
{
    public class DynamicSliderDrawer : TriAttributeDrawer<DynamicSliderAttribute>
    {
        public override TriElement CreateElement(TriProperty property, TriElement next)
        {
            return new DynamicSliderElement(property, Attribute);
        }

        private class DynamicSliderElement : TriElement
        {
            private readonly DynamicSliderAttribute _dynamicSliderAttribute;
            private readonly TriProperty _property;

            public DynamicSliderElement(TriProperty property,
                DynamicSliderAttribute dynamicSliderAttribute)
            {
                _property = property;
                _dynamicSliderAttribute = dynamicSliderAttribute;
            }

            public override float GetHeight(float width) => EditorGUIUtility.singleLineHeight;

            public override void OnGUI(Rect position)
            {
                var propertyType = _property.ValueType;
                var label = _property.DisplayNameContent;

                var min = GetMemberValue(_property, _dynamicSliderAttribute.MinMemberValue);
                var max = GetMemberValue(_property, _dynamicSliderAttribute.MaxMemberValue);

                label.tooltip = $"{min:F2} to {max:F2}";

                EditorGUI.BeginChangeCheck();

                if (propertyType == typeof(float) && _property.Value != null)
                {
                    var value = (float)_property.Value;

                    value = EditorGUI.Slider(position, label, value, min, max);

                    if (EditorGUI.EndChangeCheck())
                    {
                        _property.SetValue(value);
                    }
                }
                else if (propertyType == typeof(int) && _property.Value != null)
                {
                    float floatValue = (int)_property.Value;

                    floatValue = EditorGUI.Slider(position, label, floatValue, min, max);

                    var intValue = Mathf.RoundToInt(floatValue);

                    if (EditorGUI.EndChangeCheck())
                    {
                        _property.SetValue(intValue);
                    }
                }
                else
                {
                    EditorGUI.LabelField(position, label.text, "Use [DynamicSlider] with float or int.");
                }
            }

            private static Rect[] SplitRect(Rect rectToSplit, int n)
            {
                var rects = new Rect[n];

                for (var i = 0; i < n; i++)
                {
                    rects[i] = new Rect(rectToSplit.position.x + (i * rectToSplit.width / n), rectToSplit.position.y,
                        rectToSplit.width / n, rectToSplit.height);
                }

                var padding = (int) rects[0].width - 40;
                const int space = 5;

                rects[0].width -= padding + space;
                rects[2].width -= padding + space;

                rects[1].x -= padding;
                rects[1].width += padding * 2;

                rects[2].x += padding + space;

                return rects;
            }
            
            private static float GetMemberValue(TriProperty property, string memberName)
            {
                if (property.Owner.ValueType == null) return 0f;

                var type = property.Owner.ValueType;
                var targetObject = property.Owner.Value;
                if (targetObject == null) return 0f;

                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

                var methodInfo = type.GetMethod(memberName, flags);
                if (methodInfo != null && methodInfo.ReturnType == typeof(float))
                {
                    return (float) methodInfo.Invoke(targetObject, null);
                }

                var propertyInfo = type.GetProperty(memberName, flags);
                if (propertyInfo != null && propertyInfo.PropertyType == typeof(float) && propertyInfo.CanRead)
                {
                    return (float) propertyInfo.GetValue(targetObject);
                }

                Debug.LogError($"Member {memberName} not found or does not return float.");
                return 0f;
            }
        }
    }
}