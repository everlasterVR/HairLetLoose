using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HairLetLoose
{
    public class HairSimHandler : MonoBehaviour
    {
        private bool enableCheck = true;
        private int waitCounter;
        private int waitSeconds = 2;
        private int waitLimit = 60;
        private float timeSinceLastRefresh;
        private float refreshFrequency = 1f;
        private float timeSinceLastUpdate;
        private float updateFrequency = 1/30f;

        private string defaultOption = "<i>Select...</i>";
        private Dictionary<string, ActiveHairSim> activeHairSims;

        private Transform head;
        private DAZHairGroup[] hairItems;

        public JSONStorableStringChooser hairUISelect;
        public JSONStorableString valuesUIText;
        public JSONStorableString settingsInfoUIText;
        public string hairNameText;

        public void OnEnable()
        {
            waitCounter = 0;
            timeSinceLastUpdate = 0f;
            activeHairSims = new Dictionary<string, ActiveHairSim>();
            if (hairUISelect != null)
            {
                hairUISelect.val = defaultOption;
            }
        }

        public void Init(Atom containingAtom)
        {
            head = containingAtom.GetStorableByID("head").transform;
            DAZCharacterSelector geometry = containingAtom.GetComponentInChildren<DAZCharacterSelector>();
            hairItems = geometry.hairItems;
        }

        public void CreateHairSelect()
        {
            hairUISelect = new JSONStorableStringChooser(
                "Hair select",
                activeHairSims.Keys.ToList(),
                defaultOption,
                "Current hairstyle",
                LetHairLoose
            );
        }

        private void RefreshHairOptions()
        {
            foreach(DAZHairGroup it in hairItems)
            {
                string option = $"{it.creatorName} | {it.displayName}";
                if(it.active && it.name == "CustomHairItem" && !activeHairSims.ContainsKey(option))
                {
                    Log.Message($"{option} is active and not yet in activeHairSims. Adding new!");
                    HairSimControl hairSim = it.GetComponentInChildren<HairSimControl>();
                    activeHairSims.Add(option, new ActiveHairSim(hairSim));
                }
                else if(!it.active && activeHairSims.ContainsKey(option))
                {
                    Log.Message($"{option} is not active and still in activeHairSims. Restoring original physics and removing!");
                    activeHairSims[option].RestoreOriginalPhysics();
                    activeHairSims.Remove(option);
                }
            }

            if(hairUISelect != null)
            {
                hairUISelect.choices = activeHairSims.Keys.ToList();
                TryAutoSelect();
            }
        }

        private void TryAutoSelect()
        {
            if(activeHairSims.Count == 0)
            {
                hairUISelect.val = defaultOption;
            }
            else if(activeHairSims.Count == 1)
            {
                string option = activeHairSims.First().Key;
                if(!string.Equals(hairUISelect.val, option))
                {
                    hairUISelect.val = option;
                }
            }
        }

        // TODO decide if timeout is needed
        private void CheckHairSimStatus()
        {
            //if(waitCounter >= waitLimit)
            //{
            //    string msg = "Select a hairstyle and reload the plugin.";
            //    Log.Message($"No hair was selected in {waitLimit} seconds. {msg}");
            //}
            //else if(!loadHairSimInProgress)
            //{
            //    LetHairLoose();
            //}

            //if(hairSimOld != null && !hairSimOld.isActiveAndEnabled)
            //{
            //    RestoreOriginalPhysics();
            //}
            //if(hairSimOld == null || !hairSimOld.isActiveAndEnabled)
            //{
            //    NullifyCurrent();
            //    yield return new WaitForSecondsRealtime(waitSeconds);
            //    waitCounter += waitSeconds;
            //    //loadHairSimInProgress = false;
            //    yield break;
            //}
        }

        private void LetHairLoose(string option)
        {
            RefreshUI(option);
            if(string.Equals(option, defaultOption))
            {
                return;
            }

            ActiveHairSim activeHairSim = activeHairSims[option];
            if(activeHairSim.wasLetLoose)
            {
                return;
            }

            activeHairSim.wasLetLoose = true;
            StartCoroutine(LetHairLooseInternal(activeHairSim));
        }

        private void RefreshUI(string option)
        {
            foreach(KeyValuePair<string, ActiveHairSim> it in activeHairSims)
            {
                if(!string.Equals(it.Key, option))
                {
                    it.Value.UnsetSliders();
                }
            }

            if(activeHairSims.ContainsKey(option))
            {
                activeHairSims[option].InitSliders();
            }
        }

        private IEnumerator LetHairLooseInternal(ActiveHairSim activeHairSim)
        {
            yield return new WaitForEndOfFrame();

            try
            {
                activeHairSim.LetLoose();
            }
            catch(Exception e)
            {
                Log.Message($"LetHairLooseInternal failed! {e}");
                throw;
            }

            // TODO set settingsInfoUIText from selected hair

            //string settingInfo = "";
            //if(settingInfo.Length == 0)
            //{
            //    settingInfo = "\nNone";
            //}
            //settingsInfoUIText.SetVal($"<b><size=30>Changes to hair physics on load</size></b>\n{settingInfo}");
        }

        public void UpdateLimits()
        {
            foreach(KeyValuePair<string, ActiveHairSim> it in activeHairSims)
            {
                it.Value.UpdateUpperLimit();
                it.Value.UpdateLowerLimit();
            }
        }

        // TODO setval "" when no active hair selected
        // TODO checkbox to select if plugin should remember hair settings during session?
        //public void NullifyCurrent()
        //{
        //    settingsInfoUIText.SetVal("");
        //    hairSimOld = null;
        //}

        public void Update()
        {
            if(enableCheck)
            {
                timeSinceLastRefresh += Time.deltaTime;
                if(timeSinceLastRefresh >= refreshFrequency)
                {
                    timeSinceLastRefresh -= refreshFrequency;
                    RefreshHairOptions();
                }
            }

            timeSinceLastUpdate += Time.deltaTime;
            if(timeSinceLastUpdate >= updateFrequency)
            {
                timeSinceLastUpdate -= updateFrequency;
                UpdateAllPhysics();
            }
        }

        private void UpdateAllPhysics()
        {
            float tiltY = (1 + Vector3.Dot(head.up, Vector3.up)) / 2; // 1 = upright, 0 = upside down
            foreach(KeyValuePair<string, ActiveHairSim> it in activeHairSims)
            {
                it.Value.UpdatePhysics(tiltY);
                UpdateValuesUIText(tiltY);
            }
        }

        // TODO get status from current active hair sim
        private void UpdateValuesUIText(float tiltY)
        {
            int angleDegrees = Mathf.RoundToInt((tiltY * 180) - 90);
            valuesUIText.SetVal(
                $"<b><size=30>Current values</size></b>\n\n" +
                $"Angle: {angleDegrees}°\n"
            //$"Main rigidity: {FormatValue(mainRigidity, minMainRigidity, maxMainRigidity)}\n" +
            //$"Tip rigidity: {FormatValue(tipRigidity, minTipRigidity, maxTipRigidity)}\n" +
            //$"Style cling: {FormatValue(styleCling, minStyleCling, maxStyleCling)}"
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

        public void OnDisable()
        {
            RestoreAllOriginalPhysics();
            //hairSimHandler.NullifyCurrent();

        }

        public void OnDestroy()
        {
            RestoreAllOriginalPhysics();
        }

        private void RestoreAllOriginalPhysics()
        {
            foreach(KeyValuePair<string, ActiveHairSim> it in activeHairSims)
            {
                it.Value.RestoreOriginalPhysics();
            }
        }
    }
}
