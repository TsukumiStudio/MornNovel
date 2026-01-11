using System;
using Arbor;
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