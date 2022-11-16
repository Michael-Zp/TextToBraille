using System.Numerics;
using System.Text;

namespace StlGenerator
{
    /// <summary>
    /// https://en.wikipedia.org/wiki/STL_(file_format) for details
    /// </summary>
    /// 
    internal class TextBasedStlExporter : IStlExporter
    {
        private StringBuilder modelStringBuilder = new StringBuilder();


        public void WriteStl(string modelName, string outputPath, List<StlTriangle> triangles)
        {
            AddModelHeader(modelName);

            foreach (var triangle in triangles)
            {
                AddVertexHeader(triangle.Normal);
                AddVertex(triangle.Vertex0);
                AddVertex(triangle.Vertex1);
                AddVertex(triangle.Vertex2);
                AddVertexFooter();
            }

            AddModelFooter(modelName);

            File.WriteAllText(outputPath, modelStringBuilder.ToString());
        }

        private void AddModelHeader(string modelName)
        {
            modelStringBuilder.AppendLine($"solid {modelName}");
        }
        private void AddModelFooter(string modelName)
        {
            modelStringBuilder.Append($"endsolid {modelName}");
        }

        private void AddVertexHeader(Vector3 normal)
        {
            modelStringBuilder.AppendLine($"facet normal {VertexToString(normal)}");
            modelStringBuilder.AppendLine("    outer loop");
        }

        private void AddVertex(Vector3 vertex)
        {
            modelStringBuilder.AppendLine($"        vertex {VertexToString(vertex)}");
        }

        private string VertexToString(Vector3 vertex)
        {
            return $"{FormatDouble(vertex.X)} {FormatDouble(vertex.Y)} {FormatDouble(vertex.Z)}";
        }

        private string FormatDouble(double number)
        {
            var epsilon = 1e-4;
            if (number < epsilon && number > -epsilon)
            {
                // Cut out small numbers, because at these dimensions it does not matter and is just annoying for slicing
                return "0.0";
            }
            else
            {
                // As my computer is set to german it does this stupid , for a . thing. Can probably be handled by some culture thingy, but I'm lazy and it should be fine.
                return $"{number}".Replace(",", ".");
            }
        }

        private void AddVertexFooter()
        {
            modelStringBuilder.AppendLine($"    endloop");
            modelStringBuilder.AppendLine($"endfacet");
        }
    }
}
