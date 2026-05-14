#if USE_MORNSTATE || USE_ARBOR
#if USE_MORNSTATE
using MornLib;
using StateBehaviour = MornLib.MornStateBehaviour;
#elif USE_ARBOR
using Arbor;
#endif
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using VContainer;

namespace MornLib
{
    [Serializable]
    internal class MornNovelSceneChangeState : StateBehaviour
    {
        [SerializeField, Label("null可")] private MornNovelAddress _novelAddress;
        [Inject] private MornNovelService _novelManager;

        public override void OnStateBegin()
        {
            if (!_novelAddress.IsNullOrEmpty())
            {
                _novelManager.SetNovelAddress(_novelAddress, MornNovelSetType.RegisterAsReading);
            }

            SceneManager.LoadScene(MornNovelGlobal.I.NovelScene);
        }
    }
}
#endif // USE_MORNSTATE || USE_ARBOR
