using System;
using UnityEditor.EditorTools;
using UnityEngine;

namespace UnityEditor.ProBuilder
{
    [ToolbarIcon("Packages/com.unity.probuilder/Content/Icons/Modes/Mode_Vertex.png")]
    public class VertexEditMode : EditorToolContext
    {
        SelectionTool m_SelectionTool;

        public void OnEnable()
        {
            m_SelectionTool = new SelectionTool();
        }

        public override Type GetOverride(Tool tool)
        {
            switch (tool)
            {
                case Tool.Move:
                    return typeof(VertexMoveTool);
                default:
                    return null;
            }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            m_SelectionTool.OnGUI();
        }
    }

    class VertexMoveTool : EditorTool
    {
        VertexManipulationTool m_Tool;

        void OnEnable()
        {
            m_Tool = new PositionMoveTool();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            base.OnToolGUI(window);
            var evt = Event.current;
            m_Tool.OnSceneGUI(evt);
        }
    }
}
