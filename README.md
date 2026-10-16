# ELITEShaderTool
A Shader tool to assist Live testing and development of shaders in Terraria mods.

--- THIS IS VERY EARLY IN DEVLOPMENT AND A WORK IN PROGRESS, DONT EXPECT A POLISHED/FINISHED STATE ---

So far, This tool allows for development on miscellaneous Pixel Shaders that meant to be drawn on opaque quads through the spriteBatch.Draw method.
When used correctly, this program will update your chosen previewed shader on the screen starting from your player's center heading bottom left like this.
<img width="1991" height="1029" alt="image" src="https://github.com/user-attachments/assets/3977baca-2a0a-45af-9ed3-51469686a08d" />

To install the mod, download this project, unzip in the ModSources folder of TmodLoader (Can be found in tmodloaders menu through Workshop>Develop Mods>Open Sources) and click Build+Reload
When Loading this mod for the first time, it will create the directory "C:\ELITEShaderTool\Workspace\DefaultEffect". that folder will further contain DefaultEffect.fx, and a Textures folder.

These Default workspace settings and effect names can, and should, be changed in the Mod's config file. 
<img width="781" height="512" alt="image" src="https://github.com/user-attachments/assets/78aa4edf-1cec-4033-9b9c-f5cd77bf0997" />

Do not rename your Generated .fx script on its own, as the working environment associates the "Current Effect Name" field in the config to match up with the file names. Instead, set your desired name in the config, save, and reload the mod.

"show Quad" will actually display the shader on the screen, but there are things you need to do first before this actually works.

First and foremost, make sure your .fx file is good to go and has valid syntax of course.
Then, and this is important (unless you dont have any variables in your hlsl code lol), you must mark all your variables and texture names in a JSON file that the mod reads.
Setting up the JSON is demonstrated below, even if you dont have parameters, creating the json is required or the program will crash

-- ASSIGNING PARAMETERS -- 
To do this, create a .json file in the same folder as your generated .fx file. Make sure it has the same name as your .fx file as well
<img width="341" height="164" alt="image" src="https://github.com/user-attachments/assets/7b6f0629-cc92-4e52-a46a-cb5a1eb913fb" />

This is where you will assign all your parameters to be used in your shader. The program reads from this example structure shown here:
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
using XYZW notation for this is for now, not an option

BoolParams - boolean parameters

If those names and structures are setup correctly, your parameters should be mapped to your shader when it renders :)

--- RENDERING THE SHADER ---
If your .json exists, your .fx file has functional compileable shader code, and show Quad is enabled in the config, your shader should appear within 300 ticks (linked to another config variable) after you enter a world.
From this point on, your game will update the display every time you save the shader!

If you would like to do multiple shader projects with this tool, changing the "Current Effect Name" variable in config wont delete or modify your existing names, it will make a new folder in your workspace directory and set it up as its own project. If you change it back to an existing name, it will just use the existing directory. 
This way, if you want to flip between different shaders, you can do so easily :)  

--- KNOWN CRASHES, BUGS, CONCERNS ---
- The game may crash if your .json references a texture that doesn't exist in the folder. Make sure the file names of textures arent misspelled and that the textures are in their appropriate Textures folder.
- Likely another edge case I am not aware of that may crash the game. Sorry about that if this happens.
- If you have not yet compiled a functional .fxc shader due to a compilation error of some sort, the mod wont tell you that yet, nor will it tell you what the error is. The only place you can find this information is through client.log in tmodloader's logs folder.
- The shader will not update upon a parameter/json change. It will only update upon changing the .fx file. If you only want to change a parameter for now just add or remove one line of whitespace on your .fx file lol
- Likely crash when there is no .json file in the directory.
- Yes, I will get to generating the .json file outright, instead of having you create it.
- May add templates upon the creation of the workspace folders so that its easier to get started
- If you notice the photino app in the code, that is meant to be a future external UI window so that perhaps I can make the parameters and some config variables easy to change in custom UI. Far fetched goal for now. Why is it an external window instead of terraria UI? The idea seemed cool to me..

This repository will be updated to address these points. This repo is just uploaded at a time where I first got this tool to work at its fundamental use.

--- FILE ACCESS ACKNOWLEDGEMENT ---
Lastly, I must acknowledge this mod's ability to create and access data on your device beyond the mod's directory. I am not sure if this is allowed, or a grey area for publishing mods in Tmodloader. 
This is intended to be (or become) a handy tool for development and not a content mod of sorts, and its access is still limited only to what you decide in the config. This is intended to give you control of where you want to easily access this workspace, and at no point does this mod actually modify existing files. It only creates the directories needed and compiles the .fx as a new .fxc file.



