# Version 0.2b

<b>Windows behavior:</b><br />
- added window iteration states (such as Initializing, Initialized, Showing, Hiding etc.)<br />
- added 3D->UI component (useful for tooltips window type)<br />
<br />
<b>Core:</b><br />
- all settings from window system moved to settings file<br />
- improved layouts drawer behavior (fixes)<br />
<br />
<b>Examples:</b><br />
- added alert message<br />
- added modules (BackgroundCloseable, BackgroundLock)<br />
- added 3D->UI HUD<br />
- split button components (Default button, Button with tooltip)<br />

# Unity3d UI Windows Extension

<b>Features:</b><br />
- MVC<br />
You can store your layout out of your logic and model.
<br /><br />
- Modules<br />
Like clickable backgrounds, other features like animated decorators and so on.
<br /><br />
- Layouts<br />
Each window links to a some layout which can store any of containers and animations.
<br /><br />
- Animations<br />
Any component, layout container or module can be animated.
<br /><br />
- Events<br />
OnInit - fire once on first window initialize<br />
OnDeinit - fire once on destroy<br />
OnShowBegin(System.Action = null)<br />
OnShowEnd<br />
OnHideBegin(System.Action = null)<br />
OnHideEnd<br />
OnParametersPass(T param, T2 param, ...) - pass parameters of any type to init your window
<br /><br />
- Extensible<br />
You can extend any kind of component you want.
