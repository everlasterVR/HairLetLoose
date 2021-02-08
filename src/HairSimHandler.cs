using System.Collections;
using UnityEngine;

namespace HairLetLoose
{
    public class HairSimHandler : MonoBehaviour
    {
        private bool enableUpdate = false;
        private bool enableCheck = true;
        private bool loadHairSimInProgress = false;
        private int waitCounter;
        private int waitSeconds = 2;
        private int waitLimit = 60;
        private float timeSinceLastUpdate;
        private float updateFrequency = 1/30f;

        private bool? originalPaintedRigidity;
        private float? originalWeight;
        private float? originalDrag;
        private float? originalGravity;
        private float? originalMainRigidity;
        private float? originalTipRigidity;
        private float? originalStyleCling;

        private Transform head;
        private DAZHairGroup[] hairItems;
        private JSONStorableFloat minMainRigidity;
        private JSONStorableFloat maxMainRigidity;
        private JSONStorableFloat minTipRigidity;
        private JSONStorableFloat maxTipRigidity;
        private JSONStorableFloat minStyleCling;
        private JSONStorableFloat maxStyleCling;

        private JSONStorableFloat mainRigidity;
        private JSONStorableFloat tipRigidity;
        private JSONStorableFloat styleCling;
        private float upperLimit;
        private float lowerLimit;

        public JSONStorableString statusUIText;
        public JSONStorableString valuesUIText;
        public JSONStorableString settingsInfoUIText;
        public HairSimControl hairSim = null;
        public string hairNameText;

        public void OnEnable()
        {
            waitCounter = 0;
            timeSinceLastUpdate = 0f;
        }

        public void Init(
            Atom containingAtom,
            JSONStorableFloat minMainRigidity,
            JSONStorableFloat maxMainRigidity,
            JSONStorableFloat minTipRigidity,
            JSONStorableFloat maxTipRigidity,
            JSONStorableFloat minStyleCling,
            JSONStorableFloat maxStyleCling
        )
        {
            head = containingAtom.GetStorableByID("head").transform;
            DAZCharacterSelector geometry = containingAtom.GetComponentInChildren<DAZCharacterSelector>();
            hairItems = geometry.hairItems;
            this.minMainRigidity = minMainRigidity;
            this.maxMainRigidity = maxMainRigidity;
            this.minTipRigidity = minTipRigidity;
            this.maxTipRigidity = maxTipRigidity;
            this.minStyleCling = minStyleCling;
            this.maxStyleCling = maxStyleCling;
        }

        public void LoadHairSim()
        {
            StartCoroutine(LoadHairSimInternal());
        }

        private IEnumerator LoadHairSimInternal()
        {
            loadHairSimInProgress = true;
            yield return new WaitForEndOfFrame();
            string settingInfo = "";

            FindAndSetActiveHairSim();

            if(hairSim != null && !hairSim.isActiveAndEnabled)
            {
                RestoreOriginalPhysics();
            }
            if(hairSim == null || !hairSim.isActiveAndEnabled)
            {
                NullifyCurrent();
                statusUIText.val = $"<b>Active hair\n<color=#AA0000>None</color></b>";
                yield return new WaitForSecondsRealtime(waitSeconds);
                waitCounter += waitSeconds;
                loadHairSimInProgress = false;
                yield break;
            }

            originalPaintedRigidity = hairSim.GetBoolParamValue("usePaintedRigidity");
            if((bool) originalPaintedRigidity)
            {
                settingInfo = $"{settingInfo}\n- disabled painted rigidity";
                hairSim.SetBoolParamValue("usePaintedRigidity", false);
            }

            originalWeight = hairSim.GetFloatParamValue("weight");
            float adjustedWeight = Mathf.Clamp((float) originalWeight, 1.350f, 1.650f);
            if(originalWeight != adjustedWeight)
            {
                settingInfo = $"{settingInfo}\n- weight set to {adjustedWeight} (was {Calc.RoundToDecimals((float) originalWeight, 1000f)})";
                hairSim.SetFloatParamValue("weight", adjustedWeight);
            }

            originalDrag = hairSim.GetFloatParamValue("drag");
            float adjustedDrag = Mathf.Clamp((float) originalDrag, 0.050f, 0.150f);
            if(originalDrag != adjustedDrag)
            {
                settingInfo = $"{settingInfo}\n- drag set to {adjustedDrag} (was {Calc.RoundToDecimals((float) originalDrag, 1000f)})";
                hairSim.SetFloatParamValue("drag", adjustedDrag);
            }

            originalGravity = hairSim.GetFloatParamValue("gravityMultiplier");
            float adjustedGravity = Mathf.Clamp((float) originalGravity, 0.900f, 1.100f);
            if(originalGravity != adjustedGravity)
            {
                settingInfo = $"{settingInfo}\n- gravity multiplier set to {adjustedGravity} (was {Calc.RoundToDecimals((float) originalGravity, 1000f)})";
                hairSim.SetFloatParamValue("gravityMultiplier", adjustedGravity);
            }

            mainRigidity = hairSim.GetFloatJSONParam("mainRigidity");
            originalMainRigidity = mainRigidity.val;
            if(mainRigidity.val > maxMainRigidity.max)
            {
                settingInfo = $"{settingInfo}\n- main rigidity set to {maxMainRigidity.max} (was {Calc.RoundToDecimals((float) originalMainRigidity, 1000f)})";
                maxMainRigidity.val = maxMainRigidity.max;
            }
            else
            {
                maxMainRigidity.val = mainRigidity.val;
            }
            maxMainRigidity.defaultVal = maxMainRigidity.val;
            minMainRigidity.val = maxMainRigidity.val / 10;

            tipRigidity = hairSim.GetFloatJSONParam("tipRigidity");
            originalTipRigidity = tipRigidity.val;
            if(tipRigidity.val > maxTipRigidity.max)
            {
                settingInfo = $"{settingInfo}\n- tip rigidity set to {maxTipRigidity.max} (was {Calc.RoundToDecimals((float) originalTipRigidity, 1000f)})";
                maxTipRigidity.val = maxTipRigidity.max;
            }
            else
            {
                maxTipRigidity.val = tipRigidity.val;
            }
            maxTipRigidity.defaultVal = maxTipRigidity.val;

            styleCling = hairSim.GetFloatJSONParam("cling");
            originalStyleCling = styleCling.val;
            if(styleCling.val > maxStyleCling.max)
            {
                settingInfo = $"{settingInfo}\n- style cling set to {maxStyleCling.max} (was {Calc.RoundToDecimals((float) originalStyleCling, 1000f)})";
                maxStyleCling.val = maxStyleCling.max;
            }
            else
            {
                maxStyleCling.val = styleCling.val;
            }
            maxStyleCling.defaultVal = maxStyleCling.val;
            minStyleCling.val = maxStyleCling.val;
            minStyleCling.defaultVal = minStyleCling.val;

            statusUIText.val = $"<b>Active hair\n{hairNameText}</b>";
            enableUpdate = true;
            loadHairSimInProgress = false;

            if(settingInfo.Length == 0)
            {
                settingInfo = "\nNone";
            }
            settingsInfoUIText.SetVal($"<b><size=30>Changes to hair physics on load</size></b>\n{settingInfo}");
        }

        private void FindAndSetActiveHairSim()
        {
            foreach(DAZHairGroup it in hairItems)
            {
                if(it.active && it.name == "CustomHairItem")
                {
                    hairNameText = $"<color=#007700>{it.creatorName}</color>" +
                        $" | <color=#007700>{it.displayName}</color>";
                    hairSim = it.GetComponentInChildren<HairSimControl>();
                    break;
                }
            }
        }

        public void NullifyCurrent()
        {
            settingsInfoUIText.SetVal("");
            hairSim = null;
            originalPaintedRigidity = null;
            originalWeight = null;
            originalDrag = null;
            originalGravity = null;
            originalMainRigidity = null;
            originalTipRigidity = null;
            originalStyleCling = null;
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

            if(enableUpdate)
            {
                timeSinceLastUpdate += Time.deltaTime;
                if(timeSinceLastUpdate >= updateFrequency)
                {
                    timeSinceLastUpdate -= updateFrequency;
                    UpdateHairPhysics();
                }
            }
        }

        private void UpdateHairPhysics()
        {
            float tiltY = (1 + Vector3.Dot(head.up, Vector3.up)) / 2; // 1 = upright, 0 = upside down
            float baseVal = Mathf.Clamp(Mathf.Lerp(lowerLimit, upperLimit, tiltY), 0f, 1f); // map tilt to lower-upper range, clamp to 0-1
            mainRigidity.val = Calc.RoundToDecimals(Mathf.Lerp(minMainRigidity.val, maxMainRigidity.val, baseVal), 1000f);
            tipRigidity.val = Calc.RoundToDecimals(Mathf.Lerp(minTipRigidity.val, maxTipRigidity.val, baseVal), 10000f);
            styleCling.val = Calc.RoundToDecimals(Mathf.Lerp(minStyleCling.val, maxStyleCling.val, baseVal), 100f);

            UpdateValuesUIText(tiltY);
        }

        public void RestoreOriginalPhysics()
        {
            if(originalPaintedRigidity.HasValue)
            {
                hairSim.SetBoolParamValue("usePaintedRigidity", (bool) originalPaintedRigidity);
            }
            if(originalWeight.HasValue)
            {
                hairSim.SetFloatParamValue("weight", (float) originalWeight);
            }
            if(originalDrag.HasValue)
            {
                hairSim.SetFloatParamValue("drag", (float) originalDrag);
            }
            if(originalGravity.HasValue)
            {
                hairSim.SetFloatParamValue("gravity", (float) originalGravity);
            }
            if(originalMainRigidity.HasValue)
            {
                hairSim.SetFloatParamValue("mainRigidity", (float) originalMainRigidity);
            }
            if(originalTipRigidity.HasValue)
            {
                hairSim.SetFloatParamValue("tipRigidity", (float) originalTipRigidity);
            }
            if(originalStyleCling.HasValue)
            {
                hairSim.SetFloatParamValue("cling", (float) originalStyleCling);
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

        private void UpdateValuesUIText(float tiltY)
        {
            int angleDegrees = Mathf.RoundToInt((tiltY * 180) - 90);
            valuesUIText.SetVal(
                $"<b><size=30>Current values</size></b>\n\n" +
                $"Angle: {angleDegrees}°\n" +
                $"Main rigidity: {FormatValue(mainRigidity, minMainRigidity, maxMainRigidity)}\n" +
                $"Tip rigidity: {FormatValue(tipRigidity, minTipRigidity, maxTipRigidity)}\n" +
                $"Style cling: {FormatValue(styleCling, minStyleCling, maxStyleCling)}"
            );
        }

        private string FormatValue(JSONStorableFloat storable, JSONStorableFloat min, JSONStorableFloat max)
        {
            string text = $"{storable.val}";
            if(min.val == max.val)
            {
                return text;
            }

            if(storable.val >= max.val)
            {
                text += " (highest)";
            }
            else if(storable.val <= min.val)
            {
                text += " (lowest)";
            }
            return text;
        }
    }
}
