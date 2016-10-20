﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using StevenUniverse.FanGame.Util.Collections;
using StevenUniverse.FanGame.Overworld;
using System.Linq;

namespace StevenUniverse.FanGame.Battle
{

    public class Grid : MonoBehaviour
    {
        Dictionary<IntVector3, Node> dict_ = new Dictionary<IntVector3, Node>();

        /// <summary>
        /// Populate the given list with the neighbours of the given node.
        /// </summary>
        /// <param name="nodeBuffer"></param>
        public IList<Node> GetNeighbours( IntVector3 pos )
        {
            var current = GetNode(pos);

            if (current == null)
                return null;
            return current.Neighbours_;
        }

        /// <summary>
        /// Retrieve the node at the given position. Returns null if no node is present.
        /// </summary>
        public Node GetNode( IntVector3 pos )
        {
            Node n;
            dict_.TryGetValue(pos, out n);
            return n;
        }


        /// <summary>
        /// Populates the node buffer with all nodes in the given range starting from the given point with the given movement type.
        /// </summary>
        /// <param name="pos">The position to start the search.</param>
        /// <param name="range">The range of the search (in tiles).</param>
        /// <param name="nodeBuffer">All discovered nodes will be added to the buffer.</param>
        /// <param name="movementType">The movement type to be used during the search.</param>
        public void GetNodesInRange( IntVector3 pos, int range, Path path, MovementType movementType = MovementType.GROUNDED)
        {
            // Use dijkstra's to retrieve all nodes in range.
            var current = GetNode(pos);

            if (current == null)
                return;

            // Our frontier will be a priority queue. The cost to reach each node in our path will determine it's
            // priority in the queue.
            var frontier = new MinPriorityQueue<Node>();
            // Maps each node in our path to the node it came from
            var cameFrom = new Dictionary<Node, Node>();
            // Maps each node in our path to the total cost to reach that node from the starting point
            var costSoFar = new Dictionary<Node, int>();

            path.AddToPath(current);
            frontier.Add(current, 0);
            costSoFar[current] = 0;

            while( frontier.Count != 0 )
            {
                current = frontier.Remove();

                var adjNodes = current.Neighbours_;
                if (adjNodes == null)
                    continue;

                for( int i = 0; i < adjNodes.Count; ++i )
                {
                    var next = adjNodes[i];

                    // Get the total cost to move to this node from the start of our path
                    int newCost = costSoFar[current] + next.GetCost(movementType);

                    // Check if the new cost is within our range and if we've visited the tile yet.
                    // If we have visited the tile we only want to consider it if it's a lower cost
                    // alternative on our path.
                    if( newCost <= range && (!costSoFar.ContainsKey(next) || newCost < costSoFar[next]) )
                    {
                        // Update our path data
                        costSoFar[next] = newCost;
                        frontier.Add(next, newCost);
                        cameFrom[next] = current;
                        path.AddToPath(current.Pos_, next);
                    }
                }
                
            }
        }

        void OnDrawGizmosSelected()
        {
        }

        public void ProcessChunk( Chunk chunk )
        {
            int xMin = chunk.MinX;
            int yMin = chunk.MinY;
            int xMax = chunk.MaxX;
            int yMax = chunk.MaxY;

            int width = xMax - xMin + 1;
            int height = yMax - yMin + 1;
            


            // Run through each cell of the chunk and add valid positions as nodes.
            for( int x = 0; x < width; ++ x )
            {
                for( int y = 0; y < height; ++y )
                {
                    int worldX = x + (int)chunk.Position.x;
                    int worldY = y + (int)chunk.Position.y;
                    var tileStack = chunk.AllInstancesFlattenedCoordinated.Get(worldX, worldY);

                    // Sort the tilestack into a qeury where tiles are grouped by elevation then by sorting order
                    var query = tileStack.OrderBy(t => t.Elevation).ThenBy(t => t.TileTemplate.TileLayer.SortingValue).GroupBy(t => t.Elevation, t => t);

                    foreach( var orderedTiles in query)
                    {
                        // The walkable status of the set of tiles at this elevation
                        // ( which cumulatively represents a node )
                        bool walkable = false;
                        int elevation = orderedTiles.Key;

                        foreach ( var tile in orderedTiles )
                        {
                            var mode = tile.TileTemplate.TileModeName;

                            // Ignore "normal" tiles
                            if (mode == "Normal")
                                continue;

                            if (mode == "Surface" || mode == "Transitional")
                                walkable = true;

                            if (mode == "Collidable")
                                walkable = false;
                        }

                        if( walkable )
                        {
                            // Add node to grid at current position and elevation
                            AddNode(new IntVector3(x, y, elevation));
                        }
                    }
                }
            }
        }
        
        void AddNode( IntVector3 pos )
        {
            dict_.Add(pos, new Node(pos));
        }

    }
}

/*
    Dust on transitioning between elevations:

    The logic for that is basically: if you are on a Surface tile at elevation n, 
    you can go to any adjacent Surface tile at elevation n or any Transitional tile at elevation n or n-1.
    And if you are on a Transitional tile at elevation n, 
    you can go to an adjacent Surface or Transitional tile at either n or n+1(edited)
    Which allows you to move up to non-transitonal tiles
    */
