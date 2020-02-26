using System;
using System.Collections.Generic;
using UnityEngine;
using VrmLib;

namespace UniVRMUtility.PrimMan
{
    [Serializable]
    public struct PrimManSettings
    {
        public float HeightCM;

        public float HeightMeter => HeightCM * 0.01f;

        public static PrimManSettings Default =>
            new PrimManSettings
            {
                HeightCM = 160,
            };

        public float GetLength(HumanoidBones bone)
        {
            var head = (float)HeightMeter / 6;
            var upper = head * 2;
            var lower = HeightMeter / 2;
            var footHeight = head / 2;
            switch (bone)
            {
                case HumanoidBones.head:
                    return head;

                case HumanoidBones.neck:
                    return upper / 7;

                case HumanoidBones.chest:
                case HumanoidBones.spine:
                case HumanoidBones.hips:
                    return upper / 7 * 2;

                case HumanoidBones.leftUpperLeg:
                case HumanoidBones.rightUpperLeg:
                    return lower / 2;

                case HumanoidBones.leftLowerLeg:
                case HumanoidBones.rightLowerLeg:
                    return lower / 2 - footHeight;

                case HumanoidBones.leftFoot:
                case HumanoidBones.rightFoot:
                    return footHeight;

                case HumanoidBones.leftToes:
                case HumanoidBones.rightToes:
                    return footHeight * 1.5f;

                case HumanoidBones.leftShoulder:
                case HumanoidBones.rightShoulder:
                    return head / 2;

                case HumanoidBones.leftUpperArm:
                case HumanoidBones.leftLowerArm:
                case HumanoidBones.rightUpperArm:
                case HumanoidBones.rightLowerArm:
                    return head;

                case HumanoidBones.leftHand:
                case HumanoidBones.rightHand:
                    return head / 2;
            }

            throw new NotImplementedException();
        }
    }

    public class PrimManBuilder : IDisposable
    {
        Transform m_root;

        public Transform Root => m_root;

        public PrimManBuilder(Transform root)
        {
            m_root = root;
            Updated += UpdateSize;
            BuildHierarchy();
            UpdateSize();
        }

        public event Action Updated;
        void RaiseUpdated()
        {
            var handler = Updated;
            if (handler == null) return;
            handler();
        }

        public PrimManSettings Settings = PrimManSettings.Default;

        public float Height
        {
            get
            {
                return Settings.HeightCM;
            }

            set
            {
                if (Settings.HeightCM == value)
                {
                    return;
                }
                Settings.HeightCM = value;
                // on value change
                RaiseUpdated();
            }
        }

        class Bone : IDisposable
        {
            public readonly VrmLib.HumanoidBones HumanBone;
            readonly float m_width;
            readonly float m_depth;
            Vector3? m_offset; // not connected

            public readonly Transform Transform;
            public readonly Transform Shape;

            static UnityEngine.Mesh CopyMesh(UnityEngine.Mesh src, bool copyBlendShape = false)
            {
                var dst = new UnityEngine.Mesh();
                dst.name = src.name + "(copy)";
#if UNITY_2017_3_OR_NEWER
                dst.indexFormat = src.indexFormat;
#endif

                dst.vertices = src.vertices;
                dst.normals = src.normals;
                dst.tangents = src.tangents;
                dst.colors = src.colors;
                dst.uv = src.uv;
                dst.uv2 = src.uv2;
                dst.uv3 = src.uv3;
                dst.uv4 = src.uv4;
                dst.boneWeights = src.boneWeights;
                dst.bindposes = src.bindposes;

                dst.subMeshCount = src.subMeshCount;
                for (int i = 0; i < dst.subMeshCount; ++i)
                {
                    dst.SetIndices(src.GetIndices(i), src.GetTopology(i), i);
                }

                dst.RecalculateBounds();

                if (copyBlendShape)
                {
                    var vertices = src.vertices;
                    var normals = src.normals;
#if VRM_NORMALIZE_BLENDSHAPE_TANGENT
                var tangents = src.tangents.Select(x => (Vector3)x).ToArray();
#else
                    Vector3[] tangents = null;
#endif

                    for (int i = 0; i < src.blendShapeCount; ++i)
                    {
                        src.GetBlendShapeFrameVertices(i, 0, vertices, normals, tangents);
                        dst.AddBlendShapeFrame(
                            src.GetBlendShapeName(i),
                            src.GetBlendShapeFrameWeight(i, 0),
                            vertices,
                            normals,
                            tangents
                            );
                    }
                }

                return dst;
            }

            /// <summary> 
            /// ローカル軸のついたボーンを生成する
            /// * forward: Z+
            /// * axis: X+
            /// </summary>
            public Bone(VrmLib.HumanoidBones bone, PrimitiveType primitive,
            Vector3 forward, Vector3 bendDir, float width, float depth, Vector3? offset = null)
            {
                var up = -bendDir;
                HumanBone = bone;
                m_width = width;
                m_depth = depth;
                m_offset = offset;
                Transform = new GameObject(bone.ToString()).transform;
                Shape = GameObject.CreatePrimitive(primitive).transform;
                var meshFilter = Shape.GetComponent<MeshFilter>();
                meshFilter.sharedMesh = CopyMesh(meshFilter.sharedMesh);
                Shape.SetParent(Transform);

                var axis = Vector3.Cross(up, forward);
                var m = new Matrix4x4(
                    axis,
                    up,
                    forward,
                    new Vector4(0, 0, 0, 1)
                );
                Transform.rotation = m.rotation;
            }

            public float Length
            {
                get
                {
                    return 0;
                }
            }

            List<Bone> m_children = new List<Bone>();
            Bone m_parent;

            public Bone AddChild(Bone child)
            {
                m_children.Add(child);
                child.m_parent = this;
                child.Transform.SetParent(Transform);
                return child;
            }

            public IEnumerable<Bone> Traverse()
            {
                yield return this;

                foreach (var child in m_children)
                {
                    foreach (var x in child.Traverse())
                    {
                        yield return x;
                    }
                }
            }

            public void UpdateSize(in PrimManSettings settings, float parentLength = 0)
            {
                if (parentLength > 0)
                {
                    if (m_offset.HasValue)
                    {
                        // connected
                        Transform.localPosition = m_offset.Value * parentLength;
                    }
                    else
                    {
                        // hips
                        Transform.localPosition = new Vector3(0, 0, parentLength);
                    }
                }

                var length = settings.GetLength(HumanBone);
                // Debug.Log($"{m_bone} => {length}");
                Shape.localScale = new Vector3(length * m_width, length * m_depth, length);
                Shape.localPosition = new Vector3(0, 0, length / 2);
                foreach (var child in m_children)
                {
                    child.UpdateSize(settings, length);
                }
            }

            public void Dispose()
            {
                foreach (var child in m_children)
                {
                    child.Dispose();
                }
                GameObject.Destroy(Transform.gameObject);
            }
        };

        Bone m_hips;

        const float CM_TO_METER = 0.01f;

        /// <summary>
        /// 骨格を作る
        ///
        /// ボーンの
        ///
        /// </summary>
        void BuildHierarchy()
        {
            m_hips = new Bone(HumanoidBones.hips, PrimitiveType.Cube,
                Vector3.up, Vector3.forward, 2, 1);
            m_hips.Transform.SetParent(m_root);

            // spine
            var spine = m_hips.AddChild(new Bone(HumanoidBones.spine, PrimitiveType.Cube,
                Vector3.up, Vector3.forward, 1, 1));
            var chest = spine.AddChild(new Bone(HumanoidBones.chest, PrimitiveType.Cube,
                Vector3.up, Vector3.forward, 2, 1));
            var neck = chest.AddChild(new Bone(HumanoidBones.neck, PrimitiveType.Cube,
                Vector3.up, Vector3.forward, 0.5f, 0.5f));
            var head = neck.AddChild(new Bone(HumanoidBones.head, PrimitiveType.Cube,
                Vector3.up, Vector3.forward, 1, 1));

            // Left leg
            var leftUpperLeg = m_hips.AddChild(new Bone(HumanoidBones.leftUpperLeg, PrimitiveType.Cube,
                Vector3.down, Vector3.back, 0.2f, 0.2f, new Vector3(-1, 0, 0)));
            var leftLowerLeg = leftUpperLeg.AddChild(new Bone(HumanoidBones.leftLowerLeg, PrimitiveType.Cube,
                Vector3.down, Vector3.back, 0.2f, 0.2f));
            var leftFoot = leftLowerLeg.AddChild(new Bone(HumanoidBones.leftFoot, PrimitiveType.Cube,
                Vector3.down, Vector3.back, 0.5f, 1));
            var leftToes = leftFoot.AddChild(new Bone(HumanoidBones.leftToes, PrimitiveType.Cube,
                Vector3.forward, Vector3.up, 0.5f, 0.6f));
            // Right leg
            var rightUpperLeg = m_hips.AddChild(new Bone(HumanoidBones.rightUpperLeg, PrimitiveType.Cube,
                Vector3.down, Vector3.back, 0.2f, 0.2f, new Vector3(1, 0, 0)));
            var rightLowerLeg = rightUpperLeg.AddChild(new Bone(HumanoidBones.rightLowerLeg, PrimitiveType.Cube,
                Vector3.down, Vector3.back, 0.2f, 0.2f));
            var rightFoot = rightLowerLeg.AddChild(new Bone(HumanoidBones.rightFoot, PrimitiveType.Cube,
                Vector3.down, Vector3.back, 0.5f, 1));
            var rightToes = rightFoot.AddChild(new Bone(HumanoidBones.rightToes, PrimitiveType.Cube,
                Vector3.forward, Vector3.up, 0.5f, 0.6f));

            // Left arm
            var leftShoulder = chest.AddChild(new Bone(HumanoidBones.leftShoulder, PrimitiveType.Cube,
                Vector3.left, Vector3.forward, 0.2f, 0.2f));
            var leftUpperArm = leftShoulder.AddChild(new Bone(HumanoidBones.leftUpperArm, PrimitiveType.Cube,
                Vector3.left, Vector3.forward, 0.2f, 0.2f));
            var leftLowerArm = leftUpperArm.AddChild(new Bone(HumanoidBones.leftLowerArm, PrimitiveType.Cube,
                Vector3.left, Vector3.forward, 0.2f, 0.2f));
            var leftHand = leftLowerArm.AddChild(new Bone(HumanoidBones.leftHand, PrimitiveType.Cube,
                Vector3.left, Vector3.down, 0.5f, 0.2f));

            // Right arm
            var rightShoulder = chest.AddChild(new Bone(HumanoidBones.rightShoulder, PrimitiveType.Cube,
                Vector3.right, Vector3.forward, 0.2f, 0.2f));
            var rightUpperArm = rightShoulder.AddChild(new Bone(HumanoidBones.rightUpperArm, PrimitiveType.Cube,
                Vector3.right, Vector3.forward, 0.2f, 0.2f));
            var rightLowerArm = rightUpperArm.AddChild(new Bone(HumanoidBones.rightLowerArm, PrimitiveType.Cube,
                Vector3.right, Vector3.forward, 0.2f, 0.2f));
            var rightHand = rightLowerArm.AddChild(new Bone(HumanoidBones.rightHand, PrimitiveType.Cube,
                Vector3.right, Vector3.down, 0.5f, 0.2f));
        }

        void UpdateSize()
        {
            m_hips.UpdateSize(Settings);
            m_hips.Transform.localPosition = new Vector3(0, Settings.HeightMeter / 2, 0);
        }

        public void Dispose()
        {
            Updated -= UpdateSize;
            // m_hips.Dispose();
        }

        public VrmLib.HumanoidBones? GetHumanBone(Transform t)
        {
            foreach (var bone in m_hips.Traverse())
            {
                if (bone.Transform == t)
                {
                    return bone.HumanBone;
                }
            }

            return null;
        }
    }
}
