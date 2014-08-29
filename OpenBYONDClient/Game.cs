using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using log4net;

namespace OpenBYOND.Client
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class OpenBYONDGame : Game
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Utils));
        private SpriteBatch[] batchPool = new SpriteBatch[3];
        public int[] drawCount = new int[3];

        public SpriteBatch BATCHPOOL
        {
            get
            {
                for (var i = 0; i < batchPool.Length; i++)
                {
                    // If the current spritebatch has less than 1000 draws it's good to go.
                    if (drawCount[i] < 10000)
                    {
                        return batchPool[i];
                    }
                }
                SpriteBatch sb = new SpriteBatch(base.GraphicsDevice);
                batchPool[batchPool.Length] = sb;
                drawCount[Array.IndexOf(batchPool, sb)] = 0;
                return sb;
            }
        }
        GraphicsDeviceManager graphics;
        int cdir = 0;
        uint tick = 0;
        Direction[] Wiggle = new[] {
            Direction.WEST,
            Direction.SOUTH,
            Direction.EAST,
            Direction.SOUTH
        };

        public OpenBYONDGame()
        {
            log.Info("BYONDGame Starting.");
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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
            log.Info("LoadContent()");
            for (var i = 0; i < batchPool.Length; i++)
            {
                batchPool[i] = new SpriteBatch(base.GraphicsDevice);
                drawCount[i] = 0;
            }
 
            // Create a new SpriteBatch, which can be used to draw textures.
            DMIManager.Preload("TestFiles/human.dmi");
 
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
            // TODO: Add your update logic here

            // Every 30 ticks...
            // TODO: Make it every 3 seconds (BYOND: 1 tick = 0.1 second)
            if ((tick++ % 30) == 0)
            {
                cdir = (cdir + 1) % Wiggle.Length;
            }
            else
            {
                SuppressDraw();
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            Texture2D texture1px = new Texture2D(graphics.GraphicsDevice, 1, 1);
            texture1px.SetData(new Color[] { Color.Black });
            SpriteBatch spriteBatch = BATCHPOOL;
                        
            spriteBatch.Begin();
            var index = Array.IndexOf(batchPool, spriteBatch);
            for (float x = -30; x < 30; x++)
            {
                Rectangle rectangle = new Rectangle((int)(0 + x * 32), 0, 1, 800);
                spriteBatch.Draw(texture1px, rectangle, Color.Black);
                drawCount[index]++;
            }
            for (float y = -30; y < 30; y++)
            {
                Rectangle rectangle = new Rectangle(0, (int) (0 + y*32), 800, 1);
                spriteBatch.Draw(texture1px, rectangle, Color.Black);
                drawCount[index]++;
            }
            
            
            var yy = 1;
            var ii = 1;
            for (var i = 1; i <= 10000; i++)
            {
                
                //DMIManager.GetSpriteBatch(this, "TestFiles/human.dmi", "fatbody_s", new Vector2(32f, 32f), dir: Wiggle[cdir]);
                //DMIManager.GetSpriteBatch(this, "TestFiles/spacerat.dmi", "rat_brown", new Vector2(64f, 32f), dir: Wiggle[cdir]);
                //DMIManager.DrawSpriteBatch(spriteBatch, this, "TestFiles/robots.dmi", "mommi", new Vector2(32f * (float)ii, 32f * (float)yy), dir: Wiggle[cdir]);
                DMIManager.DrawSpriteBatch(spriteBatch, this, "TestFiles/robots.dmi", "mommi", new Vector2(32f * (float)ii, 32f * (float)yy), dir: Wiggle[cdir]);
                drawCount[index]++;
                if (i%20 == 0)
                {
                    yy++;
                    ii = 1;
                }
                else
                {
                    ii++;
                }
                if (drawCount[index] >= 10000)
                {
                    spriteBatch.End();
                    spriteBatch = BATCHPOOL;
                    index = Array.IndexOf(batchPool, spriteBatch);
                    spriteBatch.Begin();
                }
            }
            spriteBatch.End();
            for (var n = 0; n < drawCount.Length;n++)
            {
                //Console.WriteLine("SpriteBatch n = " + n + " had " + drawCount[n] + " calls this draw.");
                drawCount[n] = 0;
            }
            base.Draw(gameTime);
        }
    }
}
