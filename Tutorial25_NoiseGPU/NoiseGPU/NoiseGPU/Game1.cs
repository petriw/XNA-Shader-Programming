using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace NoiseGPU
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Model meshObject;
        Matrix projection, view;
        Effect perlinNoiseEffect;

        Texture2D permTexture2d;
        Texture2D permGradTexture;


        PerlinNoise noiseEngine = new PerlinNoise();

        private void DrawModel(Model m, Matrix projection, Matrix view)
        {
            Matrix[] transforms = new Matrix[m.Bones.Count];
            m.CopyAbsoluteBoneTransformsTo(transforms);
            

            foreach (ModelMesh mesh in m.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = perlinNoiseEffect.Techniques["PerlinNoise"];
                    effect.Parameters["permTexture2d"].SetValue(permTexture2d);
                    effect.Parameters["permGradTexture"].SetValue(permGradTexture);
                    effect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index]);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);
                }
                mesh.Draw();
            }
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.IsFullScreen = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                aspectRatio, 1.0f, 10000.0f);
            

            noiseEngine.InitNoiseFunctions(3435, graphics.GraphicsDevice);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            meshObject = Content.Load<Model>("sphere");
            perlinNoiseEffect = Content.Load<Effect>("perlinNoiseEffect");

            foreach (ModelMesh mesh in meshObject.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = perlinNoiseEffect;
                }
            }

            permTexture2d = noiseEngine.GeneratePermTexture2d();
            permGradTexture = noiseEngine.GeneratePermGradTexture();

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        double timer = 0;
        protected override void Update(GameTime gameTime)
        {
            timer += gameTime.ElapsedGameTime.Milliseconds/5000.0;
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            view = Matrix.CreateLookAt(new Vector3(100.0f * (float)Math.Sin(timer), 0.0f, 100.0f * (float)Math.Cos(timer)),
                Vector3.Zero, Vector3.Up);

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkOliveGreen);

            DrawModel(meshObject, projection, view);

            spriteBatch.Begin(SpriteSortMode.Deferred, new BlendState());
            spriteBatch.Draw(permGradTexture, new Rectangle(0, 0, 256, 32), Color.White);
            spriteBatch.Draw(permTexture2d, new Rectangle(GraphicsDevice.Viewport.Width-256, 0, 256, 256), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
