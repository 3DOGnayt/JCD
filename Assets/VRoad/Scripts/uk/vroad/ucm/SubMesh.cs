using UnityEngine;

namespace uk.vroad.ucm
{

    public class SubMesh
    {
        public Vector3[] Vertices { get; private set;  }
        public int[] Triangles { get; private set; }
        public Vector3[] Normals { get; private set; }
        public Vector2[] TexUV { get; private set; }

        public int MaterialIndex { get; private set; }

        public SubMesh(Vector3[] va, int[] ta, Vector3[] na, Vector2[] uva, int mati)
        {
            Vertices = va;
            Triangles = ta;
            Normals = na;
            TexUV = uva;

            MaterialIndex = mati;
        }

    }
}