using SlimDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Zbx1425.DXDynamicTexture {

    public static partial class TextureManager {

        internal static partial class PostInstantiateTexturePatcher {

            class BveModelInfoClassWrapper {

                private static bool haveFieldInfosSet = false;

                public object Src { get; }

                public BveModelInfoClassWrapper(object src) {
                    Src = src;

                    if (!haveFieldInfosSet) {
                        Type type = src.GetType();
                        FieldInfo[] fields = type.GetFields(DefaultBindingFlags);

                        MeshField = MeshField ?? fields.First(f => f.FieldType == typeof(Mesh));
                        MaterialInfosField = MaterialInfosField ?? fields.First(f => {
                            if (!f.FieldType.IsArray) return false;

                            var materialInfoTypeFields = f.FieldType.GetElementType().GetFields(DefaultBindingFlags);
                            bool hasMaterialTypeField = materialInfoTypeFields.Any(f2 => f2.FieldType == typeof(Material));
                            bool hasTextureTypeField = materialInfoTypeFields.Any(f2 => f2.FieldType == typeof(Texture));

                            return hasMaterialTypeField && hasTextureTypeField;
                        });

                        haveFieldInfosSet = true;
                    }
                }

                private static FieldInfo MeshField;
                public Mesh Mesh {
                    get => MeshField.GetValue(Src) as Mesh;
                }

                private static FieldInfo MaterialInfosField;
                public BveMaterialInfoClassWrapper[] MaterialInfos {
                    get {
                        object[] objs = MaterialInfosField.GetValue(Src) as object[];
                        BveMaterialInfoClassWrapper[] result = new BveMaterialInfoClassWrapper[objs.Length];
                        for (int i = 0; i < objs.Length; i++) {
                            result[i] = new BveMaterialInfoClassWrapper(objs[i]);
                        }
                        return result;
                    }
                }
            }

            class BveMaterialInfoClassWrapper {

                private static bool haveFieldInfosSet = false;

                public object Src { get; }

                public BveMaterialInfoClassWrapper(object src) {
                    Src = src;

                    if (!haveFieldInfosSet) {
                        Type type = src.GetType();
                        FieldInfo[] fields = type.GetFields(DefaultBindingFlags);
                        MaterialField = fields.FirstOrDefault(f => f.FieldType == typeof(Material));
                        TextureField = fields.FirstOrDefault(f => f.FieldType == typeof(Texture));

                        haveFieldInfosSet = true;
                    }
                }

                private static FieldInfo MaterialField;
                public Material Material {
                    get => (Material)MaterialField.GetValue(Src);
                    set => MaterialField.SetValue(Src, value);
                }

                private static FieldInfo TextureField;
                public Texture Texture {
                    get => TextureField.GetValue(Src) as Texture;
                    set => TextureField.SetValue(Src, value);
                }
            }
        }
    }
}
