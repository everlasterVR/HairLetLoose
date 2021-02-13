﻿using SimpleJSON;
using System;

namespace HairLetLoose
{
    internal class Script : MVRScript
    {
        private string pluginVersion = "0.0.0";

        private HairSimHandler hairSimHandler;

        public override void Init()
        {
            try
            {
                if(containingAtom.type != "Person")
                {
                    Log.Error($"Plugin is for use with 'Person' atom, not '{containingAtom.type}'");
                    return;
                }

                if(gameObject.GetComponent<HairSimHandler>() == null)
                {
                    hairSimHandler = gameObject.AddComponent<HairSimHandler>();
                }

                InitPluginUILeft();
                hairSimHandler.Init(containingAtom);
                InitPluginUIRight();
            }
            catch(Exception e)
            {
                Log.Error($"Exception caught: {e}");
                hairSimHandler.enabled = false;
            }
        }

        public void OnEnable()
        {
            if(hairSimHandler != null)
            {
                hairSimHandler.enabled = true;
            }
        }

        #region User interface

        private void InitPluginUILeft()
        {
            JSONStorableString titleUIText = new JSONStorableString("titleText", "");
            UIDynamicTextField titleUITextField = CreateTextField(titleUIText);
            titleUITextField.UItext.fontSize = 30;
            titleUITextField.height = 100;
            titleUIText.val = $"<b>{nameof(HairLetLoose)}</b>\n<size=26>v{pluginVersion}</size>";

            UIElementStore.Init();
            NewSlider(UIElementStore.dummyLowerAngleLimit, valueFormat: "F0");
            NewSlider(UIElementStore.dummyUpperAngleLimit, valueFormat: "F0");
            NewSlider(UIElementStore.dummyMinMainRigidity, valueFormat: "F3");
            NewSlider(UIElementStore.dummyMaxMainRigidity, valueFormat: "F3");
            NewSlider(UIElementStore.dummyMinTipRigidity, valueFormat: "F4");
            NewSlider(UIElementStore.dummyMaxTipRigidity, valueFormat: "F4");
            NewSlider(UIElementStore.dummyMinStyleCling, valueFormat: "F2");
            NewSlider(UIElementStore.dummyMaxStyleCling, valueFormat: "F2");
            UIElementStore.StoreSliders();
        }

        private void InitPluginUIRight()
        {
            hairSimHandler.CreateHairSelect();
            UIDynamicPopup hairUISelectPopup = CreatePopup(hairSimHandler.hairUISelect, rightSide: true);
            hairUISelectPopup.height = 100;

            UIElementStore.toggleEnableButton = CreateButton("Disable for selected hairstyle", rightSide: true);
            UIElementStore.toggleEnableButton.height = 50;
            UIElementStore.toggleEnableButton.button.onClick.AddListener(() => {
                if(enabled)
                {
                    bool? result = hairSimHandler.ToggleEnableCurrent();
                    UIElementStore.UpdateToggleButtonText(result);
                }
            });
            UIElementStore.UpdateToggleButtonText(null);

            JSONStorableString helpUIText = new JSONStorableString("helpText", "");
            UIDynamicTextField helpUITextField = CreateTextField(helpUIText, rightSide: true);
            helpUITextField.UItext.fontSize = 26;
            helpUITextField.height = 325;
            helpUIText.val = $"\n<b><size=30>How it works</size></b>\n\n" +
                $"Hair is the least rigid at the lower limit angle, and the most rigid at the upper limit angle.\n\n" +
                $"90° is upright, 0° is horizontal, -90° is upside down.";

            hairSimHandler.valuesUIText = new JSONStorableString("valuesText", "");
            UIDynamicTextField valuesUITextField = CreateTextField(hairSimHandler.valuesUIText, rightSide: true);
            valuesUITextField.UItext.fontSize = 26;
            valuesUITextField.height = 255;

            hairSimHandler.notificationsUIText = new JSONStorableString("logText", "");
            UIDynamicTextField notificationsUITextField = CreateTextField(hairSimHandler.notificationsUIText, rightSide: true);
            notificationsUITextField.UItext.fontSize = 26;
            notificationsUITextField.height = 390;
        }

        private void NewSlider(
            JSONStorableFloat storable,
            string valueFormat, // 3 decimal places
            bool rightSide = false
        )
        {
            UIDynamicSlider slider = CreateSlider(storable, rightSide);
            slider.valueFormat = valueFormat;
        }

        public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
        {
            JSONClass json = base.GetJSON(includePhysical, includeAppearance, forceStore);

            //var animationJSON = new JSONClass
            //{
            //    { "Speed", animation.speed.ToString(CultureInfo.InvariantCulture) },
            //    { "Master", animation.master ? "1" : "0" }
            //};
            //var clipsJSON = new JSONArray();
            //foreach(var clip in animation.clips.Where(c => animationNameFilter == null || c.animationName == animationNameFilter))
            //{
            //    clipsJSON.Add(SerializeClip(clip));
            //}
            //animationJSON.Add("Clips", clipsJSON);
            //return animationJSON;

            //json["HairPhysics"] = serializer.SerializeAnimation(animation);
            //json["Options"] = AtomAnimationSerializer.SerializeEditContext(animationEditContext);

            return json;
        }

        #endregion User interface

        public void OnDisable()
        {
            if(hairSimHandler != null)
            {
                hairSimHandler.enabled = false;
            }
        }

        public void OnDestroy()
        {
            Destroy(gameObject.GetComponent<HairSimHandler>());
        }
    }
}
