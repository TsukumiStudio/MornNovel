#if USE_MORNSTATE || USE_ARBOR
using System;
#if USE_ARBOR && !USE_MORNSTATE
using Arbor;
#endif
using UnityEngine;
using VContainer;

namespace MornLib
{
    [Serializable]
    internal sealed class MornNovelShakeCommand : MornNovelCommandBase
    {
        [Inject] private readonly MornNovelControllerMono _controller;
        [SerializeField, ViewableSearch, Label("対象（未指定で画面）")] private MornNovelTalkerSo _target;
        [SerializeField, Min(0f), Label("強さ")] private float _strength = 0.18f;
        [SerializeField, Min(0f), Label("長さ（秒）")] private float _duration = 0.25f;
        [SerializeField] private StateLink _nextState;

        public override string Tips => "画面またはキャラクターを揺らす";

        public override async void OnStateBegin()
        {
            try
            {
                var ct = CancellationTokenOnEnd;
                await _controller.ShakeAsync(_target, _strength, _duration, ct);
                ct.ThrowIfCancellationRequested();
                Transition(_nextState);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
#endif
