using System.Numerics;

namespace StlGenerator
{
    /// <summary>
    /// https://en.wikipedia.org/wiki/STL_(file_format) for details
    /// </summary>
    internal class BinaryStlExporter : IStlExporter
    {
        private const int HeaderSize = 80;
        private const int NumTriangleSize = 4;
        private const int TriangleSize = 50;

        private int CurrentIndex = 0;

        public void WriteStl(string _, string outputPath, List<StlTriangle> triangles)
        {
            CurrentIndex = 0;
            var outputBuffer = new byte[HeaderSize + NumTriangleSize + TriangleSize * triangles.Count];

            AddHeader(ref outputBuffer);
            AddNumTriangles(ref outputBuffer, triangles.Count);

            foreach (var triangle in triangles)
            {
                AddVec3(ref outputBuffer, triangle.Normal);
                AddVec3(ref outputBuffer, triangle.Vertex0);
                AddVec3(ref outputBuffer, triangle.Vertex1);
                AddVec3(ref outputBuffer, triangle.Vertex2);
                AddAttributeBytes(ref outputBuffer);
            }

            File.WriteAllBytes(outputPath, outputBuffer);
        }

        private void AddHeader(ref byte[] outputBuffer)
        {
            // Just clear the array, as the header is ignored anyway, so we just write 0s and jump over it
            Array.Clear(outputBuffer);
            CurrentIndex = HeaderSize;
        }

        private void AddNumTriangles(ref byte[] outputBuffer, int triangleCount)
        {
            var numBytes = BitConverter.GetBytes(triangleCount);

            // Format expects little endian, so reverse if we are in big endian
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(numBytes);
            }
            Array.Copy(numBytes, 0, outputBuffer, CurrentIndex, numBytes.Length);
            CurrentIndex += numBytes.Length;
        }

        private void AddVec3(ref byte[] outputBuffer, Vector3 vector)
        {
            AddFloat(ref outputBuffer, vector.X);
            AddFloat(ref outputBuffer, vector.Y);
            AddFloat(ref outputBuffer, vector.Z);
        }

        private void AddFloat(ref byte[] outputBuffer, float val)
        {
            var bytes = BitConverter.GetBytes(val);

            // Format expects little endian, so reverse if we are in big endian
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            Array.Copy(bytes, 0, outputBuffer, CurrentIndex, bytes.Length);
            CurrentIndex += bytes.Length;
        }

        private void AddAttributeBytes(ref byte[] outputBuffer)
        {
            // Are ignored anyway, so just zero 'em
            outputBuffer[CurrentIndex] = 0;
            outputBuffer[CurrentIndex + 1] = 0;
            CurrentIndex += 2;
        }

    }
}
