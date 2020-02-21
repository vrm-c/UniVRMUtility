using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;
using UniVRM10;
using VrmLib;

namespace UniVRMUtility.PrimMan
{
    [DisallowMultipleComponent]
    public class PrimManUI : MonoBehaviour
    {
        [Serializable]
        public struct UIFields
        {
            [SerializeField]
            public InputField HeightInput;

            [SerializeField]
            public Button HeightUp;

            [SerializeField]
            public Button HeightDown;

            [SerializeField]
            public Button Export;
        };

        public UIFields Fields;

        [SerializeField]
        public float m_height = 160;

        void Find<T>(ref T t, string name) where T : Component
        {
            t = transform.Traverse()
            .Select(x => x.GetComponent<T>())
            .First(x => x != null && x.name == name);
        }

        void Reset()
        {
            Find(ref Fields.HeightInput, nameof(Fields.HeightInput));
            Find(ref Fields.HeightUp, nameof(Fields.HeightUp));
            Find(ref Fields.HeightDown, nameof(Fields.HeightDown));
            Find(ref Fields.Export, nameof(Fields.Export));
        }

        PrimManBuilder m_builder;

        void OnDisable()
        {
            if (m_builder != null)
            {
                m_builder.Dispose();
                m_builder = null;
            }
        }

        void OnEnable()
        {
            var go = new GameObject("PrimMan");
            m_builder = new PrimManBuilder(go.transform);
            m_builder.Updated += () =>
              {
                  Fields.HeightInput.text = m_builder.Height.ToString();
              };
            m_builder.Height = m_height;

            Fields.HeightUp.onClick.AddListener(() =>
            {
                m_builder.Height += 1.0f;
            });
            Fields.HeightDown.onClick.AddListener(() =>
            {
                m_builder.Height -= 1.0f;
            });
            Fields.Export.onClick.AddListener(Export);
        }

        void Update()
        {
            m_builder.Height = float.Parse(Fields.HeightInput.text);
        }

        void Export()
        {
            // var path = FileDialogForWindows.SaveDialog("write file", "export.vrm");
            // if (string.IsNullOrEmpty(path))
            // {
            //     Debug.Log($"cancel save");
            //     return;
            // }

            var path = Path.Combine(Application.dataPath, "../tmp.vrm");

            Debug.Log($"save to {path}");
            var exporter = new UniVRM10.RuntimeVrmConverter();
            var meta = ScriptableObject.CreateInstance<UniVRM10.VRMMetaObject>();
            meta.Name = "";
            meta.Copyrights = "";
            meta.Version = "";
            meta.Authors = new[]{
                "PriMan"
            };
            meta.ContactInformation = "";
            meta.Reference = "";
            meta.OtherPermissionUrl = "";
            meta.OtherLicenseUrl = "";

            var model = exporter.ToModelFrom10(m_builder.Root.gameObject, meta);
            foreach(var kv in exporter.Nodes)
            {
                kv.Value.HumanoidBone = m_builder.GetHumanBone(kv.Key.transform);
            }

            // normalize
            var modifier = new ModelModifier(model);
            modifier.SkinningBake();

            VrmLib.ModelExtensionsForCoordinates.ConvertCoordinate(model, VrmLib.Coordinates.Gltf);
            var bytes = Vrm10.ModelExtensions.ToGlb(model);

            File.WriteAllBytes(path, bytes);
        }
    }
}
