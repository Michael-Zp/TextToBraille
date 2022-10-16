using System.Numerics;

namespace StlGenerator
{
    internal class ModelBuilder
    {
        private IStlExporter StlExporter;
        private string ModelName;
        private List<StlTriangle> StlTriangles = new List<StlTriangle>();

        public ModelBuilder(string modelName, IStlExporter stlExporter)
        {
            ModelName = modelName;
            StlExporter = stlExporter;
        }

        public void AddPlane(Vector3 normal, Vector2 dimensions, Vector3 center)
        {
            var defaultNormal = Vector3.UnitZ; // In Stl z is up
            var dimensOffset = dimensions / 2;
            var points = new Vector3[2][];
            points[0] = new Vector3[2];
            points[1] = new Vector3[2];
            points[0][0] = new Vector3(-dimensOffset.X, -dimensOffset.Y, 0);
            points[0][1] = new Vector3(dimensOffset.X, -dimensOffset.Y, 0);
            points[1][0] = new Vector3(-dimensOffset.X, dimensOffset.Y, 0);
            points[1][1] = new Vector3(dimensOffset.X, dimensOffset.Y, 0);

            var triangles = new Vector3[2][];
            triangles[0] = new Vector3[3];
            triangles[1] = new Vector3[3];
            triangles[0][0] = points[0][0];
            triangles[0][1] = points[0][1];
            triangles[0][2] = points[1][0];
            triangles[1][0] = points[0][1];
            triangles[1][1] = points[1][1];
            triangles[1][2] = points[1][0];

            Matrix4x4 rotationMatrix;
            var normalizedNormal = Vector3.Normalize(normal);
            var v = Vector3.Cross(defaultNormal, normalizedNormal);
            if (v == Vector3.Zero)
            {
                if (normalizedNormal == defaultNormal)
                {
                    // If identical then, it can stay as it is
                    rotationMatrix = Matrix4x4.Identity;
                }
                else
                {
                    // If they are not identical, this means they are pointed at the exact opposite direction, so just rotate it
                    rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(0, (float)Math.PI, 0);
                }
            }
            else
            {
                var c = Vector3.Dot(defaultNormal, normalizedNormal);
                var s = Vector3.Normalize(v);
                var kmat = new Matrix4x4(
                    0, v.Z, -v.Y, 0,
                    -v.Z, 0, v.X, 0,
                    v.Y, -v.X, 0, 0,
                    0, 0, 0, 0);
                rotationMatrix = Matrix4x4.Identity + kmat + kmat * kmat * ((1 - c) / s.LengthSquared());
            }

            foreach (var triangle in triangles)
            {
                var vertices = new Vector3[3];
                for (int i = 0; i < triangle.Length; i++)
                {
                    var translatedVertex = Vector3.Transform(triangle[i], rotationMatrix);
                    translatedVertex += center;
                    vertices[i] = translatedVertex;
                }

                StlTriangles.Add(new StlTriangle(normal, vertices[0], vertices[1], vertices[2]));
            }
        }

        public void AddAABox(Vector3 dimensions, Vector3 center)
        {
            // Top
            AddPlane(
                new Vector3(0, 0, 1), 
                new Vector2(dimensions.X, dimensions.Y),
                new Vector3(center.X, center.Y, center.Z + dimensions.Z / 2));

            // Bottom
            AddPlane(
                new Vector3(0, 0, -1),
                new Vector2(dimensions.X, dimensions.Y),
                new Vector3(center.X, center.Y, center.Z - dimensions.Z / 2));

            // Right
            AddPlane(
                new Vector3(1, 0, 0),
                new Vector2(dimensions.Z, dimensions.Y),
                new Vector3(center.X + dimensions.X / 2, center.Y, center.Z));

            // Left
            AddPlane(
                new Vector3(-1, 0, 0),
                new Vector2(dimensions.Z, dimensions.Y),
                new Vector3(center.X - dimensions.X / 2, center.Y, center.Z));

            // Back
            AddPlane(
                new Vector3(0, 1, 0),
                new Vector2(dimensions.X, dimensions.Z),
                new Vector3(center.X, center.Y + dimensions.Y / 2, center.Z));

            // Front
            AddPlane(
                new Vector3(0, -1, 0),
                new Vector2(dimensions.X, dimensions.Z),
                new Vector3(center.X, center.Y - dimensions.Y / 2, center.Z));
        }

        public void AddZFacingZylinder(float radius, float height, uint segments, Vector3 center)
        {
            // Create rotation matrix that needs segment-times rotaton for 360 degree
            var rotationMatrix = Matrix4x4.CreateRotationZ((float)Math.Tau / (float)segments);

            var points = new Vector3[segments];

            // Find all points on the circle of the cylinder
            points[0] = Vector3.UnitX * radius;
            for (int i = 1; i < segments; i++)
            {
                points[i] = Vector3.Transform(points[i - 1], rotationMatrix);
            }

            // Position the zylinder at the center
            for (int i = 0; i < segments; i++)
            {
                points[i] += center;
            }

            // Top plane
            var topOffset = Vector3.UnitZ * height / 2.0f;
            for (int i = 0; i < segments; i++)
            {
                StlTriangles.Add(new StlTriangle(
                    Vector3.UnitZ,
                    center + topOffset,
                    points[i] + topOffset,
                    points[(i + 1) % segments] + topOffset));
            }

            // Bottom plane
            var bottomOffset = -topOffset;
            for (int i = 0; i < segments; i++)
            {
                StlTriangles.Add(new StlTriangle(
                    -Vector3.UnitZ,
                    center + bottomOffset,
                    points[(i + 1) % segments] + bottomOffset,
                    points[i] + bottomOffset));
            }

            // Sides
            for (int i = 0; i < segments; i++)
            {
                var topPoints = new Vector3[] { points[i] + topOffset, points[(i + 1) % segments] + topOffset };
                var bottomPoints = new Vector3[] { points[i] + bottomOffset, points[(i + 1) % segments] + bottomOffset };

                var normal = Vector3.Normalize(points[i] + points[(i + 1) % segments]);

                StlTriangles.Add(new StlTriangle(
                    normal,
                    bottomPoints[0],
                    bottomPoints[1],
                    topPoints[0]));

                StlTriangles.Add(new StlTriangle(
                    normal,
                    bottomPoints[1],
                    topPoints[1],
                    topPoints[0]));
            }
        }

        public void Build(string outputPath)
        {
            StlExporter.WriteStl(ModelName, outputPath, StlTriangles);
        }
    }
}
