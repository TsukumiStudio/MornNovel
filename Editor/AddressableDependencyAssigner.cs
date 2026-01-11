using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace MornLib
{
    public static class AddressableDependencyAssigner
    {
        private static bool IsNg(string assetPath)
        {
            // .csファイルは除外
            if (assetPath.EndsWith(".cs"))
            {
                return true;
            }
                    
            // Packagesフォルダ内のアセットは除外
            if (assetPath.StartsWith("Packages/"))
            {
                return true;
            }
                    
            // HideFlags.DontSaveは除外
            var obj = AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (obj != null && (obj.hideFlags & HideFlags.DontSave) != 0)
            {
                return true;
            }

            return false;
        }
        
        [MenuItem("Tools/Addressableの依存アセットのアドレスを登録")]
        public static void AutoAssignDependencies()
        {
            
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("AddressableAssetSettings not found!");
                return;
            }

            HashSet<string> removeAssets = new HashSet<string>();
            HashSet<string> addressableAssets = new HashSet<string>();

            // 既存のAddressableアセットを取得
            foreach (var group in settings.groups)
            {
                foreach (var entry in group.entries)
                {
                    addressableAssets.Add(entry.AssetPath);
                    if (IsNg(entry.AssetPath))
                    {
                        removeAssets.Add(entry.AssetPath);
                    }
                }
            }

            foreach (var assetPath in removeAssets)
            {
                var guid = AssetDatabase.AssetPathToGUID(assetPath);
                settings.RemoveAssetEntry(guid);
                Debug.Log($"Removed from Addressables: {assetPath}");
            }

            // 依存関係を解析して、Addressableでないものを追加
            List<string> assetsToAdd = new List<string>();

            foreach (string assetPath in addressableAssets)
            {
                string[] dependencies = AssetDatabase.GetDependencies(assetPath, true);
                foreach (string depPath in dependencies)
                {
                    if (IsNg(depPath))
                    {
                        continue;
                    }
                    
                    if (!addressableAssets.Contains(depPath) && !AssetDatabase.IsValidFolder(depPath))
                    {
                        assetsToAdd.Add(depPath);
                    }
                }
            }

            // Addressableに追加
            foreach (string asset in assetsToAdd)
            {
                var guid = AssetDatabase.AssetPathToGUID(asset);
                var entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
                Debug.Log($"Added to Addressables: {asset}");
            }
            
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
            AssetDatabase.SaveAssets();
            Debug.Log("Addressable dependency assignment completed.");
        }
    }
}