using UnityEngine;

namespace MornLib
{
    [CreateAssetMenu(fileName = nameof(MornNovelCameraShakeSettings), menuName = nameof(MornNovelCameraShakeSettings))]
    internal sealed class MornNovelCameraShakeSettings : UnityEngine.ScriptableObject
    {
        public float ShakeStrength = 1f;
        public int ShakeCount = 5;
        public float ShakeInterval = 0.05f;
    }
}