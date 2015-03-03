<img src="https://raw.githubusercontent.com/chromealex/Unity3d.UI.Windows/master/README/1.png" width="370" align="left" />
<img src="https://raw.githubusercontent.com/chromealex/Unity3d.UI.Windows/master/README/2.png" width="170" />
<img src="https://raw.githubusercontent.com/chromealex/Unity3d.UI.Windows/master/README/3.png" width="170"/>
<img src="https://raw.githubusercontent.com/chromealex/Unity3d.UI.Windows/master/README/4.png" width="170" />
<img src="https://raw.githubusercontent.com/chromealex/Unity3d.UI.Windows/master/README/5.png" width="170" />

# Online Samples:
https://c11cf9e2a0f3c662eb191e3f47f2d05514d5cbc0.googledrive.com/host/0B9amRnYzMgMneGxpRXZNdDNNUDA/Basic.html
https://ec5dbe2d80c5f4f1a03800ad67c1340fe62155d9.googledrive.com/host/0B9amRnYzMgMnTkRkVGs3S290bjg/Transitions.html

# Version 0.4a

<b>Components:</b>
<ul>
<li>Added ButtonWithText component</li>
<li>Added callback with type support to Button Component</li>
</ul>

<b>Transitions:</b>
<ul>
<li>Splitted layout transitions and fullscreen transitions</li>
<li>Added 7 basic fullscreen transitions</li>
</ul>

<b>Core:</b>
<ul>
<li>Added fullscreen transitions support</li>
<li>Windows sorting by Z axis now</li>
<li>Some minor fixes</li>
</ul>

<b>Examples:</b>
<ul>
<li>Added Basic and Transitions scenes</li>
</ul>

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
