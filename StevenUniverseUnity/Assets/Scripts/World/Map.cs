﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using StevenUniverse.FanGame.Util.Collections;
using StevenUniverse.FanGame.Util;
using StevenUniverse.FanGame.Util.MapEditing;

namespace StevenUniverse.FanGame.World
{
    // For animated cells: These will be separated into a separate mesh built for this purpose?
    // FOr instance : If you have a layer consisting of a bunch of different types of non animated tiles, it may still be one mesh.
    // If you then try to paint an animating water tile on top of one of those non-animating tiles, it would have to:
    // Remove the existing cell from the non-animating mesh
    // Create a new animating mesh and build a cell at that position
    // Populate the cell on the animating mesh with the animating tile.
    // ^^ This method seems awkward. Is there a better solution?

    // TODO : Import Dust's rules for how tiles should be placed.
    // Keep in mind the features we'll need for map editing. Things like modifying entire layers of tiles, flood fill, cursor resizing, Undo, etc

    // TODO : Layers can be hidden or shown via the map editor. The chunk should ignore ANY operations attempted
    // on a "hidden" layer. The Map class will follow these rules as well.
    // Layers will be hidden visually as well. There's a few options - since Meshes are already divided by sorting layers it's probably
    // easiest to just iterate through all chunks and hide/show the matching layer for each chunk.
    // Could also use Unity's camera culling: http://answers.unity3d.com/questions/561274/using-layers-to-showhide-different-players.html
    // Note in both the latter examples this would require setting up Layers that match the SortingLayers and having the meshes be set to the
    // appropriate Layer.

    /// <summary>
    /// A map consisting of chunks of tiles. The map will add and manage chunks as needed as tiles are painted in.
    /// The chunks themselves (or some component of them) are responsible for rendering the tiles.
    /// The map has no actual "borders", tiles should be able to be painted anywhere in the world.
    /// </summary>
    [ExecuteInEditMode]
    public class Map : MonoBehaviour, IEnumerable<Chunk>
    {
        [SerializeField]
        IntVector2 chunkSize_;
        public IntVector2 ChunkSize_
        {
            get { return chunkSize_; }
            set { chunkSize_ = value; }
        }
        [SerializeField,HideInInspector]
        IntVector2 lastChunkSize_;

        /// <summary>
        /// Dictionary mapping chunks to their 3D index (convert world to index via <seealso cref="GetChunkIndex(IntVector3)"/>.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        ChunkToPosDict chunkDict_ = new ChunkToPosDict();

        /// <summary>
        /// Dictionary mapping chunks to their SortingLayer.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        ChunksToIntDict layersDict_ = new ChunksToIntDict();

        /// <summary>
        /// Visibility data for the sorting layers in this map.
        /// </summary>
        [SerializeField]
        SortingLayerVisibility isLayerVisible_ = null;

        /// <summary>
        /// Optional way to hide and prevent access to all tiles above a certain height. 
        /// Useful for seeing and operating in chunks below other chunks.
        /// </summary>
        [SerializeField]
        int? heightCutoff_ = null;

        [SerializeField]
        bool showDebugGUI_ = true;


        // This could be useful for things like hiding all tiles at a certain height.
        /// <summary>
        /// Dictionary mapping chunks to their height.
        /// </summary>
        //[SerializeField]
        //[HideInInspector]
        //ChunksToIntDict heightDict_ = new ChunksToIntDict();

        public Tile tile_;

        public IntVector3 pos_;
  
        void Awake()
        {
            isLayerVisible_.Awake();
        }

        /// <summary>
        /// Retrieve the list of tiles at the given position, where each index of the list represents a SortingLayer.Value.
        /// The lists will be pre-populated to match the size of SortingLayer.layers.Length.
        /// Note that raw SortingLayer.Value can start below zero, see <seealso cref="SortingLayerUtil.GetLayerIndex(SortingLayer)"/>
        /// </summary>
        public List<Tile> GetTiles(IntVector3 pos)
        {
            if (heightCutoff_ != null && heightCutoff_ > pos.z)
                return null;

            var chunk = GetChunkWorld(pos);
            if( chunk == null )
                return null;

            return chunk.GetTilesWorld((IntVector2)pos);
        }

        // TODO : Should be able to set null to "erase" a cell (AKA set the alpha of the cell to 0)
        /// <summary>
        /// Sets the given tile in the map at the given index. 
        /// </summary>
        public void SetTile(TileIndex tIndex, Tile t)
        {
            if (heightCutoff_ != null && tIndex.Position_.z > heightCutoff_)
                return;

            // Do nothing if the target layer is hidden
            if (!isLayerVisible_.Get(tIndex.Layer_))
                return;

            var chunkIndex = GetChunkIndex(tIndex.Position_);
            var chunk = GetChunk(chunkIndex);

            if (chunk == null)
            {
                chunk = MakeChunk(chunkIndex, tIndex.Layer_ );
            }

            chunk.SetTileWorld(tIndex, t);
        }

        /// <summary>
        /// Sets the given tile in the map at the given position. The tile's default layer will be used.
        /// </summary>
        public void SetTile( IntVector3 pos, Tile t )
        {
            if (heightCutoff_ != null && heightCutoff_ > pos.z)
                return;

            if (t == null)
            {
                Debug.LogErrorFormat("Attempting to set tile to null at {0}. Because the tile is null the" +
                    " SortingLayer cannot be known, use SetTile(Pos,Layer,Tile) or SetTile(TileIndex)", pos);
                return;
            }

            // Do nothing if the target layer is hidden
            if ( !isLayerVisible_.Get(t.DefaultSortingLayer_) )
                return;

            var index = new TileIndex(pos, t.DefaultSortingLayer_);
            SetTile(index, t);
        }

        // TODO : Should be able to set null to "erase" a cell (AKA set the alpha of the cell to 0)
        //        Should do nothing if the layer was "hidden" via the map editor.
        /// <summary>
        /// Sets the given tile in the map at the given position and layer.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="layer"></param>
        /// <param name="t"></param>
        public void SetTile( IntVector3 pos, SortingLayer layer, Tile t )
        {
            if (heightCutoff_ != null && heightCutoff_ > pos.z)
                return;

            // Do nothing if the target layer is hidden
            if (!isLayerVisible_.Get(layer))
                return;

            var index = new TileIndex(pos, layer);
            SetTile(index, t);
        }

        /// <summary>
        /// Retrieve a chunk from a world position.
        /// </summary>
        public Chunk GetChunkWorld( IntVector3 worldPos )
        {
            var chunkIndex = GetChunkIndex(worldPos);

            return GetChunk(chunkIndex);
        }

        public void SetHeightCutoff( int cutoff )
        {
            if( heightCutoff_ == null || cutoff != heightCutoff_ )
            {
                foreach( var pair in chunkDict_ )
                {
                    var chunk = pair.Value;
                    int chunkHeight = (int)chunk.transform.position.z;
                    if (chunkHeight > cutoff)
                        chunk.gameObject.SetActive(false);
                    else
                        chunk.gameObject.SetActive(true);
                }
            }

            heightCutoff_ = cutoff;
        }

        public void DisableHeightCutoff()
        {
            heightCutoff_ = null;
        }

        /// <summary>
        /// Retrieve a chunk from a ChunkIndex.
        /// </summary>
        Chunk GetChunk( IntVector3 chunkIndex )
        {
            Chunk chunk;
            chunkDict_.TryGetValue(chunkIndex, out chunk);
            return chunk;
        }

        /// <summary>
        /// Get the 3D chunk index from the given world position.
        /// </summary>
        IntVector3 GetChunkIndex(IntVector3 pos)
        {
            pos.x = Mathf.FloorToInt((float)pos.x / (float)chunkSize_.x);
            pos.y = Mathf.FloorToInt((float)pos.y / (float)chunkSize_.y);

            return pos;
        }

        /// <summary>
        /// Create a chunk at the given position in the given sorting layer. The chunks name will match the given index.
        /// </summary>
        Chunk MakeChunk( IntVector3 chunkIndex, SortingLayer layer )
        {
            var go = new GameObject(chunkIndex.ToString());
            go.transform.SetParent(transform);
            IntVector2 chunkXY = IntVector2.Scale((IntVector2)chunkIndex, chunkSize_);
            go.transform.localPosition = new Vector3(chunkXY.x, chunkXY.y, chunkIndex.z);

            var chunk = go.AddComponent<Chunk>();

            // First add the chunk to the position dict.
            chunkDict_[chunkIndex] = chunk;

            // Then to the layer dict.
            ChunkToIntDict layerChunks;
            if( !layersDict_.TryGetValue(layer.value, out layerChunks) )
            {
                layersDict_[layer.value] = layerChunks = new ChunkToIntDict();
            }

            layerChunks[layer.value] = chunk;

            return chunk;
        }

        void OnGUI()
        {
            if (!showDebugGUI_ || !isActiveAndEnabled)
                return;

            var pos = Camera.main.ScreenPointToRay(Input.mousePosition).origin;
            GUILayout.Label("MousePos: " + pos.ToString());
            GUILayout.Label("ChunkIndex: " + GetChunkIndex((IntVector3)pos).ToString());

            if (GUILayout.Button("AddTile"))
            {
                SetTile(pos_, tile_);
            }

            if( GUILayout.Button("Print Tiles"))
            {
                foreach (var pair in chunkDict_)
                {
                    var chunk = pair.Value;

                    chunk.Print();
                }
            }
        }

        void OnValidate()
        {
            if (!isActiveAndEnabled)
                return;

            chunkSize_ = IntVector2.Clamp(chunkSize_, 1, 20);

            if( chunkSize_ != lastChunkSize_ )
            {
                lastChunkSize_ = chunkSize_;

                // TODO: If chunk size is change the entire map would have to be rebuilt. Probably a way to maintain tile references in the cases
                // where the chunks are resized.
            }

        }

        /// <summary>
        /// Set the visibility state of the given layer.
        /// </summary>
        public void SetLayerVisible( SortingLayer layer, bool visible )
        {
            if( visible == false )
            {
                HideLayer(layer);
            }
            else
            {
                ShowLayer(layer);
            }
        }

        /// <summary>
        /// Hide all tiles of the given layer for the entire map.
        /// </summary>
        /// <param name="layer"></param>
        public void HideLayer( SortingLayer layer )
        {
            if (!isLayerVisible_.Get(layer))
                return;

            isLayerVisible_.Set(layer, false);

            var enumerator = chunkDict_.GetEnumerator();
            while( enumerator.MoveNext())
            {
                var chunk = enumerator.Current.Value;
                chunk.HideLayer(layer);
            }
        }

        /// <summary>
        /// Show all tiles of the given layer for the entire map.
        /// </summary>
        /// <param name="layer"></param>
        public void ShowLayer( SortingLayer layer )
        {
            if (isLayerVisible_.Get(layer))
                return;

            isLayerVisible_.Set(layer, false);

            var enumerator = chunkDict_.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var chunk = enumerator.Current.Value;
                chunk.ShowLayer(layer);
            }
        }

        public IEnumerator<Chunk> GetEnumerator()
        {
            foreach( var pair in chunkDict_ )
            {
                yield return pair.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        // Subclasses so dictionaries are serializable.
        [System.Serializable]
        class ChunkToPosDict : SerializableDictionary<IntVector3, Chunk> { }
        [System.Serializable]
        class ChunksToPosDict : SerializableDictionary<IntVector3, List<Chunk>> { }
        [System.Serializable]
        class ChunkToIntDict : SerializableDictionary<int, Chunk> { }
        [System.Serializable]
        class ChunksToIntDict : SerializableDictionary<int, ChunkToIntDict> { }
    }


}