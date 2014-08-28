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

        GraphicsDeviceManager graphics;
        int cdir = 0;
        uint tick = 0;
        Direction[] Wiggle = new[] {
            Direction.WEST,
            Direction.SOUTH,
            Direction.EAST,
            Direction.SOUTH,
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

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            // Every 30 ticks...
            if((tick++ % 30)==0) {
                cdir = (cdir+1) % Wiggle.Length;
            }

            DMIManager.GetSpriteBatch(this, "TestFiles/human.dmi",    "fatbody_s", new Vector2(32f, 32f), dir: Wiggle[cdir]);
            DMIManager.GetSpriteBatch(this, "TestFiles/spacerat.dmi", "rat_brown", new Vector2(64f, 32f), dir: Wiggle[cdir]);
            DMIManager.GetSpriteBatch(this, "TestFiles/robots.dmi",   "mommi",     new Vector2(96f, 32f), dir: Wiggle[cdir]);

            base.Draw(gameTime);
        }
    }
}
