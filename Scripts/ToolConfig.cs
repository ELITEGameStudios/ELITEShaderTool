using System.ComponentModel;
using System.Runtime.Serialization;
using Terraria.ModLoader.Config;

namespace ELITEShaderTool.Scripts
{
    
    [BackgroundColor(49, 32, 36, 216)]
    public class ToolConfig : ModConfig
    {
        public static ToolConfig Instance;

        public override ConfigScope Mode => ConfigScope.ClientSide;
    
        [BackgroundColor(100, 0, 100, 216)]
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
        [DefaultValue(300)]
        public int TicksUntilRefresh {get; set;}

        [BackgroundColor(100, 0, 100, 216)]
        [DefaultValue(500)]
        public int PreviewEffectSizeX {get; set;}
        
        [BackgroundColor(100, 0, 100, 216)]
        [DefaultValue(500)]
        public int PreviewEffectSizeY {get; set;}

        [BackgroundColor(100, 0, 100, 216)]
        [DefaultValue(false)]
        public bool showQuad;


    }
}