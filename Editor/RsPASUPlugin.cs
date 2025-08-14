#nullable enable
using System.Linq;
using nadena.dev.ndmf;


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
                .AfterPlugin("nadena.dev.modular-avatar")
                .Run(MirroringTransformPass.Instance).Then
                .Run(AnkletBraceletStealerScalerPass.Instance).Then
                .Run(CopyToSerializedComponentValuePass.Instance)
            ;
        }
    }
}
