using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HairLetLoose
{
    public class HairSimHandler : MonoBehaviour
    {
        private string currentValuesHeader = $"\n<b><size=30>Current values</size></b>";
        private string notificationsHeader = $"\n<b><size=30>Physics settings info</size></b>";

        private float timeSinceLastCheck;
        private float checkFrequency = 1f;
        private float timeSinceLastUpdate;
        private float updateFrequency = 1/30f;
        private int checkCounter;

        private Dictionary<string, ActiveHairSim> activeHairSims;

        private Transform head;
        private DAZHairGroup[] hairItems;

        public JSONStorableStringChooser hairUISelect;
        public JSONStorableString valuesUIText;
        public JSONStorableString notificationsUIText;
        public string hairNameText;

        public void OnEnable()
        {
            timeSinceLastUpdate = 0f;
            checkCounter = 0;
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
            //Log.Message($"Calling RefreshUI");
            activeHairSims.ToList()
                .Where(kvp => kvp.Key != option && kvp.Value.hasSliders).ToList()
                .ForEach(kvp => kvp.Value.UnsetSliders());

            if(activeHairSims.ContainsKey(option))
            {
                ActiveHairSim selected = activeHairSims[option];
                if(!selected.hasSliders)
                {
                    selected.InitSliders();
                }
                if(selected.wasLetLoose)
                {
                    UIElementStore.UpdateToggleButtonText(selected.enabled);
                    RefreshNotifications(selected);
                }
            }
            else
            {
                UIElementStore.UpdateToggleButtonText(null);
                UIElementStore.ApplyDummySliders();
            }
        }

        public void Update()
        {
            timeSinceLastCheck += Time.deltaTime;
            if(timeSinceLastCheck >= checkFrequency)
            {
                timeSinceLastCheck -= checkFrequency;
                StartCoroutine(RunCheck());
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
            yield return new WaitForEndOfFrame();

            while(activeHairSims == null)
            {
                yield return null;
            }

            RefreshHairOptions();
            MaybeAutoSelect();
            hairUISelect.label = $"Selected{(hairUISelect.choices.Count > 1 ? $"\n(total: {activeHairSims.Count})" : "")}";
            RefreshNotifications();

            if(checkCounter < 3)
            {
                checkCounter += 1;
            }
        }

        private void RefreshHairOptions()
        {
            foreach(DAZHairGroup it in hairItems)
            {
                string optionKey = $"{it.creatorName} | {it.displayName}";
                if(it.active && it.name == "CustomHairItem" && !activeHairSims.ContainsKey(optionKey))
                {
                    //Log.Message($"Adding option {optionId}");
                    HairSimControl hairSim = it.GetComponentInChildren<HairSimControl>();
                    ActiveHairSim activeHairSim = new ActiveHairSim(it.internalUid, hairSim);
                    activeHairSims.Add(optionKey, activeHairSim);
                    activeHairSim.LetLoose();
                }
                else if(!it.active && activeHairSims.ContainsKey(optionKey))
                {
                    //Log.Message($"Removing option {optionId} and restoring original physics");
                    activeHairSims[optionKey].RestoreOriginalPhysics();
                    activeHairSims.Remove(optionKey);
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
            if(hairUISelect.choices.Count == 0)
            {
                hairUISelect.val = "None";
            }
            else if(hairUISelect.val == "None" || !activeHairSims.ContainsKey(hairUISelect.val))
            {
                hairUISelect.val = hairUISelect.choices.First();
            }
        }

        private void RefreshNotifications(ActiveHairSim selected = null)
        {
            if (selected == null)
            {
                if(!activeHairSims.ContainsKey(hairUISelect.val))
                {
                    notificationsUIText.val = notificationsHeader;
                    return;
                }

                selected = activeHairSims[hairUISelect.val];
            }
            
            if(!selected.enabled)
            {
                notificationsUIText.val = notificationsHeader;
                return;
            }

            string changes = selected.TrackPhysics();
            notificationsUIText.val = notificationsHeader + $"{(changes.Length > 0 ? changes : "\n\nHair physics settings OK.")}";
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
            string text = currentValuesHeader + $"\n\nAngle: {Mathf.RoundToInt(tiltY)}°";

            if(activeHairSims.ContainsKey(hairUISelect.val))
            {
                text = $"{text}\n" +
                    $"{activeHairSims[hairUISelect.val].GetStatus()}";
            }

            valuesUIText.val = text;
        }

        public bool? ToggleEnableSelected()
        {
            if(!activeHairSims.ContainsKey(hairUISelect.val))
            {
                return null;
            }

            ActiveHairSim selected = activeHairSims[hairUISelect.val];
            if(selected.enabled)
            {
                selected.enabled = false;
                selected.RestoreOriginalPhysics();
            }
            else
            {
                selected.enabled = true;
                selected.ReLetLoose();
            }

            RefreshNotifications(selected);
            return selected.enabled;
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

        public JSONArray Serialize()
        {
            JSONArray array = new JSONArray();
            foreach(KeyValuePair<string, ActiveHairSim> it in activeHairSims)
            {
                array.Add(it.Value.Serialize());
            }
            return array;
        }

        public string GetSelectedControlInternalUid()
        {
            ActiveHairSim selected = activeHairSims[hairUISelect.val];
            if(selected != null)
            {
                return selected.controlInternalUid;
            }

            return "";
        }

        public void RestoreFromJSON(string selected, JSONArray array)
        {
            StartCoroutine(RestoreFromJSONInternal(selected, array));
        }

        private IEnumerator RestoreFromJSONInternal(string selected, JSONArray array)
        {
            while(activeHairSims == null || checkCounter < 3)
            {
                yield return null;
            }

            string optionKey = "";
            foreach(JSONClass jc in array)
            {
                ActiveHairSim match = null;
                foreach(KeyValuePair<string, ActiveHairSim> it in activeHairSims)
                {
                    if(it.Value.controlInternalUid == jc["id"].Value)
                    {
                        match = it.Value;
                    }
                    if(it.Value.controlInternalUid == selected)
                    {
                        optionKey = it.Key;
                    }
                }
                if(match != null)
                {
                    match.RestoreFromJSON(jc);
                }
            }

            if(hairUISelect.val == optionKey)
            {
                RefreshUI(optionKey);
            }
            else
            {
                hairUISelect.val = optionKey;
            }
        }
    }
}
