using UnityEngine;
using UnityEngine.InputSystem;
using FPSGame.Player;

namespace FPSGame.Editor
{
    public static class CharacterBuilder
    {
        const float cameraHeightOffset = 0.4f;

        public static GameObject Build(CharacterSetup setup)
        {
            var root = new GameObject(setup.CharacterName);

            var cc = root.AddComponent<CharacterController>();
            cc.height = setup.Height;
            cc.radius = setup.Radius;
            cc.center = new Vector3(0f, setup.Height * 0.5f, 0f);

            // Model
            if (setup.ModelPrefab != null)
            {
                var model = Object.Instantiate(setup.ModelPrefab, root.transform);
                model.name = "Model";
            }
            else
            {
                CreateCapsulePlaceholder(root.transform, setup.Height, setup.Radius);
            }

            // Camera
            if (setup.Abilities.HasFlag(CharacterAbilities.Look))
            {
                var cameraHolder = new GameObject("CameraHolder");
                cameraHolder.transform.SetParent(root.transform);
                cameraHolder.transform.localPosition = new Vector3(0f, setup.Height - cameraHeightOffset, 0f);

                var cam = cameraHolder.AddComponent<Camera>();
                cam.nearClipPlane = 0.1f;
                cam.fieldOfView = 75f;

                cameraHolder.AddComponent<AudioListener>();

                var cameraController = cameraHolder.AddComponent<PlayerCameraController>();
            }

            // Input
            var playerInput = root.AddComponent<PlayerInput>();

            // Pawn
            var pawn = root.AddComponent<PlayerPawn>();

            // Controller
            var controller = root.AddComponent<PlayerController>();

            // Config asset — attach reference
            if (setup.MovementConfig != null)
            {
                // PlayerPawn has a serialized config field; we set it via SerializedObject in the editor
            }

            return root;
        }

        static void CreateCapsulePlaceholder(Transform parent, float height, float radius)
        {
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "PlaceholderModel";
            capsule.transform.SetParent(parent);
            capsule.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            capsule.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);

            // Remove the collider — CharacterController handles collision
            var collider = capsule.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
        }
    }
}
