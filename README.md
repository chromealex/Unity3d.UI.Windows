Quick start: https://docs.google.com/document/d/1U1We9b9x6UR5U6ZKktigeX8wxMVvp0fxML-t2B3nIRY

<img src="https://github.com/chromealex/Unity3d.UI.Windows/blob/master/UIWImage.png"/>

### Current Version: 1.0.6a

## 1.0.6a
- Audio Manager play queue fixes
- Components and Windows Hide/Show behaviour full refactoring
- Many other fixes

## 1.0.5a
- Draggable windows
- FullTextFormat added to TextComponent
- Window statuses output fix
- FindOpened method added to WindowSystem
- GD & Loc versions added
- GameData service name fix
- WARNING! Old window Preferences->Depth removed. New layers are come!
- Window Layers implemented
- Analytics alpha implementation (Unity Analytics works only)
- A/B Testing module added. Now you can add tests to your flow and filter users by parameters.
- Audio improvement
- Global scroll sens added
- Netclient fixes
- UI mesh renderer clean up
- Modifiedmesh fix
- Logger added
- WindowSystemInput GetPointerScroll
- "Need compile" flag implemented
- LocalizationSystem declinations fixes
- ListViewComponent GetItem fix
- WindowSystem forceSingleInstance fix
- Linker window type added
- Bug fixes/minor improvements

## 1.0.4a
- SFX field improvements
- Audio KeepCurrent/RestartIfEquals fix
- New Popup implemented

## 1.0.3a
- Audio editor improvemets
- Audio transitions implemented

## 1.0.2a
- Audio (Music&FX) support added
	- Need to implement fade in/out
- Flow Window "Screen->Components Editor" feature implemented
- Visual improvements & Minor bug fix 

## 1.0.1a
- Audio module added to FlowSettings & WindowBase

## 1.0.0a
- DevicePreview comes to beta
- Component Parameters are available now below link fields
- Button*Component depricated. ButtonComponent is complex now.
- issue fix #21
- Builds directory removed

## 0.9.9a
- ObjectPool RecycleAll fix
- CircleLayoutGroup fix
- GetRoot() null fix
- PartcileSystemCached updated
- Examples updated
- minor fixes
- hotfix compile error
- Flow Example Initializer added
- Project was updated to Unity 5.2
- Fixed issue #18
- Transition null fix
- bitmask attribute fixed
- minor editor improvements
- WindowSystem::GetCurrentWindow() fixed
- webgl support
- Heatmap build fix
- Animation tags fixed
- TextComponent types added
- ME.Macros implemented (https://github.com/chromealex/ME.Macros)
- Flow run without reflection now

## 0.9.8a
- Flow Editor improvements
- Scale in Layout Editor Preview fixed
- Layout Editor Preview improvements
- many others minor improvements

## 0.9.7a
- CircleLayoutGroup pivot
- CircleLayoutGroup null fixes
- minor changes
- unused code removed
- Compiler: Methods with types will be generated now (Re-compile UI required)
- Core Editor: "Create on Scene" Context function added to the each Screen Settings (Select screen prefab and select *settings* context menu)
- Core Editor: Scene View drawing changed (Works with stretchX/stretchY)

## 0.9.6a
- ParticleSystemCached Rewind added
- InputField Caret bug fix now is not needed
- TextComponent enum changed
- Gradient are now supports bitmask
- AutoRegisterInRoot boolean working on Layout Initialization
- TipWindowType refactored: working on multiply screen resolutions
- minor fixes
- Generator improvemets
- UI Button Extended supports multiply transitions
- HideAllAndClean fix wrong depth reset
- Button extended with alpha transition
- Flow zoom 0.98
- ProgressAnimation Component null fix
- isDirty check
- test label removed
- particles
- Tweener setValue added
- Flat icons
- Force Canvas update
- tags fix
- Create Project fixed
- Hierarchy View improvements
- Flow: "Create Project" button added on splash screen
- Flow: Create Screen function -> ReplaceComponents function fixed
- Minor improvements and fixes

## 0.9.5a
- Examples fixed
- Transition FlowHide## method variation added
- Button with label interactable flag fixed in editor
- skin improvements
- warning fix
- attach revert fixed
- Flow attaches doubleSided fixed
- WindowLayoutRoot ICanvasElement now
- layout fixes
- Upgrade fix
- Functions Iteration fixed
- WindowSystem start ObjectPool ref fixed
- Styles changed
- UI.Windows updated
- Bug merge fixes
- ComboBox removed
- Flow visual improvements
- Function transitions full support
- Tags popup fixed
- Social plugin having uniqueTag now
- Warnings fixed
- readme.txt removed
- Version updater added
- Transitions added (pre-alpha)
- Some bugs fixed
- Many other improvements

## 0.9.4a
- Transition CameraSlide added
- Transition states instancing fixed
- Flow editor small fixes
- Scene [A] objects spam fixed
- Preallocated windows pool size parameter added
- CricleLayoutGroup added
- SizeToScaleLayoutGroup added
- CanvasUpdater added
- FlowLayoutGroup added (3rdParty)
- BackButton action added
- History preferences per window added
- Many other fixes

## 0.9.3a
- Events are now public
- Show/Hide SetActive according on animation
- Flow Editor create window coords fix
- Hide/Show behaviour refactoring

## 0.9.2a
- Tweener extension refactoring
- Sorting Layers bug fix
- List Component navigation added
- major performance fixes in editor flow
- flow editor refactor
- flow editor zoom added
- android devices transition issues resolved by TRANSITION_ENABLE compile flag
- isContainer and isDefaultLink flags are completely removed
- many minor fixes

## 0.9.1a
- Fillable boolean added
- forced flag added into tabs
- Tags editor added
- Tags adding fixed
- Screen layout editor added
- Flow: Functions module call stack fixed
- Flow: Functions compiler class added
- Flow: Create Screen/Layout buttons added into each window view
- Flow: "Functions" module added (beta)
- Flow: Visual improvements
- Minor fixes

## 0.9.0a
- Flow: Comments class added
- Warnings fixed
- Smart script template added
- Preserve aspect added to ImageComponent
- Icons added
- Heatmap addon (pre-alpha)
- Social settings
- Macros removed
- Components register fix
- Minor fixes
- Modules installation
- Social installer
- Social: FB implementation at the beginning
- Social: VK beta
- Minor fixes
- Clean up
- Social addon started (VK.com login & friends added by token)
- Some examples fixed
- List component will no longer clean up on hide
- Components Show/Hide behaviour changed to recursive
- Added button with text and image
- Minor fixes
- FlowCompiler: Hot fix path error
- Flow: Arrows visual fixes
- Flow: Window states fixed
- Flow: Minor GUI fixes
- Layout: Light skin size fixes

## Older
View older version info on commit changes

### Online Examples:
https://c11cf9e2a0f3c662eb191e3f47f2d05514d5cbc0.googledrive.com/host/0B9amRnYzMgMneGxpRXZNdDNNUDA/Basic.html
https://ec5dbe2d80c5f4f1a03800ad67c1340fe62155d9.googledrive.com/host/0B9amRnYzMgMnTkRkVGs3S290bjg/Transitions.html
https://a93da4c04a4b6b6bbd36378a5ea81e0216581113.googledrive.com/host/0B9amRnYzMgMnfjRHeGh6QlVBMUlFd21DRXp5bGZqOVg2RzJ0ZV9ueldyTlg3dFBkOFJyUkE/Components.html

### Project Site

http://chromealex.github.io/Unity3d.UI.Windows/

### Documentation

http://chromealex.github.io/Unity3d.UI.Windows/docs/api/

### Code Style

(EN): https://github.com/chromealex/Unity3d.UI.Windows/wiki/Code-Style-EN

(RU): https://github.com/chromealex/Unity3d.UI.Windows/wiki/Code-Style-RU
