#if USE_ADDRESSABLE
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace MornLib
{
    [CustomPropertyDrawer(typeof(MornNovelAddress))]
    internal sealed class MornNovelAddressDrawer : PropertyDrawer
    {
        private const int ButtonWidth = 60;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // ScriptableObject を通常の ObjectField で選択
            var normalRect = new Rect(position.x, position.y, position.width - ButtonWidth * 2, position.height);
            var buttonRect1 = new Rect(
                position.x + position.width - ButtonWidth * 2,
                position.y,
                ButtonWidth,
                position.height);
            var buttonRect2 = new Rect(
                position.x + position.width - ButtonWidth * 1,
                position.y,
                ButtonWidth,
                position.height);
            EditorGUI.BeginProperty(normalRect, label, property);
            {
                // Addressを描画
                var addressProperty = property.FindPropertyRelative("_address");
                {
                    var cachedEnabled = GUI.enabled;
                    GUI.enabled = false;
                    EditorGUI.PropertyField(normalRect, addressProperty, new GUIContent(label));
                    GUI.enabled = cachedEnabled;
                }
                if (GUI.Button(buttonRect1, "Select"))
                {
                    MornNovelAddressWindow.ShowWindow(addressProperty);
                }

                if (GUI.Button(buttonRect2, "Ping"))
                {
                    var address = addressProperty.stringValue;
                    var settings = AddressableAssetSettingsDefaultObject.Settings;
                    if (settings == null)
                    {
                        MornNovelGlobal.Logger.LogError("Addressable Asset Settings が見つかりません。");
                    }
                    else
                    {
                        var entry = settings.groups.SelectMany(g => g.entries)
                                            .FirstOrDefault(e => e.address == address);
                        if (entry == null)
                        {
                            MornNovelGlobal.Logger.LogError($"Addressable Asset {address} が見つかりません。");
                        }
                        else
                        {
                            //Selection.SetActiveObjectWithContext(entry.MainAsset, null);
                            EditorGUIUtility.PingObject(entry.MainAsset);
                        }
                    }
                }
            }
            EditorGUI.EndProperty();
        }
    }
}
#endif