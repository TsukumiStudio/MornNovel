using System;
using Arbor;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace MornLib
{
    internal class MornNovelCameraShakeCommand : MornNovelCommandBase
    {
        public override string Tips => "カメラを揺らす";
        [SerializeField, Label("揺らし続ける")] private bool _isLoop;
        [SerializeField] private MornNovelCameraShakeSettings _settings;
        [SerializeField] private StateLink _onComplete;
        [Inject] private MornNovelSettings _novelSettings;

        public override async void OnStateBegin()
        {
            base.OnStateBegin();
            try
            {
                if (_novelSettings.SourcePrefab != null)
                {
                    var source = Instantiate(_novelSettings.SourcePrefab);
                    await ShakeAsync(source);
                    Destroy(source.gameObject);
                }

                Transition(_onComplete);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async UniTask ShakeAsync(CinemachineImpulseSource source)
        {
            for (var i = 0; i < _settings.ShakeCount || _isLoop; i++)
            {
                var randomDir = Random.insideUnitCircle.normalized;
                var shakeVector = new Vector3(randomDir.x, randomDir.y, 0f) * _settings.ShakeStrength;
                var impulse = source.ImpulseDefinition;
                impulse.ImpulseDuration = _settings.ShakeInterval;
                source.ImpulseDefinition = impulse;
                source.GenerateImpulse(shakeVector);
                await UniTask.Delay(
                    TimeSpan.FromSeconds(_settings.ShakeInterval),
                    cancellationToken: CancellationTokenOnEnd);
            }
        }
    }
}