using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace MornLib
{
    internal sealed class MornNovelControllerMono : MonoBehaviour
    {
        private static readonly int _prevTex = Shader.PropertyToID("_PrevTex");
        private static readonly int _nextTex = Shader.PropertyToID("_NextTex");
        private static readonly int _phase = Shader.PropertyToID("_Phase");
        [SerializeField] private AudioSource _novelAudioSource;
        [SerializeField] private MornNovelBubbleMono _bubble;
        [SerializeField] private Image _backgroundA;
        [SerializeField] private Image _backgroundB;
        [SerializeField] private Transform _charaParent;
        [SerializeField] private MornNovelCharaMono _charaPrefab;
        public AudioMixerGroup AudioMixerGroup => _novelAudioSource.outputAudioMixerGroup;
        [Inject] private MornNovelSettings _novelSettings;
        [Inject] private IObjectResolver _resolver;
        [Inject] private MornNovelService _novelService;
        private bool _usingBackgroundA;
        private CancellationTokenSource _backgroundCts;
        private readonly Dictionary<MornNovelTalkerSo, MornNovelCharaMono> _cachedCharaDict = new();
        private Image Current => _usingBackgroundA ? _backgroundA : _backgroundB;
        private Image Next => _usingBackgroundA ? _backgroundB : _backgroundA;
        public TMP_Text MessageText => _bubble.MessageText;

        private MornNovelCharaMono InstantiateImpl()
        {
            var instance = _resolver.Instantiate(_charaPrefab, _charaParent);
            var trans = instance.transform;
            trans.localPosition = new Vector3(
                trans.localPosition.x,
                _novelSettings.HeightUnfocus,
                trans.localPosition.z);
            return instance;
        }

        private void Start()
        {
            _bubble.HideAsync(true).Forget();
        }

        public async UniTask BubbleHideAsync()
        {
            await _bubble.HideAsync();
        }

        public async UniTask SetBackgroundAsync(Sprite sprite, bool isImmediate, CancellationToken ct = default)
        {
            _backgroundCts?.Cancel();
            Next.color = new Color(1, 1, 1, 0);
            Next.sprite = sprite;
            _novelService.OnShowBackground(sprite);
            Next.transform.SetAsLastSibling();
            if (isImmediate)
            {
                _backgroundCts = null;
                MornNovelUtil.SetAlpha(Next, 1);
            }
            else
            {
                _backgroundCts = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
                await Next.DOFade(1, _novelSettings.BackgroundFadeSec, _backgroundCts.Token);
            }

            _usingBackgroundA = !_usingBackgroundA;
        }

        public async UniTask SetBackgroundDistortTransitionAsync(Sprite prevSprite, Sprite nextSprite,
            CancellationToken ct = default)
        {
            _backgroundCts?.Cancel();
            _backgroundCts = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
            Next.color = new Color(1, 1, 1, 0);
            Next.sprite = nextSprite;
            _novelService.OnShowBackground(nextSprite);
            MornNovelUtil.SetAlpha(Next, 1);
            Next.transform.SetAsLastSibling();
            Next.material = _novelSettings.DistortTransitionMaterial;
            Next.material.SetTexture(_prevTex, prevSprite.texture);
            Next.material.SetTexture(_nextTex, nextSprite.texture);
            Next.material.SetFloat(_phase, 0);
            await Next.DoMaterialFloat("_Phase", 1, _novelSettings.DistortTransitionSec, _backgroundCts.Token);
            Next.material = null;
            _usingBackgroundA = !_usingBackgroundA;
        }

        public async UniTask RemoveAllAsync(CancellationToken ct = default)
        {
            _backgroundCts?.Cancel();
            _backgroundCts = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
            var fadeDurationA = _novelSettings.BackgroundFadeSec * _backgroundA.color.a;
            var fadeDurationB = _novelSettings.BackgroundFadeSec * _backgroundB.color.a;
            var taskA = _backgroundA.DOFade(0, fadeDurationA, _backgroundCts.Token);
            var taskB = _backgroundB.DOFade(0, fadeDurationB, _backgroundCts.Token);
            await UniTask.WhenAll(taskA, taskB);
        }

        public MornNovelCharaMono GetChara(MornNovelTalkerSo talkerSo)
        {
            if (_cachedCharaDict.TryGetValue(talkerSo, out var chara))
            {
                return chara;
            }

            _cachedCharaDict[talkerSo] = InstantiateImpl();
            return _cachedCharaDict[talkerSo];
        }

        public async UniTask AllHideAsync(CancellationToken ct = default)
        {
            var taskList = new List<UniTask>();
            foreach (var chara in _cachedCharaDict.Values)
            {
                taskList.Add(chara.HideAsync(ct: ct));
            }

            taskList.Add(_bubble.HideAsync(ct: ct));
            await UniTask.WhenAll(taskList);
        }

        public void AllFocus()
        {
            foreach (var chara in _cachedCharaDict.Values)
            {
                chara.Focus();
            }
        }

        public void AllDecreaseOrderInLayer()
        {
            foreach (var chara in _cachedCharaDict.Values)
            {
                chara.DecreaseOrderInLayer();
            }
        }

        public void SetFocus(MornNovelTalkerSo talker)
        {
            foreach (var (key, _) in _cachedCharaDict)
            {
                if (key == talker)
                {
                    GetChara(key).Focus();
                }
                else
                {
                    GetChara(key).SetUnfocus();
                }
            }
        }

        public void SetBubble(MornNovelBubbleSo bubbleSo, MornNovelTalkerSo talker)
        {
            if (bubbleSo == null)
            {
                return;
            }

            _bubble.SetBubble(bubbleSo, talker);
        }

        public void SetWaitInputIcon(bool isShown)
        {
            _bubble.SetWaitInputIcon(isShown);
        }

        public void SetMessage(string message)
        {
            _bubble.SetMessage(message);
        }

        public void PlayOneShot(AudioClip audioClip)
        {
            _novelAudioSource.PlayOneShot(audioClip);
        }
    }
}