#if USE_ARBOR
using Arbor;
#elif USE_MORNSTATE
using MornLib;
using StateBehaviour = MornLib.MornStateBehaviour;
using StateLink = MornLib.Connection;
#endif
using System;
using UnityEngine;
using VContainer;

namespace MornLib
{
    [Serializable]
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