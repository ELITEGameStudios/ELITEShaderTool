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
        bool currentlyCompiling, refreshing, environmentExists, shaderDataErrorBlockade;
        bool ReadyForSetup => ToolConfig.Instance.WorkspacePath != "null";
        public static Asset<Texture2D> mainRect;
        ELITEShaderTool ModScript => Mod as ELITEShaderTool;

        public struct LiveShaderData
        {
            public Ref<Effect> tangibleEffect;
            public string effectName;
            public string passName;
            public string FXFilePath;
            public string FXFileString, JSONFileString;
            public byte[] compiledFXCBytes;
            public bool useable;
            public string shaderDirectory;
            public string textureDirectory;
            public string pathOfJSONFile; //=> $@"{shaderDirectory}\{effectName}.json";
            public readonly string pathOfFXCFile => $@"{shaderDirectory}\{effectName}.fxc";
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

            public bool CheckJSONFileLiveChanges(bool updateString = true)
            {
                if (!File.Exists(pathOfJSONFile)) {Main.NewText("a .fx file at path " + FXFilePath + " has gone missing!"); return false;}
            
                string newString = File.ReadAllText(pathOfJSONFile);

                bool hasChanged = newString != JSONFileString;
                if(hasChanged && updateString) JSONFileString = newString;

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
                            shaderData.JSONFileString = File.ReadAllText(shaderData.pathOfJSONFile);
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
            ModScript.SetSystem(this);
        }


        public override void OnModLoad()
        {
            if(!ReadyForSetup){return;}

            SetupEffectEnvironment(ToolConfig.Instance.CurrentEffectName, ToolConfig.Instance.CurrentEffectPass);
        }

        public void SetupEffectEnvironment(string name, string passName = "Main", bool reinit = false)
        {
            // if()
            string workspace = ToolConfig.Instance.WorkspacePath;
            string effectEnvironmentPath = workspace+"\\"+name;
            if (!Directory.Exists(workspace)){
                Directory.CreateDirectory(workspace);
            }

            DirectoryInfo directory = Directory.CreateDirectory(effectEnvironmentPath);
            string textureDirPath =effectEnvironmentPath+"\\"+ToolConfig.Instance.TextureSubdirectorySuffix;
            if (!Directory.Exists(textureDirPath))
            {
                // Creates texture subdirectory
                DirectoryInfo textureDir = directory.CreateSubdirectory(ToolConfig.Instance.TextureSubdirectorySuffix);
                
                // Adds template texture
                byte[] templateBytes = File.ReadAllBytes(ModScript.GetModDirectory()+@"\Assets\Textures\TemplateTexture.png");
                using (FileStream newFile = File.Create(textureDir+@"\TemplateTexture.png"))
                {
                    newFile.Write(templateBytes);
                }; 
            }

            string fxFilePath = $@"{effectEnvironmentPath}\{name}.fx";
            string jsonFilePath = $@"{effectEnvironmentPath}\{name}.json";

            if(!File.Exists(fxFilePath)) {
                // Adds template .fx
                byte[] templateBytes = File.ReadAllBytes(ModScript.GetModDirectory()+@"\Assets\TemplateFiles\TemplateEffect.fx");

                using (FileStream newFile = File.Create(fxFilePath) )
                {
                    newFile.Write(templateBytes);
                }; 
            }

            if(!File.Exists(jsonFilePath)) {
                // Adds template .json
                byte[] templateBytes = File.ReadAllBytes(ModScript.GetModDirectory()+@"\Assets\TemplateFiles\TemplateJson.json");
                using (FileStream newFile = File.Create(jsonFilePath))
                {
                    newFile.Write(templateBytes);
                }; 
            }
            
            LiveShaderData data = new();
            
            data.useable = false;
            data.effectName = name;
            data.passName = passName;
            data.shaderDirectory = directory.FullName;
            data.textureDirectory = textureDirPath;
            
            data.FXFilePath = fxFilePath;
            data.FXFileString = "";
            data.JSONFileString = "";
            data.pathOfJSONFile = jsonFilePath;
            activeDisplayedShader = data;
            activeDisplayedShader.CheckFXFileLiveChanges(reinit); // should update file strings and refresh automatically
            // RefreshEffects();

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
                    LiveShaderData.SetupEffectByFXC(activeDisplayedShader);                    
                }
                else
                {
                    Main.NewText("Change in .fx file detected, but couldnt compile new FXC");
                }
            }
            else if(activeDisplayedShader.CheckJSONFileLiveChanges())
            {
                LiveShaderData.SetupEffectByFXC(activeDisplayedShader);
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
						Main.NewText($"FXC Compiler Output: {e.Data}");
				};

				compiler.ErrorDataReceived += (_, e) =>
				{
					if (e.Data != null)
						Main.NewText($"FXC Compiler Error: {e.Data}");
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
				Mod.Logger.Info("good luck buddy.");
                
                return false;
			}
        }

        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if(!environmentExists || shaderDataErrorBlockade) return;

            if(activeDisplayedShader.useable && ToolConfig.Instance.showQuad)
            {
                Vector2 screenMiddle = new Vector2(Main.screenWidth, Main.screenHeight)/2;
                Vector2 rectOffset = new Vector2(ToolConfig.Instance.PreviewEffectSizeX, ToolConfig.Instance.PreviewEffectSizeY)/2;
                Vector2 mainOffset = new Vector2(ToolConfig.Instance.PreviewOffsetX, ToolConfig.Instance.PreviewOffsetY);
                Vector2 final = screenMiddle + mainOffset;

                spriteBatch.End();
                GameShaders.Misc[activeDisplayedShader.shaderDataKey].Apply();
                spriteBatch.Begin(SpriteSortMode.Immediate, ToolConfig.Instance.GetBlending(), SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, activeDisplayedShader.tangibleEffect.Value, Main.GameViewMatrix.TransformationMatrix);
                
                // Draw plane here
                spriteBatch.Draw(
                    mainRect.Value, 
                    new Rectangle((int)final.X, (int)final.Y, ToolConfig.Instance.PreviewEffectSizeX, ToolConfig.Instance.PreviewEffectSizeY),
                    null,
                    // new Rectangle((int)Main.player[0].Center.X - (int)Main.screenPosition.X, (int)Main.player[0].Center.Y - (int)Main.screenPosition.Y, ToolConfig.Instance.PreviewEffectSizeX, ToolConfig.Instance.PreviewEffectSizeY),
                    Color.White,
                    0,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0);// null, Color.White, 0, Vector2.Zero, SpriteEffect(), 0);
                
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                
            }
        
        }
    
        public void BlockadeRendering()
        {
            shaderDataErrorBlockade = true;
        }

        public void LiftShaderDataBlockade()
        {
            shaderDataErrorBlockade = false;
        }
    }
}