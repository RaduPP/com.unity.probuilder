using System;
using UnityEditor.EditorTools;
using UnityEngine;

namespace UnityEditor.ProBuilder
{
    abstract class ProBuilderOverrideTool<T> : EditorTool where T : VertexManipulationTool, new()
    {
        T m_Tool;

        void OnEnable()
        {
            m_Tool = new T();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            var evt = Event.current;
            m_Tool.OnSceneGUI(evt);
        }
    }

    class ProBuilderMoveTool : ProBuilderOverrideTool<PositionMoveTool> { }
    class ProBuilderRotateTool : ProBuilderOverrideTool<PositionRotateTool> { }
    class ProBuilderScaleTool : ProBuilderOverrideTool<PositionScaleTool> { }
}
