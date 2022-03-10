English | [日本語](README_JP.md)

# DXDynamicTexture for BVE Trainsim 5/6

Allows the ATS plugin to update in-game texture dynamically. Can apply to panel images as well as scenario textures on the models. Helps you create a more lively environment.

## Prerequisites

You must write your Plugin with C# and DllExport instead of C++. See Mr.Rock_On's example at https://github.com/mikangogo/BveAtsPluginCsharpTemplate.

DllExport can be installed via Nuget. You should see a dialog when installing, where you should select your project. If you accidentally skipped it when installing, you can use `DllExport.bat -action Configure`.

- [Quick start · 3F/DllExport Wiki (github.com)](https://github.com/3F/DllExport/wiki/Quick-start)
- [.NET DllExport. Various scenarios (Configuring, Automatic restoring, Pe-Viewer) - YouTube](https://www.youtube.com/watch?v=sBWt-KdQtoc)

Using DXDynamicTexture requires you to have a general knowledge of the C# programming language.

## Set up your project

An additional requirement is to make sure 32-bit plugins are targeting .Net 3.5 and 64-bit plugins targeting .Net 4. Remember, 3.5 for x86 (32-bit) and 4.5/4.8 for x64 (64-bit). You can change that in the project settings.

You should select the different targets every time in the project settings, and that would do the job.

But it can be a bit of a labor if you often compile both version. If you want to automate this, open the `.csproj` file with a text editor, and remove these lines:

```xml
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
    ...
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ">
    ...
  </PropertyGroup>
```

And replace them with: (Replace v4.5 with v4.8 at the bottom if you have build tool for 4.8 instead of 4.5)

```xml
  <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'Debug|x64'">
    <DebugSymbols>true</DebugSymbols>
    <OutputPath>bin\x64\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <DebugType>full</DebugType>
    <PlatformTarget>x64</PlatformTarget>
    <ErrorReport>prompt</ErrorReport>
  </PropertyGroup>
  <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'Release|x64'">
    <OutputPath>bin\x64\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <Optimize>true</Optimize>
    <DebugType>none</DebugType>
    <PlatformTarget>x64</PlatformTarget>
    <ErrorReport>prompt</ErrorReport>
  </PropertyGroup>
  <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'Debug|x86'">
    <DebugSymbols>true</DebugSymbols>
    <OutputPath>bin\x86\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <DebugType>full</DebugType>
    <PlatformTarget>x86</PlatformTarget>
    <ErrorReport>prompt</ErrorReport>
  </PropertyGroup>
  <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'Release|x86'">
    <OutputPath>bin\x86\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <Optimize>true</Optimize>
    <DebugType>none</DebugType>
    <PlatformTarget>x86</PlatformTarget>
    <ErrorReport>prompt</ErrorReport>
  </PropertyGroup>
  <PropertyGroup>
    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>
    <TargetFrameworkVersion Condition="'$(Platform)' == 'x86'">v3.5</TargetFrameworkVersion>
  </PropertyGroup>
```

Reload the project and set up DllExport normally if you haven't, and you can now compile with the correct target. Remember to compile for both x64 and x86 and use the output files at `bin/x64/(Debug or Release)/x64/(Project Name).dll` for BVE5 and `bin/x86/(Debug or Release)/x86/(Project Name).dll` for BVE6.

## Importing DXDynamicTexture

Download the DLLs from the release section and add `Zbx1425.DXDynamicTexture-net35.dll` to your project reference.

**You don't need to and probably shouldn't manually copy `0Harmony.dll` to BVE's installation directory.**

Add these to your main class: (Replace AtsMain with something else if you changed the class name)

```csharp
public static class AtsMain {
    ...
    static AtsMain() {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }
    private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
       if (args.Name.Contains("DXDynamicTexture")) {
            var libPath = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                "(Path To The Folder Containing Zbx1425.DXDynamicTexture-net35.dll)"
            ));
            var fileName = (Environment.Version.Major >= 4) ? 
                "Zbx1425.DXDynamicTexture-net48.dll" : "Zbx1425.DXDynamicTexture-net35.dll";
            return Assembly.LoadFile(Path.Combine(libPath, fileName));
        }
        return null;
    }
```

Replace the "(Path To The Folder Containing Zbx1425.DXDynamicTexture-net48.dll)" with the actual relative path. For example, if your plugin is in `Scenarios/someone/sometrain/ats32/plugin.dll`, and the `Zbx1425.DXDynamicTexture-net35.dll` is at `Scenarios/zbx1425/Zbx1425.DXDynamicTexture-net35.dll`, then you should write `../../../zbx1425`.

Next, in your Load() function,initialize the TextureManager (you need to `using Zbx1425.DXDynamicTexture;`)

```csharp
[DllExport(CallingConvention.StdCall)]
public static void Load() {
    TextureManager.Initialize(true);
}
```

The TextureManager.Initialize method parameter specifies whether to replace textures that are "registered" at the time this method is executed, such as textures in the map.  
The parameter can also be omitted, in which case it is assumed to be true.

Now compile the plugin and start the game, to see if there are any errors.

If you have problems during the setup, feel free to contact me via email or twitter.

## Dynamic Texture

Here's an example. (Not the complete source code, only the important functions)

```csharp
private static TextureHandle hTex;
private static GDIHelper gClock;
private static Bitmap imgClock;
private static string dllParentPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

[DllExport(CallingConvention.StdCall)]
public static void Load() {
    TextureManager.Initialize(true);
    hTex = TextureManager.Register("clock_back_tex.png", 128, 128);
    gClock = new GDIHelper(128, 128);
    imgClock = new Bitmap(Path.Combine(dllParentPath, "clock_back.png"));
}

[DllExport(CallingConvention.StdCall)]
public static void Dispose() {
    TextureManager.Dispose();
    imgClock.Dispose();
    gClock.Dispose();
}

[DllExport(CallingConvention.StdCall)]
public static AtsHandles Elapse(AtsVehicleState state, IntPtr hPanel, IntPtr hSound) {
    var panel = new AtsIoArray(hPanel); var sound = new AtsIoArray(hSound);
    if (hTex.IsCreated) {
        panel[100] = 0;
        if (hTex.HasEnoughTimePassed(5)) {
            gClock.BeginGDI();
            gClock.DrawImage(imgClock, 0, 0);
            gClock.EndGDI();

            gClock.Graphics.DrawString(
                (state.Time / 1000).ToString(), 
                new Font(SystemFonts.DefaultFont.FontFamily, 20, GraphicsUnit.Pixel),
                Brushes.Black, 0, 0
            );

            hTex.Update(gClock);
        }
    } else {
     	panel[100] = 1;
    }
}
```

This replaces a texture called "clock_back_tex.png" in the world, then draws "clock_back.png" in the directory same as the DLL and a text representing the current time on it. If it fails, ats100 in panel will be set to 1.

Now we'll explain how it works.

- TextureManager.Register(*texturePathSuffix*, *width*, *height*);

  Schedules to try locating a texture file while the scenario loads. Returns a TextureHandle. If the texture is successfully located, IsCreated (`hTex.IsCreated` in this case) will be true.
  You should call `Register` in the Load event, then check `IsCreated` in the Tick event.
  It locates and replaces a texture file (discarding its content) with a new blank texture.

  Due to limitations, textures on the train panel can be located directly, but textures in the map requires another way to be located; set the TextureManager.Initialize method parameter to true or omit it. This will enable the option to allow textures that are "registered" at the time TextureManager.Initialize is executed (e.g., textures in the map) to be replaced.  
  If TextureManager fails locating the texture, the IsCreated property will be false. You should check if your texture has `IsCreated` in `Tick`, and if not, show a tip message on the panel to remind the player to close and reopen the scenario.
  
  - texturePathSuffix: A part of the path of the texture in scenraio you want to replace. For example, for `....../Scenario/shuttle/hrd/structures/back_a.png` you should use `shuttle/hrd/structures/back_a.png`.
  - width, height: The size of the new texture. Width and Height needs to be a power of 2. Don't need to be the same as the original texture.
  
- new GDIHelper(*width*, *height*);
  You can use a System.Drawing.Bitmap to replace the texture directly. But since drawing things directly on Bitmap is a bit difficult, I made this GDIHelper class to help. You can create a GDIHelper for each texture, with the same size as that texture. This acts as a buffer image on which you perform all these drawing.

  - GDIHelper.Graphics

    You can use this to get a System.Drawing.Graphics, and use GDI+ to do all these fancy drawing stuff, such like drawing circles, texts and transparent images.

  - GDIHelper.DrawImage(*bitmapToDraw*, *x*, *y*)
    GDIHelper.DrawImage(*bitmapToDraw*, *x*, *y*, *sourceOffsetY*, *height*)

    Because GDI+'s Graphics.DrawImage is quite slow, we have a GDI32 version here that runs faster.
    But make sure you call `BeginGDI()` before using this function and `EndGDI()` after using.

  - GDIHelper.FillRect12(*color*, *x1*, *y1*, *x2*, *y2*)
    GDIHelper.FillRectWH(*color*, *x1*, *y1*, *width*, *height*)

    Fills a rectangle section of the buffer image with a color.

  Don't use `GDIHelper.Graphics` between `BeginGDI()` and `EndGDI()`, and don't use `GDIHelper.DrawImage` and `GDIHelper.FillRectxx` outside `BeginGDI()` and `EndGDI()`.

- TextureHandle.HasEnoughTimePassed(*fps*)

  Limits the update interval to that amount of times per second. Because replacing texture is a costy process, it is recommended to update as less frequent as possible.

  This just checks "Has sufficient time passed since the last update?", you can call Update directly to update anyway.

- TextureHandle.Update(*gdiHelper or bitmap*)

  Pushes the content in a GDIHelper or Bitmap to your video card, updating its texture in game.
  It is recommended to check for `HasEnoughTimePassed` before doing these drawing and calling Update.

- TextureManager.Dispose()

  Don't forget to release all resources when the plugin unloads, including all the images that you loaded and all the `GDIHelper`s.

## Touching

Here's an example. (Not the complete source code, only the important functions)

```csharp
private static TouchTextureHandle hTIMSTex;

[DllExport(CallingConvention.StdCall)]
public static void Load(){
    TextureManager.Initialize(true);
    hTIMSTex = TouchManager.Register("foo/bar/tims_placeholder.png", 512, 512);
    hTIMSTex.SetClickableArea(0, 0, 400, 300);
    TouchManager.EnableEvent(MouseButtons.Left, TouchManager.EventType.Down);
    hTIMSTex.MouseDown += HTIMSTex_MouseDown;
}

private static void HTIMSTex_MouseDown(object sender, TouchEventArgs e) {
    MessageBox.Show(String.Format("X: {0}, Y: {1}", e.X, e.Y));
}
```

The registration is more or less the same. The difference is that you use `TouchManager.Register` and it returns a `TouchTextureHandle`. TouchTextureHandle is also a TextureHandle so you can do these dynamic texture things on it too.

You cannot register a file both at TouchManager and TextureManager. (TouchManager.Register also does the things TextureManager.Register does, so why)

Because it depends on the color to detect the position, please make sure this texture does not change its brightness due to CabIlluminance (Set the same DaytimeImage and NighttimeImage in panel file).

- TouchTextureHandle.SetClickableArea(*x0*, *y0*, *width*, *height*)

  Maybe only a part of your texture is clickable (For example, your screen just takes up a part of your texture), so you can specify your clickable area here. If not specified, the entire texture will be clickable.

  When handling click events, the clickable area will blink (because it depends on color to detect the position). So setting a smaller area (if you don't require a very large one) means a smaller area will blink, and might look better.

- TouchManager.EnableEvent(*button*, *type*)

  Because handling all these events can result in some intense blinking, you can choose what kinds of events you want to handle here. For example, `TouchManager.EnableEvent(MouseButtons.Left, TouchManager.EventType.Down);` Only handles when the left mouse button is pressed.

  Add a reference to `System.Windows.Forms`   and `using System.Windows.Forms;` if you can't use `MouseButtons`.

- TouchTextureHandle.MouseDown += *handler*, TouchTextureHandle.MouseUp += *handler*

  Register a event handler function to be called when it's clicked.

  `e.X` and `e.Y` is the position of the mouse, in pixels (relative to the size when registering `TouchManager`), relative to the top-left point of the texture's clickable area.