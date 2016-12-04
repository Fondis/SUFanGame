﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SUGame.Util.MapEditing;
using System.Linq;
using SUGame.Util;
using SUGame.Util.Common;

namespace SUGame.World.DynamicMesh
{
    /// <summary>
    /// A Chunk mesh that maintains a stack of tiled meshes, where each layer of the stack represents the
    /// sorting layers of the project.
    /// </summary>
    [ExecuteInEditMode]
    public class ChunkMesh : MonoBehaviour
    {
        [SerializeField]
        TiledMesh[] meshes_ = null;

        [SerializeField, HideInInspector]
        IntVector2 size_;
        
        /// <summary>
        /// The size of each of the tiled meshes.
        /// </summary>
        public IntVector2 Size_
        {
            get
            {
                return size_;
            }
            set
            {
                size_ = value;
            }
        }

        void Awake()
        {
            if( meshes_ == null )
                meshes_ = new TiledMesh[SortingLayer.layers.Length];
        }
        
        /// <summary>
        /// Create a TiledMesh with the given Sorting Layer, parent, and size.
        /// </summary>
        public TiledMesh CreateLayerMesh( int layerIndex, IntVector2 size, Material material )
        {
            var layer = SortingLayerUtil.GetLayerFromIndex(layerIndex);
            var go = new GameObject(layer.name + " Mesh");
            var mesh = go.AddComponent<TiledMesh>();
            mesh.SortingLayer_ = SortingLayerUtil.GetLayerFromIndex(0);
            go.transform.SetParent(transform, false);
            go.transform.SetSiblingIndex(layerIndex);
            mesh.Size_ = size;
            mesh.renderer_.sharedMaterial = material;
            mesh.renderer_.sortingOrder = (int)transform.position.z * 100 + layerIndex;

            meshes_[layerIndex] = mesh;
            mesh.ImmediateUpdate();
            //Debug.LogFormat("Creating mesh of size {0}", size);

            return mesh;
        }


        /// <summary>
        /// Retrieve the tiled mesh matching the given sorting layer.
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public TiledMesh GetMesh( SortingLayer layer )
        {
            int index = SortingLayerUtil.GetLayerIndex(layer);

            return GetLayerMesh(index);
        }

        /// <summary>
        /// Retrieve the Tiled Mesh matching the given zero based sorting layer index.
        /// </summary>
        public TiledMesh GetLayerMesh( int index )
        {
            return meshes_[index];
        }

        public void HideLayer( SortingLayer layer )
        {
            int layerIndex = SortingLayerUtil.GetLayerIndex(layer);

            var mesh = meshes_[layerIndex];
            if (mesh != null)
                mesh.renderer_.enabled = false;
        }

        public void ShowLayer( SortingLayer layer )
        {
            int layerIndex = SortingLayerUtil.GetLayerIndex(layer);

            var mesh = meshes_[layerIndex];
            if (mesh != null)
                mesh.renderer_.enabled = true;
        }

        public void RefreshLayers()
        {
            for( int i = 0; i < meshes_.Length; ++i )
            {
                if (meshes_[i] != null)
                {
                    meshes_[i].RefreshUVs();
                    meshes_[i].RefreshColors();
                    //meshes_[i].ImmediateUpdate();
                }
            }
        }
    }
}