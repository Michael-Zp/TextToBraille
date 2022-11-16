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

        public void AddZFacingRoundedZylinder(float bodyRadius, float bodyHeight, uint bodySegments, float tipRadius, float tipHeight, uint tipSegments, Vector3 center)
        {
            // Create rotation matrix that needs segment-times rotaton for 360 degree
            var rotationMatrix = Matrix4x4.CreateRotationZ((float)Math.Tau / (float)bodySegments);

            var bodyCenterPoints = new Vector3[bodySegments];

            // Find all points on the circle of the cylinder
            bodyCenterPoints[0] = Vector3.UnitX * bodyRadius;
            for (int i = 1; i < bodySegments; i++)
            {
                bodyCenterPoints[i] = Vector3.Transform(bodyCenterPoints[i - 1], rotationMatrix);
            }

            var tipPoints = new List<Vector3[]>();

            Func<float, (float Radius, float Height)> getTipCircleParams = (float segmentNumber) =>
            {
                var linearFactor = Math.Clamp((segmentNumber + 1) / tipSegments, 0, 1);
                var height = linearFactor * tipHeight;
                var radiusFactor = (float)Math.Cos(linearFactor * (Math.PI / 2.0f));
                radiusFactor = (float)Math.Sqrt(radiusFactor);
                var minFactor = (tipRadius / bodyRadius);
                var factorRange = (1.0f - minFactor);
                var radius = radiusFactor * factorRange + minFactor;
                return (height, radius);
            };

            var topOffset = Vector3.UnitZ * bodyHeight / 2.0f;
            var bottomOffset = -topOffset;

            // Generate points for the tip
            for (int i = 0; i < tipSegments; i++)
            {
                tipPoints.Add(new Vector3[bodySegments]);
                (var circleHeight, var circleRadiusFactor) = getTipCircleParams(i);
                for (int j = 0; j < bodySegments; j++)
                {
                    tipPoints[i][j] = (bodyCenterPoints[j] * circleRadiusFactor) + Vector3.UnitZ * circleHeight + center + topOffset;
                }
            }

            // Position the zylinder at the center
            for (int i = 0; i < bodySegments; i++)
            {
                bodyCenterPoints[i] += center;
            }


            // Bottom plane
            for (int i = 0; i < bodySegments; i++)
            {
                StlTriangles.Add(new StlTriangle(
                    -Vector3.UnitZ,
                    center + bottomOffset,
                    bodyCenterPoints[(i + 1) % bodySegments] + bottomOffset,
                    bodyCenterPoints[i] + bottomOffset));
            }

            // Sides
            for (int i = 0; i < bodySegments; i++)
            {
                var topPoints = new Vector3[] {
                    bodyCenterPoints[i] + topOffset,
                    bodyCenterPoints[(i + 1) % bodySegments] + topOffset
                };

                var bottomPoints = new Vector3[] {
                    bodyCenterPoints[i] + bottomOffset,
                    bodyCenterPoints[(i + 1) % bodySegments] + bottomOffset
                };

                var normal = Vector3.Normalize(bodyCenterPoints[i] + bodyCenterPoints[(i + 1) % bodySegments]);

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

            // All but last tip segments
            for (int i = 0; i < tipSegments - 1; i++)
            {
                for (int j = 0; j < bodySegments; j++)
                {
                    var topPoints = new Vector3[] {
                        tipPoints[i + 1][j],
                        tipPoints[i + 1][(j + 1) % bodySegments]
                    };

                    Vector3[] bottomPoints;
                    if (i == 0)
                    {
                        bottomPoints = new Vector3[] {
                            bodyCenterPoints[j] + topOffset,
                            bodyCenterPoints[(j + 1) % bodySegments] + topOffset
                        };
                    }
                    else
                    {
                        bottomPoints = new Vector3[] {
                            tipPoints[i][j],
                            tipPoints[i][(j + 1) % bodySegments]
                        };
                    }

                    var normal = Vector3.Normalize(bodyCenterPoints[i] + bodyCenterPoints[(i + 1) % bodySegments]);

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

            // Close tip
            for (int i = 0; i < bodySegments; i++)
            {
                StlTriangles.Add(new StlTriangle(
                    Vector3.UnitZ,
                    center + topOffset + Vector3.UnitZ * tipHeight,
                    tipPoints[tipPoints.Count - 1][i],
                    tipPoints[tipPoints.Count - 1][(i + 1) % bodySegments]));
            }
        }

        public void Build(string outputPath)
        {
            StlExporter.WriteStl(ModelName, outputPath, StlTriangles);
        }
    }
}
