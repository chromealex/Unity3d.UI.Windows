<img src="https://raw.githubusercontent.com/chromealex/Unity3d.UI.Windows/master/README/1.png" width="370" align="left" />
<img src="https://raw.githubusercontent.com/chromealex/Unity3d.UI.Windows/master/README/2.png" width="170" />
<img src="https://raw.githubusercontent.com/chromealex/Unity3d.UI.Windows/master/README/3.png" width="170"/>
<img src="https://raw.githubusercontent.com/chromealex/Unity3d.UI.Windows/master/README/4.png" width="170" />
<img src="https://raw.githubusercontent.com/chromealex/Unity3d.UI.Windows/master/README/5.png" width="170" />

# Version 0.3a

<b>Windows Behavior:</b>
<ul>
<li>Added dontDestroyOnLoad flag in window preferences</li>
</ul>

<b>Core:</b>
<ul>
<li>Fix WindowSystem linq query - ignore null windows</li>
<li>ObjectPool now in Extensions namespace</li>
<li>Some minor fixes</li>
</ul>

# Version 0.2a

<b>Windows Behavior:</b>
<ul>
<li>added window iteration states (such as Initializing, Initialized, Showing, Hiding etc.)</li>
<li>added 3D->UI component (useful for tooltips window type)</li>
</ul>

<b>Core:</b>
<ul>
<li>all settings from window system moved to settings file</li>
<li>improved layouts drawer behavior (fixes)</li>
</ul>

<b>Examples:</b>
<ul>
<li>added alert message</li>
<li>added modules (BackgroundCloseable, BackgroundLock)</li>
<li>added 3D->UI HUD</li>
<li>split button components (Default button, Button with tooltip)</li>
</ul>

# Version 0.1a

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
