#if USE_MORNSTATE || USE_ARBOR
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace MornLib
{
    internal class MornNovelEndCommand : MornNovelCommandBase
    {
        private enum NovelEndTransitionType
        {
            他シーンへ遷移,
            ノベルシーンだけ消す,
            次のノベルをこのまま読み込む,
            次のノベルへトランジション,
        }

        public override string Tips => "ノベルを終了する";
        [Inject] private MornBeatController _beatController;
        [Inject] private MornNovelControllerMono _novelController;
        [Inject] private MornNovelSettings _settings;
        [Inject] private MornNovelService _novelManager;
        [SerializeField, Label("既読フラグをつける")] private bool _checkNovelRead = true;
        [SerializeField, Label("BGMを止めるか")]
        private bool _isStopBgm = true;
        [SerializeField, Label("終了時の処理")]
        private NovelEndTransitionType _endTransitionType;
        [SerializeField, ShowIf(nameof(IsNeedTransition)), Label("遷移時のトランジション")]
        private MornTransitionType _transitionType;
        [SerializeField, ShowIf(nameof(IsChangeScene)), Label("遷移先のシーン")]
        private MornSceneObject _scene;
        [SerializeField, ShowIf(nameof(IsChangeNovel)), Label("遷移先のノベル")] 
        private MornNovelAddress _address;
        [SerializeField, ShowIf(nameof(IsChangeNovel)), Label("読みかけ登録設定")] 
        private MornNovelSetType _setType;
        public bool IsChangeScene => _endTransitionType == NovelEndTransitionType.他シーンへ遷移;
        private bool IsCloseScene => _endTransitionType == NovelEndTransitionType.ノベルシーンだけ消す;
        private bool IsChangeNovel => _endTransitionType == NovelEndTransitionType.次のノベルをこのまま読み込む ||
                                     _endTransitionType == NovelEndTransitionType.次のノベルへトランジション;
        private bool IsNeedTransition => IsChangeScene || _endTransitionType == NovelEndTransitionType.次のノベルへトランジション;

        public override async void OnStateBegin()
        {
            if (_checkNovelRead && !_novelManager.CurrentNovelAddress.IsNullOrEmpty())
            {
                _novelManager.AtNovelReadEnd(_novelManager.CurrentNovelAddress);
            }

            if (IsChangeNovel)
            {
                _novelManager.SetNovelAddress(_address, _setType);
            }

            var taskList = new List<UniTask>();
            var ct = CancellationTokenOnEnd;
            if (_isStopBgm)
            {
                taskList.Add(_beatController.StopBeatAsync(_settings.BgmChangeSec, ct));
            }

            if (_novelManager.IsDebug) _endTransitionType = NovelEndTransitionType.ノベルシーンだけ消す;

            if (IsNeedTransition || _novelManager.IsDebug)
            {
                var transition = _novelManager.IsDebug ? MornNovelGlobal.I.DebugTransition : _transitionType;
                taskList.Add(MornTransitionCore.FillAsync(transition, ct));
                taskList.Add(MornSoundService.I.FadeAsync(new MornSoundFadeInfo
                {
                    SoundVolumeType = _settings.FadeVolumeType,
                    Duration = _settings.BgmChangeSec,
                    IsFadeIn = false,
                    CancellationToken = ct,
                }));
            }

            if (IsCloseScene && !_novelManager.IsDebug)
            {
                taskList.Add(_novelController.RemoveAllAsync(ct));
            }

            await UniTask.WhenAll(taskList);
            if (IsChangeScene)
            {
                SceneManager.LoadScene(_scene);
            }

            if (IsChangeNovel)
            {
                _novelManager.SetNovelAddress(_address, _setType);
                SceneManager.LoadScene(MornNovelGlobal.I.NovelScene);
            }

            if (IsCloseScene)
            {
                SceneManager.UnloadSceneAsync(gameObject.scene).WithCancellation(ct).Forget();
            }
        }
    }
}
#endif // USE_MORNSTATE || USE_ARBOR
