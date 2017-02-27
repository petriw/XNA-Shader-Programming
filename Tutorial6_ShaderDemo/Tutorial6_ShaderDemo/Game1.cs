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
using WindowsDemo.Engine;

namespace Tutorial6_ShaderDemo
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ShaderTutorial : Microsoft.Xna.Framework.Game
    {
        // Audio objects
        AudioEngine engine;
        SoundBank soundBank;
        WaveBank waveBank;

        // Variables for Matrix calculations, viewport and object movment
        float width, height;
        float x = 0, y = 0;
        float zHeight = 15.0f;
        float moveObject = 0;
        float fadeHelper = 0.0f;

        bool firstTime = true;
        int mode = 1;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // 3D Object
        Model m_Model;
        Model m_Ocean;
        Model m_Sky;

        Camera m_Camera;

        // Our effect object, this is where our shader will be loaded abd compiled
        Effect effect;
        Effect effectOcean;
        Effect effectBasic;


        // Textures
        Texture2D colorMap;
        Texture2D normalMap;
        Texture2D normalIdentityMap;

        Texture2D colorOceanMap;
        Texture2D normalOceanMap;
        Texture2D reflectOceanMap;

        Texture2D colorSkyMap;
        Texture2D normalSkyMap;

        Texture2D XNAColor;
        Texture2D XNAAlpha;

        // Matrices
        Matrix renderMatrix, objectMatrix, worldMatrix, viewMatrix, projMatrix;
        Matrix[] bones, oceanBones, skyBones;

        int fadeLogo = 255;



        // Constructor
        public ShaderTutorial()
        {
            Window.Title = "Dark Codex Studios :: XNA Shader programming Demo";
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferMultiSampling = true;
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.IsFullScreen = false;

            m_Model = null;

            // Initialize audio objects.
            engine = new AudioEngine("Content\\Music.xgs");
            soundBank = new SoundBank(engine, "Content\\sound.xsb");
            waveBank = new WaveBank(engine, "Content\\wave.xwb");
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

            this.m_Camera = new Camera(this.GraphicsDevice.Viewport);

            // Set worldMatrix to Identity
            worldMatrix = Matrix.Identity;

            projMatrix = m_Camera.ProjectionMatrix;

            // Load and compile our Shader into our Effect instance.
            effect = Content.Load<Effect>("Shader");
            effectOcean = Content.Load<Effect>("OceanFX");
            effectBasic = Content.Load<Effect>("Basic");

            colorMap = Content.Load<Texture2D>("island");
            normalMap = Content.Load<Texture2D>("islandNormal");
            XNAColor = Content.Load<Texture2D>("xnacolor");
            XNAAlpha = Content.Load<Texture2D>("xnalpha");

            colorSkyMap = Content.Load<Texture2D>("sky_dome");
            normalSkyMap = Content.Load<Texture2D>("normalIdentity");

            colorOceanMap = Content.Load<Texture2D>("water");
            normalOceanMap = Content.Load<Texture2D>("wavesbump");
            reflectOceanMap = Content.Load<Texture2D>("sky_dome2");

            // Vertex declaration for rendering our 3D model.
            graphics.GraphicsDevice.VertexDeclaration = new VertexDeclaration(graphics.GraphicsDevice, VertexPositionNormalTexture.VertexElements);

            graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;



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
            m_Model = Content.Load<Model>("fy_faen_ass");
            bones = new Matrix[this.m_Model.Bones.Count];
            this.m_Model.CopyAbsoluteBoneTransformsTo(bones);

            m_Ocean = Content.Load<Model>("ocean");
            oceanBones = new Matrix[this.m_Ocean.Bones.Count];
            this.m_Ocean.CopyAbsoluteBoneTransformsTo(oceanBones);

            m_Sky = Content.Load<Model>("sphere");
            skyBones = new Matrix[this.m_Sky.Bones.Count];
            this.m_Sky.CopyAbsoluteBoneTransformsTo(skyBones);

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
            if (firstTime)
            {
                // Play the sound.
                soundBank.PlayCue("soundscape_wav2");
                firstTime = false;
            }


            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            GamePadState currentState = GamePad.GetState(PlayerIndex.One);
            if (currentState.Buttons.A == ButtonState.Pressed)
            {
                mode = 1;
            }
            if (currentState.Buttons.X == ButtonState.Pressed)
            {
                mode = 0;
            }

            //mode = 0;

            m_Camera.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            zHeight = 100;

            float m = (float)gameTime.ElapsedGameTime.Milliseconds / 1000;
            moveObject += m;
            fadeHelper += m * 100;

            // Move our object by doing some simple matrix calculations.
            objectMatrix = Matrix.CreateTranslation(Vector3.Zero);
            renderMatrix = Matrix.CreateScale(1.0f);
            viewMatrix = m_Camera.ViewMatrix;

            renderMatrix = objectMatrix * renderMatrix;


            if (fadeHelper >= 1.0f)
            {
                fadeHelper = 0;
                fadeLogo--;
                if (fadeLogo <= 0)
                    fadeLogo = 0;
            }

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // Use the AmbientLight technique from Shader.fx. You can have multiple techniques in a effect file. If you don't specify
            // what technique you want to use, it will choose the first one by default.


            switch (mode)
            {
                case 0:
                    {
                        effect.CurrentTechnique = effect.Techniques["NormalMapping"];

                        // Begin our effect
                        effect.Begin();

                        renderMatrix = Matrix.CreateScale(1.0f) * Matrix.CreateRotationZ(MathHelper.ToRadians(270));
                        // A shader can have multiple passes, be sure to loop trough each of them.
                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            // Begin current pass
                            pass.Begin();

                            foreach (ModelMesh mesh in m_Sky.Meshes)
                            {
                                foreach (ModelMeshPart part in mesh.MeshParts)
                                {
                                    // calculate our worldMatrix..
                                    worldMatrix = skyBones[mesh.ParentBone.Index] * renderMatrix;

                                    Vector4 vecEye = new Vector4(m_Camera.Position.X, m_Camera.Position.Y, m_Camera.Position.Z, 0);


                                    // .. and pass it into our shader.
                                    // To access a parameter defined in our shader file ( Shader.fx ), use effectObject.Parameters["variableName"]
                                    Matrix worldInverse = Matrix.Invert(worldMatrix);
                                    Vector4 vLightDirection = new Vector4(0.5f, 1.0f, 0.0f, 1.0f);
                                    effect.Parameters["matWorldViewProj"].SetValue(worldMatrix * viewMatrix * projMatrix);
                                    effect.Parameters["matWorld"].SetValue(worldMatrix);
                                    effect.Parameters["vecEye"].SetValue(vecEye);
                                    effect.Parameters["vecLightDir"].SetValue(vLightDirection);
                                    effect.Parameters["ColorMap"].SetValue(colorMap);
                                    effect.Parameters["NormalMap"].SetValue(normalMap);
                                    effect.Parameters["A"].SetValue(0.5f);

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


                        effect.CurrentTechnique = effect.Techniques["NormalMapping"];

                        // Begin our effect
                        effect.Begin();

                        renderMatrix = Matrix.CreateTranslation(-0.6f, 0, 0) * Matrix.CreateScale(100.0f) * Matrix.CreateRotationZ(MathHelper.ToRadians(90)) * Matrix.CreateRotationY(MathHelper.ToRadians(moveObject));
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
                                    worldMatrix = Matrix.Identity;

                                    worldMatrix = bones[mesh.ParentBone.Index] * renderMatrix;

                                    Vector4 vecEye = new Vector4(m_Camera.Position.X, m_Camera.Position.Y, m_Camera.Position.Z, 0);


                                    // .. and pass it into our shader.
                                    // To access a parameter defined in our shader file ( Shader.fx ), use effectObject.Parameters["variableName"]
                                    Matrix worldInverse = Matrix.Invert(worldMatrix);
                                    Vector4 vLightDirection = new Vector4(1.0f, 0.0f, -1.0f, 1.0f);
                                    effect.Parameters["matWorldViewProj"].SetValue(worldMatrix * viewMatrix * projMatrix);
                                    effect.Parameters["matWorld"].SetValue(worldMatrix);
                                    effect.Parameters["vecEye"].SetValue(vecEye);
                                    effect.Parameters["vecLightDir"].SetValue(vLightDirection);
                                    effect.Parameters["ColorMap"].SetValue(colorSkyMap);
                                    effect.Parameters["NormalMap"].SetValue(normalSkyMap);
                                    effect.Parameters["A"].SetValue(2.0f);

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


                        effectOcean.CurrentTechnique = effectOcean.Techniques["OceanEffect"];

                        // Begin our effect
                        effectOcean.Begin();

                        renderMatrix = Matrix.CreateScale(50.0f) * Matrix.CreateRotationX(MathHelper.ToRadians(270)) * Matrix.CreateTranslation(0, -60, 0);
                        // A shader can have multiple passes, be sure to loop trough each of them.
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                        graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                        graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;

                        foreach (EffectPass pass in effectOcean.CurrentTechnique.Passes)
                        {
                            // Begin current pass
                            pass.Begin();

                            foreach (ModelMesh mesh in m_Ocean.Meshes)
                            {
                                foreach (ModelMeshPart part in mesh.MeshParts)
                                {
                                    // calculate our worldMatrix..
                                    worldMatrix = oceanBones[mesh.ParentBone.Index] * renderMatrix;

                                    Vector4 vecEye = new Vector4(m_Camera.Position.X, m_Camera.Position.Y, m_Camera.Position.Z, 0);


                                    // .. and pass it into our shader.
                                    // To access a parameter defined in our shader file ( Shader.fx ), use effectObject.Parameters["variableName"]
                                    Matrix worldInverse = Matrix.Invert(worldMatrix);
                                    Vector4 vLightDirection = new Vector4(1.0f, 0.0f, -1.0f, 1.0f);
                                    effectOcean.Parameters["matWorldViewProj"].SetValue(worldMatrix * viewMatrix * projMatrix);
                                    effectOcean.Parameters["matWorld"].SetValue(worldMatrix);
                                    effectOcean.Parameters["vecEye"].SetValue(vecEye);
                                    effectOcean.Parameters["vecLightDir"].SetValue(vLightDirection);
                                    effectOcean.Parameters["ColorMap"].SetValue(colorOceanMap);
                                    effectOcean.Parameters["BumpMap"].SetValue(normalOceanMap);
                                    effectOcean.Parameters["EnvMap"].SetValue(reflectOceanMap);
                                    effectOcean.Parameters["time"].SetValue(moveObject / 2);
                                    effectOcean.Parameters["A"].SetValue(1.0f);
                                    effectOcean.Parameters["bSpecular"].SetValue(true);

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
                        effectOcean.End();

                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                        break;
                    }
                case 1:
                    {
                        effect.CurrentTechnique = effect.Techniques["NormalMapping"];

                        // Begin our effect
                        effect.Begin();

                        renderMatrix = Matrix.CreateScale(50.0f) * Matrix.CreateRotationX(MathHelper.ToRadians(270)) * Matrix.CreateTranslation(0, -60, 0);
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

                                    Vector4 vecEye = new Vector4(m_Camera.Position.X, m_Camera.Position.Y, m_Camera.Position.Z, 0);


                                    // .. and pass it into our shader.
                                    // To access a parameter defined in our shader file ( Shader.fx ), use effectObject.Parameters["variableName"]
                                    Matrix worldInverse = Matrix.Invert(worldMatrix);
                                    Vector4 vLightDirection = new Vector4(0.5f, 1.0f, 0.0f, 1.0f);
                                    effect.Parameters["matWorldViewProj"].SetValue(worldMatrix * viewMatrix * projMatrix);
                                    effect.Parameters["matWorld"].SetValue(worldMatrix);
                                    effect.Parameters["vecEye"].SetValue(vecEye);
                                    effect.Parameters["vecLightDir"].SetValue(vLightDirection);
                                    effect.Parameters["ColorMap"].SetValue(colorOceanMap);
                                    effect.Parameters["NormalMap"].SetValue(normalSkyMap);
                                    effect.Parameters["A"].SetValue(0.5f);

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


                        effect.CurrentTechnique = effect.Techniques["NormalMapping"];

                        // Begin our effect
                        effect.Begin();

                        renderMatrix = Matrix.CreateTranslation(-0.6f, 0, 0) * Matrix.CreateScale(100.0f) * Matrix.CreateRotationZ(MathHelper.ToRadians(90)) * Matrix.CreateRotationY(MathHelper.ToRadians(moveObject));
                        // A shader can have multiple passes, be sure to loop trough each of them.
                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            // Begin current pass
                            pass.Begin();

                            foreach (ModelMesh mesh in m_Ocean.Meshes)
                            {
                                foreach (ModelMeshPart part in mesh.MeshParts)
                                {
                                    // calculate our worldMatrix..
                                    worldMatrix = Matrix.Identity;

                                    worldMatrix = oceanBones[mesh.ParentBone.Index] * renderMatrix;

                                    Vector4 vecEye = new Vector4(m_Camera.Position.X, m_Camera.Position.Y, m_Camera.Position.Z, 0);


                                    // .. and pass it into our shader.
                                    // To access a parameter defined in our shader file ( Shader.fx ), use effectObject.Parameters["variableName"]
                                    Matrix worldInverse = Matrix.Invert(worldMatrix);
                                    Vector4 vLightDirection = new Vector4(1.0f, 0.0f, -1.0f, 1.0f);
                                    effect.Parameters["matWorldViewProj"].SetValue(worldMatrix * viewMatrix * projMatrix);
                                    effect.Parameters["matWorld"].SetValue(worldMatrix);
                                    effect.Parameters["vecEye"].SetValue(vecEye);
                                    effect.Parameters["vecLightDir"].SetValue(vLightDirection);
                                    effect.Parameters["ColorMap"].SetValue(colorSkyMap);
                                    effect.Parameters["NormalMap"].SetValue(normalSkyMap);
                                    effect.Parameters["A"].SetValue(2.0f);

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


                        effect.CurrentTechnique = effect.Techniques["NormalMapping"];

                        // Begin our effect
                        effect.Begin();

                        renderMatrix = Matrix.CreateScale(1.0f) * Matrix.CreateRotationZ(MathHelper.ToRadians(270));
                        // A shader can have multiple passes, be sure to loop trough each of them.
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                        graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                        graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;

                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            // Begin current pass
                            pass.Begin();

                            foreach (ModelMesh mesh in m_Sky.Meshes)
                            {
                                foreach (ModelMeshPart part in mesh.MeshParts)
                                {
                                    // calculate our worldMatrix..
                                    worldMatrix = skyBones[mesh.ParentBone.Index] * renderMatrix;

                                    Vector4 vecEye = new Vector4(m_Camera.Position.X, m_Camera.Position.Y, m_Camera.Position.Z, 0);


                                    // .. and pass it into our shader.
                                    // To access a parameter defined in our shader file ( Shader.fx ), use effectObject.Parameters["variableName"]
                                    Matrix worldInverse = Matrix.Invert(worldMatrix);
                                    Vector4 vLightDirection = new Vector4(1.0f, 0.0f, -1.0f, 1.0f);
                                    effect.Parameters["matWorldViewProj"].SetValue(worldMatrix * viewMatrix * projMatrix);
                                    effect.Parameters["matWorld"].SetValue(worldMatrix);
                                    effect.Parameters["vecEye"].SetValue(vecEye);
                                    effect.Parameters["vecLightDir"].SetValue(vLightDirection);
                                    effect.Parameters["ColorMap"].SetValue(colorMap);
                                    effect.Parameters["NormalMap"].SetValue(normalSkyMap);
                                    effect.Parameters["A"].SetValue(1.0f);

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

                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                        break;
                    }
            }



            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            //this.spriteBatch.Draw(this.XNAAlpha, new Vector2(0, 0), new Color((byte)255, (byte)255, (byte)255, (byte)(255)));
            this.spriteBatch.Draw(this.XNAColor, new Vector2(0, 0), new Color((byte)255, (byte)255, (byte)255, (byte)fadeLogo));
            this.spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
