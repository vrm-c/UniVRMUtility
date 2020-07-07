using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniVRM10;

namespace UniVRMUtility.VRMViewer
{
    public static class UnityBuilder
    {
        public static void ToUnityAsset(VrmLib.Model model, ModelAsset asset)
        {
            // texture
            for (int i = 0; i < model.Textures.Count; ++i)
            {
                var src = model.Textures[i];
                var name = !string.IsNullOrEmpty(src.Name)
                    ? src.Name
                    : string.Format("{0}_img{1}", model.Root.Name, i);
                if (src is VrmLib.ImageTexture imageTexture)
                {
                    var texture = RuntimeUnityBuilder.CreateTexture(imageTexture);
                    texture.name = name;
                    asset.Map.Textures.Add(src, texture);
                    asset.Textures.Add(texture);
                }
                else
                {
                    Debug.LogWarning($"{name} not ImageTexture");
                }
            }

            // material
            foreach (var src in model.Materials)
            {
                // TODO: material has VertexColor
                var material = RuntimeUnityMaterialBuilder.CreateMaterialAsset(src, hasVertexColor: false, asset.Map.Textures);
                material.name = src.Name;
                asset.Map.Materials.Add(src, material);
                asset.Materials.Add(material);
            }

            // mesh
            for (int i = 0; i < model.MeshGroups.Count; ++i)
            {
                var src = model.MeshGroups[i];
                if (src.Meshes.Count == 1)
                {
                    // submesh 方式
                    var mesh = new Mesh();
                    mesh.name = src.Name;
                    mesh.LoadMesh(src.Meshes[0], src.Skin);
                    asset.Map.Meshes.Add(src, mesh);
                    asset.Meshes.Add(mesh);
                }
                else
                {
                    // 頂点バッファの連結が必用
                    throw new NotImplementedException();
                }
            }

            // node: recursive
            CreateNodes(model.Root, null, asset.Map.Nodes);
            asset.Root = asset.Map.Nodes[model.Root];

            // renderer
            var map = asset.Map;
            foreach (var (node, go) in map.Nodes)
            {
                if (node.MeshGroup is null)
                {
                    continue;
                }

                if (node.MeshGroup.Meshes.Count > 1)
                {
                    throw new NotImplementedException("invalid isolated vertexbuffer");
                }

                var renderer = RuntimeUnityBuilder.CreateRenderer(node, go, map);
                renderer.enabled = false;
                map.Renderers.Add(node, renderer);
                asset.Renderers.Add(renderer);
            }

            // humanoid            
            var boneMap = map.Nodes
                .Where(x => x.Key.HumanoidBone.GetValueOrDefault() != VrmLib.HumanoidBones.unknown)
                    .Select(x => (x.Value.transform, x.Key.HumanoidBone.Value)).AsEnumerable();

            asset.HumanoidAvatar = HumanoidLoader.LoadHumanoidAvatar(asset.Root.transform, boneMap);
            asset.HumanoidAvatar.name = "VRM";

            var animator = asset.Root.AddComponent<Animator>();
            animator.avatar = asset.HumanoidAvatar;
        }

        /// <summary>
        /// ヒエラルキーを再帰的に構築する
        /// <summary>
        public static void CreateNodes(VrmLib.Node node, GameObject parent, Dictionary<VrmLib.Node, GameObject> nodes)
        {
            GameObject go = new GameObject(node.Name);
            go.transform.SetPositionAndRotation(node.Translation.ToUnityVector3(), node.Rotation.ToUnityQuaternion());
            nodes.Add(node, go);
            if (parent != null)
            {
                go.transform.SetParent(parent.transform);
            }

            if (node.Children.Count > 0)
            {
                for (int n = 0; n < node.Children.Count; n++)
                {
                    CreateNodes(node.Children[n], go, nodes);
                }
            }
        }
    }
}
