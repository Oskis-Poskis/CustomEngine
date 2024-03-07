using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Engine.Common
{
    public class Camera
    {
        public float aspect_ratio = 1.0f;
        public Vector3 position = Vector3.Zero;
        public Vector3 direction = -Vector3.UnitZ;
        public Matrix4 projection, view;

        public float theta = -90, phi = -45;
        public float sensitivity = 0.3f;
        public float zoom_sensitivity = 1.0f;

        public float speed;
        public int fov;
        private bool trackball = false;
        private float trackball_distance = 5.0f;

        private Vector3 target_position;
        private Vector3 target_direction;
        private readonly float movement_interp_speed = 10.0f;
        private readonly float camera_interp_speed = 15.0f;

        private static int VAO, VBO;
        private readonly static float[] vertices =
        { 
            -1.0f, -1.0f,
            -1.0f,  1.0f,
             1.0f,  1.0f,
             1.0f, -1.0f,
        };

        public Camera(Vector3 Position, Vector3 Direction, int FOV, float Speed)
        {
            position = Position;
            target_position =Position;
            direction = Direction;
            target_direction = Direction;
            
            speed = Speed;
            fov = FOV;

            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FOV), 1, 0.1f, 1000.0f);
            view = Matrix4.LookAt(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY);

            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        }

        public void Zoom(float offset)
        {
            position = position + offset * direction * zoom_sensitivity;
            target_position = target_position + offset * target_direction * zoom_sensitivity;
        }

        public void Input(Vector2 delta, bool trackball)
        {
            float deltaX = delta.X;
            float deltaY = delta.Y;
            theta += deltaX * sensitivity;
            phi -= deltaY * sensitivity;

            theta = (theta + 360) % 360;
            phi = Math.Clamp(phi, -89, 89);

            if (!trackball)
            {
                this.trackball = false;
                target_direction = new Vector3(
                    (float)Math.Cos(MathHelper.DegreesToRadians(theta)) * (float)Math.Cos(MathHelper.DegreesToRadians(phi)),
                    (float)Math.Sin(MathHelper.DegreesToRadians(phi)),
                    (float)Math.Sin(MathHelper.DegreesToRadians(theta)) * (float)Math.Cos(MathHelper.DegreesToRadians(phi)));
            }

            else
            {
                this.trackball = true;

                target_position = trackball_distance * new Vector3(
                    -(float)Math.Cos(MathHelper.DegreesToRadians(theta)) * (float)Math.Cos(MathHelper.DegreesToRadians(phi)),
                     (float)Math.Sin(MathHelper.DegreesToRadians(phi * -1.0f)),
                    -(float)Math.Sin(MathHelper.DegreesToRadians(theta)) * (float)Math.Cos(MathHelper.DegreesToRadians(phi)));
                
                target_direction = -Vector3.Normalize(target_position);
            }
        }

        public void Move(float delta, KeyboardState state)
        {
            float delta_speed = delta * speed;
            if (state.IsKeyDown(Keys.W)) target_position += delta_speed * direction;
            if (state.IsKeyDown(Keys.S)) target_position -= delta_speed * direction;
            if (state.IsKeyDown(Keys.A)) target_position -= delta_speed * Vector3.Normalize(Vector3.Cross(direction, Vector3.UnitY));
            if (state.IsKeyDown(Keys.D)) target_position += delta_speed * Vector3.Normalize(Vector3.Cross(direction, Vector3.UnitY));
            if (state.IsKeyDown(Keys.E) | state.IsKeyDown(Keys.Space)) target_position += delta_speed * Vector3.UnitY;
            if (state.IsKeyDown(Keys.Q) | state.IsKeyDown(Keys.LeftShift)) target_position -= delta_speed * Vector3.UnitY;

            trackball_distance = Vector3.Distance(target_position, Vector3.Zero);
        }

        public void Update(float delta)
        {
            // Interpolate position and direction
            position = trackball ? target_position : Vector3.Lerp(position, target_position, movement_interp_speed * delta);
            direction = trackball ? target_direction : Vector3.Lerp(direction, target_direction, camera_interp_speed * delta);

            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fov), aspect_ratio, 0.1f, 1000.0f);
            view = Matrix4.LookAt(position, position + direction, Vector3.UnitY);
        }
    }
}