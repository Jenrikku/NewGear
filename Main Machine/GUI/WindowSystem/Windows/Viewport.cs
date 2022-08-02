using ImGuiNET;
using NewGear.MainMachine.FileSystem;
using Raylib_cs;
using System.Numerics;

using static NewGear.MainMachine.GUI.Constants;
using static NewGear.MainMachine.GUI.MainWindow;

namespace NewGear.MainMachine.GUI.WindowSystem.Windows {
    internal class Viewport : ImGUIWindow {
        public Vector2 WindowPositionMin { get; set; }
        public Vector2 WindowPositionMax { get; set; }

        private Camera3D camera = new(new(0, 2, -10), new(), Vector3.UnitY, 60, CameraProjection.CAMERA_PERSPECTIVE);
        private Vector3 cameraDirection = new(0, 0, 1);

        public void Render() {
            // Draw ViewportTexture:
            Raylib.BeginTextureMode(ViewportTexture);
            Raylib.ClearBackground(VIEWPORT_BACKGROUND);

            Raylib.DrawText($"Position: {camera.position}", 5, 5, 20, Color.WHITE);
            Raylib.DrawText($"Target: {camera.target}", 5, 30, 20, Color.WHITE);
            Raylib.DrawText($"Direction: {cameraDirection}", 5, 55, 20, Color.WHITE);

            camera.target = camera.position + cameraDirection;

            Raylib.BeginMode3D(camera);

            float modelScale = 1f;

            Raylib.DrawGrid((int) (2 / modelScale), modelScale * 10);

            // Draw example cube
            Raylib.DrawCube(new(0, 2, 0), 4, 4, 4, Color.GREEN);

            Raylib.EndMode3D();
            Raylib.EndTextureMode();

            // Update:

            ImGui.SetNextWindowSize(new(500, 350), ImGuiCond.FirstUseEver);
            ImGui.Begin("Viewport", ref OpenedWindows[1]);
            WindowPositionMin = ImGui.GetWindowPos();
            WindowPositionMax = WindowPositionMin + ImGui.GetWindowSize();

            //if(FileManager.CurrentFile?.CurrentSubFile is null) {
            //    ImGui.TextDisabled("There is no model data to show.");
            //    return;
            //}

            // Draw Viewport:

            var size = ImGui.GetContentRegionAvail();

            if(ViewportTexture.texture.width != size.X || ViewportTexture.texture.height != size.Y) {
                Raylib.UnloadRenderTexture(ViewportTexture);
                ViewportTexture = Raylib.LoadRenderTexture((int) size.X, (int) size.Y);
            }

            ImGui.Image((IntPtr) ViewportTexture.texture.id, size, new(0, 1), new(1, 0));

            // Input:

            if(ImGui.IsWindowFocused()) {
                Vector3 right = Vector3.Cross(cameraDirection, camera.up) * 0.1f;
                Vector3 forward = cameraDirection * 0.1f;
                Vector3 up = camera.up * 0.1f;

                Vector2 mouseDelta = Raylib.GetMouseDelta();
                float mouseWheelMove = Raylib.GetMouseWheelMove() * 3;

                // Move left
                if(Raylib.IsKeyDown(KeyboardKey.KEY_A)) {
                    camera.position -= right;
                    camera.target -= right;
                }

                // Move right
                if(Raylib.IsKeyDown(KeyboardKey.KEY_D)) {
                    camera.position += right;
                    camera.target += right;
                }

                // Move up
                if(Raylib.IsKeyDown(KeyboardKey.KEY_E)) {
                    camera.position += up;
                    camera.target += up;
                }

                // Move down
                if(Raylib.IsKeyDown(KeyboardKey.KEY_Q)) {
                    camera.position -= up;
                    camera.target -= up;
                }

                // Move forwards
                if(Raylib.IsKeyDown(KeyboardKey.KEY_W)) {
                    camera.position += forward;
                    camera.target += forward;
                }

                // Move backwards
                if(Raylib.IsKeyDown(KeyboardKey.KEY_S)) {
                    camera.position -= forward;
                    camera.target -= forward;
                }

                // Zoom with Ctrl
                if(Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL)
                    || Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT_CONTROL)) {

                    camera.position -= mouseDelta.Y * forward;
                    camera.target -= mouseDelta.Y * forward;
                }
                
                // Zoom with wheel
                if(mouseWheelMove != 0) {
                    camera.position += mouseWheelMove * forward;
                    camera.target += mouseWheelMove * forward;
                }

                // Rotation
                if(Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT)) {
                    RotateVectorY(ref cameraDirection, mouseDelta.X * 0.001f);
                    cameraDirection.Y -= mouseDelta.Y * 0.001f;

                    cameraDirection = Vector3.Normalize(cameraDirection);
                }

                // Orthographic perspective
                if(Raylib.IsKeyPressed(KeyboardKey.KEY_O)
                    || Raylib.IsKeyPressed(KeyboardKey.KEY_KP_5)) {

                    if(camera.projection == CameraProjection.CAMERA_PERSPECTIVE)
                        camera.projection = CameraProjection.CAMERA_ORTHOGRAPHIC;
                    else
                        camera.projection = CameraProjection.CAMERA_PERSPECTIVE;
                }
            }

            static void RotateVectorY(ref Vector3 vector, double angle) {
                double cos = Math.Cos(angle);
                double sin = Math.Sin(angle);

                vector.X = (float) (vector.X * cos - vector.Z * sin);
                vector.Z = (float) (vector.X * sin + vector.Z * cos);
            }
        }

        public void ContextMenu(Vector2 mousePosition) {
            return;
        }
    }
}
