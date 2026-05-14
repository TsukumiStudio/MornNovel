using System;
#if USE_MORNSTATE || USE_ARBOR
#if USE_MORNSTATE
using MornLib;
using StateBehaviour = MornLib.MornStateBehaviour;
#elif USE_ARBOR
using Arbor;
#endif
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace MornLib
{
    [Serializable]
    internal class MornNovelWaitSubmitCommand : MornNovelCommandBase
    {
        public override string Tips => "入力を待機する";
        [SerializeField] private StateLink _stateLink;
        [Inject] private MornNovelService _novelManager;

        public override async void OnStateBegin()
        {
            var ct = CancellationTokenOnEnd;
            while (true)
            {
                if (_novelManager.Input())
                {
                    // 次Fへ入力を渡さないために1F待機
                    await UniTask.Yield(ct);
                    break;
                }

                await UniTask.Yield(ct);
            }

            Transition(_stateLink);
        }
    }
}
#endif // USE_MORNSTATE || USE_ARBOR
