#if USE_LUA
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MornLib
{
    /// <summary>Luaノベルで使用するアセットのルックアップ設定</summary>
    [CreateAssetMenu(fileName = nameof(MornNovelLuaSettingsSo), menuName = "Morn/" + nameof(MornNovelLuaSettingsSo))]
    internal sealed class MornNovelLuaSettingsSo : ScriptableObject
    {
        [Header("背景")]
        [SerializeField] private List<SpriteEntry> _backgroundEntries;

        [Header("キャラクター（キー → ポーズ）")]
        [SerializeField] private List<PoseEntry> _characterEntries;

        [Header("話者名（表示名 → Talker）")]
        [SerializeField] private List<TalkerEntry> _talkerEntries;

        [Header("SE")]
        [SerializeField] private List<AudioEntry> _seEntries;

        public Sprite FindBackground(string key)
        {
            if (_backgroundEntries == null)
            {
                return null;
            }

            foreach (var entry in _backgroundEntries)
            {
                if (entry.Key == key)
                {
                    return entry.Sprite;
                }
            }

            Debug.LogWarning($"[MornNovelLua] 背景 '{key}' が見つかりません");
            return null;
        }

        public MornNovelPoseSo FindCharacter(string key)
        {
            if (_characterEntries == null)
            {
                return null;
            }

            foreach (var entry in _characterEntries)
            {
                if (entry.Key == key)
                {
                    return entry.Pose;
                }
            }

            Debug.LogWarning($"[MornNovelLua] キャラクター '{key}' が見つかりません");
            return null;
        }

        public MornNovelTalkerSo FindTalker(string name)
        {
            if (_talkerEntries == null)
            {
                return null;
            }

            foreach (var entry in _talkerEntries)
            {
                if (entry.DisplayName == name)
                {
                    return entry.Talker;
                }
            }

            return null;
        }

        public AudioClip FindSe(string key)
        {
            if (_seEntries == null)
            {
                return null;
            }

            foreach (var entry in _seEntries)
            {
                if (entry.Key == key)
                {
                    return entry.Clip;
                }
            }

            Debug.LogWarning($"[MornNovelLua] SE '{key}' が見つかりません");
            return null;
        }

        [Serializable]
        public sealed class SpriteEntry
        {
            public string Key;
            public Sprite Sprite;
        }

        [Serializable]
        public sealed class PoseEntry
        {
            public string Key;
            public MornNovelPoseSo Pose;
        }

        [Serializable]
        public sealed class TalkerEntry
        {
            [Tooltip("Luaスクリプト内のmessage()で使う表示名")]
            public string DisplayName;
            public MornNovelTalkerSo Talker;
        }

        [Serializable]
        public sealed class AudioEntry
        {
            public string Key;
            public AudioClip Clip;
        }
    }
}
#endif
