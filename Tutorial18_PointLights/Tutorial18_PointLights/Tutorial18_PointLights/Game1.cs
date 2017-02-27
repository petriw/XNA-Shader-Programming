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

namespace Tutorial18_PointLights
{
    public struct VertexPositionNormalTextureTangentBinormal
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        public Vector3 Tangent;
        public Vector3 Binormal;
        public static readonly VertexElement[] VertexElements =
        new VertexElement[]
    {
        new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
        new VertexElement(0, sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0),
        new VertexElement(0, sizeof(float) * 6, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(0, sizeof(float) * 8, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Tangent, 0),
        new VertexElement(0, sizeof(float) * 11, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Binormal, 0),
    };
        public VertexPositionNormalTextureTangentBinormal(Vector3 position, Vector3 normal, Vector2 textureCoordinate, Vector3 tangent, Vector3 binormal)
        {
            Position = position;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
            Tangent = tangent;
            Binormal = binormal;
        }
        public static int SizeInBytes { get { return sizeof(float) * 14; } }
    }
 

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

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // 3D Object
        Model m_Model;
        Model m_BGModel;

        // Our effect object, this is where our shader will be loaded abd compiled
        Effect effect;


        // Textures
        Texture2D colorMap;
        Texture2D normalMap;
        Texture2D m_Overlay;

        // Matrices
        Matrix renderMatrix, objectMatrix, worldMatrix, viewMatrix, projMatrix;
        Matrix[] bones;
        Matrix[] bones2;




        // Constructor
        public ShaderTutorial()
        {
            Window.Title = "Dark Codex Studios :: Shader programming, Tutorial 18";
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
            m_Overlay = Content.Load<Texture2D>("Overlay");
            colorMap = Content.Load<Texture2D>("diffuse_test");
            normalMap = Content.Load<Texture2D>("normal_test");

            // Vertex declaration for rendering our 3D model.
            graphics.GraphicsDevice.VertexDeclaration = new VertexDeclaration(graphics.GraphicsDevice, VertexPositionNormalTextureTangentBinormal.VertexElements);


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
            m_BGModel = Content.Load<Model>("sphere");
            bones = new Matrix[this.m_Model.Bones.Count];
            this.m_Model.CopyAbsoluteBoneTransformsTo(bones);

            bones2 = new Matrix[this.m_BGModel.Bones.Count];
            this.m_BGModel.CopyAbsoluteBoneTransformsTo(bones2);

            
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
            zHeight = 30;

            float m = (float)gameTime.ElapsedGameTime.Milliseconds / 1000;
            moveObject += m;

            // Move our object by doing some simple matrix calculations.
            objectMatrix = Matrix.CreateRotationX(moveObject / 2) * Matrix.CreateRotationZ(-moveObject / 4);
            renderMatrix = Matrix.CreateScale(0.8f);
            viewMatrix = Matrix.CreateLookAt(new Vector3(x, y, zHeight), Vector3.Zero, Vector3.Up);

            renderMatrix = objectMatrix * renderMatrix;

            Vector3 vLightPosition = new Vector3(50 * (0.5f * (float)Math.Sin(moveObject)), 0.0f, 50 * (0.5f * (float)Math.Cos(moveObject)));
            Vector3 vLightPosition2 = new Vector3(-50 * (0.5f * (float)Math.Sin(moveObject)), 50 * (0.5f * (float)Math.Cos(moveObject)), 50 * (0.5f * (float)Math.Cos(moveObject)));
            Vector3 vLightPosition3 = new Vector3(50 * (0.5f * (float)Math.Cos(moveObject)), 50 * (0.5f * (float)Math.Sin(moveObject)), 0.0f);
            Vector4 vLightColor = new Vector4(0.0f, 0.5f, 1.0f, 1.0f);
            Vector4 vLightColor2 = new Vector4(0.0f, 0.0f, 0.5f, 1.0f);
            Vector4 vLightColor3 = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            Vector4 vecEye = new Vector4(x, y, zHeight, 0);

            effect.Parameters["vecEye"].SetValue(vecEye);
            effect.Parameters["matWorldViewProj"].SetValue(worldMatrix * viewMatrix * projMatrix);
            effect.Parameters["matWorld"].SetValue(worldMatrix);

            effect.Parameters["ColorMap"].SetValue(colorMap);
            effect.Parameters["NormalMap"].SetValue(normalMap);

            // Set the light positions for each of the lights
            effect.Parameters["vecLightPos"].SetValue(vLightPosition);
            effect.Parameters["vecLightPos2"].SetValue(vLightPosition2);
            effect.Parameters["vecLightPos3"].SetValue(vLightPosition3);

            // Set the light range and color for each of the lights
            effect.Parameters["LightRange"].SetValue(100.0f);
            effect.Parameters["LightColor"].SetValue(vLightColor);
            effect.Parameters["LightRange2"].SetValue(20.0f);
            effect.Parameters["LightColor2"].SetValue(vLightColor2);
            effect.Parameters["LightRange3"].SetValue(20.0f);
            effect.Parameters["LightColor3"].SetValue(vLightColor3);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);
            graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;


            // Use the AmbientLight technique from Shader.fx. You can have multiple techniques in a effect file. If you don't specify
            // what technique you want to use, it will choose the first one by default.
            effect.CurrentTechnique = effect.Techniques["PointLight"];

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
                        worldMatrix = bones[mesh.ParentBone.Index] * renderMatrix * Matrix.CreateScale(10.0f);

                        // .. and pass it into our shader.
                        // To access a parameter defined in our shader file ( Shader.fx ), use effectObject.Parameters["variableName"]
                        effect.Parameters["matWorldViewProj"].SetValue(worldMatrix * viewMatrix * projMatrix);
                        effect.Parameters["matWorld"].SetValue(worldMatrix);
                        

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

            effect.Begin();


            // A shader can have multiple passes, be sure to loop trough each of them.
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                // Begin current pass
                pass.Begin();

                foreach (ModelMesh mesh in m_BGModel.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        // calculate our worldMatrix..
                        worldMatrix = bones2[mesh.ParentBone.Index] * renderMatrix;


                        // .. and pass it into our shader.
                        // To access a parameter defined in our shader file ( Shader.fx ), use effectObject.Parameters["variableName"]
                        effect.Parameters["matWorldViewProj"].SetValue(worldMatrix * viewMatrix * projMatrix);
                        effect.Parameters["matWorld"].SetValue(worldMatrix);

                        


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

            // Draw our overlay
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            {
                spriteBatch.Draw(m_Overlay, new Rectangle(0, 0, 800, 600), Color.White);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
