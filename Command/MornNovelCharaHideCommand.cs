using System;
using Arbor;
using MornEditor;
using UnityEngine;

namespace MornLib
{
    [Serializable]
    internal class MornNovelCharaHideCommand : MornNovelCommandBase
    {
        public override string Tips => "キャラクターを退場させる";
        [SerializeField] [ViewableSearch] private MornNovelTalkerSo _talker;
        [SerializeField] [Label("退場方向")] private MornNovelCharaMoveType _moveType = MornNovelCharaMoveType.ToOuter;
        [SerializeField] private StateLink _nextState;
        public override Color? CommandColor => _talker == null ? null : _talker.CommandColor;

        public override async void OnStateBegin()
        {
            try
            {
                var controller = FindFirstObjectByType<MornNovelControllerMono>();
                if (_talker.IsMulti)
                {
                    Debug.LogWarning("複数人Talkerのキャラ制御はできません");
                    return;
                }

                await controller.GetChara(_talker).HideAsync(_moveType);
                Transition(_nextState);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}