using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("MornNovel.Editor")]
namespace MornLib
{
    [CreateAssetMenu(fileName = nameof(MornNovelGlobal), menuName = "Morn/" + nameof(MornNovelGlobal))]
    public sealed class MornNovelGlobal : MornGlobalBase<MornNovelGlobal>
    {
        [SerializeField] private string _addressGroupName = "Main";
        [SerializeField] private string _addressLabelTag = "Novel";
        [SerializeField] private string _ignoreAddressPrefix;
        [SerializeField] private MornSceneObject _novelScene;
        [SerializeField] private string _uploadUrl;
        public AudioClip SubmitClip;
        public float MessageOffset = 0.1f;
        public float CharInterval = 0.05f;
        public float CharReturnInterval = 0.1f;
        public MornTransitionType DebugTransition;
        protected override string ModuleName => "MornNovel";
        public string AddressGroupName => _addressGroupName;
        public string AddressLabelTag => _addressLabelTag;
        public string IgnoreAddressPrefix => _ignoreAddressPrefix;
        public MornSceneObject NovelScene => _novelScene;
        public string UploadUrl => _uploadUrl;

        public static void Log(string message)
        {
            I.LogInternal(message);
        }

        public static void LogWarning(string message)
        {
            I.LogWarningInternal(message);
        }

        public static void LogError(string message)
        {
            I.LogErrorInternal(message);
        }
    }
}