using System.Collections.Generic;
using Arbor;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace MornLib
{
    internal class MornNovelStartCommand : MornNovelCommandBase
    {
        public override string Tips => "ノベルを開始する";
        [SerializeField]
        [Label("BGM")] private MornBeatMemoSo _beatMemo;
        [SerializeField] private StateLink _nextState;
        [Inject] private MornNovelSettings _settings;
        [Inject] private MornSoundVolumeCore _volumeCore;
        [Inject] private MornBeatControllerMono _beatController;
        [Inject] private MornNovelService _novelService;

        public override async void OnStateBegin()
        {
            if (!_novelService.CurrentNovelAddress.IsNullOrEmpty())
            {
                _novelService.AtNovelStart(_novelService.CurrentNovelAddress);
            }

            // 上乗せノベル判定（デバッグ時は考慮しない）
            if (MornNovelUtil.IsUpperNovel && !_novelService.IsDebug)
            {
                Transition(_nextState);
                return;
            }

            var list = new List<UniTask>();
            var ct = MornApp.QuitToken;
            if (_beatMemo != null)
            {
                list.Add(_beatController.StartAsync(new MornBeatStartInfo() { BeatMemo = _beatMemo, FadeDuration = _settings.BgmChangeSec, Ct = ct, }));
            }
            else
            {
                list.Add(_beatController.StopBeatAsync(_settings.BgmChangeSec, ct));
            }

            list.Add(
                _volumeCore.FadeAsync(
                    new MornSoundVolumeFadeInfo
                    {
                        SoundVolumeType = _settings.FadeVolumeType, Duration = _settings.BgmChangeSec, IsFadeIn = true, CancellationToken = ct,
                    }));
            list.Add(MornTransitionCore.ClearAsync(ct: ct));
            await UniTask.WhenAll(list);
            Transition(_nextState);
        }
    }
}