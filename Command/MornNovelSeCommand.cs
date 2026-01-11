using System;
using UnityEngine;
using VContainer;

namespace MornLib
{
    [Serializable]
    internal class MornNovelSeCommand : MornNovelCommandBase
    {
        public override string Tips => "SEを再生する";
        [Inject] private MornNovelControllerMono _novelController;
        [SerializeField] private AudioClip _audioClip;

        public override void OnStateBegin()
        {
            _novelController.PlayOneShot(_audioClip);
        }
    }
}