using System;
using System.Globalization;
using System.IO;
using System.Text;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace PhysicsBattler.Diagnostics
{
    /// <summary>
    /// Saves small pictures of the game view while a playtest is recorded, so the log can be read
    /// alongside what it looked like: frames/NNNNNN.jpg (square, low resolution) and frames/frames.csv
    /// (when each was taken and from where).
    ///
    /// A second camera copies the main camera each time and renders into a small texture; the
    /// pixels are read back without stalling the game (AsyncGPUReadback) and written as JPEGs. Its
    /// field of view is widened so it shows the same width as the main view, in a square. The UI is
    /// not in the pictures. Times are game time, so frames line up with the rest of the log and a
    /// paused game takes no pictures.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FrameCapture : MonoBehaviour
    {
        private Camera captureCamera;
        private RenderTexture target;
        private StreamWriter index;
        private string folder;
        private int size = 300;
        private float interval = 0.125f;
        private int quality = 70;
        private int maxFrames = 2400;
        private float startTime;
        private float nextCapture;
        private int requested;
        private int saved;
        private int failed;
        private bool renderPending;
        private float pendingTime;
        private int pendingStep;
        private bool warnedNoCamera;

        public bool Capturing => index != null;
        public int Saved => saved;
        public int Failed => failed;
        public float Rate => interval > 0f ? 1f / interval : 0f;
        public int Size => size;

        /// <summary>The physics step the recorder is on, for the index.</summary>
        public Func<int> CurrentStep;

        public void Begin(string sessionPath, float start, int pixels, float perSecond, int jpegQuality, int limit)
        {
            End();
            size = Mathf.Clamp(pixels, 64, 1024);
            interval = 1f / Mathf.Clamp(perSecond, 0.5f, 30f);
            quality = Mathf.Clamp(jpegQuality, 10, 100);
            maxFrames = Mathf.Max(1, limit);
            startTime = start;
            nextCapture = Time.time;
            requested = 0;
            saved = 0;
            failed = 0;
            renderPending = false;
            warnedNoCamera = false;

            if (!SystemInfo.supportsAsyncGPUReadback)
            {
                Debug.LogWarning("[Playtest] This GPU can't read pictures back asynchronously; no frames will be saved.");
                return;
            }

            try
            {
                folder = Path.Combine(sessionPath, "frames");
                Directory.CreateDirectory(folder);
                index = new StreamWriter(Path.Combine(folder, "frames.csv"), false, new UTF8Encoding(false));
                index.WriteLine("frame,t,step,file,camX,camY,camZ,camYaw,camPitch,fov");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Playtest] Could not start saving frames: {exception.Message}");
                index = null;
                return;
            }

            target = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { name = "PlaytestFrame" };
            target.Create();
            var cameraObject = new GameObject("PlaytestFrameCamera") { hideFlags = HideFlags.HideAndDontSave };
            captureCamera = cameraObject.AddComponent<Camera>();
            captureCamera.enabled = false;
        }

        public void End()
        {
            renderPending = false;
            if (captureCamera != null)
            {
                Destroy(captureCamera.gameObject);
                captureCamera = null;
            }

            if (target != null)
            {
                target.Release();
                Destroy(target);
                target = null;
            }

            if (index != null)
            {
                try
                {
                    index.Dispose();
                }
                catch (Exception)
                {
                    // Shutting down; the index is as complete as it will get.
                }

                index = null;
            }
        }

        private void OnDisable() => End();

        private void Update()
        {
            if (!Capturing || captureCamera == null)
            {
                return;
            }

            // Last frame's picture has been rendered: read it back and stop the camera again.
            if (renderPending)
            {
                renderPending = false;
                captureCamera.enabled = false;
                int frame = requested++;
                float t = pendingTime;
                int stepAtCapture = pendingStep;
                var cam = captureCamera.transform;
                Vector3 position = cam.position;
                Vector3 angles = cam.eulerAngles;
                float fov = captureCamera.fieldOfView;
                AsyncGPUReadback.Request(target, 0, TextureFormat.RGBA32, request => Save(request, frame, t, stepAtCapture, position, angles, fov));
            }

            if (Time.timeScale <= 0f || Time.time < nextCapture || requested >= maxFrames)
            {
                return;
            }

            var main = MainCamera();
            if (main == null)
            {
                if (!warnedNoCamera)
                {
                    warnedNoCamera = true;
                    Debug.LogWarning("[Playtest] No camera to take frames from.");
                }

                return;
            }

            nextCapture = Mathf.Max(nextCapture + interval, Time.time);
            // (Its render happens in Unity's own loop this frame: Camera.Render in the counters.)
            using var renderScope = Perf.Begin(PerfSection.CaptureRender);
            captureCamera.CopyFrom(main);
            captureCamera.targetTexture = target;
            captureCamera.aspect = 1f;
            // The same width as the main view, in a square picture.
            captureCamera.fieldOfView = Mathf.Min(120f, Camera.VerticalToHorizontalFieldOfView(main.fieldOfView, main.aspect));
            captureCamera.depth = main.depth - 1f;
            captureCamera.enabled = true;   // renders this frame; read back next Update
            renderPending = true;
            pendingTime = Time.time - startTime;
            pendingStep = CurrentStep != null ? CurrentStep() : 0;
        }

        private Camera MainCamera()
        {
            var main = Camera.main;
            if (main != null && main != captureCamera)
            {
                return main;
            }

            foreach (var cam in Camera.allCameras)
            {
                if (cam != captureCamera && cam.targetTexture == null)
                {
                    return cam;
                }
            }

            return null;
        }

        private void Save(AsyncGPUReadbackRequest request, int frame, float t, int stepAtCapture, Vector3 position, Vector3 angles, float fov)
        {
            using (Perf.Begin(PerfSection.CaptureSave))
            {
                SaveCore(request, frame, t, stepAtCapture, position, angles, fov);
            }
        }

        private void SaveCore(AsyncGPUReadbackRequest request, int frame, float t, int stepAtCapture, Vector3 position, Vector3 angles, float fov)
        {
            if (index == null)
            {
                return;   // the session ended while this was on its way
            }

            if (request.hasError)
            {
                failed++;
                return;
            }

            string file = frame.ToString("000000", CultureInfo.InvariantCulture) + ".jpg";
            try
            {
                NativeArray<byte> pixels = request.GetData<byte>();
                // Read back in texture order (bottom row first), as EncodeNativeArrayToJPG expects.
                using (var jpg = ImageConversion.EncodeNativeArrayToJPG(pixels, GraphicsFormat.R8G8B8A8_SRGB, (uint)size, (uint)size, 0, quality))
                {
                    File.WriteAllBytes(Path.Combine(folder, file), jpg.ToArray());
                }

                saved++;
                var inv = CultureInfo.InvariantCulture;
                index.WriteLine(string.Join(",",
                    frame.ToString(inv), t.ToString("0.000", inv), stepAtCapture.ToString(inv), file,
                    position.x.ToString("0.###", inv), position.y.ToString("0.###", inv), position.z.ToString("0.###", inv),
                    angles.y.ToString("0.#", inv), angles.x.ToString("0.#", inv), fov.ToString("0.#", inv)));
                if (saved % 40 == 0)
                {
                    index.Flush();
                }
            }
            catch (Exception exception)
            {
                failed++;
                if (failed == 1)
                {
                    Debug.LogWarning($"[Playtest] Could not save a frame: {exception.Message}");
                }
            }
        }
    }
}
