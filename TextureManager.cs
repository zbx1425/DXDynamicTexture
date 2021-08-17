using HarmonyLib;
using SlimDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Zbx1425.DXDynamicTexture {

    public static class TextureManager {

        private static Harmony Harmony = new Harmony("cn.zbx1425.dxdynamictexture");
        private static Dictionary<string, TextureHandle> Handles = new Dictionary<string, TextureHandle>();

        private static string DllDir;

        public static void Initialize() {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Harmony.Patch(typeof(Texture).GetMethods()
                .Where(m => m.Name == "FromFile" && m.GetParameters().Length == 11)
                .FirstOrDefault(),
                null, new HarmonyMethod(typeof(TextureManager), "FromFilePostfix")
            );
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

        private static void FromFilePostfix(ref Texture __result, Device device, string fileName) {
            var normalizedName = fileName.Replace('\\', '/').ToLowerInvariant();
            foreach (var item in Handles) {
                if (normalizedName.EndsWith(item.Key)) {
                    __result = Handles[item.Key].GetOrCreate(device);
                }
            }
        }

        public static TextureHandle Register(string fileNameEnding, int width, int height, double fps = 0) {
            if (fileNameEnding.Trim().Length == 0) throw new ArgumentException("Must not be empty.", "fileNameEnding");
            if (!isPowerOfTwo(width)) throw new ArgumentException("Must be a integral power of 2.", "width");
            if (!isPowerOfTwo(height)) throw new ArgumentException("Must be a integral power of 2.", "height");

            fileNameEnding = fileNameEnding.ToLowerInvariant().Replace('\\', '/');
            if (Handles.ContainsKey(fileNameEnding)) return Handles[fileNameEnding];
            var result = new TextureHandle(width, height, fps);
            Handles.Add(fileNameEnding, result);
            return result;
        }

        private static bool isPowerOfTwo(int x) {
            return (x & (x - 1)) == 0;
        }

        public static void Dispose() {
            foreach (var item in Handles) {
                if (item.Value != null && item.Value.IsCreated) item.Value.Dispose();
            }
        }

        public static void Clear() {
            Dispose();
            Handles.Clear();
        }
    }
}
