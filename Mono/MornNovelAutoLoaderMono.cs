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
        [SerializeField] private MornNovelAddress _debugNovelKey;
#if USE_LUA
        [Tooltip("デバッグ用Luaファイル。設定されている場合、Addressablesより優先して再生される")]
        [SerializeField] private LuaAsset _debugLuaAsset;
#endif
        [Inject] private MornNovelService _novelService;
        [Inject] private IObjectResolver _resolver;

        private AsyncOperationHandle _dependencyHandle;

        private async void Awake()
        {
#if USE_LUA
            // Lua優先: LuaAssetがセットされていればLuaRunnerで再生
            var luaAsset = _novelService.CurrentLuaAsset;

            // デバッグ用: Serviceに何もセットされていなければデバッグ用LuaAssetを使用
            if (luaAsset == null && _debugLuaAsset != null &&
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
            // 一緒に依存アセットもロード
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
            // 依存アセットの解放
            if (!_dependencyHandle.IsValid()) return;
            Addressables.Release(_dependencyHandle);
            _dependencyHandle = default;
        }
    }
}
#endif
