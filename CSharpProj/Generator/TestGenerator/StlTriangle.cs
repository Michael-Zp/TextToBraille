using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StlGenerator
{
    internal class StlTriangle
    {
        public Vector3 Normal;
        public Vector3 Vertex0;
        public Vector3 Vertex1;
        public Vector3 Vertex2;

        private StlTriangle()
        { }

        public StlTriangle(Vector3 normal, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2)
        {
            Normal = normal;
            Vertex0 = vertex0;
            Vertex1 = vertex1;
            Vertex2 = vertex2;
        }
    }
}
