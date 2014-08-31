using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace OpenBYOND.Client
{
    public class Mob : DrawableGameComponent, IFocusable
    {
        public string name = "Player";
        public string icon = "TestFiles/human.dmi";
        public string icon_state = "body_m_s";

        public Vector2 Position
        {
            get { return position; } 
            set { position = value; }
        }

        private Vector2 position;
        public OpenBYONDGame client;
        public Direction dir = Direction.SOUTH;

        public Vector2 movingto;
        public Vector2 newpos;
        public double next_move = 0F;
        public double delay = 0.1F;
        private Vector2 spritepos;

        public Mob(OpenBYONDGame game, Vector2 pos) : base(game)
        {
            client = game;
            position = pos;
            newpos = pos;
            
        }

        public override void Update(GameTime gameTime)
        {
            
            if (movingto != Vector2.Zero)
            {
                switch (dir)
                {
                    case Direction.NORTH:
                        if (spritepos.Y - position.Y < 5f)
                            movingto = Vector2.Zero;
                        break;
                    case Direction.SOUTH:
                        if (position.Y - spritepos.Y < 5F)
                        {
                            movingto = Vector2.Zero;
                        }
                        break;
                    case Direction.EAST:
                        if (position.X - spritepos.X < 5F)
                        {
                            movingto = Vector2.Zero;
                        }
                        break;
                    case Direction.WEST:
                        if (spritepos.X - position.X < 5f)
                            movingto = Vector2.Zero;
                        break;
                }
            }
            if (next_move > gameTime.TotalGameTime.TotalSeconds)
            {
                //Console.WriteLine("next move = " + next_move + " time = " + gameTime.ElapsedGameTime.TotalSeconds);
                //client.eye.Position = position;
                base.Update(gameTime);
                return;
            }
 
            KeyboardState kb = Keyboard.GetState();
            if (kb.IsKeyDown(Keys.D) && next_move <= gameTime.TotalGameTime.TotalSeconds)
            {
                spritepos = position;
                newpos = position;
                newpos.X += 32F;
                movingto = newpos - position;
                position = newpos;
                movingto.Normalize();
                dir = Direction.EAST;
                next_move = gameTime.TotalGameTime.TotalSeconds + delay;
                Console.WriteLine("X " + newpos.X + " Y " + newpos.Y);
            }
            if (kb.IsKeyDown(Keys.S) && next_move <= gameTime.TotalGameTime.TotalSeconds)
            {
                spritepos = position;
                newpos = position;
                newpos.Y += 32F;
                movingto = newpos - position;
                position = newpos;
                movingto.Normalize();
                dir = Direction.SOUTH;
                next_move = gameTime.TotalGameTime.TotalSeconds + delay;
                Console.WriteLine("X " + newpos.X + " Y " + newpos.Y);
            }
            if (kb.IsKeyDown(Keys.A) && next_move <= gameTime.TotalGameTime.TotalSeconds)
            {
                spritepos = position;
                newpos = position;
                newpos.X -= 32F;
                movingto = newpos - position;
                position = newpos;
                movingto.Normalize();
                dir = Direction.WEST;
                next_move = gameTime.TotalGameTime.TotalSeconds + delay;
                Console.WriteLine("X " + newpos.X + " Y " + newpos.Y);
            }
            if (kb.IsKeyDown(Keys.W) && next_move <= gameTime.TotalGameTime.TotalSeconds)
            {
                spritepos = position;
                newpos = position;
                newpos.Y -= 32F;
                movingto = newpos - position;
                position = newpos;
                movingto.Normalize();
                dir = Direction.NORTH;
                next_move = gameTime.TotalGameTime.TotalSeconds + delay;
                Console.WriteLine("X " + newpos.X + " Y " + newpos.Y);
            }

            //client.eye.Position = position;
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            client.spriteBatch.Begin(/*SpriteSortMode.FrontToBack,
                BlendState.AlphaBlend,
                null,
                null,
                null,
                null,
                client.eye.Transform*/);
            if (movingto != Vector2.Zero)
            {
                spritepos += movingto * 200F * ((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            else
            {
                spritepos = position;
            }
            spritepos.X = (float)(spritepos.X);
            spritepos.Y = (float)(spritepos.Y);

            //Console.WriteLine(spritepos.X + " " + spritepos.Y + " | " + position.X + " " + position.Y);
            DMIManager.DrawSpriteBatch(client.spriteBatch,client,icon,icon_state,spritepos,dir);
            
            client.spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
