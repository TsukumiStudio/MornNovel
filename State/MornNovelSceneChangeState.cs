using Arbor;
using MornEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace MornLib
{
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