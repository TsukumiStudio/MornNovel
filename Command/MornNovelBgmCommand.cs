using System;
using Arbor;
using UnityEngine;
using VContainer;

namespace MornLib
{
    [Serializable]
    internal class MornNovelBgmCommand : MornNovelCommandBase
    {
        public override string Tips => "BGMを再生する(nullで停止)";
        [SerializeField] private MornBeatMemoSo _beatMemo;
        [SerializeField] private StateLink _onComplete;
        [Inject] private MornBeatControllerMono _beatController;
        [Inject] private MornNovelSettings _novelSettings;

        public override async void OnStateBegin()
        {
            // Stateに紐づかず、独立で稼働
            var ct = destroyCancellationToken;
            if (_beatMemo != null)
            {
                await _beatController.StartAsync(
                    new MornBeatStartInfo
                    {
                        BeatMemo = _beatMemo,
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