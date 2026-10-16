using Terraria.ModLoader.Config.UI;

namespace ELITEShaderTool.Scripts
{
    public class BiggerTextField : ConfigElement<string>
    {
        // public override void OnInitialize()
        // {
        //     Width.Set(0f, 5f);
        //     Height.Set(100f, 0f);
        // }
        public override void Recalculate()
        {
            base.Recalculate();
        }
    }
}