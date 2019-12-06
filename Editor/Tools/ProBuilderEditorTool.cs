using System;
using UnityEditor.EditorTools;

namespace UnityEditor.ProBuilder
{
    abstract class ProBuilderEditorTool : EditorTool
    {
        SelectionTool m_SelectionTool;

        void OnEnable()
        {
            m_SelectionTool = new SelectionTool();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            m_SelectionTool.OnGUI();
        }
    }
}
