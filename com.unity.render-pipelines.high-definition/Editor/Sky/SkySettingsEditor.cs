using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace UnityEditor.Rendering.HighDefinition
{
    public abstract class SkySettingsEditor : VolumeComponentEditor
    {
        [System.Flags]
        protected enum SkySettingsUIElement
        {
            SkyIntensity = 1 << 0,
            Rotation = 1 << 1,
            UpdateMode = 1 << 2,
            IncludeSunInBaking = 1 << 3,
        }

        GUIContent m_SkyIntensityModeLabel = new UnityEngine.GUIContent("Intensity Mode");

        SerializedDataParameter m_SkyExposure;
        SerializedDataParameter m_SkyMultiplier;
        SerializedDataParameter m_SkyRotation;
        SerializedDataParameter m_EnvUpdateMode;
        SerializedDataParameter m_EnvUpdatePeriod;
        SerializedDataParameter m_IncludeSunInBaking;
        SerializedDataParameter m_DesiredLuxValue;
        SerializedDataParameter m_IntensityMode;
        SerializedDataParameter m_UpperHemisphereLuxValue;

        protected uint m_CommonUIElementsMask = 0xFFFFFFFF;
        protected bool m_EnableLuxIntensityMode = false;

        GUIContent[]    m_IntensityModes = { new GUIContent("Exposure"), new GUIContent("Multiplier"), new GUIContent("Lux") };
        int[]           m_IntensityModeValues = { (int)SkyIntensityMode.Exposure, (int)SkyIntensityMode.Multiplier, (int)SkyIntensityMode.Lux };
        GUIContent[]    m_IntensityModesNoLux = { new GUIContent("Exposure"), new GUIContent("Multiplier") };
        int[]           m_IntensityModeValuesNoLux = { (int)SkyIntensityMode.Exposure, (int)SkyIntensityMode.Multiplier };


        public override void OnEnable()
        {
            var o = new PropertyFetcher<SkySettings>(serializedObject);

            m_SkyExposure = Unpack(o.Find(x => x.exposure));
            m_SkyMultiplier = Unpack(o.Find(x => x.multiplier));
            m_SkyRotation = Unpack(o.Find(x => x.rotation));
            m_EnvUpdateMode = Unpack(o.Find(x => x.updateMode));
            m_EnvUpdatePeriod = Unpack(o.Find(x => x.updatePeriod));
            m_IncludeSunInBaking = Unpack(o.Find(x => x.includeSunInBaking));
            m_DesiredLuxValue = Unpack(o.Find(x => x.desiredLuxValue));
            m_IntensityMode = Unpack(o.Find(x => x.skyIntensityMode));
            m_UpperHemisphereLuxValue = Unpack(o.Find(x => x.upperHemisphereLuxValue));
        }

        protected void CommonSkySettingsGUI()
        {
            if ((m_CommonUIElementsMask & (uint)SkySettingsUIElement.SkyIntensity) != 0)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawOverrideCheckbox(m_IntensityMode);
                    using (new EditorGUI.DisabledScope(!m_IntensityMode.overrideState.boolValue))
                    {
                        if (m_EnableLuxIntensityMode)
                        {
                            m_IntensityMode.value.intValue = EditorGUILayout.IntPopup(m_SkyIntensityModeLabel, (int)m_IntensityMode.value.intValue, m_IntensityModes, m_IntensityModeValues);
                        }
                        else
                        {
                            m_IntensityMode.value.intValue = EditorGUILayout.IntPopup(m_SkyIntensityModeLabel, (int)m_IntensityMode.value.intValue, m_IntensityModesNoLux, m_IntensityModeValuesNoLux);
                        }
                    }
                }

                EditorGUI.indentLevel++;
                if (m_IntensityMode.value.GetEnumValue<SkyIntensityMode>() == SkyIntensityMode.Exposure)
                    PropertyField(m_SkyExposure);
                else if (m_IntensityMode.value.GetEnumValue<SkyIntensityMode>() == SkyIntensityMode.Multiplier)
                    PropertyField(m_SkyMultiplier);
                else if (m_IntensityMode.value.GetEnumValue<SkyIntensityMode>() == SkyIntensityMode.Lux)
                {
                    PropertyField(m_DesiredLuxValue);

                    // Show the multiplier
                    EditorGUILayout.HelpBox(System.String.Format("Upper hemisphere lux value: {0}\nAbsolute multiplier: {1}",
                        m_UpperHemisphereLuxValue.value.floatValue,
                        (m_DesiredLuxValue.value.floatValue / m_UpperHemisphereLuxValue.value.floatValue)
                    ), MessageType.Info);
                }

                EditorGUI.indentLevel--;
            }
            if ((m_CommonUIElementsMask & (uint)SkySettingsUIElement.Rotation) != 0)
                PropertyField(m_SkyRotation);

            if ((m_CommonUIElementsMask & (uint)SkySettingsUIElement.UpdateMode) != 0)
            {
                PropertyField(m_EnvUpdateMode);
                if (!m_EnvUpdateMode.value.hasMultipleDifferentValues && m_EnvUpdateMode.value.intValue == (int)EnvironmentUpdateMode.Realtime)
                {
                    EditorGUI.indentLevel++;
                    PropertyField(m_EnvUpdatePeriod);
                    EditorGUI.indentLevel--;
                }
            }
            if ((m_CommonUIElementsMask & (uint)SkySettingsUIElement.IncludeSunInBaking) != 0)
                PropertyField(m_IncludeSunInBaking);
        }
    }
}
