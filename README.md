# GUI for [OpenGL 4 for C#](https://github.com/giawa/opengl4csharp)
This code originated with my second generation graphics engine (Orchard Sun) and was then incorporated into my voxel engine (Summer Dawn) which powers [Live Love Farm](http://giawa.com/llf/).  I thought it would be neat to release all of this code as open source, since it might be interesting and helpful.  This code also supports the new System.Numerics code from the OpenGL library, so you can compile with USE_NUMERICS if you would like!

## License
Check the included [LICENSE.md](https://github.com/giawa/gui4opengl4csharp/blob/master/LICENSE.md) file for the license associated with this code.  The code is currently licensed under the MIT license.

## Building the Project
This project includes a .sln and .csproj file which will create a class library.  This class library uses the OpenGL.UI namespace, which extends the OpenGL namespace from [OpenGL 4 for C#](https://github.com/giawa/opengl4csharp).

## Built-In Controls
OpenGL.UI.Controls contains several built-in controls.
* Button
* Check Box
* Console (a TextBox with input)
* Dialog Box (a container for controls)
* List Box
* Slider
* Text
* Text Box
* Text Input

There are several more controls on the way.  They just have to be converted from Live Love Farm over to this new OpenGL.UI project.  Currently I have only tested the Text and Button Controls in this new project.  However, I expect the Check Box, Dialog Box, List Box, Slider and Text Box should (for the most part) function correctly.  I do not expect the Console or Text Input to work at all, since they rely on Keyboard input, which OpenGL.UI does not currently support.

## Examples
I will include several example projects in the Examples directory.
### Example 1
![Example 1](https://giawa.github.com/ui/example1.png)

In this first example we create a window using FreeGlut.  Nearly all of the code in this example is either to initialize FreeGlut, or to hook up the callbacks from FreeGlut correctly.  The interesting portion of the code is here:

```csharp
// create some centered text
OpenGL.UI.Controls.Text welcome = new OpenGL.UI.Controls.Text(OpenGL.UI.Controls.Text.FontSize._24pt, 
	"Welcome to OpenGL", OpenGL.UI.Controls.BMFont.Justification.Center);
welcome.RelativeTo = OpenGL.UI.Corner.Center;

// create some colored text
OpenGL.UI.Controls.Text coloredText = new OpenGL.UI.Controls.Text(OpenGL.UI.Controls.Text.FontSize._24pt, 
	"using C#", OpenGL.UI.Controls.BMFont.Justification.Center);
coloredText.Position = new OpenGL.UI.Point(0, -30);
coloredText.Color = new Vector3(0.2f, 0.3f, 1f);
coloredText.RelativeTo = OpenGL.UI.Corner.Center;

// add the two text object to the UI
OpenGL.UI.UserInterface.AddElement(welcome);
OpenGL.UI.UserInterface.AddElement(coloredText);
```

We add two Text objects to the screen.  Notice that they are marked as being relative to the center of the screen.  This means their positions will update properly when the screen is resized.  Try it out!

### Example 2
![Example 2](https://giawa.github.com/ui/example2.gif)

In this second example we create several Button elements and attach OnMouseEnter, OnMouseLeave, OnMouseClick, OnMouseDown and OnMouseUp events.  Each Button has a Texture that is loaded from an included data directory.  Here's the interesting bit of code, which makes heavy use of lambda functions.

```csharp
// create buttons in a row, each of which uses a Texture (the Texture gives the initial size of the Button in pixels)
OpenGL.UI.Controls.Button button = new OpenGL.UI.Controls.Button(textures[i]);
button.Position = new OpenGL.UI.Point(xoffset, 5);
button.RelativeTo = OpenGL.UI.Corner.Center;

// change the color of the button when entering/leaving/clicking with the mouse
button.OnMouseEnter = (sender, e) => button.BackgroundColor = new Vector4(0, 1f, 0.2f, 1.0f);
button.OnMouseLeave = (sender, e) => button.BackgroundColor = Vector4.Zero;
button.OnMouseDown = (sender, e) => button.BackgroundColor = new Vector4(0, 0.6f, 1f, 1f);
button.OnMouseUp = (sender, e) => button.BackgroundColor = (OpenGL.UI.UserInterface.Selection == button ? new Vector4(0, 1f, 0.2f, 1.0f) : Vector4.Zero);

// update the text with the character name when the button is clicked
button.OnMouseClick = (sender, e) => characterName.String = string.Format("You selected {0}!", character);
```

Note:  Icons made by (Freepik)[http://www.freepik.com] from (http://www.flaticon.com)[www.flaticon.com] are licensed by CC 3.0 BY.