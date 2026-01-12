using MornColor;
using UnityEngine;

namespace MornLib
{
    [CreateAssetMenu(fileName = nameof(MornNovelTalkerSo), menuName = "Morn/" + nameof(MornNovelTalkerSo))]
    internal sealed class MornNovelTalkerSo : ScriptableObject
    {
        [SerializeField] [Label("日本語")] private MornLocalizeString _localize;
        [SerializeField] [Label("複数人")] private bool _isMulti;
        [SerializeField] [Label("文字色")] private MornColorInfo _textColor;
        [SerializeField] [Label("名前グラデーション")] private MornGradientInfo _nameGradient;
        [SerializeField] [Label("吹き出しフチ色/文字送り色")] private MornColorInfo _edgeColor;
        [SerializeField] private AudioClip[] _clip;
        [SerializeField] private float _clipLength;
        public bool IsMulti => _isMulti;
        public Color CommandColor => _edgeColor.Color;
        public Color TextColor => _textColor.Color;
        public Color NameBackTopGradientColor => _nameGradient.Gradient.Evaluate(0);
        public Color NameBackCenterGradientColor => _nameGradient.Gradient.Evaluate(0.5f);
        public Color NameBackBottomGradientColor => _nameGradient.Gradient.Evaluate(1);
        public Color BubbleEdgeColor => _edgeColor.Color;
        public Color MessageIconColor => _edgeColor.Color;
        public AudioClip[] Clips => _clip;
        public float ClipLength => _clipLength;
        public string GetText(string language) => _localize.Get(language);
        [Header("Debug")]
        [SerializeField] private Sprite _debugSprite;
        public Sprite Preview => _debugSprite;
    }
}