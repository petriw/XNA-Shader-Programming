using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WindowsDemo.Engine
{
	/// <summary>
	/// Represents a LookAt Camera in 3D space
	/// </summary>
	public class Camera
	{
		private Vector3 _lookAt;
		private Matrix _projectionMatrix;
		private Matrix _viewMatrix;
        private Vector3 _position;

		public Camera(Viewport viewport)
		{
			this._projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
				MathHelper.ToRadians(45.0f),
				viewport.AspectRatio,
				1.0f,
				10000.0f);

            Position = new Vector3(0, 100, 006);
		}

        private Vector3 angle = new Vector3();

        private float speed = 500.0f;
        private float turnSpeed = 200.0f;

        public Vector3 Angle
        {
            get { return angle; }
            set { angle = value; }
        }

		public Vector3 LookAt
		{
			get { return this._lookAt; }
			set { this._lookAt = value; }
		}

        public Vector3 Position
        {
            get { return this._position; }
            set { this._position = value; }
        }

		public Matrix ProjectionMatrix { get { return this._projectionMatrix; } }
		public Matrix ViewMatrix { get { return this._viewMatrix; } }


		public void Update(float time)
		{
            float delta = time;

            bool inputKeyb = false;
            if (!inputKeyb)
            {
                GamePadState currentState = GamePad.GetState(PlayerIndex.One);

                if (currentState.IsConnected)
                {
                    angle.X += MathHelper.ToRadians((currentState.ThumbSticks.Left.Y * 0.50f) * turnSpeed * 0.01f); // pitch
                    angle.Y += MathHelper.ToRadians((currentState.ThumbSticks.Left.X * 0.50f) * turnSpeed * 0.01f); // yaw
                }

                Vector3 forward = Vector3.Normalize(new Vector3((float)Math.Sin(-angle.Y), (float)Math.Sin(angle.X), (float)Math.Cos(-angle.Y)));
                Vector3 left = Vector3.Normalize(new Vector3((float)Math.Cos(angle.Y), 0f, (float)Math.Sin(angle.Y)));


                this.Position -= forward * currentState.ThumbSticks.Right.Y * speed * delta;
                this.Position += left * currentState.ThumbSticks.Right.X * speed * delta;


                _viewMatrix = Matrix.Identity;
                _viewMatrix *= Matrix.CreateTranslation(-Position);
                _viewMatrix *= Matrix.CreateRotationZ(angle.Z);
                _viewMatrix *= Matrix.CreateRotationY(angle.Y);
                _viewMatrix *= Matrix.CreateRotationX(angle.X);
            }
            else
            {
                //KeyboardState keyboard = Keyboard.GetState();
                //MouseState mouse = Mouse.GetState();

                //int centerX = 1024 / 2;
                //int centerY = 768 / 2;

                //Mouse.SetPosition(centerX, centerY);

                //angle.X += MathHelper.ToRadians((mouse.Y - centerY) * 50 * 0.01f); // pitch
                //angle.Y += MathHelper.ToRadians((mouse.X - centerX) * 50 * 0.01f); // yaw

                //Vector3 forward = Vector3.Normalize(new Vector3((float)Math.Sin(-angle.Y), (float)Math.Sin(angle.X), (float)Math.Cos(-angle.Y)));
                //Vector3 left = Vector3.Normalize(new Vector3((float)Math.Cos(angle.Y), 0f, (float)Math.Sin(angle.Y)));

                //if (keyboard.IsKeyDown(Keys.Up))
                //    Position -= forward * speed * delta;

                //if (keyboard.IsKeyDown(Keys.Down))
                //    Position += forward * speed * delta;

                //if (keyboard.IsKeyDown(Keys.Left))
                //    Position -= left * speed * delta;

                //if (keyboard.IsKeyDown(Keys.Right))
                //    Position += left * speed * delta;

                //if (keyboard.IsKeyDown(Keys.PageUp))
                //    Position += Vector3.Down * speed * delta;

                //if (keyboard.IsKeyDown(Keys.PageDown))
                //    Position += Vector3.Up * speed * delta;

                //_viewMatrix = Matrix.Identity;
                //_viewMatrix *= Matrix.CreateTranslation(-Position);
                //_viewMatrix *= Matrix.CreateRotationZ(angle.Z);
                //_viewMatrix *= Matrix.CreateRotationY(angle.Y);
                //_viewMatrix *= Matrix.CreateRotationX(angle.X);
            }
		}
	}
}
