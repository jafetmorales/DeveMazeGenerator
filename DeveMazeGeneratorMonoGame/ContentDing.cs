﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeveMazeGeneratorMonoGame
{
    public static class ContentDing
    {
        public static Texture2D grasTexture;
        public static Texture2D skyTexture1;
        public static Texture2D skyTexture2;
        public static Texture2D wallTexture;
        public static Texture2D blankTexture;

        public static SpriteFont spriteFont;

        public static void GoLoadContent(GraphicsDevice graphicsDevice, ContentManager Content)
        {
            grasTexture = Content.Load<Texture2D>("gras");
            skyTexture1 = Content.Load<Texture2D>("sky");
            skyTexture2 = Content.Load<Texture2D>("sky2");
            wallTexture = Content.Load<Texture2D>("wall");
            blankTexture = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
            blankTexture.SetData(new[] { Color.White });

            spriteFont = Content.Load<SpriteFont>("SpriteFont1");
        }
    }
}
