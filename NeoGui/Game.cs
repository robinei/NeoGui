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
        private readonly RasterizerState rasterizerState = new RasterizerState {ScissorTestEnable = true};
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private Texture2D pixel;
        
        private bool inputInited;
        private readonly InputState input = new InputState();
        private NeoGuiContext ui;

        private int frameRate = 0;
        private int frameCounter = 0;
        private TimeSpan elapsedTime = TimeSpan.Zero;

        public Vec2 TextSize(string text, int fontId = 0)
        {
            var size = font.MeasureString(text);
            return new Vec2(size.X, size.Y);
        }

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.ApplyChanges();
            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
            Content.RootDirectory = "Content";
        }
        
        protected override void Initialize()
        {
            base.Initialize();
            IsMouseVisible = true;
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) {
                Exit();
            }

            elapsedTime += gameTime.ElapsedGameTime;
            if (elapsedTime > TimeSpan.FromSeconds(1)) {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }

            if (!inputInited) {
                UpdateInput(gameTime);
                inputInited = true;
            }

            var bounds = GraphicsDevice.Viewport.Bounds;
            TestUi.DoUi(ui, bounds.Width, bounds.Height);
            UpdateInput(gameTime);
            ui.RunUpdateTraversals();

            base.Update(gameTime);
        }

        private void UpdateInput(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            input.Time = gameTime.TotalGameTime.TotalSeconds;
            input.MousePos = new Vec2(mouseState.X, mouseState.Y);
            input.MouseButtonDown[(int)MouseButton.Left] = mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
            input.MouseButtonDown[(int)MouseButton.Right] = mouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
            input.MouseButtonDown[(int)MouseButton.Middle] = mouseState.MiddleButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
            ui.SetCurrentInputState(input);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            ++frameCounter;
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, rasterizerState);

            var prevClipRect = Rect.Empty;
            foreach (var command in ui.DrawCommandBuffer) {
                var clipRect = command.ClipRect;
                if ((clipRect.Pos - prevClipRect.Pos).SqrLength > 0 || (clipRect.Size - prevClipRect.Size).SqrLength > 0) {
                    spriteBatch.GraphicsDevice.ScissorRectangle = clipRect.ToMonoGameRectangle();
                }
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
            
            spriteBatch.GraphicsDevice.ScissorRectangle = GraphicsDevice.Viewport.Bounds;
            var memMegaBytes = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
            string fps = $"fps: {frameRate}  mem: {memMegaBytes:F1} MB";
            spriteBatch.DrawString(font, fps, new Vector2(5, 1), Color.White);
            spriteBatch.DrawString(font, fps, new Vector2(4, 0), Color.Black);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
