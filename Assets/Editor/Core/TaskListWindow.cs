using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor.Core {
    public class TaskListWindow : EditorWindow {

        #region Tasks

        private readonly List<Task> _hullTasks = new() {
            new Task("Create hull", () => GameObject.FindWithTag(Tags.HULL_VISUAL) != null),
            new Task("Create plates", Tags.PLATES, 5),
        };

        private readonly List<Task> _turretTasks = new() {
            new Task("Create turret", () => GameObject.FindWithTag(Tags.TURRET_VISUAL) != null),
        };

        private readonly List<Task> _weaponryTasks = new() {
            new Task("Create weaponry", () => GameObject.FindWithTag(Tags.WEAPONRY_VISUAL) != null),
        };

        private readonly List<Task> _suspensionTasks = new() {
            new Task("Create suspension", () => GameObject.FindWithTag(Tags.SUSPENSION_VISUAL) != null),
        };

        #endregion

        private static List<Task> _tasks;

        static TaskListWindow() {
            EditorApplication.update += CheckTasks;
        }

        private static void CheckTasks() {
            if (_tasks == null) return;

            foreach (var task in _tasks)
            {
                task.CheckCompletion();
            }

            // GetWindow<TaskListWindow>().Repaint();
        }

        [MenuItem("UTW/Task list")]
        public static void ShowWindow() {
            GetWindow<TaskListWindow>("To-Do");
        }

        private void OnGUI() {
            if (!OpenProjectController.IsOpenedProject) return;

            EditorGUILayout.Space();

            var tasks = GetTasks();
            _tasks = tasks;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("State", EditorStyles.boldLabel, GUILayout.Width(50));
            EditorGUILayout.LabelField("Result", EditorStyles.boldLabel, GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            int completedTasks = 0;

            foreach (var task in tasks) {
                task.CheckCompletion();

                if (task.IsCompleted) completedTasks++;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(task.Description);

                EditorGUILayout.LabelField($"{task.CurrentCount}/{task.RequiredCount}", GUILayout.Width(50));

                EditorGUILayout.Toggle(task.IsCompleted, GUILayout.Width(70));
                EditorGUILayout.EndHorizontal();
            }

            CreateFooterInfo(completedTasks, tasks.Count);
        }

        private void CreateFooterInfo(int completedTasks, int totalCount) {
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            string text = $"Completed {completedTasks}/{totalCount}";

            if (completedTasks == totalCount) {
                text = "All tasks completed";
            }

            GUILayout.Label(text, new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 14
            });

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }

        private List<Task> GetTasks() {
            TankPart part = (TankPart)Enum.Parse(typeof(TankPart), OpenProjectController.MetaData.tankPart);

            switch (part) {
                case TankPart.HULL:
                    return _hullTasks;
                case TankPart.TURRET:
                    return _turretTasks;
                case TankPart.WEAPONRY:
                    return _weaponryTasks;
                case TankPart.SUSPENSION:
                    return _suspensionTasks;
                default:
                    return new List<Task>();
            }
        }

        /// <summary>
        /// Represents a task in the to-do list within the Unity editor. Each task has a description, a completion state,
        /// and a specific condition that must be met to consider it complete.
        /// </summary>
        private class Task
        {
            public string Description { get; }
            public bool IsCompleted { get; private set; }
            public int CurrentCount { get; private set; }
            public int RequiredCount { get; }

            private readonly string tag;
            private readonly Func<bool> completionCondition;


            /// <summary>
            /// Constructor for tasks that require a specific number of GameObjects with a specified tag.
            /// </summary>
            /// <param name="description">A description of the task.</param>
            /// <param name="tag">The required tag for GameObjects.</param>
            /// <param name="requiredCount">The number of tagged GameObjects needed to complete the task.</param>
            public Task(string description, string tag, int requiredCount)
            {
                Description = description;
                this.tag = tag;
                RequiredCount = requiredCount;
                completionCondition = null;
            }

            /// <summary>
            /// Constructor for tasks that rely on a custom completion condition defined by a function.
            /// </summary>
            /// <param name="description">A description of the task.</param>
            /// <param name="completionCondition">A function that returns true if the task is complete.</param>
            /// <param name="requiredCount">The count required for completion (default is 1).</param>
            public Task(string description, Func<bool> completionCondition, int requiredCount = 1)
            {
                Description = description;
                this.completionCondition = completionCondition;
                RequiredCount = requiredCount;
            }

            public void CheckCompletion()
            {
                if (completionCondition != null)
                {
                    if (!IsCompleted && completionCondition())
                    {
                        CurrentCount = RequiredCount;
                        IsCompleted = true;
                    }
                }
                else if (tag != null)
                {
                    CurrentCount = GameObject.FindGameObjectsWithTag(tag).Length;
                    IsCompleted = CurrentCount >= RequiredCount;
                }
            }
        }

    }
} //END