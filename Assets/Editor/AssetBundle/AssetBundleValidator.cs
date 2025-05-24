using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Editor.Const;
using Editor.Core;
using Editor.Enums;
using Editor.Helper;
using Editor.Task;
using UnityEditor;
using UnityEngine;

namespace Editor.AssetBundle {
    public static class AssetBundleValidator {

        /// <summary>
        /// Runs full validation including task and mandatory checks.
        /// </summary>
        public static bool Validate() {
            List<string> validationErrors = new List<string>();
            
            if (!ValidateTasks(out var taskIssues, out var projectName)) {
                validationErrors.AddRange(taskIssues);
                ColorLogger.LogFormatted("Task validation {0} for project {1}:\n{2}",
                    new[] { "failed", projectName, string.Join("\n", validationErrors.Select(e => " - " + e)) },
                    new[] { "red", null, null },
                    new[] { true, false, false },
                    ColorLogger.LogLevel.Error
                );

                return false;
            }

            if (!ValidateMandatory(out var mandatoryIssues, out projectName)) {
                validationErrors.AddRange(mandatoryIssues);
                ColorLogger.LogFormatted("Mandatory component validation {0} for project {1}:\n{2}",
                    new[] { "failed", projectName, string.Join("\n", validationErrors.Select(e => " - " + e)) },
                    new[] { "red", null, null },
                    new[] { true, false, false },
                    ColorLogger.LogLevel.Error
                );

                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates all defined tasks on all prefabs in all projects.
        /// </summary>
        private static bool ValidateTasks(out List<string> errors, out string projectName) {
            errors = new List<string>();
            projectName = null;

            var originalMetadata = OpenProjectController.MetaData;
            bool hadOpenedProject = OpenProjectController.IsOpenedProject;

            string[] allFolders = new[]
                { AssetPaths.HULL, AssetPaths.TURRET, AssetPaths.WEAPONRY, AssetPaths.SUSPENSION };

            foreach (var rootFolder in allFolders) {
                if (!Directory.Exists(rootFolder)) continue;

                var subFolders = Directory.GetDirectories(rootFolder);
                foreach (var folder in subFolders) {
                    string metadataPath = folder + AssetPaths.METADATA;
                    if (!File.Exists(metadataPath)) continue;

                    var metadata = ProjectManager.GetMetadata(metadataPath);
                    if (metadata == null) continue;

                    string path = metadata.prefabPath;
                    projectName = metadata.projectName;
                    if (!path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) {
                        path += ".prefab";
                    }

                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab == null) {
                        errors.Add($"Prefab not found at {metadata.prefabPath}");
                        return false;
                    }
                    
                    var importer = AssetImporter.GetAtPath(path);
                    if (string.IsNullOrEmpty(importer.assetBundleName) || importer.assetBundleName.ToLower() == "none") {
                        return true;
                    }

                    if (!Enum.TryParse(metadata.tankPart, out TankPart part)) {
                        errors.Add($"Invalid tank part: '{metadata.tankPart}'");
                        return false;
                    }

                    var validationConfig = TaskConfig.GetValidationConfig(prefab, part);
                    foreach (var validation in validationConfig) {
                        validation.CheckCompletion(prefab);
                        if (!validation.IsCompleted) {
                            errors.Add(validation.ValidationDescription);
                            return false;
                        }
                    }
                }
            }

            // Restore original project state
            OpenProjectController.MetaData = originalMetadata;
            OpenProjectController.IsOpenedProject = hadOpenedProject;

            return true;
        }

        /// <summary>
        /// Placeholder for validating that required components exist on prefab.
        /// </summary>
        private static bool ValidateMandatory(out List<string> issues, out string projectName) {
            issues = new List<string>();
            projectName = "TODO";

            // TODO: Validate presence of Rigidbody, Collider, etc.

            return true;
        }

    }
}