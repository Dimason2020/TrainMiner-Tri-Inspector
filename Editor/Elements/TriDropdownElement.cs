using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace TriInspector.Elements
{
    public class TriDropdownElement : TriElement
    {
        private readonly TriProperty _property;
        private readonly Func<TriProperty, IEnumerable<ITriDropdownItem>> _valuesGetter;

        private object _currentValue;
        private string _currentText;

        public TriDropdownElement(TriProperty property, Func<TriProperty, IEnumerable<ITriDropdownItem>> valuesGetter)
        {
            _property = property;
            _valuesGetter = valuesGetter;
        }

        public override float GetHeight(float width)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position)
        {
            if (!_property.Comparer.Equals(_currentValue, _property.Value))
            {
                _currentValue = _property.Value;
                _currentText = _valuesGetter.Invoke(_property)
                    .FirstOrDefault(it => _property.Comparer.Equals(it.Value, _property.Value))
                    ?.Text ?? (_property.Value?.ToString() ?? string.Empty);
            }

            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            position = EditorGUI.PrefixLabel(position, controlId, _property.DisplayNameContent);

            if (GUI.Button(position, _currentText, EditorStyles.popup))
            {
                var dropdown = new TriDropdown(_property, _valuesGetter, new AdvancedDropdownState());
                
                dropdown.Show(position);

                Event.current.Use();
            }
        }
        
        private class TriDropdown : AdvancedDropdown 
        {
            private readonly TriProperty _property;
            private readonly Func<TriProperty, IEnumerable<ITriDropdownItem>> _valuesGetter;
            
            public TriDropdown(TriProperty property, Func<TriProperty, IEnumerable<ITriDropdownItem>> valuesGetter, AdvancedDropdownState state) : base(state)
            {
                _property = property;
                _valuesGetter = valuesGetter;
                
                minimumSize = new Vector2(0, 175);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem(_property.ValueType?.Name);

                var values = _valuesGetter.Invoke(_property);

                foreach (var value in values)
                {
                    root.AddChild(new TriDropdownItem(value));
                }

                return root;
            }
            
            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                if (!(item is TriDropdownItem enumItem))
                {
                    return;
                }

                if (enumItem.Value == null)
                {
                    _property.SetValue(null);
                }
                else
                {
                    _property.SetValue(enumItem.Value);
                    _property.PropertyTree.RequestRepaint();
                    GUI.changed = true;
                }
            }
            
            private class TriDropdownItem : AdvancedDropdownItem
            {
                public object Value { get; }

                public TriDropdownItem(ITriDropdownItem triDropdownItem, Texture2D preview = null) : base(triDropdownItem.Text)
                {
                    Value = triDropdownItem.Value;
                    icon = preview;
                }
            }
        }
    }
}