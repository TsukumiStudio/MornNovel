using System;
#if USE_ARBOR
using Arbor;
#elif USE_MORNSTATE
using MornLib;
using StateBehaviour = MornLib.MornStateBehaviour;
#endif
using UnityEngine;

namespace MornLib
{
    [Serializable]
    internal class MornNovelBubbleHideCommand : MornNovelCommandBase
    {
        public override string Tips => "フキダシを消す";
        [SerializeField] private StateLink _nextState;

        public override async void OnStateBegin()
        {
            var controller = UnityEngine.Object.FindFirstObjectByType<MornNovelControllerMono>();
            await controller.BubbleHideAsync();
            Transition(_nextState);
        }
    }
}