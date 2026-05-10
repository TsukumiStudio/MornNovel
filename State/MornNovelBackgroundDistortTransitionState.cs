#if USE_ARBOR
using Arbor;
#elif USE_MORNSTATE
using MornLib;
using StateBehaviour = MornLib.MornStateBehaviour;
#endif
using System;
using UnityEngine;

namespace MornLib
{
    [Serializable]
    internal class MornNovelBackgroundDistortTransitionState : StateBehaviour
    {
        [SerializeField] [SpritePreview] private Sprite _prevSprite;
        [SerializeField] [SpritePreview] private Sprite _nextSprite;

        [SerializeField] private StateLink _nextState;

        public override async void OnStateBegin()
        {
            var controller = UnityEngine.Object.FindFirstObjectByType<MornNovelControllerMono>();
            await controller.SetBackgroundDistortTransitionAsync(_prevSprite, _nextSprite);
            Transition(_nextState);
        }
    }
}