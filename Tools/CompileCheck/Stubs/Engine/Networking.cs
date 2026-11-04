// Stand-ins for UnityWebRequest as the sound generator uses it (nothing is sent headless).
namespace UnityEngine.Networking
{
    public class DownloadHandler
    {
        public byte[] data => new byte[0];
        public string text => "";
    }

    public sealed class DownloadHandlerBuffer : DownloadHandler
    {
    }

    public class UploadHandler
    {
        public string contentType { get; set; }
    }

    public sealed class UploadHandlerRaw : UploadHandler
    {
        public UploadHandlerRaw(byte[] data) { }
    }

    public class UnityWebRequestAsyncOperation : AsyncOperation
    {
    }

    public sealed class UnityWebRequest : System.IDisposable
    {
        public const string kHttpVerbPOST = "POST";
        public const string kHttpVerbGET = "GET";

        public enum Result { InProgress, Success, ConnectionError, ProtocolError, DataProcessingError }

        public UnityWebRequest(string url, string method) { }
        public UnityWebRequest(string url, string method, DownloadHandler downloadHandler, UploadHandler uploadHandler) { }
        public DownloadHandler downloadHandler { get; set; }
        public UploadHandler uploadHandler { get; set; }
        public long responseCode => 0;
        public string error => "";
        public Result result => Result.Success;
        public bool isDone => true;
        public int timeout { get; set; }
        public void SetRequestHeader(string name, string value) { }
        public UnityWebRequestAsyncOperation SendWebRequest() => new UnityWebRequestAsyncOperation();
        public void Abort() { }
        public void Dispose() { }
    }
}
