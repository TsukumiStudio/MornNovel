using UnityEngine;

namespace MornLib
{
    [CreateAssetMenu(fileName = nameof(MornNovelTalkerSo), menuName = "Morn/" + nameof(MornNovelTalkerSo))]
    public sealed class MornNovelTalkerSo : ScriptableObject
    {
        [SerializeField] [Label("日本語")] private MornLocalizeString _localize;
        [SerializeField] [Label("複数人")] private bool _isMulti;
        [SerializeField] [Label("文字色")] private Color _textColor = Color.white;
        [SerializeField] [Label("名前グラデーション")] private Gradient _nameGradient = new Gradient();
        [SerializeField] [Label("吹き出しフチ色/文字送り色")] private Color _edgeColor = Color.white;
        [SerializeField] private AudioClip[] _clip;
        [SerializeField] private float _clipLength;
        public bool IsMulti => _isMulti;
        public Color CommandColor => _edgeColor;
        public Color TextColor => _textColor;
        public Color NameBackTopGradientColor => _nameGradient.Evaluate(0);
        public Color NameBackCenterGradientColor => _nameGradient.Evaluate(0.5f);
        public Color NameBackBottomGradientColor => _nameGradient.Evaluate(1);
        public Color BubbleEdgeColor => _edgeColor;
        public Color MessageIconColor => _edgeColor;
        public AudioClip[] Clips => _clip;
        public float ClipLength => _clipLength;
        public string GetText(string language) => _localize.Get(language);
        [Header("Debug")]
        [SerializeField] private Sprite _debugSprite;
        public Sprite Preview => _debugSprite;
    }
}