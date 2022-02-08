[English](README.md) | 日本語

# DXDynamicTexture for BVE Trainsim 5/6

ATS プラグインからゲーム内のテクスチャを動的に変更できるようにするライブラリです。
マップ内の 3D モデルのテクスチャだけでなく、運転台パネルにも適用できます。動きのある演出を容易に実装できるようになります。

## 前提条件

- プラグインは C++ ではなく C# + DllExport で作成する必要があります。
  C# での ATS プラグインの作成には、Rock_On 氏の [BveAtsPluginCsharpTemplate](https://github.com/mikangogo/BveAtsPluginCsharpTemplate) が便利です。

- ライブラリの使用にはC#の基本的な知識が必要です。

## プロジェクトのセットアップ

BVE 5 向けのプラグインを .NET Framework 3.5 + x86 (32bit)、BVE 6 向けのプラグインを .NET Framwork 4.5 or 4.8 + x64 (64bit) でビルドする必要があります。

この設定はプロジェクトのプロパティから変更できますが、ビルドの度に変更して 2 バージョン出力するのが面倒な場合は自動化することもできます。
`.csproj` ファイルをテキスト エディタで開き、以下のコードを削除し、

```xml
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
    ...
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ">
    ...
  </PropertyGroup>
```

代わりに以下のコードを追加します (.NET Framework 4.8 でビルドする場合はコード下部の "v4.5" を "v4.8" に変更してください)。

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

コードの変更が完了したら、プロジェクトをリロードし、まだセットアップしていない場合は DllExport をセットアップすればコンパイルの準備は完了です。

`bin/x64/(Debug or Release)/x64/(アセンブリ名).dll` に出力されたファイルが BVE 5 向け、`bin/x86/(Debug or Release)/x86/(アセンブリ名).dll` に出力されたファイルが BVE 6 向けになります。

## DXDynamicTexture のインポート

Releases から DLL をダウンロードし、プロジェクトの参照に `Zbx1425.DXDynamicTexture-net35.dll` を追加します。

メインのクラスに以下のコードを追加します。クラス名を変更している場合は、コード内の AtsMain は新しいクラス名に書きかえます。

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
                "(Zbx1425.DXDynamicTexture-net35.dll を配置したフォルダへの相対パス)"
            ));
            var fileName = (Environment.Version.Major >= 4) ? 
                "Zbx1425.DXDynamicTexture-net48.dll" : "Zbx1425.DXDynamicTexture-net35.dll";
            return Assembly.LoadFile(Path.Combine(libPath, fileName));
        }
        return null;
    }
```

コード内の "(Zbx1425.DXDynamicTexture-net35.dll を配置したフォルダへの相対パス)" を実際の相対パスに変更します。
例えば、ATS プラグインを `Scenarios/someone/sometrain/ats32/plugin.dll`、DXDynamicTexture (`Zbx1425.DXDynamicTexture-net35.dll`) を `Scenarios/zbx1425/Zbx1425.DXDynamicTexture-net35.dll` に配置する場合は、 `../../../zbx1425` を指定します。

次に、ATS プラグインの Load() メソッドに TextureManager を初期化するコードを追加します。

```csharp
// using Zbx1425.DXDynamicTexture;

[DllExport(CallingConvention.StdCall)]
public static void Load() {
    TextureManager.Initialize();
}
```

ここまで完了したら、ATS プラグインをコンパイルして BVE から読み込み、エラーが発生しないことを確認してください。

何か問題があれば、お気軽に E メールか Twitter からお問い合わせください。

## 動的テクスチャ

コード例 (重要な部分のみであり、完全なコードではありません) :

```csharp
// using System;
// using System.Drawing;
// using System.IO;
// using System.Reflection;
// using Zbx1425.DXDynamicTexture;

private static TextureHandle hTex;
private static GDIHelper gClock;
private static Bitmap imgClock;
private static string dllParentPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

[DllExport(CallingConvention.StdCall)]
public static void Load() {
    TextureManager.Initialize();
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

この例では、"clock_back_tex.png" という名前の画像ファイルから作成されたテクスチャへ、DLL と同じディレクトリに存在する "clock_back.png" と現在の日時を表す文字列を描画しています。失敗した場合は運転台パネルで使用可能な値 ats100 を 1 に設定しています。

ここからこのコードの解説をしていきます。

- TextureManager.Register(texturePathSuffix*, width*, *height*);

  シナリオの読込中、`texturePathSuffix` で指定したファイル名の画像からテクスチャが生成されるかどうか監視します。
  戻り値は TextureHandle 型で、テクスチャが生成された場合は TextureHandle.IsCreated プロパティ (コード例の場合は `hTex.IsCreated`) が true になります。 
  
  このメソッドを Load メソッド内で呼び出し、Started メソッド内か Elapse メソッド内で TextureHandle.IsCreated プロパティを確認してください。
  
  また、このメソッドは空白のテクスチャで元のテクスチャを置き換えるため注意してください。
  
  BVE のマップを車両より先に読み込む仕様の関係で、運転台パネル内のテクスチャは直接置き換えられる一方、マップ内のテクスチャを置き換えるには一度マップを読み込んだ後再度読み込み直す必要があります。
  そのため、ゲーム開始時に IsCreated が true になっているか確認し、false であれば、コード例のように利用者に対しシナリオを読み込み直す必要がある旨を表示することを推奨します。
  
  - texturePathSuffix: 置き換えたいテクスチャのパスの一部。例えば `～～～/Scenarios/shuttle/hrd/structures/back_a.png` を置き換えたい場合は `shuttle/hrd/structures/back_a.png` を指定すると良いでしょう。
  - width, height: 置換後のテクスチャのサイズ。いずれも 2 のべき乗である必要があります。置換元のテクスチャのサイズと合わせる必要はありません。
  
- new GDIHelper(*width*, *height*);
  テクスチャの描きかえには System.Drawing.Bitmap を使用できますが、Bitmap を直接描きかえる処理の実装は複雑で難易度が高いため、補助を行うための GDIHelper を提供しています。

  GDIHelper のインスタンスはテクスチャ毎に生成してください。

  - GDIHelper.Graphics

    テクスチャに紐づけられた System.Drawing.Graphics を取得します。Graphics を利用すると円や文字列、アルファチャンネル（透過）付画像などを描画することができます。
  
    **Graphics.DrawImage メソッドは遅いため推奨しません。GDIHelper.DrawImage メソッドを使用してください。**
  
  - GDIHelper.DrawImage(*bitmapToDraw*, *x*, *y*)
    GDIHelper.DrawImage(*bitmapToDraw*, *x*, *y*, *sourceOffsetY*, *height*)
  
    GDI+ による Graphics.DrawImage メソッドは遅いため、GDI32 を利用して Bitmap を描画する代替メソッドを提供しています。
    ただし、このメソッドの実行前に BeginGDI() メソッド、実行後に EndGDI() メソッドを実行する必要があることに注意してください。
  
  - GDIHelper.FillRect12(*color*, *x1*, *y1*, *x2*, *y2*)
    GDIHelper.FillRectWH(*color*, *x1*, *y1*, *width*, *height*)

    単一色で塗り潰された長方形を描画します。
    このメソッドの実行前に BeginGDI() メソッド、実行後に EndGDI() メソッドを実行する必要があることに注意してください。
  
  BeginGDI() メソッドを実行してから EndGDI メソッドを実行するまでの間に GDIHelper.Graphics を使用しないでください。
  また、BeginGDI() メソッドを実行してから EndGDI メソッドを実行するまでの間以外で GDIHelper.DrawImage メソッド、GDIHelper.FillRect から始まる名前のメソッドを使用しないでください。
  
- TextureHandle.HasEnoughTimePassed(*fps*)

  TextureHandle.Update メソッドによるテクスチャの更新回数を、1 秒当たり `fps` で指定した回数以下に制限するためのメソッドです。更新頻度が指定された値以下であれば true を返します。

  テクスチャの更新はコストが大きいため、できるだけ頻度を下げることを推奨しています。テクスチャの更新は直接 TextureHandle.Update メソッドを呼び出せばいつでも可能ですが、このメソッドを使用することで簡単にテクスチャの更新頻度を制限することができます。

- TextureHandle.Update(*gdiHelper or bitmap*)

  ビデオカードに GDIHelper や Bitmap をプッシュし、TextureHandle に紐づけられたゲーム内のテクスチャを更新します。
  テクスチャの更新はコストが大きいため、更新頻度の最適化のために TextureHandle.HasEnoughTimePassed メソッドを使用することを推奨します。

- TextureManager.Dispose()

  プラグインのアンロードの際は、全リソースの解放を忘れないようにしてください。

## タッチ

コード例 (重要な部分のみであり、完全なコードではありません) :

```csharp
// using System;
// using System.Windows.Forms;
// using Zbx1425.DXDynamicTexture;

private static TouchTextureHandle hTIMSTex;

[DllExport(CallingConvention.StdCall)]
public static void Load(){
    TextureManager.Initialize();
    hTIMSTex = TouchManager.Register("foo/bar/tims_placeholder.png", 512, 512);
    hTIMSTex.SetClickableArea(0, 0, 400, 300);
    TouchManager.EnableEvent(MouseButtons.Left, TouchManager.EventType.Down);
    hTIMSTex.MouseDown += HTIMSTex_MouseDown;
}

private static void HTIMSTex_MouseDown(object sender, TouchEventArgs e) {
    MessageBox.Show(String.Format("X: {0}, Y: {1}", e.X, e.Y));
}
```

動的テクスチャのコードとほぼ同じ処理になります。相違点としては TouchManager.Register メソッドを使用することと、同メソッドの戻り値が TouchTextureHandle 型であることくらいです。
TouchTextureHandle は TextureHandle を継承しているため、動的テクスチャのための機能は TouchTextureHandle にも含まれます。

また、同一のファイルを TouchManager と TextureManager の両方に登録しないでください。TouchManager.Register メソッドには TextureManager.Register メソッドと同一の機能が含まれます。

また、タッチされた位置を色で検出するため、CabIlluminance による運転台の明るさの設定には対応しないことをご了承ください。DaytimeImage と NighttimeImage で同一のファイルを指定してください。

- TouchTextureHandle.SetClickableArea(*x0*, *y0*, *width*, *height*)

  テクスチャ内の一部の領域のみをタッチ可能に設定します。このメソッドが呼び出されなかった場合はテクスチャ内の全領域がタッチ可能に設定されます。

- TouchManager.EnableEvent(*button*, *type*)

  イベントを発生させる条件を指定します。
  例えば、`TouchManager.EnableEvent(MouseButtons.Left, TouchManager.EventType.Down);` は左マウス ボタンが押されたときにのみイベントを発生させるように設定します。

  タッチされた位置を一瞬グラデーションを表示して検出するという本ライブラリの仕様上、全てのマウス動作に対してイベントを発生させると高頻度な点滅が発生することとなり、見栄えが悪くなってしまいます。
  これを防ぐために、このメソッドでイベントを発生させる条件を指定することができるようにしています。

  MouseButtons を使用するには、参照に `System.Windows.Forms` を追加してください。

- TouchTextureHandle.MouseDown += *handler*, TouchTextureHandle.MouseUp += *handler*

  画面にタッチされたときに発生するイベントを購読します。

  `e.X`、`e.Y` は、タッチ可能な領域の左上端を原点としたマウスの位置をピクセル単位で表します。