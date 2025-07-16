using System.Reflection;
using TriInspector;
using TriInspector.Drawers;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(DynamicMinMaxSliderDrawer), TriDrawerOrder.Decorator)]

namespace TriInspector.Drawers
{
    public class DynamicMinMaxSliderDrawer : TriAttributeDrawer<DynamicMinMaxSliderAttribute>
    {
        public override TriElement CreateElement(TriProperty property, TriElement next)
        {
            return new DynamicMinMaxSliderElement(property, Attribute);
        }

        private class DynamicMinMaxSliderElement : TriElement
        {
            private readonly DynamicMinMaxSliderAttribute _dynamicMinMaxSliderAttribute;
            private readonly TriProperty _property;

            public DynamicMinMaxSliderElement(TriProperty property,
                DynamicMinMaxSliderAttribute dynamicMinMaxSliderAttribute)
            {
                _property = property;
                _dynamicMinMaxSliderAttribute = dynamicMinMaxSliderAttribute;
            }

            public override float GetHeight(float width) => EditorGUIUtility.singleLineHeight;

            public override void OnGUI(Rect position)
            {
                var propertyType = _property.ValueType;

                var label = _property.DisplayNameContent;

                var minMethodValue = GetMemberValue(_property, _dynamicMinMaxSliderAttribute.MinMemberValue);
                var maxMethodValue = GetMemberValue(_property, _dynamicMinMaxSliderAttribute.MaxMemberValue);
                label.tooltip = minMethodValue.ToString("F2") + " to " +
                                maxMethodValue.ToString("F2");

                var controlRect = EditorGUI.PrefixLabel(position, label);

                var splitRect = SplitRect(controlRect, 3);

                if (propertyType == typeof(Vector2) && _property.Value != null)
                {
                    EditorGUI.BeginChangeCheck();

                    var vector = (Vector2) _property.Value;
                    var minValue = vector.x;
                    var maxValue = vector.y;
                    
                    minValue = EditorGUI.FloatField(splitRect[0], float.Parse(minValue.ToString("F2")));
                    maxValue = EditorGUI.FloatField(splitRect[2], float.Parse(maxValue.ToString("F2")));

                    EditorGUI.MinMaxSlider(splitRect[1], ref minValue, ref maxValue,
                        minMethodValue, maxMethodValue);

                    if (minValue < minMethodValue) minValue = minMethodValue;
                    if (maxValue > maxMethodValue) maxValue = maxMethodValue;

                    vector = new Vector2(minValue > maxValue ? maxValue : minValue, maxValue);

                    if (EditorGUI.EndChangeCheck()) _property.SetValue(vector);
                }
                else if (propertyType == typeof(Vector2Int) && _property.Value != null)
                {
                    EditorGUI.BeginChangeCheck();


                    var vector = (Vector2Int) _property.Value;
                    var minValue = (float)vector.x;
                    var maxValue = (float)vector.y;
                    
                    minValue = EditorGUI.FloatField(splitRect[0], float.Parse(minValue.ToString("F2")));
                    maxValue = EditorGUI.FloatField(splitRect[2], float.Parse(maxValue.ToString("F2")));

                    EditorGUI.MinMaxSlider(splitRect[1], ref minValue, ref maxValue,
                        minMethodValue, maxMethodValue);

                    if (minValue < minMethodValue) minValue = minMethodValue;
                    if (maxValue > maxMethodValue) maxValue = maxMethodValue;

                    vector = new Vector2Int(Mathf.FloorToInt(minValue > maxValue ? maxValue : minValue),
                        Mathf.FloorToInt(maxValue));

                    if (EditorGUI.EndChangeCheck()) _property.SetValue(vector);
                }
                else
                {
                    EditorGUI.LabelField(position, label.text, 
                        "Use [DynamicMinMaxSlider] attribute with Vector2 or Vector2Int.");
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