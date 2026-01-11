using UnityEngine;
using VContainer;

namespace MornLib
{
    public sealed class MornNovelMono : MonoBehaviour
    {
        // 暫定
        private string ReadKey
        {
            get
            {
                var objectName = gameObject.name;
                // (Clone)がついている場合は除去
                if (objectName.EndsWith("(Clone)"))
                {
                    objectName = objectName.Substring(0, objectName.Length - 7);
                }

                return $"{objectName}_Read";
            }
        }
        public MornNovelAddress Address => new(ReadKey);
        [Inject] private MornNovelControllerMono _novelController;
        [Inject] private MornNovelService _novelManager;

        private void Awake()
        {
            foreach (var audioSource in GetComponentsInChildren<AudioSource>())
            {
                audioSource.outputAudioMixerGroup = _novelController.AudioMixerGroup;
            }
        }
    }
}