using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

using TowerBreak.GameData.TowerBreaker;

namespace TowerBreak.Combat.Editor
{
    [CustomEditor(typeof(EnemyController))]
    public class EnemyControllerEditor : UnityEditor.Editor
    {
        private EnemyController controller;
        private EnemyRow testData;
        private bool showTestControls = true;
        private bool showAnimationControls = true;
        private bool showBodyParts = true;
        private bool isInitialized = false;

        // Flash effect timer for editor
        private bool isEditorFlashing = false;
        private double editorFlashStartTime;
        private const double EDITOR_FLASH_DURATION = 0.2;
        private Dictionary<SpriteRenderer, Color> editorOriginalColors = new Dictionary<SpriteRenderer, Color>();

        private void OnEnable()
        {
            controller = (EnemyController)target;
            CreateTestData();
            CheckInitialization();
        }

        private void CreateTestData()
        {
            testData = new EnemyRow
            {
                Id = 999,
                Archetype = EnemyArchetype.BasicMelee,
                Health = 100,
                Pressure = 10f,
                MoveSpeed = 3f,
                AttackCadence = 1f,
                IsArmored = false,
                PushBackDistance = 2f,
                IsBoss = false
            };
        }

        private void CheckInitialization()
        {
            // Check if controller is initialized
            isInitialized = controller.IsInitialized();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("=== Test Controls ===", EditorStyles.boldLabel);

            // Test Data Setup
            EditorGUILayout.LabelField("Test Data Configuration", EditorStyles.boldLabel);
            testData.Health = EditorGUILayout.IntField("Test Health", testData.Health);
            testData.MoveSpeed = EditorGUILayout.FloatField("Test Move Speed", testData.MoveSpeed);
            testData.PushBackDistance = EditorGUILayout.FloatField("Push Back Distance", testData.PushBackDistance);
            testData.Pressure = EditorGUILayout.FloatField("Pressure", testData.Pressure);
            testData.Archetype = (EnemyArchetype)EditorGUILayout.EnumPopup("Archetype", testData.Archetype);

            EditorGUILayout.Space(10);

            // Initialize Button
            if (GUILayout.Button("Initialize with Test Data", GUILayout.Height(30)))
            {
                InitializeWithTestData();
            }

            EditorGUILayout.Space(10);

            // Test Actions
            showTestControls = EditorGUILayout.Foldout(showTestControls, "Test Actions", true);
            if (showTestControls)
            {
                EditorGUI.indentLevel++;

                if (GUILayout.Button("Test Take Damage (10)", GUILayout.Height(25)))
                {
                    TestTakeDamage(10);
                }

                if (GUILayout.Button("Test Take Damage (50)", GUILayout.Height(25)))
                {
                    TestTakeDamage(50);
                }

                if (GUILayout.Button("Test Push Back", GUILayout.Height(25)))
                {
                    TestPushBack();
                }

                if (GUILayout.Button("Test Death", GUILayout.Height(25)))
                {
                    TestDeath();
                }

                EditorGUILayout.Space(5);

                if (GUILayout.Button("Reset Position", GUILayout.Height(25)))
                {
                    ResetPosition();
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);

            // Body Parts Section
            showBodyParts = EditorGUILayout.Foldout(showBodyParts, "Body Parts Setup", true);
            if (showBodyParts)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox("Assign SpriteRenderers for head, body, leg, sword. If not assigned, will auto-find in children.", MessageType.Info);

                SerializedObject serializedObj = new SerializedObject(controller);
                EditorGUILayout.PropertyField(serializedObj.FindProperty("headRenderer"), new GUIContent("Head Renderer"));
                EditorGUILayout.PropertyField(serializedObj.FindProperty("bodyRenderer"), new GUIContent("Body Renderer"));
                EditorGUILayout.PropertyField(serializedObj.FindProperty("leftLegRenderer"), new GUIContent("Left Leg Renderer"));
                EditorGUILayout.PropertyField(serializedObj.FindProperty("rightLegRenderer"), new GUIContent("Right Leg Renderer"));
                EditorGUILayout.PropertyField(serializedObj.FindProperty("swordRenderer"), new GUIContent("Sword Renderer"));
                serializedObj.ApplyModifiedProperties();
                serializedObj.ApplyModifiedProperties();

                EditorGUILayout.Space(5);
                if (GUILayout.Button("Auto Find Body Parts", GUILayout.Height(25)))
                {
                    AutoFindBodyParts();
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);

            // Animation Controls
            showAnimationControls = EditorGUILayout.Foldout(showAnimationControls, "Animation Test", true);
            if (showAnimationControls)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.LabelField("Flash All Body Parts", EditorStyles.boldLabel);

                if (GUILayout.Button("Flash Red (Hit Effect)", GUILayout.Height(25)))
                {
                    TestFlashEffect(Color.red);
                }

                if (GUILayout.Button("Flash White", GUILayout.Height(25)))
                {
                    TestFlashEffect(Color.white);
                }

                EditorGUILayout.Space(5);

                EditorGUILayout.LabelField("Individual Color Control");
                EditorGUI.indentLevel++;

                TestBodyPartColor("Head", controller.HeadRenderer);
                TestBodyPartColor("Body", controller.BodyRenderer);
                TestBodyPartColor("Left Leg", controller.LeftLegRenderer);
                TestBodyPartColor("Right Leg", controller.RightLegRenderer);
                TestBodyPartColor("Sword", controller.SwordRenderer);

                EditorGUI.indentLevel--;

                EditorGUILayout.Space(5);

                if (GUILayout.Button("Reset All Colors", GUILayout.Height(25)))
                {
                    ResetAllColors();
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("=== Debug Info ===", EditorStyles.boldLabel);

            // Display current state
            EditorGUILayout.LabelField($"Test Data Health: {testData.Health}");
            EditorGUILayout.LabelField($"Current Position: {controller.transform.position}");
            
            // Check initialization using public method
            bool currentlyInitialized = controller.IsInitialized();
            int currentHealth = controller.GetCurrentHealth();
            EditorGUILayout.LabelField($"Is Initialized: {currentlyInitialized}");
            EditorGUILayout.LabelField($"Current Health: {currentHealth}");
        }

        private void InitializeWithTestData()
        {
            Undo.RecordObject(controller, "Initialize EnemyController");
            controller.Initialize(testData);
            isInitialized = true;
            EditorUtility.SetDirty(controller);
            Debug.Log($"[EnemyControllerEditor] Initialized with test data - Health: {testData.Health}, Archetype: {testData.Archetype}");
        }

        private void TestTakeDamage(int damage)
        {
            if (!EnsureInitialized()) return;

            Undo.RecordObject(controller, "Test Take Damage");
            controller.TakeDamage(damage);
            testData.Health -= damage;
            EditorUtility.SetDirty(controller);
            Debug.Log($"[EnemyControllerEditor] Took {damage} damage. Remaining health: {testData.Health}");
        }

        private void TestPushBack()
        {
            if (!EnsureInitialized()) return;

            Undo.RecordObject(controller.transform, "Test Push Back");

            // Simulate push back by directly accessing transform
            float pushDistance = testData.PushBackDistance;
            controller.transform.Translate(Vector3.right * pushDistance);

            EditorUtility.SetDirty(controller);
            Debug.Log($"[EnemyControllerEditor] Pushed back by {pushDistance}");
        }

        private void TestDeath()
        {
            if (!EnsureInitialized()) return;

            Undo.RecordObject(controller, "Test Death");
            controller.TakeDamage(testData.Health);
            testData.Health = 0;
            EditorUtility.SetDirty(controller);
            Debug.Log("[EnemyControllerEditor] Death triggered");
        }

        private void ResetPosition()
        {
            Undo.RecordObject(controller.transform, "Reset Position");
            controller.transform.position = Vector3.zero;
            EditorUtility.SetDirty(controller);
            Debug.Log("[EnemyControllerEditor] Position reset to origin");
        }

        private void AutoFindBodyParts()
        {
            Undo.RecordObject(controller, "Auto Find Body Parts");

            Transform head = controller.transform.Find("head");
            Transform body = controller.transform.Find("body");
            Transform leftLeg = controller.transform.Find("leftLeg");
            Transform rightLeg = controller.transform.Find("rightLeg");
            Transform sword = controller.transform.Find("sword");

            // Alternative names for legs
            if (leftLeg == null) leftLeg = controller.transform.Find("leg_L");
            if (leftLeg == null) leftLeg = controller.transform.Find("leg_left");
            if (rightLeg == null) rightLeg = controller.transform.Find("leg_R");
            if (rightLeg == null) rightLeg = controller.transform.Find("leg_right");

            // Use SerializedObject to set private fields
            SerializedObject serializedObj = new SerializedObject(controller);

            if (head != null)
                serializedObj.FindProperty("headRenderer").objectReferenceValue = head.GetComponent<SpriteRenderer>();
            if (body != null)
                serializedObj.FindProperty("bodyRenderer").objectReferenceValue = body.GetComponent<SpriteRenderer>();
            if (leftLeg != null)
                serializedObj.FindProperty("leftLegRenderer").objectReferenceValue = leftLeg.GetComponent<SpriteRenderer>();
            if (rightLeg != null)
                serializedObj.FindProperty("rightLegRenderer").objectReferenceValue = rightLeg.GetComponent<SpriteRenderer>();
            if (sword != null)
                serializedObj.FindProperty("swordRenderer").objectReferenceValue = sword.GetComponent<SpriteRenderer>();

            serializedObj.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
            Debug.Log("[EnemyControllerEditor] Auto-found body parts");
        }

        private void TestBodyPartColor(string partName, SpriteRenderer renderer)
        {
            if (renderer != null)
            {
                EditorGUI.BeginChangeCheck();
                Color newColor = EditorGUILayout.ColorField($"{partName} Color", renderer.color);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(renderer, $"Change {partName} Color");
                    renderer.color = newColor;
                    EditorUtility.SetDirty(renderer);
                }
            }
            else
            {
                EditorGUILayout.LabelField($"{partName}: Not assigned", EditorStyles.miniLabel);
            }
        }

        private void TestFlashEffect(Color color)
        {
            if (isEditorFlashing) return; // Prevent multiple flashes

            // Save original colors
            editorOriginalColors.Clear();
            SpriteRenderer[] allRenderers = controller.GetComponentsInChildren<SpriteRenderer>();

            Undo.RecordObjects(allRenderers, "Flash All Body Parts");

            foreach (SpriteRenderer renderer in allRenderers)
            {
                if (renderer != null)
                {
                    editorOriginalColors[renderer] = renderer.color;
                    renderer.color = color;
                }
            }

            EditorUtility.SetDirty(controller);

            // Start timer
            isEditorFlashing = true;
            editorFlashStartTime = EditorApplication.timeSinceStartup;

            // Register update callback
            EditorApplication.update += OnEditorFlashUpdate;

            Debug.Log($"[EnemyControllerEditor] Flash {color} effect started - will auto-reset in {EDITOR_FLASH_DURATION}s");
        }

        private void OnEditorFlashUpdate()
        {
            if (!isEditorFlashing) return;

            double elapsed = EditorApplication.timeSinceStartup - editorFlashStartTime;

            if (elapsed >= EDITOR_FLASH_DURATION)
            {
                // Restore original colors
                foreach (var kvp in editorOriginalColors)
                {
                    SpriteRenderer renderer = kvp.Key;
                    Color originalColor = kvp.Value;
                    if (renderer != null)
                    {
                        renderer.color = originalColor;
                    }
                }

                EditorUtility.SetDirty(controller);

                // Cleanup
                isEditorFlashing = false;
                editorOriginalColors.Clear();
                EditorApplication.update -= OnEditorFlashUpdate;

                Debug.Log("[EnemyControllerEditor] Flash effect ended - colors restored");
            }
        }

        private void ResetAllColors()
        {
            Undo.RecordObject(controller, "Reset All Colors");

            // Reset to archetype color
            Color resetColor = testData.Archetype switch
            {
                EnemyArchetype.BasicMelee => Color.red,
                EnemyArchetype.ArmoredPusher => Color.blue,
                EnemyArchetype.Support => new Color(0.2f, 0.8f, 0.3f),
                EnemyArchetype.Bomber => new Color(1f, 0.6f, 0.1f),
                EnemyArchetype.BossDestroyer => new Color(0.6f, 0.2f, 0.8f),
                EnemyArchetype.BossOverlord => Color.black,
                _ => Color.gray
            };

            controller.SetBodyPartsColor(resetColor);
            EditorUtility.SetDirty(controller);
            Debug.Log("[EnemyControllerEditor] All body parts color reset to archetype default");
        }

        private bool EnsureInitialized()
        {
            // Check if enemyData is initialized by checking controller state
            // EnemyRow is not a UnityEngine.Object, so we check via controller's behavior
            bool initialized = controller.IsInitialized();
            
            if (!initialized)
            {
                EditorUtility.DisplayDialog("Not Initialized", "Please click 'Initialize with Test Data' first!", "OK");
                return false;
            }
            return true;
        }
    }
}
