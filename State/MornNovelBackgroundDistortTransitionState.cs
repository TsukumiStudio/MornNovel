using Arbor;
using MornEditor;
using UnityEngine;

namespace MornLib
{
    internal class MornNovelBackgroundDistortTransitionState : StateBehaviour
    {
        [SerializeField] [SpritePreview] private Sprite _prevSprite;
        [SerializeField] [SpritePreview] private Sprite _nextSprite;

        [SerializeField] private StateLink _nextState;

        public override async void OnStateBegin()
        {
            var controller = FindFirstObjectByType<MornNovelControllerMono>();
            await controller.SetBackgroundDistortTransitionAsync(_prevSprite, _nextSprite);
            Transition(_nextState);
        }
    }
}