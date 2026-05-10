#if USE_ARBOR
using Arbor;
#else
using MornLib;
using StateBehaviour = MornLib.MornStateBehaviour;
#endif
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace MornLib
{
    [Serializable]
    internal class MornNovelSceneAddState : StateBehaviour
    {
        [SerializeField] private MornNovelAddress _novelAddress;
        [SerializeField] private StateLink _onNovelEnd;
        [Inject] private MornNovelService _novelManager;

        public override async void OnStateBegin()
        {
            if (!_novelAddress.IsNullOrEmpty())
            {
                _novelManager.SetNovelAddress(_novelAddress, MornNovelSetType.DontRegisterAsReading);
            }

            await SceneManager.LoadSceneAsync(MornNovelGlobal.I.NovelScene, LoadSceneMode.Additive);
            var scene = SceneManager.GetSceneByName(MornNovelGlobal.I.NovelScene);
            await UniTask.Yield(CancellationTokenOnEnd);

            while (scene.isLoaded)
            {
                await UniTask.Yield(CancellationTokenOnEnd);
            }

            Transition(_onNovelEnd);
        }
    }
}