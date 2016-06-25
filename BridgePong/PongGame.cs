using System;
using System.Collections.Generic;
using Bridge.Html5;

namespace BridgePong
{
	public static class Main
	{
		static PongGame game;

		[Ready]
		public static void OnReady()
		{
			game = new PongGame();
		}
	}


	public class PongGame
	{
		HTMLCanvasElement screen;
		CanvasRenderingContext2D screenContext;
		Sprite ball, p1, p2;
		Sprite[] sprites;
		int lastFrame, p1Score, p2Score, gameStartCountdown;
		HashSet<int> keys = new HashSet<int>();

		const int CursorKeyUp = 38, CursorKeyDown = 40, NumPadUp = 104, NumPadDown = 98;
		const double PlayerMovementSpeed = 0.5;

		public PongGame()
		{
			screen = Document.QuerySelector<HTMLCanvasElement>("canvas");
			screenContext = screen.GetContext(CanvasTypes.CanvasContext2DType.CanvasRenderingContext2D);
			screenContext.FillStyle = "white";
			screenContext.Font = "40px Consolas, monospace";

			Document.OnKeyDown = e => keys.Add(e.KeyCode);
			Document.OnKeyUp = e => keys.Remove(e.KeyCode);

			ball = new Sprite { Width = 10, Height = 10, X = screen.Width / 2, Y = screen.Height / 2 };
			p1 = new Sprite { Width = 10, Height = 140, X = 30, Y = screen.Height / 2 };
			p2 = new Sprite { Width = 10, Height = 140, X = screen.Width - 30, Y = screen.Height / 2 };
			sprites = new[] { ball, p1, p2 };

			ResetGame();
			Tick();
		}

		void Tick()
		{
			var now = Window.Performance.Now();
			Update(now - lastFrame);
			Draw();
			lastFrame = now;
			Window.RequestAnimationFrame(Tick);
		}

		void Update(int elapsed)
		{
			if (gameStartCountdown > 0)
			{
				gameStartCountdown -= elapsed;
				return;
			}


			if (keys.Contains(CursorKeyDown))
				p1.Y += elapsed * PlayerMovementSpeed;
			if (keys.Contains(CursorKeyUp))
				p1.Y -= elapsed * PlayerMovementSpeed;
			if (keys.Contains(NumPadDown))
				p2.Y += elapsed * PlayerMovementSpeed;
			if (keys.Contains(NumPadUp))
				p2.Y -= elapsed * PlayerMovementSpeed;

			p1.Y = Clamp(p1.Y, p1.Height / 2, screen.Height - p1.Height / 2);
			p2.Y = Clamp(p2.Y, p2.Height / 2, screen.Height - p2.Height / 2);

			ball.X += ball.VelocityX * elapsed;
			ball.Y += ball.VelocityY * elapsed;

			if (ball.Y < ball.Height / 2 || ball.Y > screen.Height - ball.Height / 2)
				ball.VelocityY *= -1;

			if (ball.CollidesWith(p1) || ball.CollidesWith(p2))
			{
				ball.X -= ball.VelocityX * elapsed; // Undo the move into the paddle
				ball.VelocityX *= -1; // Reverse the direction
				ball.X += ball.VelocityX * elapsed; // Redo the move
			}

			if (ball.X < 0)
			{
				p2Score++;
				ResetGame();
			}
			else if (ball.X > screen.Width)
			{
				p1Score++;
				ResetGame();
			}
		}

		void Draw()
		{
			screenContext.ClearRect(0, 0, screen.Width, screen.Height);

			screenContext.TextAlign = CanvasTypes.CanvasTextAlign.Left;
			screenContext.FillText(p1Score.ToString(), 50, 50);
			screenContext.TextAlign = CanvasTypes.CanvasTextAlign.Right;
			screenContext.FillText(p2Score.ToString(), screen.Width - 50, 50);

			if (gameStartCountdown > 0)
			{
				screenContext.TextAlign = CanvasTypes.CanvasTextAlign.Center;
				screenContext.FillText(((gameStartCountdown / 1000) + 1).ToString(), screen.Width / 2, screen.Height / 2);
				return;
			}

			for (var i = 0; i < sprites.Length; i++)
				sprites[i].Draw(screenContext);
		}

		void ResetGame()
		{
			ball.VelocityX = Math.Random() < 0.5 ? -0.5 : 0.5;
			ball.VelocityY = Math.Random() / 5;
			ball.X = screen.Width / 2;
			p1.Y = p2.Y = ball.Y = screen.Height / 2;
			gameStartCountdown = 3000;
		}

		double Clamp(double val, double min, double max)
		{
			return Math.Min(Math.Max(val, min), max);
		}
	}

	public class Sprite
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public double X { get; set; }
		public double Y { get; set; }
		public double VelocityX { get; set; }
		public double VelocityY { get; set; }

		public bool CollidesWith(Sprite other)
		{
			return !(
				this.X + this.Width / 2 < other.X - other.Width / 2
				|| this.Y + this.Height / 2 < other.Y - other.Height / 2
				|| other.X + other.Width / 2 < this.X - this.Width / 2
				|| other.Y + other.Height / 2 < this.Y - this.Height / 2
			);
		}
	}

	public static class GameExtensions
	{
		public static void Draw(this Sprite sprite, CanvasRenderingContext2D screenContext)
		{
			screenContext.FillRect((int)(sprite.X - sprite.Width / 2), (int)(sprite.Y - sprite.Height / 2), sprite.Width, sprite.Height);
		}
	}
}
