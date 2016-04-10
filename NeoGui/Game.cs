using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NeoGui.Core;
using Color = Microsoft.Xna.Framework.Color;

namespace NeoGui
{
    public class Game : Microsoft.Xna.Framework.Game, INeoGuiDelegate
    {
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private Texture2D pixel;
        
        private MouseState mouseState;
        private MouseState prevMouseState;

        private NeoGuiContext ui;

        public Vec2 TextSize(string text, int fontId = 0)
        {
            var size = font.MeasureString(text);
            return new Vec2(size.X, size.Y);
        }

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        
        protected override void Initialize()
        {
            base.Initialize();
            IsMouseVisible = true;
            prevMouseState = Mouse.GetState();
        }
        
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Arial");

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new [] { Color.White });

            ui = new NeoGuiContext(this);
        }
        
        protected override void UnloadContent()
        {
        }
        
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) {
                Exit();
            }

            var bounds = GraphicsDevice.Viewport.Bounds;
            TestUi.DoUi(ui, bounds.Width, bounds.Height);
            MaybeDispatchMouseEvents();

            base.Update(gameTime);
        }

        private void MaybeDispatchMouseEvents()
        {
            mouseState = Mouse.GetState();
            if (mouseState.X != prevMouseState.X || mouseState.Y != prevMouseState.Y) {
                var pos = new Vec2(mouseState.X, mouseState.Y);
                ui.HitTest(pos)?.DispatchEvent(new MouseMotionEvent {
                    Pos = pos
                });
            }
            MaybeDispatchButtonEvent(MouseButton.Left, s => s.LeftButton);
            MaybeDispatchButtonEvent(MouseButton.Right, s => s.RightButton);
            MaybeDispatchButtonEvent(MouseButton.Middle, s => s.MiddleButton);
            prevMouseState = mouseState;
        }
        private void MaybeDispatchButtonEvent(MouseButton button, Func<MouseState, ButtonState> getButtonState)
        {
            if (getButtonState(mouseState) != getButtonState(prevMouseState)) {
                var pos = new Vec2(mouseState.X, mouseState.Y);
                ui.HitTest(pos)?.DispatchEvent(new MouseButtonEvent {
                    Pos = pos,
                    Pressed = getButtonState(mouseState) == ButtonState.Pressed,
                    Button = button
                });
            }
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            spriteBatch.Begin();
            foreach (var command in ui.DrawCommandBuffer) {
                switch (command.Type) {
                case DrawCommandType.SolidRect:
                    spriteBatch.Draw(pixel, command.SolidRect.Rect.ToMonoGameRectangle(), command.SolidRect.Color.ToMonoGameColor());
                    break;
                case DrawCommandType.GradientRect:
                    break;
                case DrawCommandType.TexturedRect:
                    break;
                case DrawCommandType.Text:
                    spriteBatch.DrawString(font, ui.GetInternedString(command.Text.StringId), command.Text.Vec2.ToMonoGameVector2(), command.Text.Color.ToMonoGameColor());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
                }
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
