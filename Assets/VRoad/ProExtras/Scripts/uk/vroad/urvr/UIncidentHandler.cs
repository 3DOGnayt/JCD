
using uk.vroad.api.events;
using uk.vroad.api.geom;
using uk.vroad.apk;
using uk.vroad.rvr;
using uk.vroad.ucm;
using UnityEngine;

namespace uk.vroad.urvr
{
    public class UIncidentHandler : MonoBehaviour, LDynamicObject, LSimTimeSec
    {
        public GameObject incidentWarningPrefab;

        public Texture texmapVehicleBreakdown;
        public Texture texmapSignalsOutOfOrder;

        private KList<Incident> justSpawned = new KList<Incident>();
        private KList<Incident> justExpired = new KList<Incident>();
        private KHash<Incident, GameObject[]> activeGO = new KHash<Incident, GameObject[]>();
        private KHash<Incident, TextMesh[]> activeText  = new KHash<Incident, TextMesh[]>();
        private bool newSec;
        private Game game;
        public bool DeregisterFireMapChange()
        {
            lock (this)
            {
                justSpawned.Clear();
                justExpired.Clear();
                activeGO.Clear();
            }

            return true;
        }

        void Awake() 
        {
            game = Game.AwakeInstance();
            game.AddEventConsumer(this);  // was in Start
        }

        void Update()
        {
            Incident[] jsia;
            Incident[] jeia;
            lock (this)
            {
                jsia = justSpawned.ToArray();
                justSpawned.Clear();
                jeia = justExpired.ToArray();
                justExpired.Clear();
            }
            
            foreach (Incident incident in jsia)
            {
                Xyzbg loc = incident.Location();
                Vector3 pos = loc.ToVector3(10.0f);

                GameObject[] igoa = new GameObject[2];
                float locAY = (float) loc.Bearing().Degrees();
                igoa[0] = Instantiate(incidentWarningPrefab, pos, Quaternion.Euler(90f, locAY + 90f, 90f), transform);
                igoa[1] = Instantiate(incidentWarningPrefab, pos, Quaternion.Euler(90f, locAY - 90f, 90f), transform);
                activeGO.Put(incident, igoa);

                Texture texmap = incident.IsVehicleBreakdown() ? texmapVehicleBreakdown : texmapSignalsOutOfOrder;
                foreach (GameObject igo in igoa)
                {
                    MeshRenderer mr = igo.GetComponent<MeshRenderer>();
                    mr.material.mainTexture = texmap;
                }

                TextMesh tm0 = igoa[0].GetComponentInChildren<TextMesh>();
                TextMesh tm1 = igoa[1].GetComponentInChildren<TextMesh>();
                activeText.Put(incident, new[] { tm0, tm1, });
                
                // 2022-03-31 TextMeshPro changed to TextMesh for simpler packaging 
                //
                // (obsolete) NOTE: Back-face culling is not enabled by default for TextMeshPro. 
                // In the Inspector for IncidentWarning/TimeRemaining, scroll down to TextMeshPro Material 
                // (Liberation Sans SDF) then click on [Edit...] next to Shader name
                // This should open TMP_SDF-Mobile.shader (a text file) in this editor
                // scroll down to SubShader (was line 73) and change Cull [_CullMode]  to Cull Back
                
            }
           
            foreach (Incident incident in jeia)
            {
                GameObject[] igoa = activeGO.Get(incident);
                foreach (var igo in igoa) Destroy(igo);

                activeGO.Remove(incident);
                activeText.Remove(incident);
            }
        }

        public void FixedUpdate()
        {
            foreach (Incident incident in activeGO.Keys)
            {
                GameObject[] igoa = activeGO.Get(incident);
                foreach (var igo in igoa)
                {
                    igo.transform.Rotate(new Vector3(0,0,1));
                }
            }

            if (newSec)
            {
                newSec = false;
                double now = game.Sim().TimeNow();
                foreach (Incident incident in activeText.Keys)
                {
                    TextMesh[] tma = activeText.Get(incident);
                    string ts = incident.TimeToDusk(now).ToString(false);
                    foreach (var tm in tma) tm.text = ts;
                }
            }

        }
        public void TimeSec()
        {
            newSec = true; // here we are in wrong thread to change text
        }

        public void ObjectCreated(object obj)
        {
            if (obj is Incident)
            {
                Incident incident = (Incident)obj;
                lock (this) { justSpawned.Add(incident); }
            }
        }

        public void ObjectDeleted(object obj)
        {
            if (obj is Incident)
            {
                Incident incident = (Incident)obj;
                lock (this) { justExpired.Add(incident); }


            }
        }
    }
}
