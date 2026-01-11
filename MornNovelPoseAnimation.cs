using System;
using System.Collections.Generic;
using MornEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MornLib
{
    [Serializable]
    public class MornNovelPoseAnimation
    {
        public MornNovelPoseAnimationType AnimationType;
        public MornNovelPoseEffectTargetType TargetType;
        public List<MornNovelPoseAnimationStill> Stills;

        public Sprite CurrentSprite => Stills[_currentIndex].Sprite;

        private int _currentIndex;
        private float _nextChangeTime;

        public void Initialize()
        {
            _currentIndex = 0;
            _nextChangeTime = Time.time +
                              Random.Range(Stills[_currentIndex].TimeRange.x, Stills[_currentIndex].TimeRange.y);
        }

        public void Update()
        {
            if (Stills.Count <= 1) return;

            if (Time.time < _nextChangeTime) return;

            _currentIndex++;
            if (_currentIndex >= Stills.Count)
            {
                _currentIndex = 0;
            }
            var nextDuration = Random.Range(Stills[_currentIndex].TimeRange.x, Stills[_currentIndex].TimeRange.y);
            _nextChangeTime = Time.time + nextDuration;
        }
    }

    public enum MornNovelPoseAnimationType
    {
        Blink,
        Custom
    }
    public enum MornNovelPoseEffectTargetType
    {
        CharacterSprite,
    }
    [Serializable]
    public struct MornNovelPoseAnimationStill
    {
        public Vector2 TimeRange;
        [SpritePreview(100f)] public Sprite Sprite;
    }
}