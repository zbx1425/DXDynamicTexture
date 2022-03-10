using SlimDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Zbx1425.DXDynamicTexture {

    public static partial class TextureManager {

        internal static partial class PostInstantiateTexturePatcher {

            private const BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.InvokeMethod;

            public static void Initialize() {
                Application.OpenForms[0].Invalidated += OnGameBegun;
            }

            private static void OnGameBegun(object sender, EventArgs e) {
                Form mainForm = Application.OpenForms[0];
                mainForm.Invalidated -= OnGameBegun;

                if (Handles.Values.All(h => h.IsCreated)) return;

                var bveAssembly = Assembly.GetEntryAssembly();
                if (bveAssembly is null) throw new NotSupportedException("Cannot find the BVE Trainsim assembly.");

                ProgressForm progressForm = new ProgressForm();
                progressForm.SetWork(() => {
                    var allChildForms = ForEachMembers(mainForm, new List<object>(0), 0)
                        .FindAll(obj => obj is Form)
                        .ConvertAll(obj => (Form)obj);

                    int patchedTextureCount = 0;
                    ForEachMembers(allChildForms.Find(f => f.Name == "SimOperationForm"), new List<object>(0), 4,
                        obj => {
                            var modelTypeFields = obj.GetType().GetFields(DefaultBindingFlags);
                            bool hasMeshTypeField = modelTypeFields.Any(f => f.FieldType == typeof(Mesh));
                            bool hasMaterialInfoTypeField = modelTypeFields.Any(f => {
                                if (!f.FieldType.IsArray) return false;

                                var materialInfoTypeFields = f.FieldType.GetElementType().GetFields(DefaultBindingFlags);
                                bool hasMaterialTypeField = materialInfoTypeFields.Any(f2 => f2.FieldType == typeof(Material));
                                bool hasTextureTypeField = materialInfoTypeFields.Any(f2 => f2.FieldType == typeof(Texture));

                                return hasMaterialTypeField && hasTextureTypeField;
                            });

                            return hasMeshTypeField && hasMaterialInfoTypeField;
                        }, obj => {
                            var model = new BveModelInfoClassWrapper(obj);

                            var texturePaths = model.Mesh.GetMaterials()
                                .Select(m => m.TextureFileName)
                                .ToArray();

                            for (int i = 0; i < model.MaterialInfos.Length; i++) {
                                Texture texture = TryPatch(texturePaths[i]);
                                if (texture != null) {
                                    var materialInfo = model.MaterialInfos[i];
                                    materialInfo.Texture = texture;
                                    DXDevice.Material = materialInfo.Material;
                                    DXDevice.SetTexture(0, texture);
                                    model.Mesh.DrawSubset(i);

                                    patchedTextureCount++;
                                    progressForm.ReportProgress(null, null, patchedTextureCount);
                                }
                            }
                        });

                    progressForm.ReportProgress(100, "Completed.");
                });
                progressForm.Show(mainForm);

                
                List<object> ForEachMembers(
                    object parent, List<object> recognizedObjs, int maxNestCount,
                    Func<object, bool> targetObjSelector = null, Action<object> action = null, int defaultCapacity = 0) {

                    if (maxNestCount < 0) return new List<object>(0);

                    if (targetObjSelector == null) targetObjSelector = _ => false;
                    if (action == null) action = _ => { };

                    var parentType = parent.GetType();
                    var fields = parentType.GetFields(DefaultBindingFlags);

                    var unrecognizedObjs = fields
                        .Where(f => IsUniqueType(f.FieldType)) // Search only for fields of BVE's unique types; target textures are only instantiated with those fields
                        .Select(f => f.GetValue(parent))
                        .Flatten()
                        .Except(recognizedObjs) // Exclude objects already recognized
                        .Where(obj => obj != null && obj != parent);

                    var targetObjs = unrecognizedObjs.Where(targetObjSelector);
                    if (targetObjs.Any()) {
                        foreach (var obj in targetObjs) action(obj);
                    }

                    var objs = new List<object>(defaultCapacity <= 0 ? recognizedObjs.Count + unrecognizedObjs.Count() : defaultCapacity);
                    objs.AddRange(recognizedObjs);
                    objs.AddRange(unrecognizedObjs);

                    foreach (var obj in unrecognizedObjs) {
                        var childObjs = ForEachMembers(obj, objs, maxNestCount - 1, targetObjSelector, action, defaultCapacity + objs.Count);
                        objs.AddRange(childObjs.Except(objs));

                        if (childObjs.Any()) {
                            int progress = objs.Count / 200;
                            if (progress > 99) progress = 99;
                            progressForm.ReportProgress(progress, objs.Count, null);
                        }
                    }

                    return objs;


                    bool IsUniqueType(Type type) {
                        if (type.IsEnum) return false;

                        return type.Assembly == bveAssembly
                            || (type.IsGenericType && type.GetGenericArguments().Any(IsUniqueType));
                    }
                }
            }
        }
    }
}
