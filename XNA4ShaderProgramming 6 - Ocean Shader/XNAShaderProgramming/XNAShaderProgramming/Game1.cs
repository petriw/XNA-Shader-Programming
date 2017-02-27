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

namespace XNAShaderProgramming
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Model theMesh;
        Model theOceanMesh;

        Texture2D diffuseIslandTexture;
        Texture2D normalIslandTexture;

        Texture2D diffuseOceanTexture;
        Texture2D normalOceanTexture;

        // The object that will contain our shader
        Effect effect;
        Effect oceanEffect;

        // Parameters for Island shader
        EffectParameter projectionIslandParameter;
        EffectParameter viewIslandParameter;
        EffectParameter worldIslandParameter;
        EffectParameter ambientIntensityIslandParameter;
        EffectParameter ambientColorIslandParameter;

        EffectParameter diffuseIntensityIslandParameter;
        EffectParameter diffuseColorIslandParameter;
        EffectParameter lightDirectionIslandParameter;

        EffectParameter eyePosIslandParameter;
        EffectParameter specularColorIslandParameter;

        EffectParameter colorMapTextureIslandParameter;
        EffectParameter normalMapTextureIslandParameter;


        // Parameters for Ocean shader
        EffectParameter projectionOceanParameter;
        EffectParameter viewOceanParameter;
        EffectParameter worldOceanParameter;
        EffectParameter ambientIntensityOceanParameter;
        EffectParameter ambientColorOceanParameter;

        EffectParameter diffuseIntensityOceanParameter;
        EffectParameter diffuseColorOceanParameter;
        EffectParameter lightDirectionOceanParameter;

        EffectParameter eyePosOceanParameter;
        EffectParameter specularColorOceanParameter;

        EffectParameter colorMapTextureOceanParameter;
        EffectParameter normalMapTextureOceanParameter;
        EffectParameter totalTimeOceanParameter;

        Matrix rotateIslandMatrix;
        Matrix world, view, projection;
        Vector4 ambientLightColor;
        Vector3 eyePos;
        float totalTime = 0.0f;

        double rotateObjects = -2000.0f;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        public void SetupOceanShaderParameters()
        {
            // Bind the parameters with the shader.
            worldOceanParameter = oceanEffect.Parameters["World"];
            viewOceanParameter = oceanEffect.Parameters["View"];
            projectionOceanParameter = oceanEffect.Parameters["Projection"];

            ambientColorOceanParameter = oceanEffect.Parameters["AmbientColor"];
            ambientIntensityOceanParameter = oceanEffect.Parameters["AmbientIntensity"];

            diffuseColorOceanParameter = oceanEffect.Parameters["DiffuseColor"];
            diffuseIntensityOceanParameter = oceanEffect.Parameters["DiffuseIntensity"];
            lightDirectionOceanParameter = oceanEffect.Parameters["LightDirection"];

            eyePosOceanParameter = oceanEffect.Parameters["EyePosition"];
            specularColorOceanParameter = oceanEffect.Parameters["SpecularColor"];

            colorMapTextureOceanParameter = oceanEffect.Parameters["ColorMap"];
            normalMapTextureOceanParameter = oceanEffect.Parameters["NormalMap"];
            totalTimeOceanParameter = oceanEffect.Parameters["TotalTime"];
        }

        public void SetupIslandShaderParameters()
        {
            // Bind the parameters with the shader.
            worldIslandParameter = effect.Parameters["World"];
            viewIslandParameter = effect.Parameters["View"];
            projectionIslandParameter = effect.Parameters["Projection"];

            ambientColorIslandParameter = effect.Parameters["AmbientColor"];
            ambientIntensityIslandParameter = effect.Parameters["AmbientIntensity"];

            diffuseColorIslandParameter = effect.Parameters["DiffuseColor"];
            diffuseIntensityIslandParameter = effect.Parameters["DiffuseIntensity"];
            lightDirectionIslandParameter = effect.Parameters["LightDirection"];

            eyePosIslandParameter = effect.Parameters["EyePosition"];
            specularColorIslandParameter = effect.Parameters["SpecularColor"];

            colorMapTextureIslandParameter = effect.Parameters["ColorMap"];
            normalMapTextureIslandParameter = effect.Parameters["NormalMap"];
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        /// 
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            theMesh = Content.Load<Model>("fy_faen_ass");
            theOceanMesh = Content.Load<Model>("ocean");

            // Load the shader
            effect = Content.Load<Effect>("Shader");
            oceanEffect = Content.Load<Effect>("OceanShader");

            // Set up the parameters
            SetupIslandShaderParameters();
            SetupOceanShaderParameters();

            // calculate matrixes
            float aspectRatio = (float)graphics.GraphicsDevice.Viewport.Width / (float)graphics.GraphicsDevice.Viewport.Height;
            float fov = MathHelper.PiOver4 * aspectRatio * 3 / 4;
            projection = Matrix.CreatePerspectiveFieldOfView(fov, aspectRatio, 0.1f, 10000.0f);

            diffuseIslandTexture = Content.Load<Texture2D>("island");
            normalIslandTexture = Content.Load<Texture2D>("islandNormal");

            diffuseOceanTexture = Content.Load<Texture2D>("water");
            normalOceanTexture = Content.Load<Texture2D>("wavesbump");

            //create a default world matrix
            world = Matrix.Identity;

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
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            ambientLightColor = Color.White.ToVector4();

            rotateObjects += gameTime.ElapsedGameTime.Milliseconds / 10000.0;
            totalTime += gameTime.ElapsedGameTime.Milliseconds / 5000.0f;
            eyePos = new Vector3(1000.0f * (float)Math.Sin(rotateObjects), 50.0f, 1000.0f * (float)Math.Cos(rotateObjects));
            view = Matrix.CreateLookAt(eyePos, new Vector3(0, 2, 0), Vector3.Up);

            rotateIslandMatrix = Matrix.CreateRotationY((float)0);

            base.Update(gameTime);
        }

        protected void DrawOcean(GameTime gameTime)
        {
            ModelMesh mesh = theOceanMesh.Meshes[0];
            ModelMeshPart meshPart = mesh.MeshParts[0];

            // Set parameters
            projectionOceanParameter.SetValue(projection);
            viewOceanParameter.SetValue(view);
            worldOceanParameter.SetValue( Matrix.CreateRotationY((float)MathHelper.ToRadians((int)270)) * Matrix.CreateRotationZ((float)MathHelper.ToRadians((int)90)) * Matrix.CreateScale(100.0f) * Matrix.CreateTranslation(0, -60, 0)); //Matrix.CreateScale(50.0f) * Matrix.CreateRotationX(MathHelper.ToRadians(270)) * Matrix.CreateTranslation(0, -60, 0);
            ambientIntensityOceanParameter.SetValue(0.4f);
            ambientColorOceanParameter.SetValue(ambientLightColor);
            diffuseColorOceanParameter.SetValue(Color.White.ToVector4());
            diffuseIntensityOceanParameter.SetValue(0.2f);
            specularColorOceanParameter.SetValue(Color.White.ToVector4());
            eyePosOceanParameter.SetValue(eyePos);
            colorMapTextureOceanParameter.SetValue(diffuseOceanTexture);
            normalMapTextureOceanParameter.SetValue(normalOceanTexture);
            totalTimeOceanParameter.SetValue(totalTime);

            Vector3 lightDirection = new Vector3(1.0f, 0.0f, -1.0f);

            //ensure the light direction is normalized, or
            //the shader will give some weird results
            lightDirection.Normalize();
            lightDirectionOceanParameter.SetValue(lightDirection);

            //set the vertex source to the mesh's vertex buffer
            graphics.GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);

            //set the current index buffer to the sample mesh's index buffer
            graphics.GraphicsDevice.Indices = meshPart.IndexBuffer;

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            oceanEffect.CurrentTechnique = oceanEffect.Techniques["Technique1"];

            for (int i = 0; i < oceanEffect.CurrentTechnique.Passes.Count; i++)
            {
                //EffectPass.Apply will update the device to
                //begin using the state information defined in the current pass
                oceanEffect.CurrentTechnique.Passes[i].Apply();

                //theMesh contains all of the information required to draw
                //the current mesh
                graphics.GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList, 0, 0,
                    meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
            }
        }

        protected void DrawIsland(GameTime gameTime)
        {
            ModelMesh mesh = theMesh.Meshes[0];
            ModelMeshPart meshPart = mesh.MeshParts[0];

            // Set parameters
            projectionIslandParameter.SetValue(projection);
            viewIslandParameter.SetValue(view);
            worldIslandParameter.SetValue(world * rotateIslandMatrix);
            ambientIntensityIslandParameter.SetValue(0.3f);
            ambientColorIslandParameter.SetValue(ambientLightColor);
            diffuseColorIslandParameter.SetValue(Color.White.ToVector4());
            diffuseIntensityIslandParameter.SetValue(0.7f);
            specularColorIslandParameter.SetValue(Color.White.ToVector4());
            eyePosIslandParameter.SetValue(eyePos);
            colorMapTextureIslandParameter.SetValue(diffuseIslandTexture);
            normalMapTextureIslandParameter.SetValue(normalIslandTexture);

            Vector3 lightDirection = new Vector3(1, -1, 1);

            //ensure the light direction is normalized, or
            //the shader will give some weird results
            lightDirection.Normalize();
            lightDirectionIslandParameter.SetValue(lightDirection);

            //set the vertex source to the mesh's vertex buffer
            graphics.GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);

            //set the current index buffer to the sample mesh's index buffer
            graphics.GraphicsDevice.Indices = meshPart.IndexBuffer;

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            effect.CurrentTechnique = effect.Techniques["Technique1"];

            for (int i = 0; i < effect.CurrentTechnique.Passes.Count; i++)
            {
                //EffectPass.Apply will update the device to
                //begin using the state information defined in the current pass
                effect.CurrentTechnique.Passes[i].Apply();

                //theMesh contains all of the information required to draw
                //the current mesh
                graphics.GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList, 0, 0,
                    meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
            }
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SkyBlue);

            DrawIsland(gameTime);
            DrawOcean(gameTime);

            base.Draw(gameTime);
        }
    }
}
