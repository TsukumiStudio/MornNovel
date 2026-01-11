using System;
using System.Collections.Generic;
using System.Linq;
using MornEditor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace MornLib
{
    internal sealed class MornNovelAddressWindow : EditorWindow
    {
        private class Address
        {
            public string Key { get; set; }
        }

        private sealed class SceneAssetTree : MornEditorTreeBase<Address>
        {
            private Action<Address> _callback;

            public SceneAssetTree(Action<Address> callback, string originalPath) : base(originalPath)
            {
                _callback = callback;
            }

            protected override string NodeToPath(Address node)
            {
                return node.Key;
            }

            protected override void NodeClicked(Address node)
            {
                _callback?.Invoke(node);
            }
        }

        private SceneAssetTree _sceneAssetTree;
        private SerializedProperty _addressProperty;
        private List<string> _allAddress = new();
        private List<string> _filteredAddress = new();
        private readonly HashSet<string> _headerHashSet = new();
        private string _searchText;
        private Vector2 _scrollPos;

        public static void ShowWindow(SerializedProperty addressProperty)
        {
            var window = GetWindow<MornNovelAddressWindow>(nameof(MornNovelAddressWindow));
            window.Initialize(addressProperty);
            window.Show();
        }

        private void Initialize(SerializedProperty property)
        {
            _sceneAssetTree = new SceneAssetTree(
                address =>
                {
                    _addressProperty.stringValue = address.Key;
                    _addressProperty.serializedObject.ApplyModifiedProperties();
                    Close();
                },
                "");
            _addressProperty = property;
            _allAddress.Clear();
            _filteredAddress.Clear();
            _headerHashSet.Clear();
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                MornNovelGlobal.LogError("Addressable Asset Settings が見つかりません。");
                return;
            }

            var guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (var guid in guids)
            {
                var entry = settings.FindAssetEntry(guid);
                if (entry == null)
                {
                    continue;
                }
                
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab == null || prefab.GetComponent<MornNovelMono>() == null)
                {
                    continue;
                }

                _allAddress.Add(entry.address);
            }

            _allAddress = _allAddress.OrderBy(obj => obj.Split('/').Length).ToList();
            foreach (var address in _allAddress)
            {
                _filteredAddress.Add(address);
                _sceneAssetTree.Add(new Address { Key = address });
            }
        }

        private void OnGUI()
        {
            var cachedText = _searchText;
            _searchText = EditorGUILayout.TextField("Search", _searchText);
            if (cachedText != _searchText)
            {
                _filteredAddress.Clear();
                _sceneAssetTree.Clear();
                foreach (var address in _allAddress.Where(obj => obj.Contains(_searchText)))
                {
                    _filteredAddress.Add(address);
                    _sceneAssetTree.Add(new Address { Key = address });
                }
            }

            using var scroll = new EditorGUILayout.ScrollViewScope(_scrollPos);
            if (GUILayout.Button("Null"))
            {
                _addressProperty.stringValue = string.Empty;
                _addressProperty.serializedObject.ApplyModifiedProperties();
                Close();
            }
            
            _sceneAssetTree.OnGUI();
            /*
            _headerHashSet.Clear();
            foreach (var address in _filteredAddress)
            {
                var addressSplit = address.Split('/');
                _headerHashSet.RemoveWhere(header => !addressSplit.Contains(header));
                EditorGUI.indentLevel = _headerHashSet.Count - 1;
                for (var i = 0; i < addressSplit.Length - 1; i++)
                {
                    var header = addressSplit[i];
                    if (_headerHashSet.Add(header))
                    {
                        EditorGUI.indentLevel = _headerHashSet.Count - 1;
                        EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
                    }
                }

                using (new GUILayout.HorizontalScope())
                {
                    var cachedBackgroundColor = GUI.backgroundColor;
                    // propertyからアドレスを取得

                    // アドレスが一致している場合は背景色を変更
                    if (_addressProperty.stringValue == address)
                    {
                        GUI.backgroundColor = Color.cyan;
                    }

                    // ボタンもIndent
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(20 * _headerHashSet.Count);
                        if (GUILayout.Button(address))
                        {
                            _addressProperty.stringValue = address;
                            _addressProperty.serializedObject.ApplyModifiedProperties();
                            Close();
                        }
                    }

                    GUI.backgroundColor = cachedBackgroundColor;
                }
            }
            */

            _scrollPos = scroll.scrollPosition;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                Close();
            }
        }

        private void OnEnable()
        {
            wantsMouseMove = true;
        }

        private void Update()
        {
            if (focusedWindow != this)
            {
                Close();
            }
        }
    }
}