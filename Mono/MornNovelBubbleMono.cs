using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MornLib
{
    internal sealed class MornNovelBubbleMono : MonoBehaviour
    {
        private static readonly int _topColor = Shader.PropertyToID("_TopColor");
        private static readonly int _centerColor = Shader.PropertyToID("_CenterColor");
        private static readonly int _bottomColor = Shader.PropertyToID("_BottomColor");
        private static readonly int _outlineColor = Shader.PropertyToID("_OutlineColor");
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _bubbleRect;
        [SerializeField] private RectTransform _nameRect;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Image _bubbleImage;
        [SerializeField] private Image _bubbleEdgeImage;
        [SerializeField] private Image _nameImage;
        [SerializeField] private Image _waitInputIcon;
        [Inject] private MornNovelSettings _novelSettings;
        [Inject] private MornLocalizeCore _localizeCore;
        public TMP_Text MessageText => _messageText;
        private Material _nameBackMaterial;
        private Material _nameMaterial;
        private CancellationTokenSource _cts;
        
        private void Start()
        {
            _nameBackMaterial = new Material(_nameImage.material);
            _nameImage.material = _nameBackMaterial;
            _nameMaterial = new Material(_nameText.fontMaterial);
            _nameText.fontMaterial = _nameMaterial;
        }

        public void SetBubble(MornNovelBubbleSo bubbleSo, MornNovelTalkerSo talker, CancellationToken ct = default)
        {
            //Animation
            _cts?.Cancel();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
            var duration = _novelSettings.AnimDuration * (1 - _canvasGroup.alpha);
            _canvasGroup.DOFade(1, duration, _cts.Token).Forget();

            // Bubble
            _bubbleRect.anchoredPosition = bubbleSo.BubblePosition;
            _nameRect.anchoredPosition = bubbleSo.NamePosition;
            _bubbleImage.sprite = bubbleSo.SpeechBubble;
            _bubbleEdgeImage.sprite = bubbleSo.SpeechBubbleEdge;
            var scale = new Vector3(1, 1, 1);
            _bubbleImage.transform.localScale = scale;
            _bubbleEdgeImage.transform.localScale = scale;

            // Talker
            _nameText.text = talker.GetText(_localizeCore.CurrentLanguage);
            _nameMaterial.SetColor(_outlineColor, talker.TextColor);
            _messageText.color = talker.TextColor;
            _nameBackMaterial.SetColor(_topColor, talker.NameBackTopGradientColor);
            _nameBackMaterial.SetColor(_centerColor, talker.NameBackCenterGradientColor);
            _nameBackMaterial.SetColor(_bottomColor, talker.NameBackBottomGradientColor);
            _bubbleEdgeImage.color = talker.BubbleEdgeColor;
            _waitInputIcon.color = talker.MessageIconColor;
        }

        public async UniTask HideAsync(bool isImmediate = false, CancellationToken ct = default)
        {
            _cts?.Cancel();
            if (isImmediate)
            {
                _cts = null;
                _canvasGroup.alpha = 0;
            }
            else
            {
                _cts = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
                
                var duration = _novelSettings.AnimDuration * _canvasGroup.alpha;
                await _canvasGroup.DOFade(0, duration, _cts.Token);
            }
        }

        public void SetWaitInputIcon(bool isWait)
        {
            _waitInputIcon.gameObject.SetActive(isWait);
        }

        public void SetMessage(string message)
        {
            _messageText.text = message;
        }
    }
}