using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;
using VContainer.Unity;

namespace MornLib
{
    internal sealed class MornNovelAutoLoaderMono : MonoBehaviour
    {
        [SerializeField] private MornNovelAddress _debugNovelKey;
        [Inject] private MornNovelService _novelService;
        [Inject] private IObjectResolver _resolver;

        private AsyncOperationHandle _dependencyHandle;

        private async void Awake()
        {
            if (_novelService.CurrentNovelPrefab != null)
            {
                _resolver.Instantiate(_novelService.CurrentNovelPrefab, transform);
                return;
            }
            
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