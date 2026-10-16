float uTime; // uTime is built-in to the shader tool and does not need to be included in the .json

sampler mainImage : register(s1);
float2 mainImageSize; // the Size suffix is built into the shader tool for texture parameters. These do not need to be included in the .json

float4 mainColor;
float opacity;
bool showUV;
bool showTexture;


float4 main(float4 inputColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    if(showUV) // try setting this to true in the .json!
    { 
        return float4(uv.x, uv.y, 0, 1);
    }


    if(showTexture) // true by default in the .json!
    {
	    float4 textureColor = tex2D(mainImage, uv);
        return textureColor;
    }
    else
    {
        return mainColor;
    }

}

technique Technique1
{
    pass Main
    {
        PixelShader = compile ps_2_0 main();
    }
}

