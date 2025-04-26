using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor.Core.Task {
    public class TaskListWindow : EditorWindow {

        private static List<CoreTask> _tasks;

        static TaskListWindow() {
            EditorApplication.update += CheckTasks;
        }

        private static void CheckTasks() {
            if (_tasks == null) return;

            foreach (var task in _tasks) {
                task.CheckCompletion();
            }
        }

        public static void DrawTasks() {
            if (_tasks == null)
                _tasks = TaskConfig.GetTaskConfig();

            if (_tasks == null || _tasks.Count == 0) {
                GUILayout.Label("No tasks available.");
                return;
            }

            DrawHeader();

            int completedTasks = 0;

            foreach (var task in _tasks) {
                task.CheckCompletion();
                if (task.IsCompleted) completedTasks++;
                DrawTaskRow(task);
            }

            DrawFooter(completedTasks, _tasks.Count);
        }

        private static void DrawHeader() {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("State", EditorStyles.boldLabel, GUILayout.Width(50));
            EditorGUILayout.LabelField("Result", EditorStyles.boldLabel, GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private static void DrawTaskRow(CoreTask task) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(task.TaskDescription);

            EditorGUILayout.LabelField($"{task.CurrentCount}/{task.RequiredCount}", GUILayout.Width(50));
            EditorGUILayout.Toggle(task.IsCompleted, GUILayout.Width(50));

            if (task.OptionalAction != null) {
                if (GUILayout.Button(task.OptionalActionLabel ?? "Action", GUILayout.Width(80))) {
                    task.OptionalAction.Invoke();
                }
            }

            EditorGUILayout.EndHorizontal();
        }


        private static void DrawFooter(int completed, int total) {
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            string text = completed == total
                ? "All tasks completed"
                : $"Completed {completed}/{total}";

            GUILayout.Label(text, new GUIStyle(GUI.skin.label) {
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