using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if USE_ADDRESSABLE
using UnityEngine.AddressableAssets;
#endif

namespace MornLib
{
    internal static class MornNovelUtil
    {
        /// <summary>Addressableに登録されている全てのNovelアドレスを取得する（エディタ専用）</summary>
        public async static UniTask<List<string>> GetAllNovelAddressesAsync()
        {
#if USE_ADDRESSABLE
            var label = MornNovelGlobal.I.AddressLabelTag;
            var locations = await Addressables.LoadResourceLocationsAsync(label, typeof(GameObject));
            return locations.Select(location => location.PrimaryKey).OrderBy(address => address.Split('/').Length).ThenBy(address => address).ToList();
#endif
            return new List<string>();
        }

        public static bool IsUpperNovel => SceneManager.sceneCount > 1;

        public async static UniTask DOTextAsync(string context,
            Action<string> setText,
            Func<(AudioClip clip, float interval)> getMessageClip,
            Func<AudioClip> getSubmitClip,
            Action<AudioClip> playSe,
            Action<bool> showWaitInputIcon,
            bool isWaitInput,
            Func<bool> submitFunc,
            Func<bool> isAutoPlayFunc,
            Func<char, float> charInterval,
            float startOffset = 0.1f,
            float endOffset = 0.1f,
            bool waitAtMark = true,
            TMP_Text autoSizeText = null,
            CancellationToken ct = default)
        {
            var prefix = "";
            if (autoSizeText != null)
            {
                autoSizeText.text = context;
                autoSizeText.ForceMeshUpdate();
                prefix = $"<size={autoSizeText.fontSize}>";
                autoSizeText.text = "";
                autoSizeText.ForceMeshUpdate();
            }

            setText("");
            await UniTask.Delay(TimeSpan.FromSeconds(startOffset), cancellationToken: ct);
            var sb = new StringBuilder();
            var nextSeTime = 0f;
            for (var i = 0; i < context.Length; i++)
            {
                var nextChar = context[i];
                if (nextChar == '@' && waitAtMark)
                {
                    await WaitSubmit();
                }
                else
                {
                    sb.Append(nextChar);
                    setText(prefix + sb);
                    var canSe = nextChar switch
                    {
                        '\n' => false, // 改行はSEを鳴らさない
                        '@'  => false,
                        _    => true,
                    };
                    if (Time.time >= nextSeTime && canSe)
                    {
                        var (clip, interval) = getMessageClip();
                        if (clip != null)
                        {
                            playSe(clip);
                            nextSeTime = Time.time + interval;
                        }
                    }

                    var waitInterval = charInterval(nextChar);
                    if (await WaitSecondsReturnSkipped(waitInterval, submitFunc, ct))
                    {
                        if (waitAtMark)
                        {
                            // 次の@まで進める
                            while (i + 1 < context.Length && context[i + 1] != '@')
                            {
                                sb.Append(context[i + 1]);
                                i++;
                            }

                            setText(prefix + sb);
                            continue;
                        }

                        break;
                    }
                }
            }

            if (waitAtMark)
            {
                context = context.Replace("@", "");
            }

            setText(prefix + context);
            if (isWaitInput)
            {
                await WaitSubmit();
            }

            await UniTask.Delay(TimeSpan.FromSeconds(endOffset), cancellationToken: ct);
            return;

            async UniTask WaitSubmit()
            {
                showWaitInputIcon(true);
                while (!submitFunc() && !isAutoPlayFunc())
                {
                    await UniTask.Yield(ct);
                }

                playSe(getSubmitClip());
                // 次Fへ入力を渡さないために1F待機
                await UniTask.Yield(ct);
                showWaitInputIcon(false);
            }
        }

        private async static UniTask<bool> WaitSecondsReturnSkipped(float seconds, Func<bool> submitFunc, CancellationToken ct = default)
        {
            var elapsedTime = 0f;
            while (elapsedTime < seconds)
            {
                elapsedTime += Time.deltaTime;
                if (submitFunc())
                {
                    // 次Fへ入力を渡さないために1F待機
                    await UniTask.Yield(ct);
                    return true;
                }

                await UniTask.Yield(ct);
            }

            return false;
        }

        public async static UniTask DOLocalMove(this Transform target, Vector3 endValue, float duration, CancellationToken ct = default)
        {
            if (target != null)
            {
                await DOAsync(target.localPosition, endValue, duration, Vector3.Lerp, x => target.localPosition = x, ct);
            }
        }

        public async static UniTask DOLocalMoveX(this Transform target, float endValue, float duration, CancellationToken ct = default)
        {
            if (target != null)
            {
                await DOAsync(target.localPosition.x, endValue, duration, Mathf.Lerp, target.SetLocalX, ct);
            }
        }

        public async static UniTask DOLocalMoveY(this Transform target, float endValue, float duration, CancellationToken ct = default)
        {
            if (target != null)
            {
                await DOAsync(target.localPosition.y, endValue, duration, Mathf.Lerp, target.SetLocalY, ct);
            }
        }

        public async static UniTask DOFade(this CanvasGroup target, float endValue, float duration, CancellationToken ct = default)
        {
            if (target != null)
            {
                await DOAsync(target.alpha, endValue, duration, Mathf.Lerp, x => target.alpha = x, ct);
            }
        }

        public async static UniTask DOFade(this Image target, float endValue, float duration, CancellationToken ct = default)
        {
            if (target)
            {
                await DOAsync(target.color.a, endValue, duration, Mathf.Lerp, x => SetAlpha(target, x), ct);
            }
        }

        public static async UniTask DoMaterialFloat(this Image target, string propertyName, float endValue, float duration, CancellationToken ct = default)
        {
            if (target) await DOAsync(target.material.GetFloat(propertyName), endValue, duration, Mathf.Lerp, x => target.material.SetFloat(propertyName, x), ct);
        }

        public async static UniTask DOFade(this SpriteRenderer target, float endValue, float duration, CancellationToken ct = default)
        {
            if (target)
            {
                await DOAsync(target.color.a, endValue, duration, Mathf.Lerp, x => SetAlpha(target, x), ct);
            }
        }

        private async static UniTask DOAsync<T>(T startValue, T endValue, float duration, Func<T, T, float, T> rateFunc, Action<T> onUpdateValue, CancellationToken ct = default)
        {
            // TODO Easingを設定できるように
            var elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                var rate = elapsedTime / duration;
                // OutQuad
                rate = 1 - (1 - rate) * (1 - rate);

                // EaseOutBack
                // const float c1 = 1.70158f;
                // const float c3 = c1 + 1;
                // rate -= 1;
                // rate = rate * rate * ((c3 + 1) * rate + c1) + 1;
                var value = rateFunc(startValue, endValue, rate);
                onUpdateValue.Invoke(value);
                elapsedTime += Time.deltaTime;
                await UniTask.Yield(cancellationToken: ct);
            }

            onUpdateValue.Invoke(endValue);
        }

        public static void SetAlpha(this Image target, float alpha)
        {
            var color = target.color;
            color.a = alpha;
            target.color = color;
        }

        public static void SetAlpha(this SpriteRenderer target, float alpha)
        {
            var color = target.color;
            color.a = alpha;
            target.color = color;
        }

        public static void SetLocalX(this Transform target, float value)
        {
            var pos = target.localPosition;
            pos.x = value;
            target.localPosition = pos;
        }

        public static void SetLocalY(this Transform target, float value)
        {
            var pos = target.localPosition;
            pos.y = value;
            target.localPosition = pos;
        }
    }
}