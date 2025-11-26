#nullable enable
using System.Linq;
using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;


[assembly: ExportsPlugin(typeof(net.rs64.PAngelsStealersUtility.RsPASUPlugin))]

namespace net.rs64.PAngelsStealersUtility
{
    internal class RsPASUPlugin : Plugin<RsPASUPlugin>
    {
        public override string QualifiedName => "net.rs64.reina-s-p-angels-stealers-utility";
        public override string DisplayName => "ReinaSakiria's P-Angel's Stealers Utility";
        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming)
                .BeforePlugin("nadena.dev.modular-avatar")

                .Run(MirroringTransformPass.Instance).Then
                .Run(AnkletBraceletStealerScalerPass.Instance).Then
                .Run(CopyToSerializedComponentValuePass.Instance)

            ;
            InPhase(BuildPhase.Transforming)
                .AfterPlugin("net.rs64.tex-trans-tool")
                .WithRequiredExtension(typeof(AnimatorServicesContext), seq =>
                {
                    seq.Run(StealingStopAFKPass.Instance)
                    .Then.Run(PASUComponentPagePass.Instance);
                });
        }
    }

    public sealed class PASUComponentPagePass : Pass<PASUComponentPagePass>
    {
        protected override void Execute(BuildContext context)
        {
            foreach (var i in context.AvatarRootObject.GetComponentsInChildren<PASUComponent>(true))
            {
                UnityEngine.Object.DestroyImmediate(i);
            }
        }
    }
}
