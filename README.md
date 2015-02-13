# Unity3d UI Windows Extension

Features:
- MVC
You can store your layout out of your logic and model.

- Modules
Like clickable backgrounds, other features like animated decorators and so on.

- Layouts
Each window links to a some layout which can store any of containers and animations.

- Animations
Any component, layout container or module can be animated.

- Events
OnInit - fire once on first window initialize
OnDeinit - fire once on destroy
OnShowBegin(System.Action = null)
OnShowEnd
OnHideBegin(System.Action = null)
OnHideEnd
OnParametersPass(T param, T2 param, ...) - pass parameters of any type to init your window

- Extensible
You can extend any kind of component you want.
