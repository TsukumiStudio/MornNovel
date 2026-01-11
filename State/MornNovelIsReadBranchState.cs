using Arbor;
using MornEditor;
using UnityEngine;
using VContainer;

namespace MornLib
{
    internal sealed class MornNovelIsReadBranchState : StateBehaviour
    {
        [SerializeField, Label("null可")] private MornNovelAddress _novelAddress;
        [SerializeField] private StateLink _isRead;
        [SerializeField] private StateLink _notRead;
        [Inject] private MornNovelService _novelManager;

        public override void OnStateBegin()
        {
            if (_novelManager.IsNovelRead(_novelAddress))
            {
                Transition(_isRead);
            }
            else
            {
                Transition(_notRead);
            }
        }
    }
}