# ELITEShaderTool
A Shader tool to assist Live testing and development of shaders in Terraria mods.

So far, This tool allows for development on miscellaneous Pixel Shaders that meant to be drawn on rectangles through the spriteBatch.Draw method.

When used correctly, this program will update your chosen previewed shader on the screen starting from your player's center heading bottom left like this.
<img width="1549" height="941" alt="image" src="https://github.com/user-attachments/assets/a3d843be-6907-4683-98b3-73ccbd2fbda4" />

To install the mod, download this project, unzip in the ModSources folder of TmodLoader (Can be found in tmodloaders menu through Workshop>Develop Mods>Open Sources) and click Build+Reload

When Loading this mod for the first time, it will create the directory "C:\ELITEShaderTool\Workspace\DefaultEffect". that folder will further contain DefaultEffect.fx, DefaultEffect.json, and a Textures folder with TemplateTexture.png.
The files generated should have an example template pixel shader and json file to help you get started! Dont worry about the lack of a .fxc file, that will be compiled automatically in game.

These Default workspace settings and effect names can be changed in the Mod's config file. 
<img width="781" height="512" alt="image" src="https://github.com/user-attachments/assets/78aa4edf-1cec-4033-9b9c-f5cd77bf0997" />

DO NOT rename your Generated .fx script on its own, as the working environment associates the "Current Effect Name" field in the config to match up with the file names. Instead, set your desired name in the config, and save. This will generate the appropriate directory with your chosen name.

Your shader will display on the screen so long as "show quad" is enabled.


-- ASSIGNING PARAMETERS -- 

You must mark all your variables and texture names in a JSON file that the mod reads. Using the JSON properly is demonstrated here. 

The program reads from this example structure shown here:
<img width="592" height="632" alt="image" src="https://github.com/user-attachments/assets/5b5aa0e0-71b2-4012-bbe3-a90d2ec2c2c0" />

TextureParams takes in textures. FileName is the name of the file in the Textures folder that is included in the generated directory, and Buffer represents the register your texture is assigned to in the shader. 
[BUG: register 0 doesnt work right now. this may or may not be due to my code or something builtin, just skip using register 0 for now.]

FloatParams - float variables

IntParams - integer variables

VecterParams - float2, float3, float4 variables. Length is required to tell the program how many elements this vector has. If not set, the program defaults to Vector4

R - Element 1
G - Element 2
B - Element 3
A - Element 4

Using XYZW notation for this is not an option at the moment.

BoolParams - boolean parameters

If those names and structures are setup correctly, your parameters should be mapped to your shader when it renders :)

--- RENDERING THE SHADER ---

Your game will update the displayed shader every time you save the shader, with the exception of missing textures or a compiler error. If this happens, the error will be sent in the game chat.

If you would like to do multiple shader projects with this tool, changing the "Current Effect Name" variable in config wont delete or modify your existing names, it will make a new folder in your workspace directory and set it up as its own project. If you change it back to an existing name, it will just use the existing directory. 
This way, if you want to flip between different shaders, you can do so easily :)  

--- KNOWN CRASHES, BUGS, CONCERNS ---

- If you notice the photino app in the code, that is meant to be a future external UI window so that perhaps I can make the parameters and some config variables easy to change in custom UI. Far fetched goal for now. Why is it an external window instead of terraria UI? The idea seemed cool to me..
- There is Likely some edge case I am not aware of that may crash the game. Sorry about that if this happens, this is still early in development.


--- FILE ACCESS ACKNOWLEDGEMENT ---
Lastly, I must acknowledge this mod's ability to create and access data on your device beyond the mod's directory. I am not sure if this is allowed, or a grey area for publishing mods in Tmodloader. 
This is intended to be (or become) a handy tool for development and not a content mod of sorts, and its access is still limited only to what you decide in the config. This is intended to give you control of where you want to easily access this workspace, and at no point does this mod actually modify existing files. It only creates the directories needed and compiles the .fx as a new .fxc file.



