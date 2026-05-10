using System;
#if USE_ARBOR
using Arbor;
#elif USE_MORNSTATE
using MornLib;
using StateBehaviour = MornLib.MornStateBehaviour;
#endif
using UnityEngine;
using VContainer;

namespace MornLib
{
    [Serializable]
    internal class MornNovelBgmCommand : MornNovelCommandBase
    {
        public override string Tips => "BGMを再生する(nullで停止)";
        [SerializeField] private MornBeatMusic _beatMusic;
        [SerializeField] private StateLink _onComplete;
        [Inject] private MornBeatController _beatController;
        [Inject] private MornNovelSettings _novelSettings;

        public override async void OnStateBegin()
        {
            // Stateに紐づかず、独立で稼働
            var ct = destroyCancellationToken;
            if (_beatMusic != null)
            {
                await _beatController.StartAsync(
                    new MornBeatStartInfo
                    {
                        Music = _beatMusic,
                        FadeDuration = _novelSettings.BgmChangeSec,
                        Ct = ct,
                    });
            }
            else
            {
                await _beatController.StopBeatAsync(_novelSettings.BgmChangeSec, ct);
            }

            Transition(_onComplete);
        }
    }
}