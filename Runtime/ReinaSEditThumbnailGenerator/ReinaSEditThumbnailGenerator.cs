#nullable enable
using UnityEngine;
#if UNITY_EDITOR
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

namespace net.rs64.PAngelsStealersUtility
{
    public class ReinaSEditThumbnailGenerator : MonoBehaviour
    {
        public Texture2D? ThumbnailSource = null;
        public string ThumbnailSourcePath = "";
        public float ThumbnailBlurSigma = 15f;

        public Camera CameraThumbnail = null!;
        public MeshRenderer ThumbnailRenderer = null!;
        public MeshRenderer ThumbnailShadowRenderer = null!;

        public void Rendering()
        {

        }
    }
    [CustomEditor(typeof(ReinaSEditThumbnailGenerator))]
    public class ReinaSEditThumbnailGeneratorEditor : Editor
    {
        ComputeShader? _blueShader;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var thumbnailGen = target as ReinaSEditThumbnailGenerator;
            if (thumbnailGen is null) { return; }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("ThumbnailPath");
                if (GUILayout.Button("Paste from wl-paste"))
                {
                    Undo.RecordObject(thumbnailGen, "Paste from wl-paste");
                    using var pasteProcess = Process.Start(new ProcessStartInfo("wl-paste") { UseShellExecute = false, RedirectStandardOutput = true });
                    pasteProcess.WaitForExit(500);
                    thumbnailGen.ThumbnailSourcePath = pasteProcess.StandardOutput.ReadToEnd().Trim();
                }
            }
            thumbnailGen.ThumbnailSourcePath = GUILayout.TextField(thumbnailGen.ThumbnailSourcePath);

            if (GUILayout.Button("OpenThumbnail"))
            {
                if (File.Exists(thumbnailGen.ThumbnailSourcePath) is false) { Debug.Log("file not found"); return; }
                if (Path.GetExtension(thumbnailGen.ThumbnailSourcePath) is not ".png") { Debug.Log("this is not png"); return; }

                var tex = new Texture2D(2, 2);
                tex.LoadImage(File.ReadAllBytes(thumbnailGen.ThumbnailSourcePath));
                EditorUtility.SetDirty(thumbnailGen);
                thumbnailGen.ThumbnailSource = tex;

                var mat = new Material(Shader.Find("Unlit/Transparent"));
                mat.mainTexture = tex;
                mat.renderQueue += 1;
                thumbnailGen.ThumbnailRenderer.sharedMaterial = mat;

                var expandSize = Mathf.CeilToInt(thumbnailGen.ThumbnailBlurSigma * 4);
                var rtDist = new RenderTexture(tex.width + expandSize, tex.height + expandSize, 0);
                rtDist.enableRandomWrite = true;

                Graphics.CopyTexture(tex, 0, 0, 0, 0, tex.width, tex.height, rtDist, 0, 0, expandSize, expandSize);

                var blurredRt = new RenderTexture(rtDist.descriptor);
                if (_blueShader == null)
                {
                    var path = AssetDatabase.GUIDToAssetPath("712a3e3aa694e56d29e01fb5c66d6e78");
                    _blueShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(path);
                }

                _blueShader.SetFloat("Sigma", thumbnailGen.ThumbnailBlurSigma);
                _blueShader.SetInts("TextureSize", new int[2] { blurredRt.width, blurredRt.height });
                _blueShader.SetTexture(0, "SourceTexture", rtDist);
                _blueShader.SetTexture(0, "TargetTexture", blurredRt);
                _blueShader.Dispatch(0, (blurredRt.width + 31) / 32, (blurredRt.height + 31) / 32, 1);

                DestroyImmediate(rtDist);
                var matS = new Material(Shader.Find("Hidden/Unlit/FakeShadowDrawer"));
                matS.renderQueue = 3000;
                matS.mainTexture = blurredRt;
                matS.color = new Color(0.22f, 0.169f, 0.18f, 0.3f);
                thumbnailGen.ThumbnailShadowRenderer.sharedMaterial = matS;
                thumbnailGen.ThumbnailShadowRenderer.transform.localScale = new Vector3(blurredRt.width / (float)tex.width, blurredRt.height / (float)tex.height, 1);
            }

            if (thumbnailGen.ThumbnailSource != null)
            {
                thumbnailGen.CameraThumbnail.Render();
                EditorGUI.DrawPreviewTexture(EditorGUILayout.GetControlRect(GUILayout.Height(400f)), thumbnailGen.CameraThumbnail.targetTexture, null, ScaleMode.ScaleToFit);
            }
            if (GUILayout.Button("Rendering"))
            {
                thumbnailGen.CameraThumbnail.Render();
                var rt = thumbnailGen.CameraThumbnail.targetTexture;

                AsyncGPUReadback.Request(rt, 0, request =>
                {
                    if (request.hasError) { Debug.LogError("read back error"); }
                    else
                    {
                        var texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
                        texture.LoadRawTextureData(request.GetData<Color32>());
                        var bytes = texture.EncodeToPNG();
                        DestroyImmediate(texture);
                        var path = EditorUtility.SaveFilePanel("result save", "~", "ReinaSEditThumbnail.png", "png");
                        if (string.IsNullOrWhiteSpace(path)) { return; }
                        File.WriteAllBytes(path, bytes);
                    }
                });
            }
        }
    }
}
#else
namespace net.rs64.PAngelsStealersUtility { public class ReinaSEditThumbnailGenerator : MonoBehaviour { } }
#endif
