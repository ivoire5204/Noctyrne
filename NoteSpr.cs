using Godot;
using System;

public class NoteSpr : Sprite
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	private Vector2[] toPos = new Vector2[2];
	private int lowInterval;
	private int upInterval;
	private int centre = 1334/2;
	private int lines;
	public int type;
	public int line;
	
	public NoteSpr(int line, int type) : base()
	{
		this.line = line;
		this.type = type;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		lines = objSys.lines;
		lowInterval = (lines == 7) ? (int)Math.Floor(144*1.2) : (int)Math.Floor(168*1.2);
		upInterval = (lines == 7) ? (int)Math.Floor(108*0.8) : (int)Math.Floor(126*0.8);
		setLines(line);
		setSpr(type);
	}
	
	public void setSpr(int type)
	{
		switch (type)
		{
			case 0:
				SetTexture(objSys.basicSpr);
				break;
			case 1:
				SetTexture(objSys.onSpr);
				break;
			case 2:
				SetTexture(objSys.offSpr);
				break;
			default :
				break;
		}
	}
	
	public void setLines(int line)
	{
		toPos[0] = new Vector2(centre + (2*line - lines + 1) * upInterval / 2, 0);
		toPos[1] = new Vector2(centre + (2*line - lines + 1) * lowInterval / 2, 750);
		Position = toPos[0];
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
 	{
 		Position += (toPos[1]-toPos[0]) * delta * (objSys.hispeed / (float)3);
		if (Position.y >= 600) 
		{ 
			DrawScore.unitPassed += 2-(type+1)/2;
			objSys.combo += 1;
			var Eff = new EffSpr(toPos[0] + (toPos[1] - toPos[0]) * 0.8f);
			GetTree().CurrentScene.AddChild(Eff);
			QueueFree();
		}
		if (type == 2)
		{
			Rotation += (float)Math.PI * delta;
		}
 	}
}
