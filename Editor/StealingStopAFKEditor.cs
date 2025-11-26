#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;
using nadena.dev.ndmf.runtime;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;
using VRC.SDK3.Avatars.Components;

namespace net.rs64.PAngelsStealersUtility
{
    [CustomEditor(typeof(StealingStopAFK))]
    public sealed class StealingStopAFKEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        public static bool Valid(StealingStopAFK stealingStopAFK)
        {
            return stealingStopAFK.ReinaSEditTargetTextures.Any();
        }
        public static void Execute(BuildContext context, StealingStopAFK stealingStop)
        {
            var asc = context.Extension<AnimatorServicesContext>();

            var controllerCtx = asc.ControllerContext;
            var controller = controllerCtx.Controllers[VRCAvatarDescriptor.AnimLayerType.FX];

            var layer = VirtualLayer.Create(controllerCtx.CloneContext, "ReinaSakiria-StealingStop-SwapMaterial");

            var stateMachine = VirtualStateMachine.Create(controllerCtx.CloneContext, "ReinaSakiria-StealingStop-SwapMaterial-state-machine");
            var idleClip = VirtualClip.Create("idle-material");
            var stopClip = VirtualClip.Create("stop-material");

            WriteClip(context, idleClip, stopClip, stealingStop);

            var idleState = stateMachine.AddState("idle", idleClip);
            var stopState = stateMachine.AddState("stop", stopClip);

            var idle2Stop = VirtualStateTransition.Create();
            idle2Stop.ExitTime = null;
            idle2Stop.Duration = 0f;
            idle2Stop.Conditions = ImmutableList.Create(new AnimatorCondition() { mode = AnimatorConditionMode.If, parameter = "AFK" });
            idle2Stop.SetDestination(stopState);
            idleState.Transitions = ImmutableList.Create(idle2Stop);


            var stop2Idle = VirtualStateTransition.Create();
            stop2Idle.ExitTime = null;
            stop2Idle.Duration = 0f;
            stop2Idle.Conditions = ImmutableList.Create(new AnimatorCondition() { mode = AnimatorConditionMode.IfNot, parameter = "AFK" });
            stop2Idle.SetDestination(idleState);
            stopState.Transitions = ImmutableList.Create(stop2Idle);

            layer.StateMachine = stateMachine;
            controller.AddLayer(LayerPriority.Default, layer);
        }

        public static void WriteClip(BuildContext context, VirtualClip idle, VirtualClip stop, StealingStopAFK stealingStop)
        {
            var renderers = context.AvatarRootObject.GetComponentsInChildren<Renderer>();
            var materials = renderers.SelectMany(r => r.sharedMaterials).Distinct().ToArray();
            var textures = materials.SelectMany(m => m.GetTextureReferences().Values).Distinct().ToArray();
            var targetTextures = Targeting(stealingStop, textures);
            if (targetTextures.Any() is false) { return; }
            TextureProcessing(context, targetTextures);
            var materialMap = MaterialProcessing(context, materials, targetTextures, stealingStop);

            foreach (var renderer in renderers)
            {
                var mats = renderer.sharedMaterials;
                if (mats.Any(m => materialMap.Keys.Contains(m)) is false) { continue; }
                var path = RuntimeUtil.RelativePath(context.AvatarRootObject, renderer.gameObject);

                for (var i = 0; mats.Length > i; i += 1)
                {
                    var mat = mats[i];
                    if (materialMap.ContainsKey(mat) is false) { continue; }

                    var matBind = EditorCurveBinding.PPtrCurve(path, typeof(Renderer), $"m_Materials.Array.data[{i}]");
                    idle.SetObjectCurve(matBind, new ObjectReferenceKeyframe[] { new() { time = 0f, value = mat }, new() { time = 1f, value = mat } });
                    stop.SetObjectCurve(matBind, new ObjectReferenceKeyframe[] { new() { time = 0f, value = materialMap[mat] }, new() { time = 1f, value = materialMap[mat] } });
                }
            }
        }

        static Dictionary<Texture, Texture2D> Targeting(StealingStopAFK stealingStop, Texture[] textures)
        {
            var originals = stealingStop.ReinaSEditTargetTextures;
            var res = new Dictionary<Texture, Texture2D>();
            foreach (var destTexture in textures)
            {
                foreach (var originalTexture in originals)
                {
                    if (ObjectRegistry.GetReference(destTexture) == ObjectRegistry.GetReference(originalTexture))
                    {
                        res.Add(destTexture, originalTexture);
                        break;
                    }
                }
            }
            return res;
        }
        static void TextureProcessing(BuildContext context, Dictionary<Texture, Texture2D> targetTextures)
        {
            foreach (var kv in targetTextures.ToArray())
            {
                var originalTexture = kv.Value.TryGetUnCompress();
                var miniRt = new RenderTexture(kv.Key.width / 4, kv.Key.height / 4, 0, RenderTextureFormat.ARGB32);
                var miniTex = new Texture2D(miniRt.width, miniRt.height, TextureFormat.RGBA32, true);
                miniTex.name = kv.Value.name + "(PASU-Minimized)";

                Graphics.Blit(originalTexture, miniRt);
                miniRt.DownloadFromRenderTexture(miniTex.GetPixelData<Color32>(0).AsSpan());
                miniTex.Apply(true);
                EditorUtility.CompressTexture(miniTex, TextureFormat.DXT1, 100);// なんと決め打ち！ まぁこんな物に対した圧縮フォマットを使っても仕方がない。
                miniTex.Apply(false, true);
                EditorTextureUtility.ForceMarkStreamingMipMap(miniTex);

                targetTextures[kv.Key] = miniTex;

                DestroyImmediate(miniRt);
                if (kv.Value != originalTexture) DestroyImmediate(originalTexture);

                ObjectRegistry.RegisterReplacedObject(kv.Value, miniTex);
                context.AssetSaver.SaveAsset(miniTex);
            }
        }

        static Dictionary<Material, Material> MaterialProcessing(BuildContext context, Material[] materials, Dictionary<Texture, Texture2D> targetTextures, StealingStopAFK stealingStop)
        {
            var dict = new Dictionary<Material, Material>(materials.Length);
            foreach (var mat in materials)
            {
                if (targetTextures.Keys.Any(t => mat.ReferencesTexture(t)) is false) { continue; }
                var isSuspendEmission = stealingStop.InSuspendingEmissionTargetMaterials.Any(m => ObjectRegistry.GetReference(m) == ObjectRegistry.GetReference(mat));

                var swapMat = Instantiate(mat);
                swapMat.name = mat.name + "(PASU-StopMat)";

                foreach (var kv in targetTextures) { swapMat.ReplaceTexture(kv.Key, kv.Value); }
                if (isSuspendEmission) { swapMat.SetInt("_UseEmission", 0); }

                context.AssetSaver.SaveAsset(swapMat);
                dict[mat] = swapMat;
            }
            return dict;
        }
    }
    public sealed class StealingStopAFKPass : Pass<StealingStopAFKPass>
    {
        protected override void Execute(BuildContext context)
        {
            foreach (var i in context.AvatarRootObject.GetComponentsInChildren<StealingStopAFK>(true))
            {
                if (StealingStopAFKEditor.Valid(i) is false) { continue; }
                StealingStopAFKEditor.Execute(context, i);
            }
        }
    }
}

// original from TexTransTool (MIT License) https://github.com/ReinaS-64892/TexTransTool/blob/a1f3f1e6e77a066b5fd47f2b692e069cf18b8ff0/Runtime/Utils/MaterialUtility.cs
internal static class MaterialUtility
{
    public static bool ReferencesTexture(this Material material, Texture target)
    {
        if (material == null || target == null)
        {
            return false;
        }
        var shader = material.shader;

        if (shader == null)
        {
            return false;
        }

        var propertyCount = shader.GetPropertyCount();
        for (var i = 0; i < propertyCount; i++)
        {

            if (shader.GetPropertyType(i) is UnityEngine.Rendering.ShaderPropertyType.Texture)
            {
                var propertyNameId = shader.GetPropertyNameId(i);
                var texture = material.GetTexture(propertyNameId);
                if (texture == target)
                {
                    return true;
                }
            }
        }
        return false;
    }
    public static void ReplaceTexture(this Material material, Texture oldTexture, Texture newTexture)
    {
        if (material == null || oldTexture == null)
        {
            return;
        }
        var shader = material.shader;

        if (shader == null)
        {
            return;
        }

        var propertyCount = shader.GetPropertyCount();
        for (var i = 0; i < propertyCount; i++)
        {

            if (shader.GetPropertyType(i) is UnityEngine.Rendering.ShaderPropertyType.Texture)
            {
                var propertyNameId = shader.GetPropertyNameId(i);
                var texture = material.GetTexture(propertyNameId);
                if (texture == oldTexture)
                {
                    material.SetTexture(propertyNameId, newTexture);
                }
            }
        }
    }

    public static Dictionary<string, Texture> GetTextureReferences(this Material material) => material.GetTextureReferences<Texture>();
    public static Dictionary<string, T> GetTextureReferences<T>(this Material material)
        where T : Texture
    {
        if (material == null)
        {
            return new();
        }

        var shader = material.shader;
        if (shader == null)
        {
            return new();
        }

        var propertyCount = shader.GetPropertyCount();
        if (propertyCount == 0)
        {
            return new();
        }
        Dictionary<string, T> dictionary = new(propertyCount);

        for (var propertyIndex = 0; propertyIndex < propertyCount; propertyIndex++)
        {
            if (shader.GetPropertyType(propertyIndex) is UnityEngine.Rendering.ShaderPropertyType.Texture)
            {
                var propertyName = shader.GetPropertyName(propertyIndex);
                var texture = material.GetTexture(propertyName);
                if (texture is T typeTexture && typeTexture != null)
                {
                    dictionary.Add(propertyName, typeTexture);
                }
            }
        }
        return dictionary;
    }
}

// original from TexTransTool (MIT License) https://github.com/ReinaS-64892/TexTransTool/blob/a1f3f1e6e77a066b5fd47f2b692e069cf18b8ff0/Editor/Utils/EditorTextureUtility.cs
internal static class EditorTextureUtility
{
    static bool TryGetUnCompress(Texture2D firstTexture, out Texture2D unCompress)
    {
        if (!AssetDatabase.Contains(firstTexture)) { unCompress = firstTexture; return false; }
        var path = AssetDatabase.GetAssetPath(firstTexture);
        if (Path.GetExtension(path) == ".png" || Path.GetExtension(path) == ".jpeg" || Path.GetExtension(path) == ".jpg")
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null || importer.textureType != TextureImporterType.Default) { unCompress = firstTexture; return false; }
            unCompress = new Texture2D(2, 2);
            unCompress.LoadImage(File.ReadAllBytes(path));
            return true;
        }
        else { unCompress = firstTexture; return false; }
    }
    public static (bool isLoadableOrigin, bool IsNormalMap) GetOriginInformation(Texture2D tex)
    {
        if (AssetDatabase.Contains(tex) is false) { return (false, false); }
        var path = AssetDatabase.GetAssetPath(tex);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        var isNormalMap = (importer?.textureType ?? TextureImporterType.Default) is TextureImporterType.NormalMap;
        if (Path.GetExtension(path) is not (".png" or ".jpeg" or ".jpg")) { return (false, isNormalMap); }
        return (true, isNormalMap);
    }

    public static Texture2D TryGetUnCompress(this Texture2D tex)
    { return TryGetUnCompress(tex, out Texture2D outUnCompress) ? outUnCompress : tex; }

    // https://github.com/ReinaS-64892/TexTransTool/blob/a1f3f1e6e77a066b5fd47f2b692e069cf18b8ff0/TTCE-Unity/TTCEUnity.cs#L264-L272
    public static void DownloadFromRenderTexture<T>(this RenderTexture rt, Span<T> dataSpan) where T : unmanaged
    {
        // wow unsafe !!!
        // if (EnginUtil.GetPixelParByte(format, channel) * rt.width * rt.height != dataSpan.Length) { throw new ArgumentException(); }

        var request = AsyncGPUReadback.Request(rt, 0);
        request.WaitForCompletion();
        request.GetData<T>().AsSpan().CopyTo(dataSpan);
    }
    // https://github.com/ReinaS-64892/TexTransTool/blob/a1f3f1e6e77a066b5fd47f2b692e069cf18b8ff0/Editor/Domain/TextureManager.cs#L192-L200
    public static void ForceMarkStreamingMipMap(Texture2D tex)
    {
        var sTexture = new SerializedObject(tex);

        var sStreamingMipmaps = sTexture.FindProperty("m_StreamingMipmaps");
        sStreamingMipmaps.boolValue = true;

        sTexture.ApplyModifiedPropertiesWithoutUndo();
    }
}
