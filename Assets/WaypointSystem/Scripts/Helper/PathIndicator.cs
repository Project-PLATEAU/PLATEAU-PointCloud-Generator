using System.Collections;
using UnityEngine;

namespace WaypointSystem
{
    public class PathIndicator : MonoBehaviour
    {
        public float modRotation = 0;
        private ParticleSystem pSys;

        void Start()
        {
            pSys = GetComponentInChildren<ParticleSystem>();
            StartCoroutine("EmitParticles");
        }


        IEnumerator EmitParticles()
        {
            yield return new WaitForEndOfFrame();
            while (true)
            {
                float rot = (transform.eulerAngles.y + modRotation) * Mathf.Deg2Rad;
                var pMain = pSys.main;
                pMain.startRotation = rot;
                
                pSys.Emit(1);
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}