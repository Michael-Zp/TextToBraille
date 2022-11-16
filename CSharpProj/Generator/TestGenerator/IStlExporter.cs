using System.Numerics;

namespace StlGenerator
{
    /// <summary>
    /// General exporter to stl, as there is a binary and text based serialization
    /// </summary>
    internal interface IStlExporter
    {
        public void WriteStl(string modelName, string outputPath, List<StlTriangle> triangles);
    }
}
