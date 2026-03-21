#if USE_LUA
using System.Threading;
using Cysharp.Threading.Tasks;
using Lua;
using Lua.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace MornLib
{
    /// <summary>Luaスクリプトでノベルを再生するランナー</summary>
    public sealed class MornNovelLuaRunner : MonoBehaviour
    {
        [Inject] private MornNovelService _novelService;
        [Inject] private MornNovelControllerMono _controller;
        private MornNovelBubbleSo _currentBubble;
        private bool _isPlaying;
        public bool IsPlaying => _isPlaying;

        /// <summary>LuaAssetを再生する</summary>
        public async UniTask PlayAsync(LuaAsset luaAsset, CancellationToken ct = default)
        {
            if (luaAsset == null || _isPlaying)
            {
                return;
            }

            _isPlaying = true;
            _currentBubble = MornNovelGlobal.I.LuaDefaultBubble;
            var lua = new MornLuaCore();
            RegisterFunctions(lua);
            await lua.DoFileAsync(luaAsset, ct: ct);

            // 終了処理（MornNovelEndCommandの「ノベルシーンだけ消す」を模倣）
            await EndNovelAsync(ct);
            _isPlaying = false;
        }

        private async UniTask EndNovelAsync(CancellationToken ct)
        {
            // ステートクリア
            _novelService.ClearNovelState();

            // キャラクター・吹き出し・背景を全て消す
            await _controller.AllHideAsync(ct);
            await _controller.RemoveAllAsync(ct);

            // ノベルシーンをアンロード
            SceneManager.UnloadSceneAsync(gameObject.scene).WithCancellation(ct).Forget();
        }

        private void RegisterFunctions(MornLuaCore lua)
        {
            var global = MornNovelGlobal.I;

            // background(key) or background(key, immediate)
            lua.AddDefaultFunction("background", new LuaFunction(async (context, token) =>
            {
                var key = context.GetArgument<string>(0);
                var immediate = context.ArgumentCount > 1 && context.GetArgument<bool>(1);
                var sprite = global.FindLuaBackground(key);
                if (sprite != null)
                {
                    await _controller.SetBackgroundAsync(sprite, immediate, token);
                }

                return 0;
            }));

            // show(key, pos) or show(key, pos, flipX)
            lua.AddDefaultFunction("show", new LuaFunction(async (context, token) =>
            {
                var key = context.GetArgument<string>(0);
                var pos = context.ArgumentCount > 1 ? (float)context.GetArgument<double>(1) : 0f;
                var flipX = context.ArgumentCount > 2 && context.GetArgument<bool>(2);
                var entry = global.FindLuaCharacter(key);
                if (entry == null)
                {
                    return 0;
                }

                var chara = _controller.GetChara(entry.Talker);
                chara.SetFlipX(flipX);
                chara.SetPose(entry);
                _controller.AllDecreaseOrderInLayer();
                chara.ResetOrderInLayer();
                chara.SetPositionX(pos);
                await chara.SpawnAsync(MornNovelCharaMoveType.ToInner, token);
                return 0;
            }));

            // hide(key)
            lua.AddDefaultFunction("hide", new LuaFunction(async (context, token) =>
            {
                var key = context.GetArgument<string>(0);
                var entry = global.FindLuaCharacter(key);
                if (entry == null)
                {
                    return 0;
                }

                await _controller.GetChara(entry.Talker).HideAsync(MornNovelCharaMoveType.ToOuter, ct: token);
                return 0;
            }));

            // all_hide()
            lua.AddDefaultFunction("all_hide", new LuaFunction(async (context, token) =>
            {
                await _controller.AllHideAsync(token);
                return 0;
            }));

            // change_bubble(key)
            lua.AddDefaultFunction("change_bubble", new LuaFunction((context, _) =>
            {
                var key = context.GetArgument<string>(0);
                _currentBubble = global.FindLuaBubble(key);
                return new System.Threading.Tasks.ValueTask<int>(0);
            }));

            // message(name, text)
            lua.AddDefaultFunction("message", new LuaFunction(async (context, token) =>
            {
                var name = context.GetArgument<string>(0);
                var text = context.GetArgument<string>(1);
                var talker = global.FindLuaTalker(name);

                // 吹き出しを設定
                if (_currentBubble != null && talker != null)
                {
                    _controller.SetBubble(_currentBubble, talker);
                }

                if (talker != null)
                {
                    _controller.SetFocus(talker);
                    _controller.AllDecreaseOrderInLayer();
                    _controller.GetChara(talker)?.ResetOrderInLayer();
                }

                await MornNovelUtil.DOTextAsync(
                    text,
                    _controller.SetMessage,
                    () =>
                    {
                        if (talker == null || talker.Clips == null || talker.Clips.Length == 0)
                        {
                            return (null, 0f);
                        }

                        var clip = talker.Clips[Random.Range(0, talker.Clips.Length)];
                        return (clip, talker.ClipLength);
                    },
                    () => global.SubmitClip,
                    _controller.PlayOneShot,
                    _controller.SetWaitInputIcon,
                    true,
                    () => _novelService.Input(),
                    () => _novelService.IsAutoPlay,
                    x => x == '\n' ? 0.1f : 0.05f,
                    autoSizeText: _controller.MessageText,
                    ct: token);
                return 0;
            }));

            // se(key)
            lua.AddDefaultFunction("se", new LuaFunction((context, _) =>
            {
                var key = context.GetArgument<string>(0);
                var clip = global.FindLuaSe(key);
                if (clip != null)
                {
                    _controller.PlayOneShot(clip);
                }

                return new System.Threading.Tasks.ValueTask<int>(0);
            }));

            // finish() — Luaスクリプトの終端マーカー（実際の終了処理はPlayAsync後処理で実行）
            lua.AddDefaultFunction("finish",
                new LuaFunction((_, _2) => new System.Threading.Tasks.ValueTask<int>(0)));
        }
    }
}
#endif
