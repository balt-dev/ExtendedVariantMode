﻿using Celeste;
using MonoMod.RuntimeDetour;
using System;

namespace ExtendedVariants.Variants {
    public class InvertVerticalControls : AbstractExtendedVariant {

        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override object GetVariantValue() {
            return Settings.InvertVerticalControls;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.InvertVerticalControls = (bool) value;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.InvertVerticalControls = (value != 0);
        }

        public override void Load() {
            using (new DetourContext {
                After = { "*" } // we want to be extra sure we're applied after other mods here.
            }) {
                On.Celeste.Level.Update += onLevelUpdate;
            }
        }

        public override void Unload() {
            On.Celeste.Level.Update -= onLevelUpdate;
        }

        private void onLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
            if (Input.Aim == null || Input.GliderMoveY == null || Input.MoveY == null) {
                orig(self);
                return;
            }

            // the upside down variant is the only way to have vertical controls inverted.
            Input.Aim.InvertedY = false;

            // there may be another mod (or the Upside Down variant :p) here. if so, it will mess with Input.Aim.InvertedY
            orig(self);

            // at this point, Input.Aim.InvertedY is either the vanilla value, or what some other mod wants.
            // either way, we should keep it or invert it based on our settings.

            bool expectedValue = Input.Aim.InvertedY;
            if (Settings.InvertVerticalControls) expectedValue = !expectedValue;

            Input.Aim.InvertedY = expectedValue;
            Input.MoveY.Inverted = expectedValue;
            Input.GliderMoveY.Inverted = expectedValue;
        }
    }
}
