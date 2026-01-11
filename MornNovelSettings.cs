using MornEditor;
using Unity.Cinemachine;
using UnityEngine;

namespace MornLib
{
    [CreateAssetMenu(fileName = nameof(MornNovelSettings), menuName = "Morn/" + nameof(MornNovelSettings))]
    public sealed class MornNovelSettings : ScriptableObject
    {
        [Header("サウンド")]
        [Label("音量フェード対象")] public MornSoundVolumeType FadeVolumeType;
        [Label("BGMフェード時間")] public float BgmChangeSec = 1f;
        [Header("トランジション")]
        [Label("背景シンプル切り替え時間")] public float BackgroundFadeSec = 1f;
        [Label("背景歪みトランジションMaterial")] public Material DistortTransitionMaterial;
        [Label("背景歪みトランジション時間")] public float DistortTransitionSec = 1f;
        [Header("目ぱち")]
        [Label("目を閉じている長さ")] public Vector2 EyeCloseRange;
        [Label("目を開いている長さ")] public Vector2 EyeOpenRange;
        [Header("キャラクター")]
        [Label("移動距離")] public float SpawnDifX = 0.5f;
        [Label("移動時間")] public float AnimDuration = 0.5f;
        [Label("フォーカス時の高さ")] public float HeightFocus = -0.8f;
        [Label("非フォーカス時の高さ")] public float HeightUnfocus = -1.2f;
        [Label("スケール")]public float PositionScale = 11;
        [Header("カメラシェイク")]
        [Label("プレハブ")] public CinemachineImpulseSource SourcePrefab;
    }
}