# GUI for [OpenGL 4.5 for C#](https://github.com/giawa/opengl4csharp)
This code originated with my second generation graphics engine (Orchard Sun) and was then incorporated into my voxel engine (Summer Dawn) which powers [Live Love Farm](http://giawa.com/llf/).  I thought it would be neat to release all of this code as open source, since it might be interesting and helpful.  This code also supports the new System.Numerics code from the OpenGL library, so you can compile with USE_NUMERICS if you would like!

## License
Check the included [LICENSE.md](https://github.com/giawa/gui4opengl4csharp/blob/master/LICENSE.md) file for the license associated with this code.  The code is currently licensed under the MIT license.

## Building the Project
This project includes a .sln and .csproj file which will create a class library.  This class library uses the OpenGL.UI namespace, which extends the OpenGL namespace from [OpenGL 4.5 for C#](https://github.com/giawa/opengl4csharp).

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
In this first example we create a window using FreeGlut.  Nearly all of the code in this example is either to initialize FreeGlut, or to hook up the callbacks from FreeGlut correctly.  The interesting portion of the code is here:

```csharp
// create some centered text
OpenGL.UI.Controls.Text welcome = new OpenGL.UI.Controls.Text(OpenGL.UI.Controls.Text.FontSize._24pt, "Welcome to OpenGL", OpenGL.UI.Controls.BMFont.Justification.Center);
welcome.RelativeTo = OpenGL.UI.Corner.Center;

// create some colored text
OpenGL.UI.Controls.Text coloredText = new OpenGL.UI.Controls.Text(OpenGL.UI.Controls.Text.FontSize._24pt, "using C#", OpenGL.UI.Controls.BMFont.Justification.Center);
coloredText.Position = new OpenGL.UI.Point(0, -30);
coloredText.Color = new Vector3(0.2f, 0.3f, 1f);
coloredText.RelativeTo = OpenGL.UI.Corner.Center;

// add the two text object to the UI
OpenGL.UI.UserInterface.AddElement(welcome);
OpenGL.UI.UserInterface.AddElement(coloredText);
```

We add two Text objects to the screen.  Notice that they are marked as being relative to the center of the screen.  This means their positions will update properly when the screen is resized.  Try it out!

![Example 1](https://giawa.github.com/ui/example1.png)