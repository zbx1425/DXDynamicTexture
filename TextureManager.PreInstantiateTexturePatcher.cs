using HarmonyLib;
using SlimDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Zbx1425.DXDynamicTexture {

    public static partial class TextureManager {

        internal static class PreInstantiateTexturePatcher {

            public static Harmony Harmony;

            static PreInstantiateTexturePatcher() {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            }

            private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
                if (args.Name.Contains("Harmony")) {
                    if (DllDir == null) DllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    if (Environment.Version.Major >= 4) {
                        return Assembly.LoadFile(Path.Combine(DllDir, "Harmony-net48.dll"));
                    } else {
                        return Assembly.LoadFile(Path.Combine(DllDir, "Harmony-net35.dll"));
                    }
                }
                return null;
            }

            public static void Initialize() {
                Harmony = new Harmony("cn.zbx1425.dxdynamictexture");
                Harmony.Patch(typeof(Texture).GetMethods()
                    .Where(m => m.Name == nameof(Texture.FromFile) && m.GetParameters().Length == 11)
                    .FirstOrDefault(),
                    null, new HarmonyMethod(typeof(PreInstantiateTexturePatcher), nameof(FromFilePostfix))
                );
            }

            private static void FromFilePostfix(ref Texture __result, Device device, string fileName) {
                if (DXDevice == null) DXDevice = device;

                var texture = TryPatch(fileName);
                if (texture != null) __result = texture;
            }
        }
    }
}
