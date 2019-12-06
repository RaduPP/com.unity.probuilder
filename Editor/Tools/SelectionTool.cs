using UnityEditor.SettingsManagement;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    class SelectionTool
    {
        static class Styles
        {
            public static GUIStyle selectionRect = new GUIStyle()
            {
                normal = new GUIStyleState()
                {
                    background = IconUtility.GetIcon("Scene/SelectionRect")
                },
                border = new RectOffset(1, 1, 1, 1),
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0)
            };
        }

        const float k_PickingDistance = 40f;
        // Match the value set in RectSelection.cs
        const float k_MouseDragThreshold = 6f;
        //Off pointer multiplier is a percentage of the picking distance
        const float k_OffPointerMultiplierPercent = 0.1f;

        static Pref<bool> s_BackfaceSelectEnabled = new Pref<bool>("editor.backFaceSelectEnabled", false);
        static Pref<RectSelectMode> s_DragSelectRectMode = new Pref<RectSelectMode>("editor.dragSelectRectMode", RectSelectMode.Partial);
        static Pref<SelectionModifierBehavior> s_SelectModifierBehavior = new Pref<SelectionModifierBehavior>("editor.rectSelectModifier", SelectionModifierBehavior.Difference);

        [UserSetting("Graphics", "Show Hover Highlight", "Highlight the mesh element nearest to the mouse cursor.")]
        static Pref<bool> s_ShowHoverHighlight = new Pref<bool>("editor.showPreselectionHighlight", true, SettingsScope.User);

        int m_SelectionControlId;
        SceneSelection m_Hovering = new SceneSelection();
        SceneSelection m_HoveringPrevious = new SceneSelection();
        ScenePickerPreferences m_ScenePickerPreferences;

        Vector2 m_InitialMousePosition;
        bool m_IsReadyForMouseDrag;
        bool m_IsDragging;
        bool m_WasDoubleClick;
        Rect m_MouseDragRect;

        static SelectMode selectMode { get { return ProBuilderEditor.selectMode; } }

        public SelectionTool()
        {
            m_ScenePickerPreferences = new ScenePickerPreferences()
            {
                offPointerMultiplier = k_PickingDistance * k_OffPointerMultiplierPercent,
                maxPointerDistance = k_PickingDistance,
                cullMode = s_BackfaceSelectEnabled ? CullingMode.None : CullingMode.Back,
                selectionModifierBehavior = s_SelectModifierBehavior,
                rectSelectMode = s_DragSelectRectMode
            };
        }

        public void OnGUI()
        {
            var evt = Event.current;

            if (EditorHandleUtility.SceneViewInUse(evt) || evt.isKey)
            {
                CancelInteraction();
                return;
            }

            m_SelectionControlId = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(m_SelectionControlId);

            switch (evt.type)
            {
                case EventType.Layout:
                    break;
                case EventType.Repaint:
                    DrawHandles(evt);
                    break;
                case EventType.MouseMove:
                    HandleSelectionPreview(evt);
                    break;
                case EventType.MouseDrag:
                    HandleMouseDrag(evt);
                    break;
                case EventType.MouseDown:
                    HandleMouseDown(evt);
                    break;
                case EventType.MouseUp:
                    HandleMouseUp(evt);
                    break;
                case EventType.Ignore:
                    CancelInteraction();
                    break;
            }
        }

        void HandleMouseDown(Event evt)
        {
            // double clicking object
            if (evt.clickCount > 1)
                HandleDoubleClick(evt);

            m_InitialMousePosition = evt.mousePosition;

            // readyForMouseDrag prevents a bug wherein after ending a drag an errant
            // MouseDrag event is sent with no corresponding MouseDown/MouseUp event.
            m_IsReadyForMouseDrag = true;
        }

        void HandleMouseUp(Event evt)
        {
            if (m_WasDoubleClick)
            {
                m_WasDoubleClick = false;
            }
            else
            {
                if (!m_IsDragging)
                {
                    if (UVEditor.instance)
                        UVEditor.instance.ResetUserPivot();

                    EditorSceneViewPicker.DoMouseClick(evt, selectMode, m_ScenePickerPreferences);
                    ProBuilderEditor.Refresh();
                }
                else
                {
                    m_IsDragging = false;
                    m_IsReadyForMouseDrag = false;

                    if (UVEditor.instance)
                        UVEditor.instance.ResetUserPivot();

                    EditorSceneViewPicker.DoMouseDrag(m_MouseDragRect, selectMode, m_ScenePickerPreferences);
                }
            }
        }

        void HandleDoubleClick(Event evt)
        {
            var mesh = EditorSceneViewPicker.DoMouseClick(evt, selectMode, m_ScenePickerPreferences);

            if (mesh != null)
            {
                if (selectMode.ContainsFlag(SelectMode.Edge | SelectMode.TextureEdge))
                {
                    if (evt.shift)
                        EditorUtility.ShowNotification(EditorToolbarLoader.GetInstance<Actions.SelectEdgeRing>().DoAction());
                    else
                        EditorUtility.ShowNotification(EditorToolbarLoader.GetInstance<Actions.SelectEdgeLoop>().DoAction());
                }
                else if (selectMode.ContainsFlag(SelectMode.Face | SelectMode.TextureFace))
                {
                    if ((evt.modifiers & (EventModifiers.Control | EventModifiers.Shift)) ==
                        (EventModifiers.Control | EventModifiers.Shift))
                        Actions.SelectFaceRing.MenuRingAndLoopFaces(MeshSelection.topInternal);
                    else if (evt.control)
                        EditorUtility.ShowNotification(EditorToolbarLoader.GetInstance<Actions.SelectFaceRing>().DoAction());
                    else if (evt.shift)
                        EditorUtility.ShowNotification(EditorToolbarLoader.GetInstance<Actions.SelectFaceLoop>().DoAction());
                    else
                        mesh.SetSelectedFaces(mesh.facesInternal);
                }
                else
                {
                    mesh.SetSelectedFaces(mesh.facesInternal);
                }

                ProBuilderEditor.Refresh();
                m_WasDoubleClick = true;
            }
        }

        void CancelInteraction()
        {
            if (m_IsDragging)
            {
                m_IsReadyForMouseDrag = false;
                m_IsDragging = false;
                EditorSceneViewPicker.DoMouseDrag(m_MouseDragRect, selectMode, m_ScenePickerPreferences);
            }

            if (m_WasDoubleClick)
                m_WasDoubleClick = false;
        }

        void HandleMouseDrag(Event evt)
        {
            if(m_IsReadyForMouseDrag)
            {
                if (!m_IsDragging && Vector2.Distance(evt.mousePosition, m_InitialMousePosition) > k_MouseDragThreshold)
                {
                    m_IsDragging = true;
                    SceneView.RepaintAll();
                }
            }
        }

        void HandleSelectionPreview(Event evt)
        {
            // Check mouse position in scene and determine if we should highlight something
            if (s_ShowHoverHighlight
                && evt.type == EventType.MouseMove)
//                && selectMode.IsMeshElementMode())
            {
                m_Hovering.CopyTo(m_HoveringPrevious);

                if (GUIUtility.hotControl == 0)
                    EditorSceneViewPicker.MouseRayHitTest(evt.mousePosition, selectMode, m_ScenePickerPreferences, m_Hovering);
                else
                    m_Hovering.Clear();

                if (!m_Hovering.Equals(m_HoveringPrevious))
                    SceneView.RepaintAll();
            }
        }

        void DrawHandles(Event evt)
        {
            if (!SceneDragAndDropListener.isDragging
                && m_Hovering != null
                && GUIUtility.hotControl == 0
                && HandleUtility.nearestControl == m_SelectionControlId)
            {
                try
                {
                    EditorMeshHandles.DrawSceneSelection(m_Hovering);
                }
                catch
                {
                    // this happens on undo, when c++ object is destroyed but c# side thinks it's still alive
                }
            }

            if (m_IsDragging)
            {
                // Always draw from lowest to largest values
                var start = Vector2.Min(m_InitialMousePosition, evt.mousePosition);
                var end = Vector2.Max(m_InitialMousePosition, evt.mousePosition);

                m_MouseDragRect = new Rect(start.x, start.y, end.x - start.x, end.y - start.y);

                Styles.selectionRect.Draw(m_MouseDragRect, false, false, false, false);
            }
        }
    }
}
