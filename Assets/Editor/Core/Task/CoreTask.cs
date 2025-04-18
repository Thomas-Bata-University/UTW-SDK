using System;
using UnityEngine;

namespace Editor.Core.Task {
    /// <summary>
    /// Represents a validation task for either scene or prefab context.
    /// </summary>
    public class CoreTask {

        /// <summary>
        /// Description used in the UI (e.g., task list).
        /// </summary>
        public string TaskDescription { get; set; }

        /// <summary>
        /// Description used during prefab validation (e.g., in build logs).
        /// </summary>
        public string ValidationDescription { get; set; }

        /// <summary>
        /// Indicates whether the task has been completed.
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Current result (e.g., found tag count).
        /// </summary>
        public int CurrentCount { get; set; }

        /// <summary>
        /// Minimum required count to consider task complete.
        /// </summary>
        public int RequiredCount { get; set; }

        public Func<bool> TaskCondition;
        public Func<int> SceneObjectCounter;
        public Func<GameObject, bool> ValidationCondition;

        /// <summary>
        /// Checks task completion in scene context.
        /// </summary>
        public void CheckCompletion() {
            if (SceneObjectCounter != null) {
                CurrentCount = SceneObjectCounter.Invoke();
                IsCompleted = CurrentCount >= RequiredCount;
            }
            else if (TaskCondition != null) {
                IsCompleted = TaskCondition.Invoke();
                CurrentCount = IsCompleted ? RequiredCount : 0;
            }
            else {
                IsCompleted = false;
                CurrentCount = 0;
            }
        }


        /// <summary>
        /// Checks task completion in prefab context.
        /// </summary>
        public void CheckCompletion(GameObject prefabRoot) {
            if (ValidationCondition != null && prefabRoot != null && ValidationCondition.Invoke(prefabRoot)) {
                CurrentCount = RequiredCount;
                IsCompleted = true;
            }
            else {
                CurrentCount = 0;
                IsCompleted = false;
            }
        }

    }
}