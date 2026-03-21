#if USE_ADDRESSABLE
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;
using VContainer.Unity;
#if USE_LUA
using Lua.Unity;
#endif

namespace MornLib
{
    internal sealed class MornNovelAutoLoaderMono : MonoBehaviour
    {
        private enum DebugLoadMode
        {
            Novel,
#if USE_LUA
            Lua,
#endif
        }

        [SerializeField] private DebugLoadMode _debugLoadMode;
        [SerializeField, ShowIf(nameof(IsNovelMode))] private MornNovelAddress _debugNovelKey;
#if USE_LUA
        [Tooltip("デバッグ用Luaファイル。LoadModeがLuaの場合に使用される")]
        [SerializeField, ShowIf(nameof(IsLuaMode))] private LuaAsset _debugLuaAsset;
#endif
        private bool IsNovelMode => _debugLoadMode == DebugLoadMode.Novel;
#if USE_LUA
        private bool IsLuaMode => _debugLoadMode == DebugLoadMode.Lua;
#endif
        [Inject] private MornNovelService _novelService;
        [Inject] private IObjectResolver _resolver;

        private AsyncOperationHandle _dependencyHandle;

        private async void Awake()
        {
#if USE_LUA
            // Lua: ServiceにセットされたLuaAsset、またはデバッグ用LuaAsset
            var luaAsset = _novelService.CurrentLuaAsset;

            if (luaAsset == null && _debugLoadMode == DebugLoadMode.Lua && _debugLuaAsset != null &&
                _novelService.CurrentNovelPrefab == null && _novelService.CurrentNovelAddress.IsNullOrEmpty())
            {
                luaAsset = _debugLuaAsset;
            }

            if (luaAsset != null)
            {
                var runner = FindFirstObjectByType<MornNovelLuaRunner>();
                if (runner != null)
                {
                    _novelService.ClearNovelState();
                    await runner.PlayAsync(luaAsset, destroyCancellationToken);
                    return;
                }

                Debug.LogWarning("[MornNovel] MornNovelLuaRunnerがシーンに存在しません");
            }
#endif
            // Prefab優先
            if (_novelService.CurrentNovelPrefab != null)
            {
                _resolver.Instantiate(_novelService.CurrentNovelPrefab, transform);
                return;
            }

            // Addressablesフォールバック
            var address = _novelService.CurrentNovelAddress.IsNullOrEmpty() ? _debugNovelKey
                : _novelService.CurrentNovelAddress;
            var handle = Addressables.LoadAssetAsync<GameObject>(address.Key);
            _dependencyHandle = Addressables.DownloadDependenciesAsync(address.Key);
            await handle.Task;
            await _dependencyHandle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                var result = handle.Result.TryGetComponent<MornNovelMono>(out var prefab);
                if (result)
                {
                    _resolver.Instantiate(prefab, transform);
                }
                else
                {
                    Debug.Log($"NovelMono {address.Key} is exists, but not MornNovelMono");
                }

                Addressables.Release(handle);
            }
            else
            {
                Debug.LogError($"Failed to load asset: {address.Key}");
            }
        }

        private void OnDestroy()
        {
            if (!_dependencyHandle.IsValid()) return;
            Addressables.Release(_dependencyHandle);
            _dependencyHandle = default;
        }
    }
}
#endif
