using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace MornLib
{
    internal static class MornNovelAddressAssigner
    {
        private static bool TryLoadPrefabWithMornNovelMono(string assetPath, out GameObject prefab)
        {
            prefab = null;
            
            try
            {
                // GameObjectとして読み込めるかチェック
                var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                if (assetType != typeof(GameObject))
                {
                    MornNovelGlobal.Log($"スキップ (GameObjectではない): {assetPath} (Type: {assetType})");
                    return false;
                }
                
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab == null || prefab.GetComponent<MornNovelMono>() == null)
                {
                    return false;
                }
                
                return true;
            }
            catch (Exception e)
            {
                MornNovelGlobal.LogError($"エラー発生: {assetPath} - {e.Message}");
                return false;
            }
        }
        private static void CleanUpAddressables()
        {
            var global = MornNovelGlobal.I;
            if (global == null)
            {
                MornNovelGlobal.LogError($"{nameof(MornNovelGlobal)} が見つかりません。");
                return;
            }

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                MornNovelGlobal.LogError("Addressable Asset Settings が見つかりません。");
                return;
            }

            var groupName = global.AddressGroupName;
            var labelTag = global.AddressLabelTag;
            var group = settings.FindGroup(groupName);
            if (group == null)
            {
                MornNovelGlobal.LogError($"グループ {groupName} が見つかりません。");
                return;
            }

            // 削除対象のエントリを収集
            var entriesToRemove = new List<AddressableAssetEntry>();
            foreach (var entry in group.entries.ToList())
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(entry.guid);

                // Novelラベルが付いていないエントリを削除対象にする
                if (!entry.labels.Contains(labelTag))
                {
                    entriesToRemove.Add(entry);
                    MornNovelGlobal.Log($"削除対象: {entry.address} ({assetPath})");
                }
                // Novelラベルが付いていても、MornNovelMonoを持たないプレハブは削除対象
                else if (assetPath.EndsWith(".prefab"))
                {
                    if (!TryLoadPrefabWithMornNovelMono(assetPath, out _))
                    {
                        entriesToRemove.Add(entry);
                        MornNovelGlobal.Log($"削除対象 (MornNovelMonoなし): {entry.address} ({assetPath})");
                    }
                }
            }

            // エントリを削除
            foreach (var entry in entriesToRemove)
            {
                settings.RemoveAssetEntry(entry.guid);
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryRemoved, null, true);
            AssetDatabase.SaveAssets();
            MornNovelGlobal.Log($"クリーンアップ完了: {entriesToRemove.Count} 個のエントリを削除しました。");
        }

        [MenuItem("Tools/MornNovelアドレス設定")]
        public static void SetAddressables()
        {
            var global = MornNovelGlobal.I;
            if (global == null)
            {
                MornNovelGlobal.LogError($"{nameof(MornNovelGlobal)} が見つかりません。");
                return;
            }

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                MornNovelGlobal.LogError("Addressable Asset Settings が見つかりません。");
                return;
            }

            // Main グループを取得または作成
            var groupName = global.AddressGroupName;
            var ignoreAddressPrefix = global.IgnoreAddressPrefix;
            var labelTag = global.AddressLabelTag;
            var group = settings.FindGroup(groupName);
            if (group == null)
            {
                group = settings.CreateGroup(groupName, false, false, false, null);
                MornNovelGlobal.Log($"グループ {groupName} を作成しました。");
            }

            // プレハブを検索
            var guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                
                if (!TryLoadPrefabWithMornNovelMono(assetPath, out _))
                {
                    continue;
                }

                var address = assetPath;
                if (!string.IsNullOrEmpty(ignoreAddressPrefix) && address.StartsWith(ignoreAddressPrefix))
                {
                    address = address[ignoreAddressPrefix.Length..];
                }

                if (address.EndsWith(".prefab"))
                {
                    address = address[..^7];
                }

                // 既に設定されている場合はスキップ
                var entry = settings.FindAssetEntry(guid);
                if (entry == null
                    || entry.parentGroup != group
                    || entry.address != address
                    || entry.labels.Count != 1
                    || !entry.labels.Contains(labelTag))
                {
                    entry = settings.CreateOrMoveEntry(guid, group);
                    entry.address = address;
                    entry.SetLabel(labelTag, true, true);
                    MornNovelGlobal.Log($"アドレス {address} を設定しました。");

                    // 一旦無視
                    //　依存しているアセットのアドレスを設定
                    // var dependencies = AssetDatabase.GetDependencies(assetPath, true);
                    // foreach (var dependency in dependencies)
                    // {
                    //     if (dependency == assetPath)
                    //     {
                    //         continue;
                    //     }
                    //
                    //     var depGuid = AssetDatabase.AssetPathToGUID(dependency);
                    //     var depEntry = settings.FindAssetEntry(depGuid);
                    //     if (depEntry == null)
                    //     {
                    //         depEntry = settings.CreateOrMoveEntry(depGuid, group);
                    //         depEntry.address = dependency;
                    //         depEntry.SetLabel(labelTag, true, true);
                    //         MornNovelGlobal.Log($"アドレス {dependency} を設定しました。");
                    //     }
                    // }
                }
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
            AssetDatabase.SaveAssets();
            MornNovelGlobal.Log("MornNovelMono の Addressable 設定が完了しました。");
        }

        [MenuItem("Tools/MornNovelアドレス再設定 (クリーンアップも実行)")]
        private static void ResetAddressables()
        {
            CleanUpAddressables();
            SetAddressables();
        }
    }
}