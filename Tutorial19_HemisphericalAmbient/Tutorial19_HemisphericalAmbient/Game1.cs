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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Tutorial19_HemiAmbient
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ShaderTutorial : Microsoft.Xna.Framework.Game
    {
        // Variables for Matrix calculations, viewport and object movment
        float width, height;
        float x = 0, y = 0;
        float zHeight = 15.0f;
        float moveObject = 0;
        float m_Timer = 0;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // 3D Object
        Model m_Model;

        // Our effect object, this is where our shader will be loaded and compiled
        Effect effect;
        Effect effectPost;

        // Model texture
        Texture2D m_ColorMap;
        Texture2D m_BgMap;
        Texture2D m_Overlay;

        RenderTarget2D renderTarget;
        Texture2D SceneTexture;

        // Matrices
        Matrix renderMatrix, objectMatrix, worldMatrix, viewMatrix, projMatrix;
        Matrix[] bones;


        // Constructor
        public ShaderTutorial()
        {
            Window.Title = "Shader programming :: Tutorial 19 - Hemispherical ambient light";
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            m_Model = null;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            width = graphics.GraphicsDevice.Viewport.Width;
            height = graphics.GraphicsDevice.Viewport.Height;

            // Set worldMatrix to Identity
            worldMatrix = Matrix.Identity;

            float aspectRatio = (float)width / (float)height;
            float FieldOfView = (float)Math.PI / 2, NearPlane = 1.0f, FarPlane = 1000.0f;
            projMatrix = Matrix.CreatePerspectiveFieldOfView(FieldOfView, aspectRatio, NearPlane, FarPlane);

            // Load and compile our Shader into our Effect instance.
            effect = Content.Load<Effect>("Shader");
            effectPost = Content.Load<Effect>("PostProcess");
            m_ColorMap = Content.Load<Texture2D>("ColorMap");
            m_BgMap = Content.Load<Texture2D>("Bg");
            m_Overlay = Content.Load<Texture2D>("Overlay");

            // Vertex declaration for rendering our 3D model.
            graphics.GraphicsDevice.VertexDeclaration = new VertexDeclaration(graphics.GraphicsDevice, VertexPositionNormalTexture.VertexElements);


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load our 3D model and transform bones.
            m_Model = Content.Load<Model>("Object");
            bones = new Matrix[this.m_Model.Bones.Count];
            this.m_Model.CopyAbsoluteBoneTransformsTo(bones);


            PresentationParameters pp = graphics.GraphicsDevice.PresentationParameters;
            renderTarget = new RenderTarget2D(graphics.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, 1, graphics.GraphicsDevice.DisplayMode.Format);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            zHeight = 80;

            float m = (float)gameTime.ElapsedGameTime.Milliseconds / 1000;
            moveObject += m;

            // Move our object by doing some simple matrix calculations.
            objectMatrix = Matrix.CreateRotationX(moveObject / 2) * Matrix.CreateRotationZ(-moveObject / 4);
            renderMatrix = Matrix.CreateScale(0.5f);
            viewMatrix = Matrix.CreateLookAt(new Vector3(x, y, zHeight), Vector3.Zero, Vector3.Up);

            renderMatrix = objectMatrix * renderMatrix;

            m_Timer += (float)gameTime.ElapsedGameTime.Milliseconds / 500;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.SetRenderTarget(0, renderTarget);
            graphics.GraphicsDevice.Clear(Color.DarkSlateBlue);


            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            {
                spriteBatch.Draw(m_BgMap, new Rectangle(0, 0, 800, 600), Color.White);
            }
            spriteBatch.End();
            // Use the ToonShader technique from Shader.fx. You can have multiple techniques in a effect file. If you don't specify
            // what technique you want to use, it will choose the first one by default.
            effect.CurrentTechnique = effect.Techniques["DiffuseShader"];

            // Begin our effect
            effect.Begin();

            // A shader can have multiple passes, be sure to loop trough each of them.
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                // Begin current pass
                pass.Begin();

                foreach (ModelMesh mesh in m_Model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        // calculate our worldMatrix..
                        worldMatrix = bones[mesh.ParentBone.Index] * renderMatrix;

                        // .. and pass it into our shader.
                        // To access a parameter defined in our shader file ( Shader.fx ), use effectObject.Parameters["variableName"]
                        Matrix worldInverse = Matrix.Invert(worldMatrix);
                        Vector4 vLightDirection = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                        effect.Parameters["matWorldViewProj"].SetValue(worldMatrix * viewMatrix * projMatrix);
                        effect.Parameters["matInverseWorld"].SetValue(worldInverse);
                        effect.Parameters["vLightDirection"].SetValue(vLightDirection);
                        effect.Parameters["ColorMap"].SetValue(m_ColorMap);

                        // Render our meshpart
                        graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, part.StreamOffset, part.VertexStride);
                        graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                        graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                                                      part.BaseVertex, 0, part.NumVertices,
                                                                      part.StartIndex, part.PrimitiveCount);
                    }
                }

                // Stop current pass
                pass.End();
            }
            // Stop using this effect
            effect.End();
            graphics.GraphicsDevice.SetRenderTarget(0, null);
            SceneTexture = renderTarget.GetTexture();




            // Render the scene with Edge Detection, using the render target from last frame.
            graphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkSlateBlue, 1.0f, 0);


            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            {
                // Apply the post process shader
                effectPost.Begin();
                {
                    effectPost.CurrentTechnique.Passes[0].Begin();
                    {
                        spriteBatch.Draw(SceneTexture, new Rectangle(0, 0, 800, 600), Color.White);
                        effectPost.CurrentTechnique.Passes[0].End();
                    }
                }
                effectPost.End();
            }
            spriteBatch.End();


            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            {
                spriteBatch.Draw(m_Overlay, new Rectangle(0, 0, 800, 600), Color.White);
            }

            spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
