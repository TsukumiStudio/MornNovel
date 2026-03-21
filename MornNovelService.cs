using System;
using UniRx;
using UnityEngine;
#if USE_LUA
using Lua.Unity;
#endif

namespace MornLib
{
    public sealed class MornNovelService
    {
        private readonly Func<MornNovelAddress, bool> _isNovelRead;
        private readonly Func<bool> _getInput;
        private readonly Action<Sprite> _backgroundShown;
        public bool IsDebug;
        public bool IsAutoPlay { get; private set; }
        private readonly Subject<MornNovelAddress> _onNovelStart = new();
        private readonly Subject<MornNovelAddress> _onNovelSet = new();
        private readonly Subject<MornNovelAddress> _onNovelEnd = new();
        public IObservable<MornNovelAddress> OnNovelStart => _onNovelStart;
        public IObservable<MornNovelAddress> OnNovelSet => _onNovelSet;
        public IObservable<MornNovelAddress> OnNovelEnd => _onNovelEnd;
        public MornNovelAddress CurrentNovelAddress { get; private set; }
        public MornNovelSetType NovelSetType { get; private set; }
        public MornNovelMono CurrentNovelPrefab { get; private set; }
#if USE_LUA
        public LuaAsset CurrentLuaAsset { get; private set; }
#endif

        public MornNovelService(
            Func<MornNovelAddress, bool> novelRead,
            Func<bool> getInput,
            Action<Sprite> backgroundShown)
        {
            _isNovelRead = novelRead;
            _getInput = getInput;
            _backgroundShown = backgroundShown;

            MornDebugCore.RegisterGUI(
                "チート/ノベル自動再生",
                () =>
                {
                    IsAutoPlay = GUILayout.Toggle(IsAutoPlay, "ノベル自動再生");
                },
                MornApp.QuitToken);
        }

        public void AtNovelStart(MornNovelAddress address)
        {
            _onNovelStart.OnNext(address);
        }

        public void AtNovelReadEnd(MornNovelAddress address)
        {
            _onNovelEnd.OnNext(address);
        }

        public bool IsNovelRead(MornNovelAddress address)
        {
            return _isNovelRead(address);
        }

        public bool Input()
        {
            return _getInput();
        }

        public void SetNovelPrefab(MornNovelMono prefab)
        {
            CurrentNovelPrefab = prefab;
#if USE_LUA
            CurrentLuaAsset = null;
#endif
        }

#if USE_LUA
        /// <summary>Luaスクリプトでノベルを再生する</summary>
        public void SetLuaAsset(LuaAsset luaAsset)
        {
            CurrentLuaAsset = luaAsset;
            CurrentNovelPrefab = null;
        }
#endif

        public void SetNovelAddress(MornNovelAddress novelAddress, MornNovelSetType novelSetType)
        {
            if (CurrentNovelAddress.Key== novelAddress.Key &&
                NovelSetType == novelSetType)
            {
                return;
            }

            CurrentNovelAddress = novelAddress;
            NovelSetType = novelSetType;
            _onNovelSet.OnNext(novelAddress);
        }

        public void OnShowBackground(Sprite sprite)
        {
            _backgroundShown?.Invoke(sprite);
        }

        /// <summary>ノベル再生後にステートをクリアする</summary>
        public void ClearNovelState()
        {
            CurrentNovelPrefab = null;
            CurrentNovelAddress = default;
            NovelSetType = default;
#if USE_LUA
            CurrentLuaAsset = null;
#endif
        }
    }
}
