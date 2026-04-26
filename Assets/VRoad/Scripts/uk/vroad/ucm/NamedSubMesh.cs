using uk.vroad.api.map;
using uk.vroad.api.str;
using UnityEngine;

namespace uk.vroad.ucm
{
    public class NamedSubMesh: SubMesh
    {
        public string GameObjectName { get; private set; }
       
        public NamedSubMesh(object mapObject, Vector3[] va, int[] ta, Vector3[] na, Vector2[] uva, int mati)
            : base(va, ta, na, uva, mati)
        {
            GameObjectName = GetNameFromObject(mapObject);
        }

        // Edit this method to change how the GameObject (and its Mesh) is named for each type of map object
        private string GetNameFromObject(object mapObject)
        {
            if (mapObject is ILane lane)
            {
                return lane.Description() + SC.UL + lane.ToString();
            }

            if (mapObject is IStreme streme)
            {
                return streme.ToString();
            }
            
            if (mapObject is IJunction jn)
            {
                return jn.ToString();
            }
            
            if (mapObject is ICrossing x)
            {
                return DescribedName(x);
            }
            if (mapObject is IFootpath fp)
            {
                return DescribedName(fp);
            }
            if (mapObject is ICorner cr)
            {
                return cr.ToString();
            }
            if (mapObject is IStop stop)
            {
                return DescribedName(stop);
            }

            if (mapObject is ITaxiZone vz)
            {
                return DescribedName(vz);
            }
           
            if (mapObject is IOutline ol)
            {
                return DescribedName(ol);
            }
            
            return mapObject.ToString();
        }

        private string DescribedName(IDescribed mapObj)
        {
            string name = mapObj.ToString();
            string desc = mapObj.Description();
            return string.IsNullOrEmpty(desc) ? name: name + SC.UL + desc;
        }
    }
}