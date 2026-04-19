using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
#if USE_ADDRESSABLE
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

[assembly: InternalsVisibleTo("MornNovel.Editor")]
namespace MornLib
{
    [CreateAssetMenu(fileName = nameof(MornNovelGlobal), menuName = "Morn/" + nameof(MornNovelGlobal))]
    public sealed class MornNovelGlobal : MornGlobalBase<MornNovelGlobal>
    {
        [SerializeField] private string _addressGroupName = "Main";
        [SerializeField] private string _addressLabelTag = "Novel";
        [SerializeField] private string _ignoreAddressPrefix;
        [SerializeField] private MornSceneObject _novelScene;
        [SerializeField] private string _uploadUrl;
        public AudioClip SubmitClip;
        public float MessageOffset = 0.1f;
        public float CharInterval = 0.05f;
        public float CharReturnInterval = 0.1f;
        public MornTransitionType DebugTransition;
        protected override string ModuleName => "MornNovel";
        public string AddressGroupName => _addressGroupName;
        public string AddressLabelTag => _addressLabelTag;
        public string IgnoreAddressPrefix => _ignoreAddressPrefix;
        public MornSceneObject NovelScene => _novelScene;
        public string UploadUrl => _uploadUrl;

#if USE_LUA
        [Header("Lua吹き出し")]
        [SerializeField] private MornNovelBubbleSo _luaDefaultBubble;
        [SerializeField] private List<LuaBubbleEntry> _luaBubbles;

        [Header("Lua背景")]
        [SerializeField] private List<LuaSpriteEntry> _luaBackgrounds;

        [Header("Luaキャラクター（キー → ポーズ）")]
        [SerializeField] private List<LuaPoseEntry> _luaCharacters;

        [Header("Lua話者名（表示名 → Talker）")]
        [SerializeField] private List<LuaTalkerEntry> _luaTalkers;

        [Header("Lua SE")]
        [SerializeField] private List<LuaAudioEntry> _luaSeEntries;

        internal MornNovelBubbleSo LuaDefaultBubble => _luaDefaultBubble;

        internal async ValueTask<MornNovelBubbleSo> FindLuaBubbleAsync(string key)
        {
            if (_luaBubbles != null)
            {
                foreach (var entry in _luaBubbles)
                {
                    if (entry.Key == key)
                    {
                        return entry.Bubble;
                    }
                }
            }

            var result = await LoadAddressableOrWarn<MornNovelBubbleSo>(key, "Lua吹き出し");
            return result != null ? result : _luaDefaultBubble;
        }

        internal async ValueTask<Sprite> FindLuaBackgroundAsync(string key)
        {
            if (_luaBackgrounds != null)
            {
                foreach (var entry in _luaBackgrounds)
                {
                    if (entry.Key == key)
                    {
                        return entry.Sprite;
                    }
                }
            }

            return await LoadAddressableOrWarn<Sprite>(key, "Lua背景");
        }

        internal async ValueTask<MornNovelPoseSo> FindLuaCharacterAsync(string key)
        {
            if (_luaCharacters != null)
            {
                foreach (var entry in _luaCharacters)
                {
                    if (entry.Key == key)
                    {
                        return entry.Pose;
                    }
                }
            }

            return await LoadAddressableOrWarn<MornNovelPoseSo>(key, "Luaキャラクター");
        }

        internal async ValueTask<MornNovelTalkerSo> FindLuaTalkerAsync(string name)
        {
            if (_luaTalkers != null)
            {
                foreach (var entry in _luaTalkers)
                {
                    if (entry.DisplayName == name)
                    {
                        return entry.Talker;
                    }
                }
            }

            return await LoadAddressableOrWarn<MornNovelTalkerSo>(name, "Lua話者");
        }

        internal async ValueTask<AudioClip> FindLuaSeAsync(string key)
        {
            if (_luaSeEntries != null)
            {
                foreach (var entry in _luaSeEntries)
                {
                    if (entry.Key == key)
                    {
                        return entry.Clip;
                    }
                }
            }

            return await LoadAddressableOrWarn<AudioClip>(key, "Lua SE");
        }

        private async ValueTask<T> LoadAddressableOrWarn<T>(string key, string label) where T : UnityEngine.Object
        {
#if USE_ADDRESSABLE
            try
            {
                var handle = Addressables.LoadAssetAsync<T>(key);
                var result = await handle.Task;
                if (handle.Status == AsyncOperationStatus.Succeeded && result != null)
                {
                    return result;
                }
            }
            catch (Exception)
            {
                // Addressableにも存在しない
            }
#endif
            Logger.LogWarning($"{label} '{key}' が見つかりません");
            return null;
        }

        [Serializable]
        internal sealed class LuaSpriteEntry
        {
            public string Key;
            public Sprite Sprite;
        }

        [Serializable]
        internal sealed class LuaPoseEntry
        {
            public string Key;
            public MornNovelPoseSo Pose;
        }

        [Serializable]
        internal sealed class LuaTalkerEntry
        {
            [Tooltip("Luaスクリプト内のmessage()で使う表示名")]
            public string DisplayName;
            public MornNovelTalkerSo Talker;
        }

        [Serializable]
        internal sealed class LuaAudioEntry
        {
            public string Key;
            public AudioClip Clip;
        }

        [Serializable]
        internal sealed class LuaBubbleEntry
        {
            public string Key;
            public MornNovelBubbleSo Bubble;
        }
#endif
    }
}
