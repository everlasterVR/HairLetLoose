//#define SHOW_DEBUG
using System;
using System.Collections;
using UnityEngine;

namespace HairLetLoose
{
    public class HairSimHandler : MonoBehaviour
    {
        private bool enableUpdate = false;
        private bool enableCheck = true;
        private bool loadHairSimInProgress = false;
        private float waitCounter = 0f;
        private float waitSeconds = 2f;
        private float waitLimit = 60f;

        private float originalMainRigidity;
        private float originalTipRigidity;

        private Transform head;
        private DAZHairGroup[] hairItems;
        private JSONStorableFloat minMainRigidity;
        private JSONStorableFloat maxMainRigidity;
        private JSONStorableFloat minTipRigidity;
        private JSONStorableFloat maxTipRigidity;
        private JSONStorableFloat mainRigidity;
        private JSONStorableFloat tipRigidity;

        private float upperLimit;
        private float lowerLimit;

        public JSONStorableString statusUIText;
        public JSONStorableString baseDebugInfo;
        public HairSimControl hairSim = null;
        public string hairNameText;

        public void Init(
            Atom containingAtom,
            JSONStorableFloat minMainRigidity,
            JSONStorableFloat maxMainRigidity,
            JSONStorableFloat minTipRigidity,
            JSONStorableFloat maxTipRigidity
        )
        {
            head = containingAtom.GetStorableByID("head").transform;
            DAZCharacterSelector geometry = containingAtom.GetComponentInChildren<DAZCharacterSelector>();
            hairItems = geometry.hairItems;
            this.minMainRigidity = minMainRigidity;
            this.maxMainRigidity = maxMainRigidity;
            this.minTipRigidity = minTipRigidity;
            this.maxTipRigidity = maxTipRigidity;
        }

        public void LoadHairSim()
        {
            StartCoroutine(LoadHairSimInternal());
        }

        public void UpdateUpperLimit(float val)
        {
            float amount = Mathf.Clamp(1 - (val + 90)/180, 0, 0.99f); //prevent division by 0
            upperLimit = 1 + amount/(1 - amount);
        }

        public void UpdateLowerLimit(float val)
        {
            float amount = Mathf.Clamp(1 - (90 - val)/180, 0, 0.99f); //prevent division by 0
            lowerLimit = -amount/(1 - amount);
        }

        public void Update()
        {
            if(enableCheck)
            {
                CheckHairSimStatus();
            }

            if(!enableUpdate)
            {
                return;
            }

            float tiltY = (1 + Vector3.Dot(head.up, Vector3.up)) / 2; // 1 = upright, 0 = upside down
            float baseVal = Mathf.Clamp(Mathf.Lerp(lowerLimit, upperLimit, tiltY), 0f, 1f); // map tilt to lower-upper range, clamp to 0-1
            mainRigidity.val = Calc.RoundToDecimals(Mathf.Lerp(minMainRigidity.val, maxMainRigidity.val, baseVal), 1000f);
            tipRigidity.val = Calc.RoundToDecimals(Mathf.Lerp(minTipRigidity.val, maxTipRigidity.val, baseVal), 1000f);

#if SHOW_DEBUG
            baseDebugInfo.SetVal(
                $"{Log.NameValueString("tiltY", tiltY, 100f, 10)}\n" +
                $"{Log.NameValueString("Base val", baseVal, 1000f, 10)}\n" +
                $"{Log.NameValueString("Tip rigidity", tipRigidity.val, 1000f, 20)}\n" +
                $"{Log.NameValueString("Main rigidity", mainRigidity.val, 1000f, 20)}"
            );
#endif
        }

        public void RestoreOriginalPhysics()
        {
            hairSim.SetFloatParamValue("mainRigidity", originalMainRigidity);
            hairSim.SetFloatParamValue("tipRigidity", originalTipRigidity);
        }

        private IEnumerator LoadHairSimInternal()
        {
            loadHairSimInProgress = true;
            yield return new WaitForEndOfFrame();

            FindAndSetActiveHairSim();

            if(hairSim == null || !hairSim.isActiveAndEnabled)
            {
                hairSim = null; // ensure reload if hair was deactivated and then reactivated
                statusUIText.val = $"Active hair:\n<b><color=#AA0000>None</color></b>";
                yield return new WaitForSecondsRealtime(waitSeconds);
                waitCounter += waitSeconds;
                loadHairSimInProgress = false;
                yield break;
            }

            if(hairSim.hairSettings.PhysicsSettings.UsePaintedRigidity)
            {
                DisablePaintedRigidity();
            }

            mainRigidity = hairSim.GetFloatJSONParam("mainRigidity");
            tipRigidity = hairSim.GetFloatJSONParam("tipRigidity");
            originalMainRigidity = mainRigidity.val;
            originalTipRigidity = tipRigidity.val;
            maxMainRigidity.val = mainRigidity.val > maxMainRigidity.max ? maxMainRigidity.max : mainRigidity.val;
            maxTipRigidity.val = tipRigidity.val > maxTipRigidity.max ? maxTipRigidity.max : tipRigidity.val;
            minMainRigidity.val = maxMainRigidity.val / 10;

            statusUIText.val = $"Active hair:\n{hairNameText}";
            enableUpdate = true;
            loadHairSimInProgress = false;
        }

        private void FindAndSetActiveHairSim()
        {
            foreach(DAZHairGroup it in hairItems)
            {
                if(it.active && it.name == "CustomHairItem")
                {
                    hairNameText = $"<b><color=#007700>{it.creatorName}</color></b>" +
                        $" | <b><color=#007700>{it.displayName}</color></b>";
                    hairSim = it.GetComponentInChildren<HairSimControl>();
                    break;
                }
            }
        }

        // toggle painted rigidity through UI if possible
        private void DisablePaintedRigidity()
        {
            try
            {
                HairSimControlUI hsc = hairSim.UITransform.GetComponentInChildren<HairSimControlUI>();
                hsc.usePaintedRigidityToggle.isOn = false;
            }
            catch(NullReferenceException)
            {
                hairSim.SyncUsePaintedRigidity(false);
            }
        }

        private void CheckHairSimStatus()
        {
            if(hairSim != null && hairSim.isActiveAndEnabled)
            {
                return;
            }

            enableUpdate = false;
            if(waitCounter >= waitLimit)
            {
                enableCheck = false;
                string msg = "Select a hairstyle and reload the plugin.";
                Log.Message($"No hair was selected in {waitLimit} seconds. {msg}");
                statusUIText.val = $"<b><color=#AA0000>{msg}</color></b>";
            }
            else if(!loadHairSimInProgress)
            {
                LoadHairSim();
            }
        }
    }
}
