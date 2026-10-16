using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using static ELITEShaderTool.TestSystem;

namespace ELITEShaderTool.Scripts
{
    public class JSONBasedShaderData : MiscShaderData
    {
        public JSONShaderParameters jsonFile;
        public LiveShaderData inputShaderData;
        public SamplerState samplerState;
        public Dictionary<string, StoredTexture> storedTextures;


        public JSONBasedShaderData(LiveShaderData liveShaderData) : base(liveShaderData.tangibleEffect, liveShaderData.passName)
        {
            jsonFile = DeserializeJSONFile(liveShaderData.pathOfJSONFile);
            inputShaderData = liveShaderData;
            samplerState = SamplerState.LinearClamp;
            Apply();
        }

        public JSONShaderParameters DeserializeJSONFile(string path)
        {
            if(!File.Exists(path)){return null;}

            string jsonString = File.ReadAllText(path);
            return JsonSerializer.Deserialize<JSONShaderParameters>(jsonString);
        }
        

        public override void Apply(DrawData? drawData = null)
        {
            
            if(jsonFile.FloatParams != null)
            {
                // Floats
                foreach (string key in jsonFile.FloatParams.Keys)
                {
                    base.Shader.Parameters[key]?.SetValue(jsonFile.FloatParams[key]);
                }
                
            }

            if(jsonFile.IntParams != null)
            {
                // Ints
                foreach (string key in jsonFile.IntParams.Keys)
                {
                    base.Shader.Parameters[key]?.SetValue(jsonFile.IntParams[key]);
                }

            }
            if(jsonFile.BoolParams != null)
            {
                // bools
                foreach (string key in jsonFile.BoolParams.Keys)
                {
                    base.Shader.Parameters[key]?.SetValue(jsonFile.BoolParams[key]);
                }
            }

            if(jsonFile.VectorParams != null)
            {
                // vectors
                foreach (string key in jsonFile.VectorParams.Keys)
                {
                    SetVectorParameterFromSerialized(jsonFile.VectorParams[key], key);
                }
            }


            if(jsonFile.TextureParams != null)
            {
                // Textures
                bool storedTexturesExists = storedTextures != null;
                if (!storedTexturesExists){ storedTextures = new(); }

                foreach (string key in jsonFile.TextureParams.Keys)
                {
                    if(!storedTexturesExists || storedTextures[key] == null) {
                        TextureSerialize serialize = jsonFile.TextureParams[key]; 
                        SetTextureFromPath(serialize.FileName, key, serialize.Buffer);
                    }
                    else
                    {
                        SetTextureFromStored(key);
                    }
                }
            }

            base.Apply(drawData);
            // vector lists
            // foreach (string key in jsonFile.vectorListParams.Keys)
            // {
            //     SetVectorParameterFromSerialized(jsonFile.vectorParams[key], key);
            // }
        }

        public void SetTextureFromPath(string texturePath, string parameter, int buffer){
            using(var stream = System.IO.File.OpenRead($@"{inputShaderData.textureDirectory}\{texturePath}")){
                Texture2D texture = Texture2D.FromStream(Main.graphics.GraphicsDevice, stream);
                if(texture != null)
                {
                    Main.graphics.GraphicsDevice.Textures[buffer] = texture;
                    Main.graphics.GraphicsDevice.SamplerStates[buffer] = samplerState;

                    base.Shader.Parameters[parameter+"Size"]?.SetValue(new Vector2(texture.Width, texture.Height));
                    storedTextures[parameter] = new StoredTexture(texture, buffer);
                }
            }
        }
        public void SetTextureFromStored(string parameter){
                
                StoredTexture storedTexture = storedTextures[parameter];
                Texture2D texture = storedTexture.Texture;
                int buffer = storedTexture.Buffer;
                
                if(texture != null)
                {
                    Main.graphics.GraphicsDevice.Textures[buffer] = texture;
                    Main.graphics.GraphicsDevice.SamplerStates[buffer] = samplerState;

                    base.Shader.Parameters[parameter+"Size"]?.SetValue(new Vector2(texture.Width, texture.Height));
                }
        }

        public void SetVectorParameterFromSerialized(VectorSerialize vectorSerialize, string key)
        {
                float r = vectorSerialize.R;
                float g = vectorSerialize.G;
                float b = vectorSerialize.B;
                float a = vectorSerialize.A;
                int length = vectorSerialize.Length;

                switch (length)
                {   
                    case 2:
                        base.Shader.Parameters[key]?.SetValue(new Vector2(r, g));
                        break;
                    case 3:
                        base.Shader.Parameters[key]?.SetValue(new Vector3(r, g, b));
                        break;
                    default: // treats as vector 4
                        base.Shader.Parameters[key]?.SetValue(new Vector4(r, g, b, a));
                        break;
                }
        }
    }

    public class JSONShaderParameters
    {
        public Dictionary<string, TextureSerialize>? TextureParams {get; set;} // Parameter name key, Path value
        public Dictionary<string, float>? FloatParams {get; set;}
        public Dictionary<string, int>? IntParams {get; set;}
        public Dictionary<string, VectorSerialize>? VectorParams {get; set;}
        // public Dictionary<string, VectorSerialize[]> vectorListParams {get; set;}
        public Dictionary<string, bool>? BoolParams {get; set;}
    }

    public class TextureSerialize
    {   
        public string FileName {get; set;}
        public int Buffer {get; set;}
    }

    public class StoredTexture
    {
        public Texture2D Texture {get; set;}
        public int Buffer {get; set;}
        public StoredTexture(Texture2D texture, int buffer)
        {
            Texture = texture;
            Buffer = buffer;
        }
    }

    public class VectorSerialize
    {
        public int Length {get; set;}
        public float R {get; set;}
        public float G {get; set;}
        public float B {get; set;}
        public float A {get; set;}
    }
}