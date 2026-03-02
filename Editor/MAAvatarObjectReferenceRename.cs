#nullable enable
using System.Linq;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;

namespace net.rs64.PAngelsStealersUtility
{
    public static class MAAvatarObjectReferenceRename
    {
        [MenuItem("Tools/PAngelsStealersUtility/MAAvatarObjectReferenceRename/CCP to Lime")]
        public static void CCP2L()
        {
            var activeGO = Selection.activeGameObject;
            if (activeGO == null) { Debug.Log("active selected game object not found !!!"); return; }

            var maComponents = activeGO.GetComponentsInChildren<nadena.dev.modular_avatar.core.AvatarTagComponent>(true);
            foreach (var maComponent in maComponents)
            {
                switch (maComponent)
                {
                    default: break;
                    case nadena.dev.modular_avatar.core.ModularAvatarShapeChanger hr:
                        {
                            RenameRefs(maComponent, hr.GetObjectReferences());
                            break;
                        }
                    case nadena.dev.modular_avatar.core.ModularAvatarObjectToggle hr:
                        {
                            RenameRefs(maComponent, hr.GetObjectReferences());
                            break;
                        }
                    case nadena.dev.modular_avatar.core.ModularAvatarMeshCutter hr:
                        {
                            RenameRefs(maComponent, hr.GetObjectReferences());
                            break;
                        }
                    case nadena.dev.modular_avatar.core.ModularAvatarBlendshapeSync hr:
                        {
                            RenameRefs(maComponent, hr.GetObjectReferences());
                            break;
                        }
                    case nadena.dev.modular_avatar.core.ModularAvatarMergeArmature hr:
                        {
                            RenameRefs(maComponent, hr.GetObjectReferences());
                            break;
                        }
                    case nadena.dev.modular_avatar.core.ModularAvatarMeshSettings hr:
                        {
                            RenameRefs(maComponent, hr.GetObjectReferences());
                            break;
                        }
                    case nadena.dev.modular_avatar.core.ModularAvatarReplaceObject hr:
                        {
                            RenameRefs(maComponent, hr.GetObjectReferences());
                            break;
                        }
                    case nadena.dev.modular_avatar.core.ModularAvatarMaterialSetter hr:
                        {
                            RenameRefs(maComponent, hr.GetObjectReferences());
                            break;
                        }
                    case nadena.dev.modular_avatar.core.ModularAvatarMaterialSwap hr:
                        {
                            RenameRefs(maComponent, hr.GetObjectReferences());
                            break;
                        }
                    case nadena.dev.modular_avatar.core.vertex_filters.VertexFilterByBoneComponent hr:
                        {
                            RenameRefs(maComponent, hr.GetObjectReferences());
                            break;
                        }
                }
            }

            static void RenameRefs(nadena.dev.modular_avatar.core.AvatarTagComponent maComponent, System.Collections.Generic.IEnumerable<nadena.dev.modular_avatar.core.AvatarObjectReference> refs)
            {
                foreach (var aoRef in refs)
                {
                    if (aoRef.referencePath is "Body_base")
                    {
                        Undo.RecordObject(maComponent, "rename path");
                        aoRef.referencePath = "Body_Base";
                    }
                }
            }
        }
    }

}
