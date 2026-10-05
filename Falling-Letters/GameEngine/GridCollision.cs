using System.Collections.Generic;
using System.Numerics;
using System;
using Desktop_App;
using System.Windows.Forms.VisualStyles;
using System.Formats.Tar;

namespace GameEngine
{
    public class GridCollisionEngine
    {
        // Using grid collision so objects only check neighboring cells for collisions.
        private readonly int cellSize;

        private readonly Dictionary<(int, int), List<FallingLetter>> grid = new();
        private readonly List<List<FallingLetter>> listPool = new();
        private int poolIndex = 0;

        private static readonly (int dx, int dy)[] uniqueNeighborOffsets = new (int, int)[]
        {
        (1, 0),   // Right
        (0, 1),   // Down
        (1, 1),   // Down-Right
        (-1, 1)   // Down-Left
        };

        public GridCollisionEngine(float cellSize)
        {
            this.cellSize = (int)cellSize;
        }

        public void UpdateGrid(List<FallingLetter> objects)
        {
            // Place each object into the grid based on its position and size
            grid.Clear();
            poolIndex = 0;

            foreach (var obj in objects)
            {
                float radius = Math.Max(obj.Size.x, obj.Size.y) * 0.7f; // Approx bounding circle radius

                int minX = (int)((obj.Position.x - radius) / cellSize);
                int maxX = (int)((obj.Position.x + radius) / cellSize);
                int minY = (int)((obj.Position.y - radius) / cellSize);
                int maxY = (int)((obj.Position.y + radius) / cellSize);

                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        var key = (x, y);
                        if (!grid.TryGetValue(key, out var cellList))
                        {
                            cellList = GetOrCreateList();
                            grid[key] = cellList;
                        }

                        // Don't add duplicates if a single box spans small sub-cells
                        if (!cellList.Contains(obj))
                        {
                            cellList.Add(obj);
                        }
                    }
                }
            }
        }

        public void CheckCollisions()
        {
            // Check for collisions within each cell and between neighboring cells
            foreach (var cell in grid)
            {
                var cellCoords = cell.Key;
                var objectsInCell = cell.Value;

                // 1. Check inner-cell pairs (standard combinations)
                for (int i = 0; i < objectsInCell.Count; i++)
                {
                    for (int j = i + 1; j < objectsInCell.Count; j++)
                    {
                        Intersect(objectsInCell[i], objectsInCell[j]);
                    }
                }

                // 2. Check half-neighbor pairs (No double-checking anymore!)
                foreach (var (dx, dy) in uniqueNeighborOffsets)
                {
                    var neighborKey = (cellCoords.Item1 + dx, cellCoords.Item2 + dy);

                    if (grid.TryGetValue(neighborKey, out var neighborObjects))
                    {
                        for (int i = 0; i < objectsInCell.Count; i++)
                        {
                            for (int j = 0; j < neighborObjects.Count; j++)
                            {
                                Intersect(objectsInCell[i], neighborObjects[j]);
                            }
                        }
                    }
                }
            }
        }

        private List<FallingLetter> GetOrCreateList()
        {
            List<FallingLetter> list;

            if (poolIndex < listPool.Count)
            {
                list = listPool[poolIndex];
                list.Clear();
            }
            else
            {
                // The grid grew larger than previous frames, allocate one new list container
                list = new List<FallingLetter>();
                listPool.Add(list);
            }

            poolIndex++;
            return list;
        }

        private void Intersect(FallingLetter a, FallingLetter b)
        {
            // Prevent calculating physics on the exact same object twice
            if (a == b) return;


            CollisionInfo? info = GetCollisionInfo(a, b);
            if (info == null) return; // ignore if no collision detected
    
            info.ContactPoint = FindContactPoint(info);
            ResolveCollision(info);

        }




        public static Vector2[] GetVertecies(FallingLetter obj)
        {
            Vector2 halfSize = obj.Size * 0.5f;

            // Calculate rotation terms once
            float cos = (float)Math.Cos(obj.Angle);
            float sin = (float)Math.Sin(obj.Angle);

            // Local coordinates of the 4 corners relative to center
            obj.localCorners[0] = new Vector2(-halfSize.x, -halfSize.y); // Top-Left
            obj.localCorners[1] = new Vector2(halfSize.x, -halfSize.y);  // Top-Right
            obj.localCorners[2] = new Vector2(halfSize.x, halfSize.y);  // Bottom-Right
            obj.localCorners[3] = new Vector2(-halfSize.x, halfSize.y);  // Bottom-Left

            // Rotate and translate corners to World Space
            for (int i = 0; i < 4; i++)
            {
                float rx = obj.localCorners[i].x * cos - obj.localCorners[i].y * sin;
                float ry = obj.localCorners[i].x * sin + obj.localCorners[i].y * cos;
                obj.corners[i] = obj.Position + new Vector2(rx, ry);
            }

            return obj.corners;
        }

        private static void ProjectVertices(Vector2[] vertices, Vector2 axis, out float min, out float max)
        {
            min = max = Vector2.Dot(vertices[0], axis);

            for (int i = 1; i < vertices.Length; i++)
            {
                float p = Vector2.Dot(vertices[i], axis);

                if (p < min) min = p;
                if (p > max) max = p;
            }
        }

        private static Vector2 FindContactPoint(CollisionInfo info)
        {
            // Find the deepest vertex from both objects along the collision normal to determine the contact point
            FallingLetter A = info.objA;
            FallingLetter B = info.objB;

            Vector2[] vertsA = GetVertecies(A);
            Vector2[] vertsB = GetVertecies(B);

            float deepest = float.MaxValue;
            Vector2 contact = Vector2.Zero;

            foreach (Vector2 v in vertsA)
            {
                float d = Math.Abs(Vector2.Dot(v - B.Position, info.Normal));

                if (d < deepest)
                {
                    deepest = d;
                    contact = v;
                }
            }

            foreach (Vector2 v in vertsB)
            {
                float d = Math.Abs(Vector2.Dot(v - A.Position, info.Normal));

                if (d < deepest)
                {
                    deepest = d;
                    contact = v;
                }
            }

            return contact;
        }


        private static SupportPoint? findSupportPoint(Vector2 normalOnEdge, Vector2 pointOnEdge, Vector2[] otherVerticies)
        {
            // Find the vertex from the other object that is deepest along the normal of the edge
            float deepestDepth = 0;
            SupportPoint? sp = null;

            for (int i = 0; i < otherVerticies.Length; i++)
            {
                Vector2 vertex = otherVerticies[i];
                Vector2 vertexToEdge = vertex - pointOnEdge;
                float penDepth = Vector2.Dot(vertexToEdge, -normalOnEdge);

                if (penDepth > deepestDepth)
                {
                    deepestDepth = penDepth;
                    sp = new SupportPoint(vertex, deepestDepth);
                }
            }
            return sp;
        }

        private struct SupportPoint
        {
            public Vector2 vertex;
            public float depth;
            public SupportPoint(Vector2 vertex, float depth)
            {
                this.vertex = vertex;
                this.depth = depth;
            }
        }

        private static Vector2[] GetEdges(Vector2[] verticies)
        {
            // Get edges from the vertices of a rectangle in clockwise order
            Vector2[] edges = new Vector2[4];
            edges[0] = verticies[1] - verticies[0];
            edges[1] = verticies[2] - verticies[1];
            edges[2] = verticies[3] - verticies[2];
            edges[3] = verticies[0] - verticies[3];
            return edges;
        }


        public static CollisionInfo? GetCollisionInfo(FallingLetter objA, FallingLetter objB)
        {
            // Use the Separating Axis Theorem (SAT) to determine if two convex shapes are colliding
            CollisionInfo? info = null;
            Vector2[] aVerts = GetVertecies(objA);
            Vector2[] bVerts = GetVertecies(objB);

            Vector2[] aEdges = GetEdges(aVerts);
            Vector2[] bEdges = GetEdges(bVerts);

            float minOverlap = float.MaxValue;
            Vector2 bestAxis = Vector2.Zero;

            List<Vector2> axes = new();

            foreach (var e in aEdges)
                axes.Add(Vector2.Normalize(new Vector2(-e.y, e.x)));

            foreach (var e in bEdges)
                axes.Add(Vector2.Normalize(new Vector2(-e.y, e.x)));

            foreach (var axis in axes)
            {
                ProjectVertices(aVerts, axis, out float minA, out float maxA);
                ProjectVertices(bVerts, axis, out float minB, out float maxB);

                if (maxA < minB || maxB < minA)
                {
                    return null;
                }

                float overlap = Math.Min(maxA, maxB)
                - Math.Max(minA, minB);

                if (overlap < minOverlap)
                {
                    minOverlap = overlap;
                    bestAxis = axis;
                }
            }

            // Make normal point from A -> B
            Vector2 centerDir = objB.Position - objA.Position;

            if (Vector2.Dot(centerDir, bestAxis) < 0)
            {
                bestAxis = -bestAxis;
            }

            info = new CollisionInfo();

            info.objA = objA;
            info.objB = objB;

            info.Normal = bestAxis;
            info.Penetration = minOverlap;

            return info;
        }

        private static void ResolveCollision(CollisionInfo info)
        {
            // Resolve the collision between two FallingLetter objects using impulse-based physics
            FallingLetter A = info.objA;
            FallingLetter B = info.objB;
            Vector2 normal = info.Normal;

            Vector2 rA = info.ContactPoint - A.Position;
            Vector2 rB = info.ContactPoint - B.Position;

            // Handle collision with an object
            Vector2 rvA = A.Velocity + new Vector2(-A.AngularVelocity * rA.y, A.AngularVelocity * rA.x);
            Vector2 rvB = B.Velocity + new Vector2(-B.AngularVelocity * rB.y, B.AngularVelocity * rB.x);
            Vector2 relativeV = rvB - rvA;

            float velAlongNormal = Vector2.Dot(relativeV, normal);
            float contactVelocity = Vector2.Dot(relativeV,normal);
            if (contactVelocity > 0) return;

            if (Math.Abs(velAlongNormal) < 0.1f)
            {
                velAlongNormal = 0;
            }

            float bounciness = Math.Min(A.Bouyency, B.Bouyency);

            if (contactVelocity < 0.1f) bounciness = 0f;

            float rAInverseIntertia = Vector2.Cross(rA, normal) * Vector2.Cross(rA, normal) * (1 / A.Intertia);
            float rBInverseIntertia = Vector2.Cross(rB, normal) * Vector2.Cross(rB, normal) * (1 / B.Intertia);

            float denominator = (1 / A.Mass) + (1 / B.Mass) + rAInverseIntertia + rBInverseIntertia;

            float j = -(1f + bounciness) * velAlongNormal / denominator;

            if (j < 0) j = 0;

            Vector2 impulse = normal * j;

            if (j < 0) j = 0;

            Vector2 tangent = relativeV - normal * Vector2.Dot(relativeV, normal);

            if (Vector2.Magnitude(tangent) * Vector2.Magnitude(tangent) > 0)
            {
                tangent = Vector2.Normalize(tangent);
            }

            float jt = -Vector2.Dot(relativeV, tangent);
            jt /= denominator;

            Vector2 frictionImpulse = tangent * Math.Clamp(jt, j * B.Friction, j * A.Friction);

            A.Velocity -= impulse * (1 / A.Mass);
            A.AngularVelocity -= Vector2.Cross(rA, impulse) * (1 / A.Intertia);
  
            B.Velocity += impulse * (1 / B.Mass);
            B.AngularVelocity += Vector2.Cross(rB, impulse) * (1 / B.Intertia);

            // Correct object sinking
            const float percent = .5f; // Penetration percentage to correct
            const float slop = 0.5f;    // Penetration allowance
            float correctionMagnitude = Math.Max(info.Penetration - slop, 0f) / ((1 / A.Mass) + (1 / B.Mass)) * percent;
            Vector2 correctionVector = info.Normal * correctionMagnitude;
            if ((1 / A.Mass) > 0) A.Position -= correctionVector * (1 / A.Mass);
            if ((1 / B.Mass) > 0) B.Position += correctionVector * (1 / B.Mass);

            A.ClampToBorders();
            B.ClampToBorders();



        }

        public static bool IsColliding(FallingLetter A, FallingLetter B)
        {
            // check if objects are colliding
            Vector2[] a = GetVertecies(A);
            Vector2[] b = GetVertecies(B);

            List<Vector2> axes = new();

            var ea = GetEdges(a);
            var eb = GetEdges(b);

            foreach (var e in ea)
                axes.Add(Vector2.Normalize(new Vector2(-e.y, e.x)));

            foreach (var e in eb)
                axes.Add(Vector2.Normalize(new Vector2(-e.y, e.x)));

            foreach (var axis in axes)
            {
                ProjectVertices(a, axis, out float minA, out float maxA);
                ProjectVertices(b, axis, out float minB, out float maxB);

                if (maxA < minB || maxB < minA)
                    return false;
            }

            return true;
        }


    }
}
