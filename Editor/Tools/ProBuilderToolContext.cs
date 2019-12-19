using System;
using UnityEditor.EditorTools;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    abstract class ProBuilderToolContext : EditorToolContext
    {
        SelectionTool m_SelectionTool;

        protected abstract SelectMode selectMode { get; }

        public void OnEnable()
        {
            m_SelectionTool = new SelectionTool(selectMode);
            MeshSelection.objectSelectionChanged += UpdateMeshGizmos;
            ProBuilderMesh.elementSelectionChanged += UpdateMeshGizmos;
            UpdateMeshGizmos();
        }

        void OnDisable()
        {
            MeshSelection.objectSelectionChanged -= UpdateMeshGizmos;
            ProBuilderMesh.elementSelectionChanged -= UpdateMeshGizmos;
        }

        void UpdateMeshGizmos(ProBuilderMesh mesh)
        {
            UpdateMeshGizmos();
        }

        void UpdateMeshGizmos()
        {
            EditorMeshHandles.RebuildSelectedHandles(MeshSelection.topInternal, selectMode);
        }

        public override Type GetOverride(Tool tool)
        {
            if(tool == Tool.Move)
                return typeof(ProBuilderMoveTool);
            if(tool == Tool.Rotate)
                return typeof(ProBuilderRotateTool);
            if(tool == Tool.Scale)
                return typeof(ProBuilderScaleTool);
            return null;
        }

        public override void OnToolGUI(EditorWindow window)
        {
            m_SelectionTool.OnGUI();
            EditorMeshHandles.DrawSceneHandles(selectMode);
        }
    }

    [ToolbarIcon("Packages/com.unity.probuilder/Content/Icons/Modes/Mode_Vertex.png")]
    class VertexToolContext : ProBuilderToolContext
    {
        protected override SelectMode selectMode { get { return SelectMode.Vertex; } }
    }

    [ToolbarIcon("Packages/com.unity.probuilder/Content/Icons/Modes/Mode_Face.png")]
    class FaceToolContext : ProBuilderToolContext
    {
        protected override SelectMode selectMode { get { return SelectMode.Face; } }
    }

    [ToolbarIcon("Packages/com.unity.probuilder/Content/Icons/Modes/Mode_Edge.png")]
    class EdgeToolContext : ProBuilderToolContext
    {
        protected override SelectMode selectMode { get { return SelectMode.Edge; } }
    }
}
