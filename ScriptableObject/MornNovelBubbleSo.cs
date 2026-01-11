using MornEditor;
using UnityEngine;

namespace MornLib
{
    [CreateAssetMenu(fileName = nameof(MornNovelBubbleSo), menuName = "Morn/" + nameof(MornNovelBubbleSo))]
    internal sealed class MornNovelBubbleSo : UnityEngine.ScriptableObject
    {
        public Vector2 NamePosition;
        public Vector2 BubblePosition;
        [SpritePreview] public Sprite SpeechBubble;
        [SpritePreview] public Sprite SpeechBubbleEdge;
        public Sprite Preview => SpeechBubble;
    }
}