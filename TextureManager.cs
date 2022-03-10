using SlimDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Zbx1425.DXDynamicTexture {

    public static partial class TextureManager {

        internal static Dictionary<string, TextureHandle> Handles = new Dictionary<string, TextureHandle>();

        internal static string DllDir;

        internal static Device DXDevice;

        public static void Initialize(bool modifyInstancedTextures = true) {
            PreInstantiateTexturePatcher.Initialize();
            if (modifyInstancedTextures) PostInstantiateTexturePatcher.Initialize();

            TouchManager.Initialize();
        }

        public static TextureHandle Register(string fileNameEnding, int width, int height) {
            if (fileNameEnding.Trim().Length == 0) throw new ArgumentException("Must not be empty.", "fileNameEnding");
            if (!IsPowerOfTwo(width)) throw new ArgumentException("Must be a integral power of 2.", "width");
            if (!IsPowerOfTwo(height)) throw new ArgumentException("Must be a integral power of 2.", "height");

            fileNameEnding = fileNameEnding.ToLowerInvariant().Replace('\\', '/');
            if (Handles.ContainsKey(fileNameEnding)) return Handles[fileNameEnding];
            var result = new TextureHandle(width, height);
            Handles.Add(fileNameEnding, result);
            return result;
        }

        internal static bool IsPowerOfTwo(int x) {
            return (x & (x - 1)) == 0;
        }

        private static Texture TryPatch(string fileName) {
            var normalizedName = fileName.Replace('\\', '/').ToLowerInvariant();
            foreach (var item in Handles) {
                if (normalizedName.EndsWith(item.Key)) {
                    if (Handles[item.Key].IsCreated) {
                        return null;
                    } else {
                        return Handles[item.Key].GetOrCreate(DXDevice);
                    }
                }
            }

            return null;
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
