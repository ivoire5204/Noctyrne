using Godot;
using System;

public class EffSpr : Sprite
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	private Vector2 Pos;
	private Vector2 InitScale = new Vector2(0.8f, 0.8f);
	private Vector2 ScaleSpeed = new Vector2(0.8f, 0.8f);
	private Image image = new Image();
	private ImageTexture spr = new ImageTexture();

	public EffSpr(Vector2 Pos) : base()
	{
		this.Pos = Pos;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		image.Load("res://Effect_Perfect.png");
		spr.CreateFromImage(image);
		SetTexture(spr);
		
		Position = new Vector2((int)(Pos.x),Pos.y);
		this.Scale = InitScale;
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		InitScale += ScaleSpeed * delta;
		this.Scale = InitScale;
		Rotation += (float)Math.PI  * delta;
		
		if (InitScale.x >= 1.2) {QueueFree();}
	}
}
