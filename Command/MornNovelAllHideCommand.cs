using System;
using Arbor;
using UnityEngine;
using VContainer;

namespace MornLib
{
    [Serializable]
    internal class MornNovelAllHideCommand : MornNovelCommandBase
    {
        [SerializeField] private StateLink _nextState;
        [Inject] private MornNovelControllerMono _controller;
        public override string Tips => "キャラクターとフキダシをすべて消す";

        public override async void OnStateBegin()
        {
            await _controller.AllHideAsync(CancellationTokenOnEnd);
            Transition(_nextState);
        }
    }
}