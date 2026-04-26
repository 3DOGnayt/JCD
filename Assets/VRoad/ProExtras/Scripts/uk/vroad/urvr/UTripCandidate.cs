using uk.vroad.api.enums;
using uk.vroad.api.etc;
using uk.vroad.api.geom;
using uk.vroad.api.map;
using uk.vroad.rvr;
using uk.vroad.ucm;
using UnityEngine;

namespace uk.vroad.urvr
{
    public class UTripCandidate : MonoBehaviour
    {
        public GameObject cheqFlagPrefab;

        private TripCandidate tripCandidate;
        private ILocus tripRibbon;

        private GameObject cheqFlag;

        internal void Build(Game game, TripCandidate tc, int ci)
        {
            tripCandidate = tc;

            Xyz p0 = tc.Origin().Location().PlusZ(50);
            Xyz p3 = tc.Destination().Location().PlusZ(50);

            Xyz v03 = p3.Minus(p0);
            Xyz along = v03.MultipliedBy(0.5);
            Xyz v03n = v03.Normalized();
            Xyz side = (new Xyz(v03n.Y(), -v03n.X(), 0)).MultipliedBy(v03.Length() * 0.25);
            
            double deflectionUp = 0.33 * v03.Length();

            Xyz up = new Xyz(0, 0, deflectionUp);

            Xyz p1 = p0.Plus(along).Plus(up).Plus(side);
            Xyz p2 = p3.Plus(up).Plus(side);

            const float halfWidth = 4.0f;
            tripRibbon = game.Map().NewRibbonLocus(p0, p1, p2, p3, halfWidth);

            // The chequered flag is an icon, and appears out of place (because of its projection?)
            // This adjustment vector moves the icon towards the map centre point by a fraction of the distance
            // and that seems to fix it for a map of the size of Isle of Dogs
            //
            // A better solution might be to create a MeshRenderer with a single rectangle mesh
            // and apply the chequered flag as a texture (just as is done for layerVial in UMapMesh)
            Vector3 adjustIconPosition = p3.Minus(game.Map().GetCentrePoint()).ToVector3();
            adjustIconPosition.y = 0;
            adjustIconPosition *= - 0.07f;
            Vector3 position = p3.ToVector3() + adjustIconPosition;
            cheqFlag = Instantiate(cheqFlagPrefab, transform);
            float size = 10.0f;
            cheqFlag.transform.localScale = new Vector3(size, size, 1);
            cheqFlag.transform.SetPositionAndRotation(position, Quaternion.Euler(90f, 0, 0));
            cheqFlag.SetActive(false);
        }

        public TripCandidate TripCandidate()
        {
            return tripCandidate;
        }

        public void CreateMesh(bool selected, int mati) 
        {
            MeshFilter mf = gameObject.GetComponent<MeshFilter>();
            MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
            Mesh mesh = mf.mesh;
            mesh.Clear();
            
            cheqFlag.SetActive(selected);
            
            TriMesh tm = UMapMeshRvr.LocusFullTriMesh(tripRibbon, 0, 1f, MaterialHint.None);
            
            SubMesh sm = UMapMeshRvr.TriangleSubMesh(tm, 0);
            UMapMeshRvr.CreateMesh(mf, mr, sm);
            Material mat = UPlayerRoute.OptionMaterialStatic(mati);
            mr.sharedMaterial = mat;

        }
    }
}
