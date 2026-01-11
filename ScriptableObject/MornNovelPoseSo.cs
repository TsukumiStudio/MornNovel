using System.Collections.Generic;
using MornEditor;
using UnityEngine;

namespace MornLib
{
    [CreateAssetMenu(fileName = nameof(MornNovelPoseSo), menuName = "Morn/" + nameof(MornNovelPoseSo))]
    internal sealed class MornNovelPoseSo : ScriptableObject
    {
        public MornNovelTalkerSo Talker;
        [SpritePreview(100f)] public Sprite DefaultSprite;
        public List<MornNovelPoseAnimation> Animations;
        
        // ViewableSearchAttributeで表示する用
        public Sprite Preview => DefaultSprite;
    }
}