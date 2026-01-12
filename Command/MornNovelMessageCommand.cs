using System;
using System.Threading;
using Arbor;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace MornLib
{
    [Serializable]
    internal class MornNovelMessageCommand : MornNovelCommandBase
    {
        public override string Tips => "メッセージを表示する";
        [SerializeField] [ViewableSearch] private MornNovelTalkerSo _talker;
        [SerializeField] [ViewableSearch] private MornNovelBubbleSo _overrideBubble;
        [SerializeField] [Label("セリフ")] private MornLocalizeString _localizeString;
        [SerializeField] private StateLink _stateLink;
        [Inject] private MornNovelControllerMono _novelController;
        [Inject] private MornNovelSettings _novelSettings;
        [Inject] private MornNovelService _novelService;
        public override Color? CommandColor => _talker == null ? null : _talker.CommandColor;

        public string GetText()
        {
            // TODO: ローカライズ
            return _localizeString.Get(MornLocalizeCore.CurrentLanguage ?? "jp");
        }

        public MornLocalizeString GetLocalizeString()
        {
            return _localizeString;
        }

        public string GetTalkerName()
        {
            return _talker.GetText(MornLocalizeCore.CurrentLanguage ?? "jp");
        }

        public override async void OnStateBegin()
        {
            try
            {
                var ct = CancellationTokenOnEnd;
                var controller = FindFirstObjectByType<MornNovelControllerMono>();
                controller.SetBubble(_overrideBubble, _talker);
                controller.SetWaitInputIcon(false);
                controller.SetMessage("");
                if (!_talker.IsMulti)
                {
                    controller.SetFocus(_talker);
                    controller.AllDecreaseOrderInLayer();
                    controller.GetChara(_talker)?.ResetOrderInLayer();
                }
                else
                {
                    controller.AllFocus();
                }

                await MornNovelUtil.DOTextAsync(
                    GetText(),
                    controller.SetMessage,
                    () =>
                    {
                        if (_talker.Clips == null || _talker.Clips.Length == 0)
                        {
                            return (null, 0f);
                        }

                        var clip = _talker.Clips[UnityEngine.Random.Range(0, _talker.Clips.Length)];
                        return (clip, _talker.ClipLength);
                    },
                    () => MornNovelGlobal.I.SubmitClip,
                    _novelController.PlayOneShot,
                    controller.SetWaitInputIcon,
                    true,
                    () => _novelService.Input(),
                    () => _novelService.IsAutoPlay,
                    x => x == '\n' ? 0.1f : 0.05f,
                    autoSizeText: controller.MessageText,
                    ct: ct);
                Transition(_stateLink);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async UniTask<bool> WaitSecondsReturnSkipped(float seconds, CancellationToken ct = default)
        {
            var elapsedTime = 0f;
            while (elapsedTime < seconds)
            {
                elapsedTime += Time.deltaTime;
                if (_novelService.Input())
                {
                    // 次Fへ入力を渡さないために1F待機
                    await UniTask.Yield(ct);
                    return true;
                }

                await UniTask.Yield(ct);
            }

            return false;
        }
    }
}