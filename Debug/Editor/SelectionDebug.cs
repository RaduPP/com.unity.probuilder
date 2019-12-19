using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	class SelectionDebug : ConfigurableWindow
	{
		static class Styles
		{
			static bool s_RowToggle;
			static readonly Color RowOddColor = new Color(.45f, .45f, .45f, .2f);
			static readonly Color RowEvenColor = new Color(.30f, .30f, .30f, .2f);
			public static readonly GUIStyle richTextLabel = new GUIStyle(EditorStyles.label) { richText = true };

			public static void BeginRow(int index = -1)
			{
				if (index > -1)
					s_RowToggle = index % 2 == 0;

				var bg = GUI.backgroundColor;
				GUI.backgroundColor = s_RowToggle ? RowEvenColor : RowOddColor;
				GUILayout.BeginHorizontal(UI.EditorStyles.rowStyle);
				s_RowToggle = !s_RowToggle;
				GUI.backgroundColor = bg;
			}

			public static void EndRow()
			{
				GUILayout.EndHorizontal();
			}
		}

		[MenuItem("Tools/ProBuilder/Debug/Selection Editor")]
		static void Init()
		{
			GetWindow<SelectionDebug>();
		}

		static bool s_DisplayElementGroups = true;
		static bool s_DisplayVertexSelection = true;

		void OnEnable()
		{
			MeshSelection.objectSelectionChanged += Repaint;
			ProBuilderMesh.elementSelectionChanged += Repaint;
		}

		void OnDisable()
		{
			MeshSelection.objectSelectionChanged -= Repaint;
			ProBuilderMesh.elementSelectionChanged -= Repaint;
		}

		void Repaint(ProBuilderMesh mesh)
		{
			Repaint();
		}

        void OnHierarchyChange()
        {
            UnityEngine.Debug.Log("Window OnHierarchyChange");
        }

        void OnGUI()
		{
			s_DisplayElementGroups = EditorGUILayout.Foldout(s_DisplayElementGroups, "Element Groups");

			if (s_DisplayElementGroups)
			{
				foreach (var group in MeshSelection.elementSelection)
				{
					GUILayout.Label(group.mesh.name);

					foreach (var element in group.elementGroups)
					{
						Styles.BeginRow();
						GUILayout.Label(element.indices.ToString(","));
						Styles.EndRow();
					}
				}
			}

            s_DisplayVertexSelection = EditorGUILayout.Foldout(s_DisplayVertexSelection, "Vertex Selection");

            if (s_DisplayVertexSelection)
            {
                var sb = new System.Text.StringBuilder();

                foreach (var mesh in MeshSelection.top)
                {
                    sb.AppendLine($"<b>{mesh.name}</b>");
                    sb.AppendLine(mesh.selectedVertices.ToString(","));
                }

                GUILayout.Label(sb.ToString(), Styles.richTextLabel);
            }
        }
	}
}
