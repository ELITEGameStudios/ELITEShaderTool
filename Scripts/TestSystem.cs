using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ELITEShaderTool.Scripts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace ELITEShaderTool
{
    public class TestSystem : ModSystem
    {

        int timer;
        int timeUntilRefresh => ToolConfig.Instance.TicksUntilRefresh > 0 ? ToolConfig.Instance.TicksUntilRefresh  : 120;
        bool currentlyCompiling, refreshing, environmentExists;
        bool ReadyForSetup => ToolConfig.Instance.WorkspacePath != "null";
        public static Asset<Texture2D> mainRect;

        public struct LiveShaderData
        {
            public Ref<Effect> tangibleEffect;
            public string effectName;
            public string passName;
            public string FXFilePath;
            public string FXFileString;
            public byte[] compiledFXCBytes;
            public bool useable;
            public string shaderDirectory;
            public string textureDirectory;
            public readonly string pathOfFXCFile => $@"{shaderDirectory}\{effectName}.fxc";
            public readonly string pathOfJSONFile => $@"{shaderDirectory}\{effectName}.json";
            public readonly string shaderDataKey => $"ELITEShaderTool:{effectName}";

            public void SetFileName(string name)
            {
                effectName = name;
            }

            public bool CheckFXFileLiveChanges(bool updateString = true)
            {
                if (!File.Exists(FXFilePath)) {Main.NewText("a .fx file at path " + FXFilePath + " has gone missing!"); return false;}
            
                string newString = File.ReadAllText(FXFilePath);

                bool hasChanged = newString != FXFileString;
                if(hasChanged && updateString) FXFileString = newString;

                return hasChanged;
            }
            public static LiveShaderData SetupEffectByFXC(LiveShaderData shaderData)
            {
                try
                {       
                    if (!File.Exists(shaderData.pathOfFXCFile)) {Main.NewText("a shader file at path " + shaderData.pathOfFXCFile + " does not exist."); return shaderData;}

                    byte[] readShader = File.ReadAllBytes(shaderData.pathOfFXCFile);
                    Main.QueueMainThreadAction( () => {
                        
                        Effect rawEffect = new Effect(Main.graphics.GraphicsDevice, readShader);
                        Ref<Effect> referencedEffect = new Ref<Effect>(rawEffect);
                        shaderData.tangibleEffect = referencedEffect;
                        
                        bool finishedSetup = SetupShaderData(shaderData);
                        if (finishedSetup)
                        {
                            shaderData.useable = true;
                            activeDisplayedShader = shaderData;
                        }
                        else
                        {
                            Main.NewText("Compiled new FXC, but couldnt setup the new Effect instance in terraria.");
                        }

                    });    
                }
                catch
                {
                    Main.NewText("Something went wrong loading the FXC file. ");
                }
                return shaderData;
            }

            public static bool SetupShaderData(LiveShaderData liveShaderData)
            {
                JSONBasedShaderData outData = new JSONBasedShaderData(liveShaderData);
                // Main.NewText(outData);
                if(outData != null)
                {
                    GameShaders.Misc[liveShaderData.shaderDataKey] = outData;
                    return true;
                }
                return false;
            }
        }

        public static LiveShaderData activeDisplayedShader;

        public override void Load()
        {
            mainRect = ModContent.Request<Texture2D>("ELITEShaderTool/Assets/Textures/OpaqueRect");
        }


        public override void OnModLoad()
        {
            if(!ReadyForSetup){return;}

            SetupEffectEnvironment(ToolConfig.Instance.CurrentEffectName, ToolConfig.Instance.CurrentEffectPass);
        }

        public void SetupEffectEnvironment(string name, string passName = "Main")
        {
            // if()
            string workspace = ToolConfig.Instance.WorkspacePath;
            string effectEnvironmentPath = workspace+"\\"+name;
            if (!Directory.Exists(workspace)){
                Directory.CreateDirectory(workspace);
            }

            DirectoryInfo directory = Directory.CreateDirectory(effectEnvironmentPath);
            DirectoryInfo textureDir = directory.CreateSubdirectory(ToolConfig.Instance.TextureSubdirectorySuffix);

            string fxFilePath = $@"{effectEnvironmentPath}\{name}.fx";
            if(!File.Exists(fxFilePath)) {
                using (File.Create(fxFilePath))
                {
                    // can do something about this in the future, like default file writes for example.
                }; 
            }
            
            LiveShaderData data = new();
            
            data.useable = false;
            data.effectName = name;
            data.passName = passName;
            data.shaderDirectory = directory.FullName;
            data.textureDirectory = textureDir.FullName;
            
            data.FXFilePath = fxFilePath;
            data.FXFileString = "";
            data.CheckFXFileLiveChanges(false);
            activeDisplayedShader = data;

            environmentExists = true;
        }


        public override void PostUpdateEverything()
        {
            if(!environmentExists) return;
            timer++;
            if(timer % timeUntilRefresh == 0)
            {
                RefreshEffects();
            }

        }

        public async void RefreshEffects()
        {
            refreshing = true;
            if (activeDisplayedShader.CheckFXFileLiveChanges())
            {
                if (await CompileFXCFile(activeDisplayedShader.effectName, activeDisplayedShader.shaderDirectory))
                {
                    LiveShaderData data = LiveShaderData.SetupEffectByFXC(activeDisplayedShader);                    
                }
                else
                {
                    Main.NewText("Change in .fx file detected, but couldnt compile new FXC");
                }
            }
            else
            {
                // Main.NewText("Change in .fx file not detected. Resuming as normal.");
            }

            refreshing = false;
        }

        public enum ShaderModelType
        {
            PIXEL_2_0
        }
        public string GetModelArgument(ShaderModelType type)
        {
            switch (type)
            {
                // case ShaderModelType.PIXEL_3_0:
                //     return "ps_3_0";
                //     break;
                default:
                    return "fx_2_0"; // was 3_0, but im target switching to debug something right now
            }
        }

        public async Task<bool> CompileFXCFile(string effectName, string effectDirectory, ShaderModelType modelType = ShaderModelType.PIXEL_2_0)
        {
            Process compiler;
			
            try
			{
				compiler = new Process();

				compiler.StartInfo.FileName = compiler.StartInfo.FileName = (Mod as ELITEShaderTool).GetModDirectory()+@"\FXC\fxc.exe";
				compiler.StartInfo.WorkingDirectory = effectDirectory;//Path.GetDirectoryName(compiler.StartInfo.FileName);
				compiler.StartInfo.UseShellExecute = false;
                // Main.NewText(Directory.Exists(GetModDirectory()+@"\FXC") + " - if fxc directory exists");

                compiler.StartInfo.Arguments = "/Zi /Gec /T "+GetModelArgument(modelType)+" /Fo "+effectName+".fxc "+effectName+".fx";

                compiler.StartInfo.RedirectStandardOutput = true;
				compiler.StartInfo.RedirectStandardError = true;
				compiler.EnableRaisingEvents = true;
				
                compiler.Start();
				compiler.BeginOutputReadLine();
				compiler.BeginErrorReadLine();
				compiler.OutputDataReceived += (_, e) =>
				{
					if (e.Data != null)
						Mod.Logger.Info($"FXC OUT: {e.Data}");
				};

				compiler.ErrorDataReceived += (_, e) =>
				{
					if (e.Data != null)
						Mod.Logger.Error($"FXC ERR: {e.Data}");
				};

				Mod.Logger.Info($"FXC ID: {compiler.Id}");
				Mod.Logger.Info($"Path: {compiler.StartInfo.FileName}");
				Mod.Logger.Info($"Working directory: {compiler.StartInfo.WorkingDirectory}");

				// FXC argument sample:
				//fxc /Zi /Gec /T ps_3_0 /Fo RocketRamEffect.fxc RocketRamEffect.fx
				// /Zi for debugging, /Gec for DX9 compatibility, /T has to match shader model, /Fo to output fxc object file, output file, input file
                
                currentlyCompiling = true;
                await compiler.WaitForExitAsync();

                currentlyCompiling = false;
                return true;
			}
			catch
			{
                currentlyCompiling = false;
                Main.NewText("good luck buddy.");
				Mod.Logger.Info("good luck buddy");
                
                return false;
			}
        }

        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if(!environmentExists) return;

            if(activeDisplayedShader.useable && ToolConfig.Instance.showQuad)
            {
                spriteBatch.End();
                GameShaders.Misc[activeDisplayedShader.shaderDataKey].Apply();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, activeDisplayedShader.tangibleEffect.Value, Main.GameViewMatrix.TransformationMatrix);
                
                // Draw plane here
                spriteBatch.Draw(
                    mainRect.Value, 
                    new Rectangle((int)Main.player[0].Center.X - (int)Main.screenPosition.X, (int)Main.player[0].Center.Y - (int)Main.screenPosition.Y, ToolConfig.Instance.PreviewEffectSizeX, ToolConfig.Instance.PreviewEffectSizeY),
                    Color.White);// null, Color.White, 0, Vector2.Zero, SpriteEffect(), 0);
                
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                
            }
        
        }
    }
}