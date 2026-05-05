using UnityEditor;
using UnityEngine;
using FPSGame.Player;

namespace FPSGame.Editor
{
    public class CharacterCreatorWindow : EditorWindow
    {
        // Identity
        string characterName = "New Character";

        // Model
        GameObject modelPrefab;

        // Abilities
        CharacterAbilities abilities = CharacterAbilities.Movement | CharacterAbilities.Jump | CharacterAbilities.Look;

        // Config
        PlayerConfig movementConfig;

        // Physics
        float height = 2f;
        float radius = 0.5f;

        // Save
        string savePath = "Assets/FPSGame/Player/Prefabs";

        // Scroll
        Vector2 scrollPosition;

        [MenuItem("FPSGame/Character Creator")]
        public static void ShowWindow()
        {
            var window = GetWindow<CharacterCreatorWindow>("Character Creator");
            window.minSize = new Vector2(400, 500);
        }

        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            EditorGUILayout.Space(10);
            DrawIdentitySection();
            EditorGUILayout.Space(10);
            DrawModelSection();
            EditorGUILayout.Space(10);
            DrawAbilitiesSection();
            EditorGUILayout.Space(10);
            DrawPhysicsSection();
            EditorGUILayout.Space(10);
            DrawConfigSection();
            EditorGUILayout.Space(10);
            DrawSaveSection();
            EditorGUILayout.Space(20);
            DrawCreateButton();

            EditorGUILayout.EndScrollView();
        }

        void DrawHeader()
        {
            EditorGUILayout.LabelField("FPSGame Character Creator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Create a character prefab with selected abilities.\n" +
                "Drag a rigged model or leave empty for a capsule placeholder.",
                MessageType.Info);
        }

        void DrawIdentitySection()
        {
            EditorGUILayout.LabelField("Identity", EditorStyles.boldLabel);
            characterName = EditorGUILayout.TextField("Character Name", characterName);
        }

        void DrawModelSection()
        {
            EditorGUILayout.LabelField("Model", EditorStyles.boldLabel);
            modelPrefab = (GameObject)EditorGUILayout.ObjectField(
                "Rigged Model (optional)",
                modelPrefab,
                typeof(GameObject),
                false);

            if (modelPrefab == null)
            {
                EditorGUILayout.HelpBox("No model assigned — a capsule placeholder will be used.", MessageType.None);
            }
        }

        void DrawAbilitiesSection()
        {
            EditorGUILayout.LabelField("Abilities", EditorStyles.boldLabel);
            abilities = (CharacterAbilities)EditorGUILayout.EnumFlagsField("Enabled Abilities", abilities);

            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Active:", FormatAbilities(abilities), EditorStyles.miniLabel);
            EditorGUI.indentLevel--;
        }

        void DrawPhysicsSection()
        {
            EditorGUILayout.LabelField("Physics (CharacterController)", EditorStyles.boldLabel);
            height = EditorGUILayout.Slider("Height", height, 0.5f, 4f);
            radius = EditorGUILayout.Slider("Radius", radius, 0.1f, 1f);
        }

        void DrawConfigSection()
        {
            EditorGUILayout.LabelField("Movement Config", EditorStyles.boldLabel);
            movementConfig = (PlayerConfig)EditorGUILayout.ObjectField(
                "Player Config (SO)",
                movementConfig,
                typeof(PlayerConfig),
                false);

            if (movementConfig == null)
            {
                EditorGUILayout.HelpBox(
                    "No config assigned. Create one via:\nCreate > FPSGame > Player > Player Config",
                    MessageType.Warning);
            }
        }

        void DrawSaveSection()
        {
            EditorGUILayout.LabelField("Save Location", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            savePath = EditorGUILayout.TextField("Prefab Path", savePath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                var selected = EditorUtility.OpenFolderPanel("Select prefab folder", "Assets", "");
                if (!string.IsNullOrEmpty(selected))
                {
                    if (selected.StartsWith(Application.dataPath))
                    {
                        savePath = "Assets" + selected.Substring(Application.dataPath.Length);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawCreateButton()
        {
            var valid = !string.IsNullOrEmpty(characterName);

            EditorGUI.BeginDisabledGroup(!valid);

            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);

            if (GUILayout.Button("Create Character Prefab", GUILayout.Height(40)))
            {
                CreateCharacter();
            }

            GUI.backgroundColor = originalColor;
            EditorGUI.EndDisabledGroup();

            if (!valid)
            {
                EditorGUILayout.HelpBox("Character name is required.", MessageType.Error);
            }
        }

        void CreateCharacter()
        {
            // Build setup
            var setup = ScriptableObject.CreateInstance<CharacterSetup>();
            setup.CharacterName = characterName;
            setup.ModelPrefab = modelPrefab;
            setup.Abilities = abilities;
            setup.MovementConfig = movementConfig;
            setup.Height = height;
            setup.Radius = radius;

            // Build the GameObject in scene
            var character = CharacterBuilder.Build(setup);

            // Wire up serialized fields via SerializedObject
            WireSerializedFields(character, setup);

            // Ensure save directory exists
            if (!AssetDatabase.IsValidFolder(savePath))
            {
                CreateFoldersRecursive(savePath);
            }

            // Save as prefab
            var prefabPath = $"{savePath}/{characterName}.prefab";
            prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);

            var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(
                character, prefabPath, InteractionMode.UserAction);

            // Cleanup temp SO
            DestroyImmediate(setup);

            // Select the new prefab
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);

            Debug.Log($"Character prefab created: {prefabPath}");
        }

        void WireSerializedFields(GameObject character, CharacterSetup setup)
        {
            // Wire PlayerPawn config
            var pawn = character.GetComponent<PlayerPawn>();
            if (pawn != null && setup.MovementConfig != null)
            {
                var so = new SerializedObject(pawn);
                var configProp = so.FindProperty("config");
                if (configProp != null)
                {
                    configProp.objectReferenceValue = setup.MovementConfig;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        static string FormatAbilities(CharacterAbilities flags)
        {
            if (flags == CharacterAbilities.None) return "None";

            var parts = new System.Collections.Generic.List<string>();
            if (flags.HasFlag(CharacterAbilities.Movement)) parts.Add("Movement");
            if (flags.HasFlag(CharacterAbilities.Jump)) parts.Add("Jump");
            if (flags.HasFlag(CharacterAbilities.Crouch)) parts.Add("Crouch");
            if (flags.HasFlag(CharacterAbilities.Sprint)) parts.Add("Sprint");
            if (flags.HasFlag(CharacterAbilities.Look)) parts.Add("Look");
            return string.Join(", ", parts);
        }

        static void CreateFoldersRecursive(string path)
        {
            var parts = path.Split('/');
            var current = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}
