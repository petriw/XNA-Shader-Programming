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

using System.Collections.Generic;
using System.Collections;

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
        float DoFRange = 10.0f;
        float DoFDistance = 70.0f;

        // Default XNA objects
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // 3D Object
        Model m_Model;

        // Our effect object, this is where our shader will be loaded and compiled
        Effect effect;
        Effect effectPostDoF;
        Effect effectPostBlur;

        // Our techniques
        EffectTechnique environmentShader;
        EffectTechnique depthMapShader;
        EffectTechnique DoFShader;

        // Model texture
        Texture2D m_ColorMap;
        Texture2D m_Overlay;
        Texture2D m_BGScene;

        // Render targets
        RenderTarget2D renderTarget;
        Texture2D SceneTexture;
        RenderTarget2D renderBluredTarget;
        Texture2D BlureSceneTexture;
        RenderTarget2D renderBluredTarget2;
        RenderTarget2D depthRT;
        DepthStencilBuffer depthSB;

        Texture2D depth1Texture;

        // Matrices
        Matrix renderMatrix, objectMatrix, worldMatrix, viewMatrix, projMatrix;
        Matrix[] bones;

        // DoF parameters
        float focusDistance = 0;
        float focusRange = 0;
        float nearClip = 0;
        float farClip = 0;
        float NearPlane = 10.0f, FarPlane = 150.0f;


        // Constructor
        public ShaderTutorial()
        {
            Window.Title = "Shader programming :: Tutorial 20 - Depth of Field";
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
            float FieldOfView = MathHelper.ToRadians(45.0f);
            projMatrix = Matrix.CreatePerspectiveFieldOfView(FieldOfView, aspectRatio, NearPlane, FarPlane);


            // Load and compile our Shader into our Effect instance.
            effect = Content.Load<Effect>("Shader");
            effectPostDoF = Content.Load<Effect>("PostProcessDoF");
            effectPostBlur = Content.Load<Effect>("PostProcessBlur");
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
            renderBluredTarget = new RenderTarget2D(graphics.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, 1, graphics.GraphicsDevice.DisplayMode.Format);
            renderBluredTarget2 = new RenderTarget2D(graphics.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, 1, graphics.GraphicsDevice.DisplayMode.Format);

            // Depth Stencil buffer
            depthSB = CreateDepthStencil(depthRT, DepthFormat.Depth24Stencil8);

            // Get our techniques and store them in variables.
            environmentShader = effect.Techniques["EnvironmentShader"];
            depthMapShader = effect.Techniques["DepthMapShader"];
            DoFShader = effect.Techniques["PostProcessBlur"];
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
            DoFRange += state.ThumbSticks.Right.X;
            DoFDistance += state.ThumbSticks.Right.Y;

            if (DoFRange <= 0)
                DoFRange = 0;

            if (DoFDistance <= 0)
                DoFDistance = 0;

            // Move our object by doing some simple matrix calculations.
            objectMatrix = Matrix.CreateRotationX(moveObject.X / 10) * Matrix.CreateRotationZ(-moveObject.Y / 10);
            renderMatrix = Matrix.CreateScale(0.4f);
            viewMatrix = Matrix.CreateLookAt(new Vector3(x, y, 80), Vector3.Zero, Vector3.Up);

            renderMatrix = objectMatrix * renderMatrix;


            // Set our shader values
            Vector4 vecEye = new Vector4(x, y, 80, 0);
            Vector4 vColorDiffuse = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            Vector4 vColorSpecular = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
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

        void  SetShaderParameters( float fD, float fR, float nC, float fC )
        {
            focusDistance = fD;
            focusRange = fR;
            nearClip = nC;
            farClip = fC;
            farClip = farClip / ( farClip - nearClip );

            effectPostDoF.Parameters["Distance"].SetValue(focusDistance);
            effectPostDoF.Parameters["Range"].SetValue(focusRange);
            effectPostDoF.Parameters["Near"].SetValue(nearClip);
            effectPostDoF.Parameters["Far"].SetValue(farClip);
        }

        /// <summary>
        /// Now as we are increasing the number of rendertargets, we need to keep track of our scene so it renders the same way
        /// each time we are rendering the scene to a texture.
        /// 
        /// Thes function should render our scene.
        /// </summary>
        void DrawScene()
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
            GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            depth1Texture = RenderDepthMap(depthSB,depthRT);

            // render our scene
            graphics.GraphicsDevice.SetRenderTarget(0, renderTarget);
            graphics.GraphicsDevice.Clear(Color.White);

            // Draw our overlay
            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            {
                spriteBatch.Draw(m_BGScene, new Rectangle(0, 0, 800, 600), Color.White);
            }
            spriteBatch.End();

            effect.CurrentTechnique = environmentShader;
            DrawScene();
            
            graphics.GraphicsDevice.SetRenderTarget(0, null);
            SceneTexture = renderTarget.GetTexture();

            // Render the scene with with post process blur.
            graphics.GraphicsDevice.SetRenderTarget(0, renderBluredTarget);
            graphics.GraphicsDevice.Clear(Color.White);

            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            {
                // Apply the post process shader
                effectPostBlur.Begin();
                {
                    effectPostBlur.CurrentTechnique.Passes[0].Begin();
                    {
                        spriteBatch.Draw(SceneTexture, new Rectangle(0, 0, 800, 600), Color.White);
                        effectPostBlur.CurrentTechnique.Passes[0].End();
                    }
                }
                effectPostBlur.End();
            }
            spriteBatch.End();
            graphics.GraphicsDevice.SetRenderTarget(0, null);
            BlureSceneTexture = renderBluredTarget.GetTexture();

            // Render the scene with with post process blur 2nd pass
            graphics.GraphicsDevice.SetRenderTarget(0, renderBluredTarget2);
            graphics.GraphicsDevice.Clear(Color.White);

            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            {
                // Apply the post process shader
                effectPostBlur.Begin();
                {
                    effectPostBlur.CurrentTechnique.Passes[0].Begin();
                    {
                        spriteBatch.Draw(BlureSceneTexture, new Rectangle(0, 0, 800, 600), Color.White);
                        effectPostBlur.CurrentTechnique.Passes[0].End();
                    }
                }
                effectPostBlur.End();
            }
            spriteBatch.End();
            graphics.GraphicsDevice.SetRenderTarget(0, null);
            BlureSceneTexture = renderBluredTarget2.GetTexture();

            // Render the scene with with post process DoF.
            graphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkSlateBlue, 1.0f, 0);

            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            {
                SetShaderParameters(DoFDistance, DoFRange, NearPlane, FarPlane);

                // Apply the post process shader
                effectPostDoF.Begin();
                {
                    effectPostDoF.CurrentTechnique.Passes[0].Begin();
                    {
                        effectPostDoF.Parameters["D1M"].SetValue(depth1Texture);
                        effectPostDoF.Parameters["BlurScene"].SetValue(BlureSceneTexture);
                        spriteBatch.Draw(SceneTexture, new Rectangle(0, 0, 800, 600), Color.White);
                        effectPostDoF.CurrentTechnique.Passes[0].End();
                    }
                }
                effectPostDoF.End();
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

            DrawScene();

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
