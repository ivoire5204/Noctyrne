using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public class DrawLine : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text
	private int lines;
	private (Vector2 startPos, Vector2 endPos)[] linePos;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		lines = objSys.lines;
		linePos = new (Vector2 startPos, Vector2 endPos)[lines];
		
		int centre = 1334 / 2;
		int lowInterval = (lines == 7) ? (int)Math.Floor(144*1.2) : (int)Math.Floor(168*1.2);
		int upInterval = (lines == 7) ? (int)Math.Floor(108*0.8) : (int)Math.Floor(126*0.8);
		for (int i = 0; i < lines * 2; i += 2)
		{
			linePos[i/2] = (new Vector2(centre + (i - lines + 1) * lowInterval / 2, 750), new Vector2(centre + (i - lines + 1) * upInterval / 2, 0));
		}
	}

	public override void _Draw()
	{
		foreach ((Vector2 startPos, Vector2 endPos) item in linePos)
		{
			DrawLine(item.startPos, item.endPos, new Color(1,1,1,1), 2f);
			DrawLine(new Vector2(0,600), new Vector2(1334,600), new Color(1,1,1,1), 2f);
		}
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
