using Arbor;
using UnityEditor;
using UnityEngine;

namespace MornLib
{
    internal abstract class MornNovelCommandBase : StateBehaviour
    {
        public virtual Color? CommandColor => null;
        public virtual string Tips { get; }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MornNovelCommandBase), true)]
    public sealed class ColorCommandBaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var commandBase = (MornNovelCommandBase)target;
            EditorGUILayout.HelpBox(commandBase.Tips, MessageType.Info);
            var topColor = commandBase.CommandColor;
            if (topColor != null)
            {
                var backgroundColor = GUI.backgroundColor;
                var contentColor = GUI.contentColor;
                var color = GUI.color;
                GUI.backgroundColor = topColor.Value;
                GUI.contentColor = topColor.Value;
                GUI.color = topColor.Value;
                base.OnInspectorGUI();
                GUI.backgroundColor = backgroundColor;
                GUI.contentColor = contentColor;
                GUI.color = color;
            }
            else
            {
                base.OnInspectorGUI();
            }
        }
    }
#endif
}