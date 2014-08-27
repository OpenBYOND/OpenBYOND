using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using log4net;

namespace OpenBYOND
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class BYONDGame : Game
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Utils));

        GraphicsDeviceManager graphics;
        int cdir = 0;
        uint tick = 0;
        Direction[] Wiggle = new[] {
            Direction.WEST,
            Direction.NORTH,
            Direction.EAST,
            Direction.NORTH,
        };

        public BYONDGame()
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
            DMIManager.Preload("../../../Test/TestFiles/human.dmi");

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
            // Every 10 ticks...
            if((tick++ % 10)==0) {
                cdir = ++cdir % Wiggle.Length;
            }

            DMIManager.GetSpriteBatch(this,"../../../Test/TestFiles/human.dmi", "fatbody_s", dir: Wiggle[cdir]);
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
