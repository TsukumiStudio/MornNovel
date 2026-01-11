using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MornLib
{
    internal static class MornSpreadSheetUploader
    {
        public async static UniTask UploadJson(string uploadUrl, string json)
        {
            //jsonをpost
            var form = new UnityWebRequest(uploadUrl, "POST");
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            
            form.uploadHandler = new UploadHandlerRaw(jsonBytes);
            form.downloadHandler = new DownloadHandlerBuffer();
            form.SetRequestHeader("Content-Type", "application/json");
            var r = await form.SendWebRequest();
            
            //結果を表示
            Debug.Log(r.downloadHandler.text);
            
            if (r.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Success");
            }
            else
            {
                Debug.Log("Failed");
            }
            
            MornLocalizeGlobal.UpdateMasterDataAsync().Forget();
        }
    }
}