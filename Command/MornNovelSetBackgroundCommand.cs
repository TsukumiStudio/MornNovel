using System;
#if USE_MORNSTATE || USE_ARBOR
#if USE_MORNSTATE
using MornLib;
using StateBehaviour = MornLib.MornStateBehaviour;
#elif USE_ARBOR
using Arbor;
#endif
using UnityEngine;

namespace MornLib
{
    [Serializable]
    internal class MornNovelSetBackgroundCommand : MornNovelCommandBase
    {
        public override string Tips => "背景を設定する";
        [SerializeField] [SpritePreview] [Label("背景")] private Sprite _background;
        [SerializeField] [Label("即表示")] private bool _isImmediate = true;
        [SerializeField] private StateLink _nextState;

        public override async void OnStateBegin()
        {
            var controller = UnityEngine.Object.FindFirstObjectByType<MornNovelControllerMono>();
            await controller.SetBackgroundAsync(_background, _isImmediate);
            Transition(_nextState);
        }
    }
}
#endif // USE_MORNSTATE || USE_ARBOR
