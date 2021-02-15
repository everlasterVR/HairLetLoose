using SimpleJSON;
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
        private List<DAZHairGroup> hairItems;

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
            hairItems = geometry.hairItems.ToList()
                .Where(it => {
                    HashSet<string> bodyHairTags = new HashSet<string> { "genital", "arms", "full body", "legs", "torso" };
                    return it.isLatestVersion && !bodyHairTags.Overlaps(it.tagsArray);
                }).ToList();

            activeHairSims = new Dictionary<string, ActiveHairSim>();
            StartCoroutine(RunCheck());
        }

        public void CreateHairSelect()
        {
            hairUISelect = new JSONStorableStringChooser(
                "Hair select",
                activeHairSims.Keys.ToList(), //choices keys
                activeHairSims.Values.ToList().Select(it => it.optionName).ToList(), //choices displaynames
                "",
                "Selected",
                RefreshUI
            );
        }

        private void RefreshUI(string key)
        {
            //Log.Message($"Calling RefreshUI");
            activeHairSims.ToList()
                .Where(kvp => kvp.Key != key && kvp.Value.hasSliders).ToList()
                .ForEach(kvp => kvp.Value.UnsetSliders());

            if(activeHairSims.ContainsKey(key))
            {
                ActiveHairSim selected = activeHairSims[key];
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
            foreach(DAZHairGroup hair in hairItems)
            {
                string uid = hair.uid;
                if(hair.active && hair.name == "CustomHairItem" && !activeHairSims.ContainsKey(uid))
                {
                    //Log.Message($"Adding option for {uid}");
                    HairSimControl hairSim = hair.GetComponentInChildren<HairSimControl>();
                    if(hairSim != null)
                    {
                        ActiveHairSim activeHairSim = new ActiveHairSim($"{hair.creatorName} | {hair.displayName}", hairSim);
                        activeHairSims.Add(uid, activeHairSim);
                        activeHairSim.LetLoose();
                    }
                }
                else if(!hair.active && activeHairSims.ContainsKey(uid))
                {
                    //Log.Message($"Removing option for {uid} and restoring original physics");
                    activeHairSims[uid].RestoreOriginalPhysics();
                    activeHairSims.Remove(uid);
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
            hairUISelect.displayChoices = activeHairSims.Values.ToList().Select(it => it.optionName).ToList();
            if(hairUISelect.choices.Count == 0)
            {
                hairUISelect.val = "None";
            }
            else if(hairUISelect.val == "None" || !hairUISelect.choices.Contains(hairUISelect.val))
            {
                hairUISelect.val = hairUISelect.choices.First();
            }
        }

        private void RefreshNotifications(ActiveHairSim selected = null)
        {
            if(selected == null)
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
                JSONClass jc = it.Value.Serialize();
                jc["id"] = it.Key;
                array.Add(jc);
            }
            return array;
        }

        public string GetSelectedControlUid()
        {
            ActiveHairSim selected = activeHairSims[hairUISelect.val];
            if(selected != null)
            {
                return selected.optionName;
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

            string optionName = "";
            foreach(JSONClass jc in array)
            {
                ActiveHairSim match = null;
                foreach(KeyValuePair<string, ActiveHairSim> it in activeHairSims)
                {
                    if(it.Key == jc["id"].Value)
                    {
                        match = it.Value;
                    }
                    if(it.Key == selected)
                    {
                        optionName = it.Value.optionName;
                    }
                }
                if(match != null)
                {
                    match.RestoreFromJSON(jc);
                }
            }

            if(hairUISelect.val == optionName)
            {
                RefreshUI(optionName);
            }
            else
            {
                hairUISelect.val = optionName;
            }
        }
    }
}
