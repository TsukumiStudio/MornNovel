using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace MornLib
{
    internal sealed class MornNovelCharaMono : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Transform _parentX;
        [SerializeField] private Transform _parentY;
        [SerializeField] [ReadOnly] private MornNovelPoseSo _pose;
        [Inject] private MornNovelSettings _novelSettings;
        private CancellationTokenSource _ctsT = new();
        private CancellationTokenSource _ctsAlpha = new();
        private CancellationTokenSource _ctsX = new();
        private CancellationTokenSource _ctsY = new();

        private void Awake()
        {
            HideAsync(isImmediate: true).Forget();
            SetUnfocus(true);
        }

        private void Update()
        {
            if (_pose == null)
            {
                return;
            }
            
            if (_pose.Animations == null || _pose.Animations.Count == 0)
            {
                _renderer.sprite = _pose.DefaultSprite;
            }
            else
            {
                foreach (var poseAnimation in _pose.Animations)
                {
                    poseAnimation.Update();
                    _renderer.sprite = poseAnimation.CurrentSprite;
                }
            }
        }

        public void DecreaseOrderInLayer()
        {
            _renderer.sortingOrder--;
        }

        public void ResetOrderInLayer()
        {
            _renderer.sortingOrder = 0;
        }

        public async UniTask SetPositionXAsync(float talkerPosition, CancellationToken ct = default)
        {
            _ctsT?.Cancel();
            _ctsT = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
            var pos = transform.localPosition;
            pos.x = talkerPosition * _novelSettings.PositionScale;
            await transform.DOLocalMove(pos, _novelSettings.AnimDuration, _ctsT.Token);
        }

        public void SetPositionX(float talkerPosition)
        {
            var pos = transform.localPosition;
            pos.x = talkerPosition * _novelSettings.PositionScale;
            transform.localPosition = pos;
        }

        public void SetFlipX(bool flipX)
        {
            _renderer.flipX = flipX;
        }

        public void SetPose(MornNovelPoseSo pose)
        {
            _pose = pose;
            foreach (var poseAnimation in _pose.Animations) poseAnimation.Initialize();
        }

        public async UniTask SpawnAsync(MornNovelCharaMoveType moveType = MornNovelCharaMoveType.ToInner,
            CancellationToken ct = default)
        {
            _ctsX?.Cancel();
            _ctsX = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
            _ctsAlpha?.Cancel();
            _ctsAlpha = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
            var localFromPos = _parentX.localPosition;
            var dif = _novelSettings.SpawnDifX;
            localFromPos.x = moveType switch
            {
                MornNovelCharaMoveType.ToOuter => transform.localPosition.x > 0 ? -dif : dif,
                MornNovelCharaMoveType.ToInner => transform.localPosition.x > 0 ? dif : -dif,
                MornNovelCharaMoveType.ToLeft => dif,
                MornNovelCharaMoveType.ToRight => -dif,
                MornNovelCharaMoveType.Slide => 0,
                _ => throw new ArgumentOutOfRangeException(nameof(moveType), moveType, null),
            };
            _parentX.localPosition = localFromPos;
            var taskA = _parentX.DOLocalMoveX(0, _novelSettings.AnimDuration, _ctsX.Token);
            var taskB = _renderer.DOFade(1, _novelSettings.AnimDuration, _ctsAlpha.Token);
            await UniTask.WhenAll(taskA, taskB);
        }

        public async UniTask HideAsync(MornNovelCharaMoveType moveType = MornNovelCharaMoveType.ToOuter,
            bool isImmediate = false, CancellationToken ct = default)
        {
            _ctsX?.Cancel();
            _ctsAlpha?.Cancel();
            var dif = _novelSettings.SpawnDifX;
            var aimX = moveType switch
            {
                MornNovelCharaMoveType.ToOuter => transform.localPosition.x > 0 ? dif : -dif,
                MornNovelCharaMoveType.ToInner => transform.localPosition.x > 0 ? -dif : dif,
                MornNovelCharaMoveType.ToLeft => -dif,
                MornNovelCharaMoveType.ToRight => dif,
                _ => throw new ArgumentOutOfRangeException(nameof(moveType), moveType, null),
            };
            if (isImmediate)
            {
                _ctsX = null;
                _ctsAlpha = null;
                _parentX.localPosition = new Vector3(aimX, _parentX.localPosition.y, _parentX.localPosition.z);
                _renderer.color = new Color(1, 1, 1, 0);
            }
            else
            {
                _ctsX = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
                _ctsAlpha = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
                var taskA = _parentX.DOLocalMoveX(aimX, _novelSettings.AnimDuration, _ctsX.Token);
                var taskB = _renderer.DOFade(0, _novelSettings.AnimDuration, _ctsAlpha.Token);
                await UniTask.WhenAll(taskA, taskB);
            }
        }

        public void Focus(CancellationToken ct = default)
        {
            _ctsY?.Cancel();
            _ctsY = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
            _parentY.DOLocalMoveY(_novelSettings.HeightFocus, _novelSettings.AnimDuration, _ctsY.Token).Forget();
        }

        public void SetUnfocus(bool isImmediate = false, CancellationToken ct = default)
        {
            _ctsY?.Cancel();
            if (isImmediate)
            {
                _ctsY = null;
                _parentY.SetLocalY(_novelSettings.HeightUnfocus);
            }
            else
            {
                _ctsY = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
                _parentY.DOLocalMoveY(_novelSettings.HeightUnfocus, _novelSettings.AnimDuration, _ctsY.Token).Forget();
            }
        }
    }
}