using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Editor.Helper {
    [InitializeOnLoad]
    public static class PrefabPreviewLightingSetup {

        private const string HiddenLightName = "Hidden_PrefabPreviewLight";
        private static GameObject hiddenLight;

        static PrefabPreviewLightingSetup() {
            EditorApplication.update += EnsureHiddenLight;
        }

        static void EnsureHiddenLight() {
            if (hiddenLight == null) {
                hiddenLight = GameObject.Find(HiddenLightName);

                if (hiddenLight == null) {
                    hiddenLight = new GameObject(HiddenLightName);
                    hiddenLight.hideFlags = HideFlags.HideAndDontSave;

                    Light light = hiddenLight.AddComponent<Light>();
                    light.type = LightType.Directional;
                    light.color = Color.white;
                    light.intensity = 1f;
                    light.shadows = LightShadows.None;

                    hiddenLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                }

                // Ambient lighting nastavení
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.47f, 0.47f, 0.47f); // #787878
                RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
                RenderSettings.reflectionIntensity = 0.5f;
            }

            EditorApplication.update -= EnsureHiddenLight;
        }

    }
}