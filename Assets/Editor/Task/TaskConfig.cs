using System;
using System.Collections.Generic;
using System.Linq;
using Editor.Const;
using Editor.Core;
using Editor.Enums;
using Editor.Helper;
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
                ColorLogger.LogError("Prefab is null, cannot validate tasks.");
                return new List<CoreTask>();
            }

            return GetConfig(part)
                .Where(d => d.ValidationCondition != null)
                .ToList();
        }

        private static List<CoreTask> GetConfig(TankPart part) {
            return part switch {
                TankPart.HULL => new List<CoreTask> {
                    CreateCoreTask(Tags.HULL_VISUAL, "Hull"),
                    CreatePlateTask(Tags.PLATES, 5, "Create plates", "There must be at least 5 plate objects."),
                    CreateMountPointTask(Tags.TURRET_MOUNT_POINT, "turret"),
                    CreateModuleTask(Tags.ENGINE_MODULE, Tags.MODULE_PARENT, "engine", "EngineModule", "module"),
                    CreateModuleTask(Tags.DRIVER_MODULE, Tags.MODULE_PARENT, "driver", "DriverModule", "module")
                },
                TankPart.TURRET => new List<CoreTask> {
                    CreateCoreTask(Tags.TURRET_VISUAL, "Turret"),
                    CreatePlateTask(Tags.PLATES, 5, "Create plates", "There must be at least 5 plate objects."),
                    CreateMountPointTask(Tags.CANNON_MOUNT_POINT, "cannon"),
                    CreateMountPointTask(Tags.HULL_MOUNT_POINT, "hull"),
                    CreateModuleTask(Tags.INTERNAL_MODULE, Tags.MODULE_PARENT, "crew", "InternalModule", "module")
                },
                TankPart.WEAPONRY => new List<CoreTask> {
                    CreateCoreTask(Tags.WEAPONRY_VISUAL, "Cannon"),
                    CreateCoreTask(Tags.BARREL_VISUAL, "Barrel"),
                    CreateCoreTask(Tags.MANTLET_VISUAL, "Mantlet"),
                    CreateMountPointTask(Tags.TURRET_MOUNT_POINT, "turret"),
                },
                TankPart.SUSPENSION => new List<CoreTask> {
                    CreateCoreTask(Tags.SUSPENSION_VISUAL, "Suspension"),
                    CreateCoreTask(Tags.SPROCKET_VISUAL, "Sprocket"),
                    CreateCoreTask(Tags.IDLER_VISUAL, "Idler"),
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
                    PrefabCreator.CreatePrefab(
                        AssetPaths.TEMPLATE + "/PlateTemplate/Prefabs/Plate.prefab",
                        "armor_plate", tag, Tags.PLATE_PARENT);
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
                    PrefabCreator.CreatePrefab(
                        AssetPaths.TEMPLATE + "/MountPoint/MountPoint.prefab",
                        mountPointName + "_MountPoint", tag, Tags.MOUNT_POINT_PARENT);
                },
                OptionalActionLabel = "Create"
            };
        }

        private static CoreTask CreateModuleTask(string tag, string parentTag, string objectName, string prefabSubPath,
            string prefabSuffix) {
            return new CoreTask {
                TaskDescription = $"Create {objectName} {prefabSuffix.Replace("_", " ")}",
                ValidationDescription = $"There must be just 1 {objectName} {prefabSuffix.Replace("_", "")}.",
                RequiredCount = 1,
                TaskCondition = () => GameObject.FindGameObjectsWithTag(tag).Length == 1,
                SceneObjectCounter = () => GameObject.FindGameObjectsWithTag(tag).Length,
                ValidationCondition = p => HasTaggedCountEq(p, tag, 1),

                OptionalAction = () => {
                    PrefabCreator.CreatePrefab(
                        AssetPaths.TEMPLATE + $"/Modules/{prefabSubPath}/{prefabSubPath}.prefab",
                        $"{objectName}_{prefabSuffix}", tag, parentTag);
                },
                OptionalActionLabel = "Create"
            };
        }

        #endregion

    }
}