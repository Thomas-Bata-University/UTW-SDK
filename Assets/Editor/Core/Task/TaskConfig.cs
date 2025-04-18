using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Editor.Core.Task {
    public class TaskConfig {

        public static List<CoreTask> GetTaskConfig() {
            if (!OpenProjectController.IsOpenedProject) {
                Debug.LogWarning("No opened project. Cannot provide scene tasks.");
                return new List<CoreTask>();
            }

            TankPart part = (TankPart)Enum.Parse(typeof(TankPart), OpenProjectController.MetaData.tankPart);
            return GetConfig(part)
                .Where(d => d.TaskCondition != null)
                .ToList();
        }

        public static List<CoreTask> GetValidationConfig(GameObject prefab, TankPart part) {
            if (prefab == null) {
                Debug.LogError("Prefab is null, cannot validate tasks.");
                return new List<CoreTask>();
            }

            return GetConfig(part)
                .Where(d => d.ValidationCondition != null)
                .ToList();
        }

        public static List<CoreTask> GetConfig(TankPart part) {
            return part switch {
                TankPart.HULL => new List<CoreTask> {
                    CreateCoreTask(Tags.HULL_VISUAL, "hull"),
                    CreateCountedTask(Tags.PLATES, 5, "Create plates", "There must be at least 5 plate objects.")
                    //TODO can add more tasks and validations
                },
                TankPart.TURRET => new List<CoreTask> {
                    CreateCoreTask(Tags.TURRET_VISUAL, "turret")
                },
                TankPart.WEAPONRY => new List<CoreTask> {
                    CreateCoreTask(Tags.WEAPONRY_VISUAL, "weaponry"),
                },
                TankPart.SUSPENSION => new List<CoreTask> {
                    CreateCoreTask(Tags.SUSPENSION_VISUAL, "suspension"),
                },
                _ => new List<CoreTask>()
            };
        }

        #region Helpers

        private static bool HasTag(GameObject root, string tag) {
            return root.GetComponentsInChildren<Transform>(true)
                .Any(t => t.CompareTag(tag));
        }

        private static bool HasTaggedCount(GameObject root, string tag, int required) {
            int count = root.GetComponentsInChildren<Transform>(true)
                .Count(t => t.CompareTag(tag));
            return count >= required;
        }

        private static CoreTask CreateCoreTask(string tag, string partName) {
            return new CoreTask {
                TaskDescription = $"Create {partName}",
                ValidationDescription = $"There must be at least 1 {partName}.",
                RequiredCount = 1,
                TaskCondition = () => GameObject.FindWithTag(tag) != null,
                ValidationCondition = p => HasTag(p, tag)
            };
        }

        private static CoreTask CreateCountedTask(string tag, int required, string taskDescription,
            string validationDescription) {
            return new CoreTask {
                TaskDescription = taskDescription,
                ValidationDescription = validationDescription,
                RequiredCount = required,
                TaskCondition = () => GameObject.FindGameObjectsWithTag(tag).Length >= required,
                SceneObjectCounter = () => GameObject.FindGameObjectsWithTag(tag).Length,
                ValidationCondition = p => HasTaggedCount(p, tag, required)
            };
        }

        #endregion

    }
}