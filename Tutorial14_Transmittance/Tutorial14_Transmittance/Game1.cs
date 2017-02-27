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

namespace Tutorial14_Transmittance
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ShaderTutorial : Microsoft.Xna.Framework.Game
    {
        // Variables for Matrix calculations, viewport and object movment
        float width, height;
        float x = 0, y = 0;
        Vector2 moveObject = new Vector2(0, 0);
        float Du = 1.0f;
        float C = 30.0f;

        // Default XNA objects
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // 3D Object
        Model m_Model;

        // Our effect object, this is where our shader will be loaded and compiled
        Effect effect;
        Effect effectPost;

        // Our techniques
        EffectTechnique environmentShader;
        EffectTechnique depthMapShader;

        // Model texture
        Texture2D m_ColorMap;
        Texture2D m_Overlay;
        Texture2D m_BGScene;

        // Render targets
        RenderTarget2D renderTarget;
        Texture2D SceneTexture;
        RenderTarget2D depthRT;
        DepthStencilBuffer depthSB;
        RenderTarget2D depthRT2;
        DepthStencilBuffer depthSB2;

        Texture2D depth1Texture;
        Texture2D depth2Texture;

        // Matrices
        Matrix renderMatrix, objectMatrix, worldMatrix, viewMatrix, projMatrix;
        Matrix[] bones;



        // Constructor
        public ShaderTutorial()
        {
            Window.Title = "Shader programming :: Tutorial 14 - Transmittance";
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

            // Create our projection matrix
            float aspectRatio = (float)width / (float)height;
            float FieldOfView = MathHelper.ToRadians(45.0f), NearPlane = 20.0f, FarPlane = 150.0f;
            projMatrix = Matrix.CreatePerspectiveFieldOfView(FieldOfView, aspectRatio, NearPlane, FarPlane);


            // Load and compile our Shader into our Effect instance.
            effect = Content.Load<Effect>("Shader");
            effectPost = Content.Load<Effect>("PostProcess");
            m_ColorMap = Content.Load<Texture2D>("ColorMap");
            m_Overlay = Content.Load<Texture2D>("Overlay");
            m_BGScene = Content.Load<Texture2D>("BGScene");

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

            // Create our render targets
            PresentationParameters pp = graphics.GraphicsDevice.PresentationParameters;
            renderTarget = new RenderTarget2D(graphics.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, 1, graphics.GraphicsDevice.DisplayMode.Format);
            depthRT = new RenderTarget2D(graphics.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, 1, SurfaceFormat.Single); // 32-bit float format using 32 bits for the red channel.
            depthRT2 = new RenderTarget2D(graphics.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, 1, SurfaceFormat.Single); // 32-bit float format using 32 bits for the red channel.


            // Depth Stencil buffer
            depthSB = CreateDepthStencil(depthRT, DepthFormat.Depth24Stencil8);
            depthSB2 = CreateDepthStencil(depthRT2, DepthFormat.Depth24Stencil8);

            // Get our techniques and store them in variables.
            environmentShader = effect.Techniques["EnvironmentShader"];
            depthMapShader = effect.Techniques["DepthMapShader"];

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
            // Default movment
            float m = (float)gameTime.ElapsedGameTime.Milliseconds / 1000;
            moveObject.X += m;
            moveObject.Y += m;

            // Add some custom movment
            GamePadState state = GamePad.GetState(PlayerIndex.One);
            moveObject.X += state.ThumbSticks.Left.Y;
            moveObject.Y += state.ThumbSticks.Left.X;
            Du += state.ThumbSticks.Right.X;
            C += state.ThumbSticks.Right.Y;

            // Move our object by doing some simple matrix calculations.
            objectMatrix = Matrix.CreateRotationX(moveObject.X / 10) * Matrix.CreateRotationZ(-moveObject.Y / 10);
            renderMatrix = Matrix.CreateScale(0.4f);
            viewMatrix = Matrix.CreateLookAt(new Vector3(x, y, 80), Vector3.Zero, Vector3.Up);

            renderMatrix = objectMatrix * renderMatrix;


            // Set our shader values
            Vector4 vecEye = new Vector4(x, y, 80, 0);
            Vector4 vColorDiffuse = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            Vector4 vColorSpecular = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            Vector4 vColorAmbient = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);


            Matrix worldInverse = Matrix.Invert(worldMatrix);
            Vector4 vLightDirection = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
            effect.Parameters["matWorldViewProj"].SetValue(worldMatrix * viewMatrix * projMatrix);
            effect.Parameters["matInverseWorld"].SetValue(worldInverse);
            effect.Parameters["vLightDirection"].SetValue(vLightDirection);
            effect.Parameters["ColorMap"].SetValue(m_ColorMap);
            effect.Parameters["vecEye"].SetValue(vecEye);
            effect.Parameters["vDiffuseColor"].SetValue(vColorDiffuse);
            effect.Parameters["vSpecularColor"].SetValue(vColorSpecular);
            effect.Parameters["vAmbient"].SetValue(vColorAmbient);


            base.Update(gameTime);
        }

        /// <summary>
        /// Now as we are increasing the number of rendertargets, we need to keep track of our scene so it renders the same way
        /// each time we are rendering the scene to a texture.
        /// 
        /// Thes function should render our scene.
        /// </summary>
        void DrawScene(bool transmittance)
        {
            // Begin our effect
            effect.Begin(SaveStateMode.SaveState);

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
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Render the backdrop to a render target. Don't render the transmitting objects
            // DrawScene(false);


            // Render our depthmaps. Here we should only render the objects that will be tramsitted
            // create depth-map 1
            effect.CurrentTechnique = depthMapShader;
            GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
            depth1Texture = RenderDepthMap(depthSB,depthRT);
            // create depth-map 2
            effect.CurrentTechnique = depthMapShader;
            GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            depth2Texture = RenderDepthMap(depthSB2, depthRT2);

            // render our trasmitting objects
            graphics.GraphicsDevice.SetRenderTarget(0, renderTarget);
            graphics.GraphicsDevice.Clear(Color.White);

            effect.CurrentTechnique = environmentShader;
            DrawScene(true);
            
            graphics.GraphicsDevice.SetRenderTarget(0, null);
            SceneTexture = renderTarget.GetTexture();




            // Render the scene with the post process transmittance shader, using the render target from last frame, our two depth buffer and our background texture.
            graphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkSlateBlue, 1.0f, 0);

            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            {
                // Apply the post process shader
                effectPost.Begin();
                {
                    effectPost.CurrentTechnique.Passes[0].Begin();
                    {
                        effectPost.Parameters["D1M"].SetValue(depth1Texture);
                        effectPost.Parameters["D2M"].SetValue(depth2Texture);
                        effectPost.Parameters["BGScene"].SetValue(m_BGScene);
                        effectPost.Parameters["Scene"].SetValue(SceneTexture);
                        effectPost.Parameters["Du"].SetValue(Du);
                        effectPost.Parameters["C"].SetValue(C);
                        spriteBatch.Draw(SceneTexture, new Rectangle(0, 0, 800, 600), Color.White);
                        effectPost.CurrentTechnique.Passes[0].End();
                    }
                }
                effectPost.End();
            }
            spriteBatch.End();

            // Draw our overlay
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            {
                spriteBatch.Draw(m_Overlay, new Rectangle(0, 0, 800, 600), Color.White);
            }
            spriteBatch.End();
            
            base.Draw(gameTime);
        }

        /// <summary>
        /// Renders our scene to a DepthMap, using a DepthStencilBuffer dsb, and a RenderTarget2D rt2D.
        /// The generated texture is returned as a Texture2D.
        /// </summary>
        /// <returns>Texture2D</returns>
        private Texture2D RenderDepthMap(DepthStencilBuffer dsb, RenderTarget2D rt2D)
        {
            GraphicsDevice.RenderState.DepthBufferFunction = CompareFunction.LessEqual;
            GraphicsDevice.SetRenderTarget(0, rt2D);

            // Save our DepthStencilBuffer, so we can restore it later
            DepthStencilBuffer saveSB = GraphicsDevice.DepthStencilBuffer;

            GraphicsDevice.DepthStencilBuffer = dsb;
            GraphicsDevice.Clear(Color.Black);

            DrawScene(true);

            // restore old depth stencil buffer
            GraphicsDevice.SetRenderTarget(0, null);
            GraphicsDevice.DepthStencilBuffer = saveSB;

            return rt2D.GetTexture();
        }


        /// <summary>
        /// Returns the supported depth stencil buffer.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private DepthStencilBuffer CreateDepthStencil(RenderTarget2D target)
        {
            return new DepthStencilBuffer(target.GraphicsDevice, target.Width,
                target.Height, target.GraphicsDevice.DepthStencilBuffer.Format,
                target.MultiSampleType, target.MultiSampleQuality);
        }

        /// <summary>
        /// Checks if we have support for the DepthFormat in depth and return the information to CreateDepthStencil(RenderTarget2D target)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        private DepthStencilBuffer CreateDepthStencil(RenderTarget2D target, DepthFormat depth)
        {
            if (GraphicsAdapter.DefaultAdapter.CheckDepthStencilMatch(DeviceType.Hardware,
               GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Format, target.Format,
                depth))
            {
                return new DepthStencilBuffer(target.GraphicsDevice, target.Width,
                    target.Height, depth, target.MultiSampleType, target.MultiSampleQuality);
            }
            else
                return CreateDepthStencil(target);
        }

    }
}
