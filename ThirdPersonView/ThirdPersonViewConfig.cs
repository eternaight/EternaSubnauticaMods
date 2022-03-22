using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;
using UnityEngine;

namespace ThirdPersonView {
    [Menu("Third Person View Options")]
    class ThirdPersonViewConfig : ConfigFile {

        [Slider("Radius of camera sphere (swimming)", 1, 10, DefaultValue = 3)]
        public float swimDistance = 3;

        [Slider("Radius of camera sphere (seamoth/prawn)", 1, 10, DefaultValue = 6)]
        public float vehicleDistance = 6;

        [Slider("Radius of camera sphere (piloting cyclops)", 1, 10, DefaultValue = 1)]
        public float cyclopsDistance = 1;

        [Toggle("Switch to first person in bases/cyclops")]
        public bool switchToFirstPersonWhenInside = true;

        [Slider("Sensitivity (deg/s)", 1, 180, DefaultValue = 90)]
        public float rotationSpeed = 90;

        [Slider("Focus area radius", 0, 1.5f, DefaultValue = 0.4f)]
        public float focusRadius = 0.4f;

        [Slider("Focus centering strength", 0, 1, DefaultValue = 0.7f)]
        public float focusCentering = 0.7f;

        [Slider("Delay of camera aligning to movement", 0, 10, DefaultValue = 3)]
        public float alignDelay = 3;

        [Slider("Range of camera alignment (deg)", 0, 90, DefaultValue = 45)]
        public float alignSmoothRange = 45f;

        [Button("Set Defaults")]
        public void SetDefaults() {
            focusRadius = 0.4f;
            focusCentering = 0.7f;
            rotationSpeed = 90;
            alignDelay = 3;
            alignSmoothRange = 45f;
            swimDistance = 3;
            vehicleDistance = 6;
            cyclopsDistance = 1;
            switchToFirstPersonWhenInside = true;
        }
    }
}
