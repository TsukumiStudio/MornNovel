using System.Reflection;
using Arbor;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace MornLib
{
    internal sealed class MornLocalizeShortcutWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private ArborFSM _arborFsm;

        [MenuItem("Tools/" + nameof(MornLocalizeShortcutWindow))]
        private static void Open()
        {
            GetWindow<MornLocalizeShortcutWindow>(nameof(MornLocalizeShortcutWindow));
        }

        private void OnGUI()
        {
            using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                DrawLocalize();
                DrawArbor();
                _scrollPosition = scroll.scrollPosition;
            }
        }

        private void DrawLocalize()
        {
            GUILayout.Label("★シート管理");
            if (GUILayout.Button("ローカライズシートを開く"))
            {
                MornLocalizeGlobal.OpenMasterData();
            }

            if (GUILayout.Button("ローカライズシートを更新"))
            {
                MornLocalizeGlobal.UpdateMasterDataAsync().Forget();
            }
        }

        private void DrawArbor()
        {
            GUILayout.Label("★シナリオアップロード");
            //テキストとボタンを横並び
            //開いているシナリオを取得ボタンの横にシナリオ名を表示
            GUILayout.Label("最初にシナリオ取得ボタンを押す");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("シナリオ取得"))
            {
                _arborFsm = GetArborFSMFromEditor();
            }

            if (_arborFsm == null)
            {
                GUILayout.Label("Null");
            }
            else
            {
                GUILayout.Label(_arborFsm.name);
            }

            GUILayout.EndHorizontal();
            if (_arborFsm == null)
            {
                return;
            }

            var uploadUrl = MornNovelGlobal.I.UploadUrl;
            
            if (GUILayout.Button("Json出力"))
            {
                CheckArbor();
                var loader = new MornNovelLocalizeBuilder(uploadUrl, _arborFsm);
                loader.UploadScenario(false, false, true);
            }
            
            if (GUILayout.Button("シナリオアップロード + FromKeyに変換 + 更新"))
            {
                CheckArbor();
                var loader = new MornNovelLocalizeBuilder(uploadUrl, _arborFsm);
                loader.UploadScenario(true);
            }

            if (GUILayout.Button("シナリオアップロード + 更新"))
            {
                CheckArbor();
                var loader = new MornNovelLocalizeBuilder(uploadUrl, _arborFsm);
                loader.UploadScenario();
            }

            if (GUILayout.Button("FromKeyに変換"))
            {
                CheckArbor();
                var loader = new MornNovelLocalizeBuilder(uploadUrl, _arborFsm);
                loader.UploadScenario(true, false);
            }

            if (GUILayout.Button("Editに変換"))
            {
                CheckArbor();
                var loader = new MornNovelLocalizeBuilder(uploadUrl, _arborFsm);
                loader.ConvertScenarioToDebug();
            }
        }

        private void CheckArbor()
        {
            if (GetArborFSMFromEditor() != _arborFsm)
            {
                Debug.LogWarning("開いているArborFSMが変わっています");
            }
        }

        //現在開いているArbor EditorのEditorウインドウから，ArborFSMを取得
        private static ArborFSM GetArborFSMFromEditor()
        {
            // ArborEditorウィンドウの取得
            var editorWindowType = typeof(EditorWindow);
            var windows = Resources.FindObjectsOfTypeAll(editorWindowType);
            foreach (var window in windows)
            {
                if (window.GetType().Name == "ArborEditorWindow")
                {
                    var arborFSMField = window.GetType().GetField(
                        "_NodeGraphCurrent",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    if (arborFSMField != null)
                    {
                        var arborFSM = arborFSMField.GetValue(window) as ArborFSM;
                        if (arborFSM != null)
                        {
                            return arborFSM;
                        }
                    }
                }
            }

            return null;
        }
    }
}