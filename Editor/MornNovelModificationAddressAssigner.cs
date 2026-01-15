#if USE_ADDRESSABLE
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace MornLib
{
    [InitializeOnLoad]
    internal static class MornNovelModificationAddressAssigner
    {
        static MornNovelModificationAddressAssigner()
        {
            // Addressableエントリが変更されたときにイベントをフック
            AddressableAssetSettings.OnModificationGlobal += OnAddressablesModified;
        }

        private static void OnAddressablesModified(AddressableAssetSettings settings,
            AddressableAssetSettings.ModificationEvent evt,
            object obj)
        {
            if (evt != AddressableAssetSettings.ModificationEvent.EntryCreated
                && evt != AddressableAssetSettings.ModificationEvent.EntryMoved)
            {
                return;
            }

            if (obj is AddressableAssetEntry entry)
            {
                // Main グループを取得または作成
                var groupName = MornNovelGlobal.I.AddressGroupName;
                var group = settings.FindGroup(groupName);
                if (group == null)
                {
                    group = settings.CreateGroup(groupName, false, false, false, null);
                    MornNovelGlobal.Logger.Log($"グループ {groupName} を作成しました。");
                }

                var addressableAssets = new HashSet<string> { entry.AssetPath };
                List<string> assetsToAdd = new List<string>();

                // 追加・更新されたアセットの依存関係をチェック
                string[] dependencies = AssetDatabase.GetDependencies(entry.AssetPath, true);
                foreach (string dep in dependencies)
                {
                    if (!addressableAssets.Contains(dep) && !AssetDatabase.IsValidFolder(dep))
                    {
                        assetsToAdd.Add(dep);
                    }
                }

                // Addressableに追加
                foreach (string asset in assetsToAdd)
                {
                    var guid = AssetDatabase.AssetPathToGUID(asset);
                    var newEntry = settings.CreateOrMoveEntry(guid, group);
                    Debug.Log($"Added to Addressables: {asset}");
                }

                if (assetsToAdd.Count > 0)
                {
                    settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
                    AssetDatabase.SaveAssets();
                    Debug.Log("Updated Addressables with new dependencies.");
                }
            }
            else
            {
                Debug.LogWarning("Unexpected object type in Addressable modification event.");
            }
        }
    }
}
#endif