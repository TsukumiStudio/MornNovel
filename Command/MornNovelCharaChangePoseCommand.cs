using Arbor;
using UnityEngine;

namespace MornLib
{
    internal class MornNovelCharaChangePoseCommand : MornNovelCommandBase
    {
        public override string Tips => "キャラクターの表情を変更する";
        [SerializeField] [ViewableSearch] private MornNovelPoseSo _pose;
        [SerializeField] [ReadOnly] [SpritePreview] private Sprite _previewTalker;
        [SerializeField] private StateLink _stateLink;
        public override Color? CommandColor => _pose == null ? null : _pose.Talker.CommandColor;

        protected override void OnValidate()
        {
            base.OnValidate();
            _previewTalker = _pose ? _pose.DefaultSprite : null;
        }

        public override void OnStateBegin()
        {
            var controller = FindFirstObjectByType<MornNovelControllerMono>();
            if (_pose.Talker.IsMulti)
            {
                Debug.LogWarning("複数人Talkerのキャラ制御はできません");
                return;
            }

            var chara = controller.GetChara(_pose.Talker);
            chara.SetPose(_pose);
            controller.AllDecreaseOrderInLayer();
            chara.ResetOrderInLayer();
            Transition(_stateLink);
        }
    }
}