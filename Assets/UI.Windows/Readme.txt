Unity3D UI Windows Extension
Current Version: 0.3a

View UI.Windows/Examples folder for current examples

Contacts:

chrome.alex@gmail.com - feel free to contact me
You can get latest version from guthub: https://github.com/chromealex/Unity3d.UI.Windows


Versions Changes:

Version 0.3a

Windows Behavior:

Added dontDestroyOnLoad flag in window preferences

Core:

Fix WindowSystem linq query - ignore null windows
ObjectPool now in Extensions namespace
Some minor fixes


Version 0.2a

Windows Behavior:

added window iteration states (such as Initializing, Initialized, Showing, Hiding etc.)
added 3D->UI component (useful for tooltips window type)

Core:

all settings from window system moved to settings file
improved layouts drawer behavior (fixes)

Examples:

added alert message
added modules (BackgroundCloseable, BackgroundLock)
added 3D->UI HUD
split button components (Default button, Button with tooltip)


Version 0.1a

Features:

MVC
You can store your layout out of your logic and model. 

Modules
Like clickable backgrounds, other features like animated decorators and so on. 

Layouts
Each window links to a some layout which can store any of containers and animations. 

Animations
Any component, layout container or module can be animated. 

Events
OnInit - fire once on first window initialize
OnDeinit - fire once on destroy
OnShowBegin(System.Action = null)
OnShowEnd
OnHideBegin(System.Action = null)
OnHideEnd
OnParametersPass(T param, T2 param, ...) - pass parameters of any type to init your window 

Extensible
You can extend any kind of component you want.