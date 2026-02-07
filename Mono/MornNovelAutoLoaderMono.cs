using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;
using VContainer.Unity;

namespace MornNovel
{
    internal sealed class MornNovelAutoLoaderMono : MonoBehaviour
    {
        [SerializeField] private MornNovelAddress _debugNovelKey;
        [Inject] private MornNovelService _novelService;
        [Inject] private IObjectResolver _resolver;

        private AsyncOperationHandle<GameObject> _loadHandle;

        private async void Awake()
        {
            if (_novelService.CurrentNovelPrefab != null)
            {
                _resolver.Instantiate(_novelService.CurrentNovelPrefab, transform);
                return;
            }

            var address = _novelService.CurrentNovelAddress.IsNullOrEmpty() ? _debugNovelKey
                : _novelService.CurrentNovelAddress;
            // LoadAssetAsync は依存アセットも自動的にロードする
            _loadHandle = Addressables.LoadAssetAsync<GameObject>(address.Key);
            await _loadHandle.Task;
            if (_loadHandle.Status == AsyncOperationStatus.Succeeded && _loadHandle.Result != null)
            {
                var result = _loadHandle.Result.TryGetComponent<MornNovelMono>(out var prefab);
                if (result)
                {
                    _resolver.Instantiate(prefab, transform);
                }
                else
                {
                    Debug.Log($"NovelMono {address.Key} is exists, but not MornNovelMono");
                }
            }
            else
            {
                Debug.LogError($"Failed to load asset: {address.Key}");
            }
        }

        private void OnDestroy()
        {
            if (_loadHandle.IsValid())
            {
                Addressables.Release(_loadHandle);
            }
        }
    }
}