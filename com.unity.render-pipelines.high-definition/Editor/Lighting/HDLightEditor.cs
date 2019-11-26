using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace UnityEditor.Rendering.HighDefinition
{
    [CanEditMultipleObjects]
    [CustomEditorForRenderPipeline(typeof(Light), typeof(HDRenderPipelineAsset))]
    sealed partial class HDLightEditor : LightEditor
    {
        public SerializedHDLight m_SerializedHDLight;

        HDAdditionalLightData[] m_AdditionalLightDatas;

        HDAdditionalLightData targetAdditionalData
            => m_AdditionalLightDatas[referenceTargetIndex];
        
        protected override void OnEnable()
        {
            base.OnEnable();

            // Get & automatically add additional HD data if not present
            m_AdditionalLightDatas = CoreEditorUtils.GetAdditionalData<HDAdditionalLightData>(targets, HDAdditionalLightData.InitDefaultHDAdditionalLightData);
            m_SerializedHDLight = new SerializedHDLight(m_AdditionalLightDatas, settings);

            // Update emissive mesh and light intensity when undo/redo
            Undo.undoRedoPerformed += () =>
            {
                // Serialized object is lossing references after an undo
                if (m_SerializedHDLight.serializedObject.targetObject != null)
                {
                    m_SerializedHDLight.serializedObject.ApplyModifiedProperties();
                    foreach (var hdLightData in m_AdditionalLightDatas)
                        if (hdLightData != null)
                            hdLightData.UpdateAreaLightEmissiveMesh();
                }
            };
        }

        public override void OnInspectorGUI()
        {
            m_SerializedHDLight.Update();

            // Add space before the first collapsible area
            EditorGUILayout.Space();

            ApplyAdditionalComponentsVisibility(true);

            EditorGUI.BeginChangeCheck();
            HDLightUI.Inspector.Draw(m_SerializedHDLight, this);
            if (EditorGUI.EndChangeCheck())
            {
                m_SerializedHDLight.Apply();

                foreach (var hdLightData in m_AdditionalLightDatas)
                    hdLightData.UpdateAllLightValues();
            }

            if (m_SerializedHDLight.needUpdateAreaLightEmissiveMeshComponents)
                UpdateAreaLightEmissiveMeshComponents();
        }

        void UpdateAreaLightEmissiveMeshComponents()
        {
            foreach (var hdLightData in m_AdditionalLightDatas)
            {
                hdLightData.UpdateAreaLightEmissiveMesh();
                hdLightData.UpdateEmissiveMeshComponents();
            }

            m_SerializedHDLight.needUpdateAreaLightEmissiveMeshComponents = false;
        }

        // Internal utilities
        void ApplyAdditionalComponentsVisibility(bool hide)
        {
            // UX team decided that we should always show component in inspector.
            // However already authored scene save this settings, so force the component to be visible
            // var flags = hide ? HideFlags.HideInInspector : HideFlags.None;
            var flags = HideFlags.None;

            foreach (var t in m_SerializedHDLight.serializedObject.targetObjects)
                ((HDAdditionalLightData)t).hideFlags = flags;
        }

        protected override void OnSceneGUI()
        {
            // Each handles manipulate only one light
            // Thus do not rely on serialized properties
            HDLightType lightType = targetAdditionalData.type;

            if (lightType == HDLightType.Directional
                || lightType == HDLightType.Point
                || lightType == HDLightType.Area && targetAdditionalData.areaLightShape == AreaLightShape.Disc)
                base.OnSceneGUI();
            else
                HDLightUI.DrawHandles(targetAdditionalData, this);
        }

        internal Color legacyLightColor
        {
            get
            {
                Light light = (Light)target;
                return light.enabled ? LightEditor.kGizmoLight : LightEditor.kGizmoDisabledLight;
            }
        }
    }
}
