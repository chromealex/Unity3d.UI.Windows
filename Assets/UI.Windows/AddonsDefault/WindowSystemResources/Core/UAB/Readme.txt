Universal Asset Bundles
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
Version 1.0

Info:
This asset help you to serialize/deserialize any kind of 
GameObjects with any custom or built-in Unity Components.
You can't build packages with binaries only (like putting
Texture2D arrays inside) without prefabs, because Prefabs
it is entry point for this asset.

How to build UAB Package with binaries
To build UAB with binaries inside, you need to check that
Asset Bundle Tag is equals between all objects that you
want to push to one UAB Package.

How to build UAB Package without binaries
To build UAB without binaries inside, you need to check
that you set Asset Bundle Tag on prefab and set no tag to
your binaries (such as Texture for example).

Features:
- Cross-platform (Tested on PC/Mac/Linux, iOS, Android, WebGL, PS4)
- Can Serialize/Deserialize all default Unity components
- Can Serialize/Deserialize any custom component inherited from MonoBehaviour
- Full GameObject serialization
- They are of less size than Unity AssetBundles
- Can store binary data (like Textures, Sprites etc)
- Can store links to any objects in the project (Don't even need to store them in Resources directory)

FAQ
- I need to create bundle without actually having binary data inside, but need link them, how can I do it?
- You must set different AssetBundle tags: to the prefab you need to set your AB Tag, but to the Texture (for example) you need to reset AB
Tag, so UAB will make bundle without binary inside, but links will be applied normally like if you'd store them inside.

- I need to save my prefab with custom scripts
- You can do this from the box, just call Builder.Run(directory) or use Tools menu to build.

- I need to save the whole scene
- You can't build using Scene objects, but you can make prefab from the scene and make it.

- I need to save Texture2D array into bundle
- You can't build binary data without using entry point. To do that create prefab with custom script which links Texture2D Array with your 
textures. When you'll get deserialized prefab, you'll get your texture instances (If you stored it inside bundle) or you'll get references
to them in your project.