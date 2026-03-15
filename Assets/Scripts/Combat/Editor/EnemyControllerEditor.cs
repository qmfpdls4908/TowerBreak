using UnityEditor;
using UnityEngine;

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
        private bool isInitialized = false;

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
            // Use serialized object to check if enemyData is set
            SerializedObject serializedObj = new SerializedObject(controller);
            SerializedProperty enemyDataProp = serializedObj.FindProperty("enemyData");
            isInitialized = enemyDataProp != null && enemyDataProp.objectReferenceValue != null;
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

            // Animation Controls
            showAnimationControls = EditorGUILayout.Foldout(showAnimationControls, "Animation Test", true);
            if (showAnimationControls)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.LabelField("Sprite Color (Visual Test)");
                SpriteRenderer sr = controller.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    EditorGUI.BeginChangeCheck();
                    Color newColor = EditorGUILayout.ColorField("Current Color", sr.color);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(sr, "Change Color");
                        sr.color = newColor;
                        EditorUtility.SetDirty(sr);
                    }
                }

                EditorGUILayout.Space(5);

                if (GUILayout.Button("Flash Red (Hit Effect)", GUILayout.Height(25)))
                {
                    TestFlashEffect();
                }

                if (GUILayout.Button("Flash White", GUILayout.Height(25)))
                {
                    TestFlashWhite();
                }

                if (GUILayout.Button("Reset Color", GUILayout.Height(25)))
                {
                    ResetColor();
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("=== Debug Info ===", EditorStyles.boldLabel);

            // Display current state
            EditorGUILayout.LabelField($"Current Health: {testData.Health}");
            EditorGUILayout.LabelField($"Current Position: {controller.transform.position}");
            
            // Check initialization using serialized property
            SerializedObject serializedObj = new SerializedObject(controller);
            SerializedProperty enemyDataProp = serializedObj.FindProperty("enemyData");
            bool currentlyInitialized = enemyDataProp != null && enemyDataProp.objectReferenceValue != null;
            EditorGUILayout.LabelField($"Is Initialized: {currentlyInitialized}");
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

        private void TestFlashEffect()
        {
            SpriteRenderer sr = controller.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Undo.RecordObject(sr, "Flash Effect");
                sr.color = Color.red;
                EditorUtility.SetDirty(sr);
                Debug.Log("[EnemyControllerEditor] Flash red effect applied");
            }
        }

        private void TestFlashWhite()
        {
            SpriteRenderer sr = controller.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Undo.RecordObject(sr, "Flash White");
                sr.color = Color.white;
                EditorUtility.SetDirty(sr);
                Debug.Log("[EnemyControllerEditor] Flash white effect applied");
            }
        }

        private void ResetColor()
        {
            SpriteRenderer sr = controller.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Undo.RecordObject(sr, "Reset Color");

                // Reset to archetype color
                switch (testData.Archetype)
                {
                    case EnemyArchetype.BasicMelee:
                        sr.color = Color.red;
                        break;
                    case EnemyArchetype.ArmoredPusher:
                        sr.color = Color.blue;
                        break;
                    case EnemyArchetype.Support:
                        sr.color = new Color(0.2f, 0.8f, 0.3f);
                        break;
                    case EnemyArchetype.Bomber:
                        sr.color = new Color(1f, 0.6f, 0.1f);
                        break;
                    case EnemyArchetype.BossDestroyer:
                        sr.color = new Color(0.6f, 0.2f, 0.8f);
                        break;
                    case EnemyArchetype.BossOverlord:
                        sr.color = Color.black;
                        break;
                    default:
                        sr.color = Color.gray;
                        break;
                }

                EditorUtility.SetDirty(sr);
                Debug.Log("[EnemyControllerEditor] Color reset to archetype default");
            }
        }

        private bool EnsureInitialized()
        {
            // Check using serialized property instead of reflection
            SerializedObject serializedObj = new SerializedObject(controller);
            SerializedProperty enemyDataProp = serializedObj.FindProperty("enemyData");
            bool initialized = enemyDataProp != null && enemyDataProp.objectReferenceValue != null;
            
            if (!initialized)
            {
                EditorUtility.DisplayDialog("Not Initialized", "Please click 'Initialize with Test Data' first!", "OK");
                return false;
            }
            return true;
        }
    }
}
