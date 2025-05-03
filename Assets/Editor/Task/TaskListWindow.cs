using System.Collections.Generic;
using Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Editor.Task {
    public class TaskListWindow : EditorWindow {

        public static List<CoreTask> tasks;

        static TaskListWindow() {
            EditorApplication.update += CheckTasks;
        }

        private static void CheckTasks() {
            if (tasks == null) return;

            foreach (var task in tasks) {
                task.CheckCompletion();
            }
        }

        public static void DrawTasks(ProjectManager.Metadata metadata) {
            if (tasks == null)
                tasks = TaskConfig.GetTaskConfig(metadata);

            if (tasks == null || tasks.Count == 0) {
                GUILayout.Label("No tasks available.");
                return;
            }

            DrawHeader();

            int completedTasks = 0;

            foreach (var task in tasks) {
                task.CheckCompletion();
                if (task.IsCompleted) completedTasks++;
                DrawTaskRow(task);
            }

            DrawFooter(completedTasks, tasks.Count);
        }

        private static void DrawHeader() {
            GUILayout.Space(4);

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUILayout.LabelField("Tasks to complete", GUILayout.MinWidth(150));
            EditorGUILayout.LabelField("Progress", GUILayout.Width(80));
            EditorGUILayout.LabelField("Done", GUILayout.Width(70));
            EditorGUILayout.LabelField("Action", GUILayout.Width(65));

            EditorGUILayout.EndHorizontal();
        }


        private static void DrawTaskRow(CoreTask task) {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(task.TaskDescription, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField($"{task.CurrentCount}/{task.RequiredCount}", GUILayout.Width(70));

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Toggle(task.IsCompleted, GUILayout.Width(40));
            EditorGUI.EndDisabledGroup();

            if (task.OptionalAction != null) {
                if (GUILayout.Button(task.OptionalActionLabel ?? "Action", GUILayout.Width(80))) {
                    task.OptionalAction.Invoke();
                }
            }
            else {
                GUILayout.Space(84);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUILayout.Space(2);
        }


        private static void DrawFooter(int completed, int total) {
            GUILayout.Space(8);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            string text = completed == total
                ? "✅ All tasks completed!"
                : $"Progress: {completed}/{total}";

            GUILayout.Label(text, new GUIStyle(EditorStyles.label) {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 14
            });

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }

    }
}