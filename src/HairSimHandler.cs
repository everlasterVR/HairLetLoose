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
                "",
                "Selected hairstyle",
                RefreshUI
            );
        }

        private void RefreshUI(string option)
        {
            foreach(KeyValuePair<string, ActiveHairSim> it in activeHairSims)
            {
                if(!string.Equals(it.Key, option) && it.Value.hasSliders)
                {
                    it.Value.UnsetSliders();
                }
            }

            if(activeHairSims.ContainsKey(option))
            {
                ActiveHairSim current = activeHairSims[option];
                current.InitSliders();
                if(current.wasLetLoose)
                {
                   UIElementStore.UpdateToggleButtonText(current.enabled);
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
                    TryAutoSelect();
                    InitActiveHairSims();
                }
            }

            timeSinceLastUpdate += Time.deltaTime;
            if(timeSinceLastUpdate >= updateFrequency)
            {
                timeSinceLastUpdate -= updateFrequency;
                UpdateAllPhysics();
            }
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
        }

        private void TryAutoSelect()
        {
            if(hairUISelect == null)
            {
                return;
            }

            hairUISelect.choices = activeHairSims.Keys.ToList();
            if(activeHairSims.Count == 0)
            {
                hairUISelect.val = "";
            }
            else if(hairUISelect.val == "")
            {
                string option = activeHairSims.First().Key;
                if(!string.Equals(hairUISelect.val, option))
                {
                    hairUISelect.val = option;
                }
            }
        }

        private void InitActiveHairSims()
        {
            foreach(KeyValuePair<string, ActiveHairSim> it in activeHairSims)
            {
                ActiveHairSim activeHairSim = it.Value;
                if(activeHairSim.wasLetLoose)
                {
                    return;
                }

                StartCoroutine(LetHairLoose(activeHairSim));
            }
        }

        private IEnumerator LetHairLoose(ActiveHairSim activeHairSim)
        {
            yield return new WaitForEndOfFrame();

            try
            {
                activeHairSim.enabled = true;
                activeHairSim.LetLoose();
                UIElementStore.UpdateToggleButtonText(true);
            }
            catch(Exception e)
            {
                Log.Error($"Exception caught: {e}", nameof(HairSimHandler));
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

        private void UpdateAllPhysics()
        {
            float tiltY = (1 + Vector3.Dot(head.up, Vector3.up)) / 2; // 1 = upright, 0 = upside down
            foreach(KeyValuePair<string, ActiveHairSim> it in activeHairSims)
            {
                if (it.Value.enabled)
                {
                    it.Value.UpdatePhysics(tiltY);
                }
            }
            UpdateValuesUIText(tiltY);
        }

        private void UpdateValuesUIText(float tiltY)
        {
            int angleDegrees = Mathf.RoundToInt((tiltY * 180) - 90);
            string text = $"<b><size=30>\nCurrent values</size></b>\n\n" +
                $"Angle: {angleDegrees}°";

            if(activeHairSims.ContainsKey(hairUISelect.val))
            {
                text = $"{text}\n" +
                    $"{activeHairSims[hairUISelect.val].GetStatus()}";
            }

            valuesUIText.SetVal(text);
        }

        public bool? ToggleEnableCurrent()
        {
            try
            {
                ActiveHairSim activeHairSim = activeHairSims[hairUISelect.val];
                if(activeHairSim.enabled)
                {
                    activeHairSim.enabled = false;
                    activeHairSim.RestoreOriginalPhysics();
                }
                else
                {
                    activeHairSim.enabled = true;
                    activeHairSim.LetLoose();
                }

                return activeHairSim.enabled;
            }
            catch(Exception)
            {
            }

            return null;
        }

        public void OnDisable()
        {
            RestoreAllOriginalPhysics();
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
