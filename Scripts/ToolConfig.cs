using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader.Config;

namespace ELITEShaderTool.Scripts
{
    
    [BackgroundColor(49, 32, 36, 216)]
    public class ToolConfig : ModConfig
    {
        public static ToolConfig Instance;

        public override ConfigScope Mode => ConfigScope.ClientSide;
    
        [BackgroundColor(100, 0, 100, 216)]
        // [Expand(true)]
        // [CustomModConfigItem(typeof(BiggerTextField))]
        [DefaultValue(@"c:\ELITEShaderTool\Workspace")]
        public string WorkspacePath {get; set;}

        [BackgroundColor(100, 0, 100, 216)]
        [DefaultValue("Textures")]
        public string TextureSubdirectorySuffix {get; set;}

        [BackgroundColor(100, 0, 100, 216)]
        [DefaultValue("DefaultEffect")]
        public string CurrentEffectName {get; set;}

        [BackgroundColor(100, 0, 100, 216)]
        [DefaultValue("Main")]
        public string CurrentEffectPass {get; set;}

        [BackgroundColor(100, 0, 100, 216)]
        [DefaultValue(60)]
        public int TicksUntilRefresh {get; set;}

        [BackgroundColor(100, 0, 100, 216)]
        [DefaultValue(500)]
        [Slider]
        [Range(0, 2048)]
        public int PreviewEffectSizeX {get; set;}
        
        [BackgroundColor(100, 0, 100, 216)]
        [DefaultValue(500)]
        [Slider]
        [Range(0, 2048)]
        public int PreviewEffectSizeY {get; set;}

        [Slider]
        [Range(-1920, 1920)]
        public int PreviewOffsetX {get; set;}
        
        [BackgroundColor(100, 0, 100, 216)]
        [Slider]
        [Range(-1920, 1920)]
        public int PreviewOffsetY {get; set;}
        
        // [Header($"Mods.ELITEShaderTool.Configs.ToolConfig.blendOption.Header")]
        [BackgroundColor(100, 0, 100, 216)]
        [DefaultValue(0)]
        [Range(-1, 3)]
        [Label("0: Additive, 1: AlphaBlend, 2: Opaque, 3: NonPremultiplied. -1: Custom")]
        public int blendOption;

        [BackgroundColor(100, 0, 100, 216)]
        [Dropdown]
        [Range(-1920, 1920)]
        public BlendState CustomBlending {get; set;}

        [BackgroundColor(100, 0, 100, 216)]
        [DefaultValue(true)]
        public bool showQuad;


        public override void OnChanged()
        {
            if((Mod as ELITEShaderTool).MainSystem != null)
            {
                (Mod as ELITEShaderTool).MainSystem.SetupEffectEnvironment(CurrentEffectName, CurrentEffectPass);
            }
        }

        public BlendState GetBlending()
        {
            switch (blendOption)
            {
                case 0:
                    return BlendState.Additive;
                case 1:
                    return BlendState.AlphaBlend;
                case 2:
                    return BlendState.Opaque;
                case 3:
                    return BlendState.NonPremultiplied;
                default:
                    return CustomBlending;
            }
        }
    }
}