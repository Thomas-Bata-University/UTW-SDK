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
            GUILayout.Space(6);
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel) {
                alignment = TextAnchor.MiddleLeft
            };

            EditorGUILayout.LabelField("Tasks to complete", headerStyle, GUILayout.MinWidth(200));
            EditorGUILayout.LabelField("Progress", headerStyle, GUILayout.Width(120));
            EditorGUILayout.LabelField("Done", headerStyle, GUILayout.Width(100));
            EditorGUILayout.LabelField("Action", headerStyle, GUILayout.Width(85));

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawTaskRow(CoreTask task) {
            EditorGUILayout.BeginHorizontal("box");

            // Task description
            EditorGUILayout.LabelField(task.TaskDescription, GUILayout.MinWidth(200));

            // Progress
            EditorGUILayout.LabelField($"{task.CurrentCount}/{task.RequiredCount}", GUILayout.Width(120));

            // Done (disabled toggle)
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Toggle(task.IsCompleted, GUILayout.Width(100));
            EditorGUI.EndDisabledGroup();

            // Action button (or empty space)
            if (task.OptionalAction != null) {
                if (GUILayout.Button(task.OptionalActionLabel ?? "Action", GUILayout.Width(80))) {
                    task.OptionalAction.Invoke();
                }
            } else {
                GUILayout.Space(84);
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2);
        }

        private static void DrawFooter(int completed, int total) {
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