using System;
using System.Collections.Generic;
using System.Linq;
using Editor.Const;
using Editor.Core;
using Editor.Enums;
using UnityEngine;

namespace Editor.Task {
    public class TaskConfig {

        public static List<CoreTask> GetTaskConfig(ProjectManager.Metadata metadata) {
            TankPart part = (TankPart)Enum.Parse(typeof(TankPart), metadata.tankPart);
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
                    CreateCoreTask(Tags.HULL_VISUAL, "Hull"),
                    CreatePlateTask(Tags.PLATES, 5, "Create plates", "There must be at least 5 plate objects."),
                    CreateMountPointTask(Tags.TURRET_MOUNT_POINT, "turret"),
                    CreateInternalModuleTask(Tags.INTERNAL_MODULE, "crew")
                },
                TankPart.TURRET => new List<CoreTask> {
                    CreateCoreTask(Tags.TURRET_VISUAL, "Turret"),
                    CreatePlateTask(Tags.PLATES, 5, "Create plates", "There must be at least 5 plate objects."),
                    CreateMountPointTask(Tags.CANNON_MOUNT_POINT, "cannon"),
                    CreateMountPointTask(Tags.HULL_MOUNT_POINT, "hull"),
                    CreateInternalModuleTask(Tags.INTERNAL_MODULE, "crew")
                },
                TankPart.WEAPONRY => new List<CoreTask> {
                    CreateCoreTask(Tags.WEAPONRY_VISUAL, "Cannon"),
                    CreateCoreTask(Tags.TURRET_MOUNT_POINT, "Barrel"),
                    CreateMountPointTask(Tags.TURRET_MOUNT_POINT, "turret"),
                },
                TankPart.SUSPENSION => new List<CoreTask> {
                    CreateCoreTask(Tags.SUSPENSION_VISUAL, "Suspension"),
                },
                _ => new List<CoreTask>()
            };
        }

        #region Helpers

        private static bool HasTag(GameObject root, string tag) {
            return root.GetComponentsInChildren<Transform>(true)
                .Any(t => t.CompareTag(tag));
        }

        private static bool HasTaggedCountGt(GameObject root, string tag, int required) {
            int count = root.GetComponentsInChildren<Transform>(true)
                .Count(t => t.CompareTag(tag));
            return count >= required;
        }

        private static bool HasTaggedCountEq(GameObject root, string tag, int required) {
            int count = root.GetComponentsInChildren<Transform>(true)
                .Count(t => t.CompareTag(tag));
            return count == required;
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

        private static CoreTask CreatePlateTask(string tag, int required, string taskDescription,
            string validationDescription) {
            return new CoreTask {
                TaskDescription = taskDescription,
                ValidationDescription = validationDescription,
                RequiredCount = required,
                TaskCondition = () => GameObject.FindGameObjectsWithTag(tag).Length >= required,
                SceneObjectCounter = () => GameObject.FindGameObjectsWithTag(tag).Length,
                ValidationCondition = p => HasTaggedCountGt(p, tag, required),
                
                OptionalAction = () => {
                    PrefabCreator.CreatePlate(
                        AssetPaths.TEMPLATE + "/PlateTemplate/Prefabs/Plate.prefab", 
                        "armor_plate", tag);
                },
                OptionalActionLabel = "Create"
            };
        }

        private static CoreTask CreateMountPointTask(string tag, string mountPointName) {
            return new CoreTask {
                TaskDescription = $"Create {mountPointName} Mount Point",
                ValidationDescription = $"There must be just 1 {mountPointName} Mount Point.",
                RequiredCount = 1,
                TaskCondition = () => GameObject.FindGameObjectsWithTag(tag).Length == 1,
                SceneObjectCounter = () => GameObject.FindGameObjectsWithTag(tag).Length,
                ValidationCondition = p => HasTaggedCountEq(p, tag, 1),

                OptionalAction = () => {
                    PrefabCreator.CreateMountPoint(
                        AssetPaths.TEMPLATE + "/MountPoint/MountPoint.prefab",
                        mountPointName + "_MountPoint", tag);
                },
                OptionalActionLabel = "Create"
            };
        }
        
        private static CoreTask CreateInternalModuleTask(string tag, string internalModuleName) {
            return new CoreTask {
                TaskDescription = $"Create {internalModuleName} Internal Module",
                ValidationDescription = $"There must be just 1 {internalModuleName} InternalModule.",
                RequiredCount = 1,
                TaskCondition = () => GameObject.FindGameObjectsWithTag(tag).Length == 1,
                SceneObjectCounter = () => GameObject.FindGameObjectsWithTag(tag).Length,
                ValidationCondition = p => HasTaggedCountEq(p, tag, 1),

                OptionalAction = () => {
                    PrefabCreator.CreateMountPoint(
                        AssetPaths.TEMPLATE + "/InternalModule/InternalModule.prefab",
                        internalModuleName + "_InternalModule", tag);
                },
                OptionalActionLabel = "Create"
            };
        }

        #endregion

    }
}