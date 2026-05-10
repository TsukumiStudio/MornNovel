#if USE_ARBOR
using Arbor;
#else
using MornLib;
using StateBehaviour = MornLib.MornStateBehaviour;
#endif
using UnityEditor;
using System;
using UnityEngine;

namespace MornLib
{
    [Serializable]
    public abstract class MornNovelCommandBase : StateBehaviour
    {
        public virtual Color? CommandColor => null;
        public virtual string Tips { get; }
    }

#if UNITY_EDITOR && USE_ARBOR
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