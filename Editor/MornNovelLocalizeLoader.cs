using Arbor;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace MornLib
{
    internal sealed class MornNovelLocalizeBuilder
    {
        private readonly string _uploadUrl;
        private readonly ArborFSM _arborFsm;
        private MornLocalizeDictionary _mornLocalizeDictionary;
        private int _index;

        public MornNovelLocalizeBuilder(string uploadUrl, ArborFSM arborFsm)
        {
            _uploadUrl = uploadUrl;
            _arborFsm = arborFsm;
        }

        public void UploadScenario(bool convertToFromKey = false, bool upload = true, bool debugJson = false)
        {
            var state = _arborFsm.GetStateFromID(_arborFsm.startStateID);
            _mornLocalizeDictionary = new MornLocalizeDictionary(_arborFsm.name);

            //ノードを順に取得
            while (true)
            {
                var nextState = BuildAndGetNextState(state, convertToFromKey);
                if (nextState == null)
                    break;
                state = nextState;
            }
            
            var json = _mornLocalizeDictionary.ToJson();
            if (debugJson)
            {
                Debug.Log(json);
            }

            //jsonを作成
            if (upload)
            {
                MornSpreadSheetUploader.UploadJson(_uploadUrl, json).Forget();
            }
        }

        public void ConvertScenarioToDebug()
        {
            var state = _arborFsm.GetStateFromID(_arborFsm.startStateID);
            _mornLocalizeDictionary = new MornLocalizeDictionary(_arborFsm.name);

            //ノードを順に取得
            while (true)
            {
                var nextState = ConvertAndGetNextState(state);
                if (nextState == null)
                    break;
                state = nextState;
            }
        }

        private Arbor.State BuildAndGetNextState(Arbor.State state, bool convertToFromKey)
        {
            var behaviourCount = state.behaviourCount;
            for (int i = 0; i < behaviourCount; i++)
            {
                var behaviour = state.GetBehaviourFromIndex(i);
                if (behaviour is MornNovelMessageCommand messageCommand)
                {
                    _index++;
                    var localizeString = messageCommand.GetLocalizeString();
                    string key = $"{_arborFsm.name}.{behaviour.nodeID}";
                    string talker = messageCommand.GetTalkerName();
                    _mornLocalizeDictionary.AddString(key, messageCommand.GetText(), _index, talker);

                    //SpreadSheetにアップロード
                    if (localizeString.StringType == MornLocalizeStringType.Edit)
                    {
                        localizeString.Key = $"{_arborFsm.name}.{behaviour.nodeID}";
                        if (convertToFromKey)
                        {
                            localizeString.StringType = MornLocalizeStringType.FromKey;
                        }

                        EditorUtility.SetDirty(messageCommand);
                    }
                }

                for (var j = 0; j < behaviour.stateLinkCount; j++)
                {
                    var stateLink = behaviour.GetStateLink(j).stateID;
                    var nextState = _arborFsm.GetStateFromID(stateLink);
                    if (nextState != null)
                    {
                        return nextState;
                    }
                }
            }

            return null;
        }

        private Arbor.State ConvertAndGetNextState(Arbor.State state)
        {
            var behaviourCount = state.behaviourCount;
            for (int i = 0; i < behaviourCount; i++)
            {
                var behaviour = state.GetBehaviourFromIndex(i);
                if (behaviour is MornNovelMessageCommand messageCommand)
                {
                    _index++;
                    var localizeString = messageCommand.GetLocalizeString();
                    Debug.Log("finish");
                    if (localizeString.StringType == MornLocalizeStringType.FromKey)
                    {
                        localizeString.DebugString = messageCommand.GetText();
                        localizeString.StringType = MornLocalizeStringType.Edit;
                        EditorUtility.SetDirty(messageCommand);
                    }
                }

                for (var j = 0; j < behaviour.stateLinkCount; j++)
                {
                    var stateLink = behaviour.GetStateLink(j).stateID;
                    var nextState = _arborFsm.GetStateFromID(stateLink);
                    if (nextState != null)
                    {
                        return nextState;
                    }

                }
            }

            return null;
        }
    }
}