﻿#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using DeveMazeGenerator;
using DeveMazeGenerator.Generators;
#endregion

namespace DeveMazeGeneratorMonoGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Camera camera;

        private BasicEffect effect;

        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;

        private VertexBuffer vertexBufferPath;
        private IndexBuffer indexBufferPath;

        private int curMazeWidth = 32;
        private int curMazeHeight = 32;
        private int wallsCount = 0;
        private int pathCount = 0;

        private bool drawRoof = true;

        private bool lighting = false;

        private float numbertje = -1f;

        public Game1()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);

            if (!true)
            {
                graphics.PreferredBackBufferWidth = 1700;
                graphics.PreferredBackBufferHeight = 900;
            }
            else
            {
                graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                graphics.IsFullScreen = true;
            }

            GenerateMaze();

            Content.RootDirectory = "Content";

            camera = new Camera(this);

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

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
            effect = new BasicEffect(GraphicsDevice);

            ContentDing.GoLoadContent(GraphicsDevice, Content);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        public void GenerateMaze()
        {
            if (indexBuffer != null)
                indexBuffer.Dispose();
            if (vertexBuffer != null)
                vertexBuffer.Dispose();



            var alg = new AlgorithmBacktrack();
            var maze = alg.Generate(curMazeWidth, curMazeHeight, InnerMapType.BitArreintjeFast, null);
            var walls = maze.GenerateListOfMazeWalls();
            var path = PathFinderDepthFirst.GoFind(maze.InnerMap, null);


            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[walls.Count * 8];
            int[] indices = new int[walls.Count * 12];

            int curVertice = 0;
            int curIndice = 0;



            foreach (var wall in walls)
            {
                //int factorHeight = 10;
                //int factorWidth = 10;

                WallModel model = new WallModel(wall);

                model.GoGenerateVertices(vertices, indices, ref curVertice, ref curIndice);

            }

            wallsCount = walls.Count;

            vertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);

            vertexBuffer.SetData(vertices);
            indexBuffer.SetData(indices);

            GeneratePath(path);
        }

        public void GeneratePath(List<MazePoint> path)
        {
            if (vertexBufferPath != null)
                vertexBufferPath.Dispose();
            if (indexBufferPath != null)
                indexBufferPath.Dispose();


            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[path.Count * 4];
            int[] indices = new int[path.Count * 6];

            int curVertice = 0;
            int curIndice = 0;



            foreach (var pathNode in path)
            {
                //int factorHeight = 10;
                //int factorWidth = 10;

                VierkantjeModel model = new VierkantjeModel();

                model.GoGenerateVertices(pathNode.X, pathNode.Y, vertices, indices, ref curVertice, ref curIndice);

            }

            pathCount = path.Count;

            vertexBufferPath = new VertexBuffer(GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            indexBufferPath = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);

            vertexBufferPath.SetData(vertices);
            indexBufferPath.SetData(indices);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            InputDing.PreUpdate();

            if (InputDing.CurKey.IsKeyDown(Keys.Escape))
                Exit();

            camera.Update(gameTime);


            if (InputDing.KeyDownUp(Keys.Up))
            {
                curMazeWidth *= 2;
                curMazeHeight *= 2;
                GenerateMaze();
            }

            if (InputDing.KeyDownUp(Keys.Down))
            {
                curMazeWidth /= 2;
                curMazeHeight /= 2;
                GenerateMaze();
            }

            if (InputDing.CurKey.IsKeyDown(Keys.R))
            {
                GenerateMaze();
            }

            if (InputDing.KeyDownUp(Keys.H))
            {
                drawRoof = !drawRoof;
            }

            if (InputDing.KeyDownUp(Keys.L))
            {
                lighting = !lighting;
            }

            numbertje += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (InputDing.CurKey.IsKeyDown(Keys.G))
            {
                numbertje = 0;
            }


            InputDing.AfterUpdate();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);




            //GraphicsDevice.BlendState = BlendState.Opaque;

            //DepthStencilState d = new DepthStencilState();
            //d.DepthBufferEnable = true;
            //GraphicsDevice.DepthStencilState = d;

            Matrix worldMatrix = Matrix.Identity;
            effect.World = worldMatrix;
            effect.View = camera.viewMatrix;
            effect.Projection = camera.projectionMatrix;

            //effect.EnableDefaultLighting();
            effect.LightingEnabled = true;
            effect.EmissiveColor = new Vector3(0.25f, 0.25f, 0.25f);
            effect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
            effect.SpecularPower = 0.1f;

            effect.AmbientLightColor = new Vector3(0.25f, 0.25f, 0.25f);
            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight0.Direction = new Vector3(1, -1, -1);
            effect.DirectionalLight0.DiffuseColor = new Vector3(0.75f, 0.75f, 0.75f);
            effect.DirectionalLight0.SpecularColor = new Vector3(0.1f, 0.1f, 0.1f);

            effect.World = Matrix.Identity;
            effect.TextureEnabled = true;


            //Skybox
            effect.LightingEnabled = false;
            effect.Texture = ContentDing.skyTexture1;
            int skyboxSize = 1000000;
            CubeModelInvertedForSkybox skybox = new CubeModelInvertedForSkybox(this, skyboxSize, skyboxSize, skyboxSize, TexturePosInfoGenerator.FullImage);
            Matrix skyboxMatrix = Matrix.CreateTranslation(camera.cameraPosition) * Matrix.CreateTranslation(new Vector3(-skyboxSize / 2, -skyboxSize / 2, -skyboxSize / 2));
            skybox.Draw(skyboxMatrix, effect);
            effect.LightingEnabled = true;




            //effect.Texture = ContentDing.wallTexture;


            //SamplerState newSamplerState = new SamplerState()
            //{
            //    AddressU = TextureAddressMode.Wrap,
            //    AddressV = TextureAddressMode.Wrap,
            //    Filter = TextureFilter.Point
            //};
            //GraphicsDevice.SamplerStates[0] = newSamplerState;

            //foreach (WallModel wallModel in wallModels)
            //{
            //    GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            //    wallModel.Draw(Matrix.Identity, effect);
            //}


            effect.LightingEnabled = false;

            //Ground
            int mazeScale = 10;
            Matrix scaleMatrix = Matrix.CreateScale(mazeScale);
            Matrix growingScaleMatrix = scaleMatrix * Matrix.CreateScale(1, (float)Math.Max(Math.Min(numbertje / 1.0f, 1), 0), 1);

            effect.World = scaleMatrix;

            //effect.Texture = ContentDing.grasTexture;

            SamplerState newSamplerState2 = new SamplerState()
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                Filter = TextureFilter.Point
            };
            GraphicsDevice.SamplerStates[0] = newSamplerState2;



            //int curmazeheight = 100;

            //WallModel wallmodel = new WallModel(this, curmazeheight * 10, curmazeheight * 10, TexturePosInfoGenerator.FullImageWithSize(15), Matrix.Identity);
            //wallmodel.Draw(Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateTranslation(0, 0, curmazeheight * 10), effect);

            effect.Texture = ContentDing.win98FloorTexture;







            CubeModel ground = new CubeModel(this, curMazeWidth - 2, 0.1f, curMazeHeight - 2, TexturePosInfoGenerator.FullImage, 2f / 3f);
            ground.Draw(Matrix.CreateTranslation(0, -0.1f, 0) * scaleMatrix, effect);


            if (drawRoof)
            {
                effect.Texture = ContentDing.win98RoofTexture;

                CubeModel roof = new CubeModel(this, curMazeWidth - 2, 0.1f, curMazeHeight - 2, TexturePosInfoGenerator.FullImage, 2f / 3f);
                roof.Draw(Matrix.CreateTranslation(0, 4f / 3f, 0) * scaleMatrix, effect);
            }

            effect.LightingEnabled = lighting;

            //Start
            effect.Texture = ContentDing.startTexture;
            CubeModel start = new CubeModel(this, 0.75f, 0.75f, 0.75f, TexturePosInfoGenerator.FullImage, 0.75f);
            start.Draw(Matrix.CreateTranslation(0.625f, 0.375f, 0.625f) * growingScaleMatrix, effect);


            //Finish
            effect.Texture = ContentDing.endTexture;
            CubeModel finish = new CubeModel(this, 0.75f, 0.75f, 0.75f, TexturePosInfoGenerator.FullImage, 0.75f);
            finish.Draw(Matrix.CreateTranslation(0.625f, 0.375f, 0.625f) * Matrix.CreateTranslation(curMazeWidth - 4, 0, curMazeHeight - 4) * growingScaleMatrix, effect);


            //Maze
            effect.World = growingScaleMatrix;

            if (vertexBuffer != null && indexBuffer != null)
            {
                GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

                GraphicsDevice.Indices = indexBuffer;
                GraphicsDevice.SetVertexBuffer(vertexBuffer);

                effect.Texture = ContentDing.win98WallTexture;

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
                }

            }

            effect.World = growingScaleMatrix *Matrix.CreateTranslation(0, 0.1f, 0);

            //Path
            if (vertexBufferPath != null && vertexBufferPath != null)
            {
                GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

                GraphicsDevice.Indices = indexBufferPath;
                GraphicsDevice.SetVertexBuffer(vertexBufferPath);

                effect.Texture = ContentDing.win98LegoTexture;

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBufferPath.VertexCount, 0, indexBufferPath.IndexCount / 3);
                }

            }




            //CubeModel redCube = new CubeModel(this, 5, 5, 5, TexturePosInfoGenerator.FullImage);
            //effect.Texture = textureRed;

            //foreach (var node in currentPath)
            //{
            //    Matrix m = Matrix.CreateTranslation(2.5f + 10f * node.X, 2.5f, 2.5f + 10f * node.Y);
            //    redCube.Draw(m, effect);
            //}


            spriteBatch.Begin();

            String stringToDraw = "Size: " + curMazeWidth + ", Walls: " + wallsCount + ", Path length: " + pathCount;

            var meassured = ContentDing.spriteFont.MeasureString(stringToDraw);

            spriteBatch.Draw(ContentDing.semiTransparantTexture, new Rectangle(5, 5, (int)meassured.X + 10, (int)meassured.Y + 10), Color.White);
            spriteBatch.DrawString(ContentDing.spriteFont, stringToDraw, new Vector2(10, 10), Color.White);

            spriteBatch.End();


            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.Draw(gameTime);
        }
    }
}