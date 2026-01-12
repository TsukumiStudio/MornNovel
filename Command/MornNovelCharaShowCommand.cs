using System;
using Arbor;
using UnityEngine;

namespace MornLib
{
    internal class MornNovelCharaShowCommand : MornNovelCommandBase
    {
        public override string Tips => "キャラクターを表示する";
        [SerializeField] [ViewableSearch] private MornNovelPoseSo _pose;
        [SerializeField] [ReadOnly] [SpritePreview] private Sprite _previewTalker;
        [SerializeField] [Label("入場方向")] private MornNovelCharaMoveType _moveType = MornNovelCharaMoveType.ToInner;
        [SerializeField] [Label("位置")] [Range(-1, 1)] private float _talkerPosition;
        [SerializeField] [Label("反転")] private bool _isFlipX;
        [SerializeField] private StateLink _stateLink;
        public override Color? CommandColor => _pose == null ? null : _pose.Talker.CommandColor;

        protected override void OnValidate()
        {
            base.OnValidate();
            _previewTalker = _pose ? _pose.DefaultSprite : null;
        }

        public override async void OnStateBegin()
        {
            try
            {
                var controller = FindFirstObjectByType<MornNovelControllerMono>();
                if (_pose.Talker.IsMulti)
                {
                    Debug.LogWarning("複数人Talkerのキャラ制御はできません");
                    return;
                }

                var chara = controller.GetChara(_pose.Talker);
                chara.SetFlipX(_isFlipX);
                chara.SetPose(_pose);
                controller.AllDecreaseOrderInLayer();
                chara.ResetOrderInLayer();
                if (_moveType == MornNovelCharaMoveType.Slide)
                {
                    await chara.SetPositionXAsync(_talkerPosition);
                }
                else
                {
                    chara.SetPositionX(_talkerPosition);
                    await chara.SpawnAsync(_moveType);
                }

                Transition(_stateLink);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}