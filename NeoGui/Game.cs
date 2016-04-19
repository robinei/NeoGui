using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
        private RenderTarget2D renderTarget;
        private SpriteBatch spriteBatch;
        private BasicEffect basicEffect;
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

            var trans = new Transform();
            trans.MakeIdentity();
            //trans.Translation.X += 10;
            trans.Rotation = Quat.FromAxisAngle(1, 0, 0, (float)Math.PI);
            trans.Rotation = Quat.FromEulerAngles(0, (float)Math.PI, 0);

            var v1 = new Vec3(1, 0, 0);
            var v2 = trans.ApplyForward(v1);
            Debug.WriteLine("v2: " + v2);
        }
        
        protected override void Initialize()
        {
            base.Initialize();
            renderTarget = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24,
                1,
                RenderTargetUsage.PreserveContents);
            IsMouseVisible = true;
        }
        
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Arial");
            basicEffect = new BasicEffect(GraphicsDevice) {
                TextureEnabled = true,
                VertexColorEnabled = true
            };
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new [] { Color.White });

            ui = new NeoGuiContext(this);
        }
        
        protected override void UnloadContent()
        {
        }
        
        protected override void Update(GameTime gameTime)
        {
            ++frameCounter;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) {
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

            UpdateInput(gameTime);
            var bounds = GraphicsDevice.Viewport.Bounds;
            TestUi.DoUi(ui, bounds.Width, bounds.Height);
            ui.RunUpdateTraversals();

            base.Update(gameTime);
        }

        private void UpdateInput(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            input.Time = gameTime.TotalGameTime.TotalSeconds;
            input.MousePos = new Vec2(mouseState.X, mouseState.Y);
            input.MouseButtonDown[(int)MouseButton.Left] = mouseState.LeftButton == ButtonState.Pressed;
            input.MouseButtonDown[(int)MouseButton.Right] = mouseState.RightButton == ButtonState.Pressed;
            input.MouseButtonDown[(int)MouseButton.Middle] = mouseState.MiddleButton == ButtonState.Pressed;
            ui.Input.SetNewState(input);
        }

        protected override bool BeginDraw()
        {
            return ui.DirtyDrawCommandBuffers.Count > 0;
        }

        private void DrawUi()
        {
            GraphicsDevice.SetRenderTarget(renderTarget);

            var viewport = GraphicsDevice.Viewport;
            basicEffect.Projection = Matrix.CreateTranslation(-0.5f, -0.5f, 0) * 
                             Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, rasterizerState, basicEffect);
            foreach (var buffer in ui.DirtyDrawCommandBuffers) {
                var prevClipRect = Rect.Empty;
                for (var i = 0; i < buffer.Count; ++i) {
                    var command = buffer[i];
                    var clipRect = command.ClipRect;
                    if ((clipRect.Pos - prevClipRect.Pos).SqrLength > 0 || (clipRect.Size - prevClipRect.Size).SqrLength > 0) {
                        GraphicsDevice.ScissorRectangle = clipRect.ToMonoGameRectangle();
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
            }
            spriteBatch.End();
            GraphicsDevice.ScissorRectangle = GraphicsDevice.Viewport.Bounds;
 
            GraphicsDevice.SetRenderTarget(null);
        }

        protected override void Draw(GameTime gameTime)
        {
            DrawUi();
            
            spriteBatch.Begin();
            spriteBatch.Draw(renderTarget, GraphicsDevice.Viewport.Bounds, Color.White);
            var memMegaBytes = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
            string fps = $"fps: {frameRate}  mem: {memMegaBytes:F1} MB";
            spriteBatch.DrawString(font, fps, new Vector2(5, 1), Color.White);
            spriteBatch.DrawString(font, fps, new Vector2(4, 0), Color.Black);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
