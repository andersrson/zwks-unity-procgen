# zwks Procedural Generator
This is a simple tool and library for generating meshes and materials from
noise, inspired by Sebastian Lague's stuff on Youtube.  

This implementation is based on Jobs + Burst and is blazing fast at generating
objects.  
A custom EditorWindow using UI Toolkit lets you configure the generator in the editor with immediate preview of the 2D maps, and if you wish you can get a real time preview of the generated mesh in the scene view.

The generated textures, materials, and meshes can be saved to your project as assets.

![Editor Window](/editor-window.png)

![Scene preview](/scene-preview.png)

## Usage
Install the package from Package Manager using the Git URL and wait for compilation. Note thtat the minimum Unity version is 2023.2. It should work on 2023.1 but haven't tested it. Modifying to support earlier versions should be relatively easy.  

Open the tool from the tools menu: Tools -> zwks Procedural generator. The tool will ask you to create a settings asset in your project. Just supply a path and file name.  

### Noise
Start by setting noise parameters as desired - here's a great start:  
![Noise settings](/noise-settings.png)  

### Terrain map
You'll note the terrain map does not show anything - first you'll need to create the mapping between noise map values and colors. Simply change the 'Size' number to the number of levels you want and start defining!  

**NOTE: Color entries have 0 alpha by default! Change to full alpha or you won't see anything in the preview :)**    

![Terrain settings](/terrain-map.png)


### Mesh generation
The height map will need a multiplier to generate any visible difference in height. Set 'Height influence' to around 20 as a start.

### In-scene preview
Enable the checkbox named 'Enable scene preview' and a new object will be created in the open scene. It will have a custom marker MonoBehaviour added for identification, call "ProcGenPreviewObject". This object can be safely deleted whenever you wish - but disable the preview checkbox first, or it will be recreated.  
![Scene preview 2](/scene-preview-2.png)
