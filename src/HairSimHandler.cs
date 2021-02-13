using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HairLetLoose
{
    public class HairSimHandler : MonoBehaviour
    {
        private int waitCounter;
        private int waitSeconds = 2;
        private int waitLimit = 60;
        private float timeSinceLastCheck;
        private float checkFrequency = 1f;
        private float timeSinceLastUpdate;
        private float updateFrequency = 1/30f;

        private Dictionary<string, ActiveHairSim> activeHairSims;

        private Transform head;
        private DAZHairGroup[] hairItems;

        public JSONStorableStringChooser hairUISelect;
        public JSONStorableString valuesUIText;
        public JSONStorableString notificationsUIText;
        public string hairNameText;

        public void OnEnable()
        {
            waitCounter = 0;
            timeSinceLastUpdate = 0f;
            if(activeHairSims != null && activeHairSims.Count > 0)
            {
                foreach(KeyValuePair<string, ActiveHairSim> it in activeHairSims)
                {
                    it.Value.forceDisabled = false;
                    it.Value.ReLetLoose();
                }
                StartCoroutine(RunCheck());
            }
        }

        public void Init(Atom containingAtom)
        {
            head = containingAtom.GetStorableByID("head").transform;
            DAZCharacterSelector geometry = containingAtom.GetComponentInChildren<DAZCharacterSelector>();
            hairItems = geometry.hairItems;
            activeHairSims = new Dictionary<string, ActiveHairSim>();
            StartCoroutine(RunCheck());
        }

        public void CreateHairSelect()
        {
            hairUISelect = new JSONStorableStringChooser(
                "Hair select",
                activeHairSims.Keys.ToList(),
                "",
                "Selected",
                RefreshUI
            );
        }

        private void RefreshUI(string option)
        {
            activeHairSims.ToList()
                .Where(kvp => kvp.Key != option && kvp.Value.hasSliders).ToList()
                .ForEach(kvp => kvp.Value.UnsetSliders());

            if(activeHairSims.ContainsKey(option))
            {
                ActiveHairSim current = activeHairSims[option];
                if(!current.hasSliders)
                {
                    current.InitSliders();
                }
                if(current.wasLetLoose)
                {
                    UIElementStore.UpdateToggleButtonText(current.enabled);
                }
                UpdateNotifications(current.TrackPhysics());
            }
            else
            {
                UIElementStore.UpdateToggleButtonText(null);
                UIElementStore.ApplyDummySliders();
                UpdateNotifications(reset: true);
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

        public void Update()
        {
            timeSinceLastCheck += Time.deltaTime;
            if(timeSinceLastCheck >= checkFrequency)
            {
                timeSinceLastCheck -= checkFrequency;
                StartCoroutine(RunCheck());
                try
                {
                    ActiveHairSim current = activeHairSims[hairUISelect.val];
                    if(current.enabled)
                    {
                        UpdateNotifications(current.TrackPhysics());
                    }
                }
                catch(Exception e)
                {
                }
            }

            timeSinceLastUpdate += Time.deltaTime;
            if(timeSinceLastUpdate >= updateFrequency)
            {
                timeSinceLastUpdate -= updateFrequency;
                UpdateAllPhysics();
            }
        }

        private IEnumerator RunCheck()
        {
            while(activeHairSims == null)
            {
                yield return null;
            }

            try
            {
                RefreshHairOptions();
                MaybeAutoSelect();
                InitActiveHairSims();
            }
            catch(Exception)
            {
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

        private void MaybeAutoSelect()
        {
            if(hairUISelect == null)
            {
                return;
            }

            hairUISelect.choices = activeHairSims.Keys.ToList();
            if(activeHairSims.Count == 0)
            {
                hairUISelect.val = "None";
            }
            else if(hairUISelect.val == "" || !activeHairSims.ContainsKey(hairUISelect.val))
            {
                hairUISelect.val = hairUISelect.choices.First();
            }
            hairUISelect.label = $"Selected{(hairUISelect.choices.Count > 1 ? $"\n(total: {activeHairSims.Count})" : "")}";
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

            UpdateNotifications(activeHairSim.TrackPhysics());
        }

        public void UpdateNotifications(string changes = "", bool reset = false)
        {
            string header = $"\n<b><size=30>Physics settings info</size></b>";
            if(reset)
            {
                notificationsUIText.val = header;
            }
            else
            {
                notificationsUIText.val = header + $"{(changes.Length > 0 ? changes : "\n\nHair physics settings OK.")}";
            }
        }

        private void UpdateAllPhysics()
        {
            float tiltY = 90 * Vector3.Dot(head.up, Vector3.up); // -90 = upside down, 90 = upright
            foreach(KeyValuePair<string, ActiveHairSim> it in activeHairSims)
            {
                it.Value.UpdatePhysics(tiltY);
            }
            UpdateValuesUIText(tiltY);
        }

        private void UpdateValuesUIText(float tiltY)
        {
            string text = $"\n<b><size=30>Current values</size></b>\n\n" +
                $"Angle: {Mathf.RoundToInt(tiltY)}°";

            if(activeHairSims.ContainsKey(hairUISelect.val))
            {
                text = $"{text}\n" +
                    $"{activeHairSims[hairUISelect.val].GetStatus()}";
            }

            valuesUIText.val = text;
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
                    UpdateNotifications(reset: true);
                }
                else
                {
                    activeHairSim.enabled = true;
                    activeHairSim.ReLetLoose();
                    UpdateNotifications(activeHairSim.TrackPhysics());
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
            ForceDisableAllActiveHairSims();
        }

        public void OnDestroy()
        {
            ForceDisableAllActiveHairSims();
        }

        private void ForceDisableAllActiveHairSims()
        {
            foreach(KeyValuePair<string, ActiveHairSim> it in activeHairSims)
            {
                it.Value.forceDisabled = true;
                it.Value.RestoreOriginalPhysics();
            }
        }
    }
}
