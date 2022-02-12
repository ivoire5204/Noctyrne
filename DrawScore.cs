using Godot;
using System;

public class DrawScore : Label
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	private double unitScore; //노트당 점수, basic노트는 노트 하나당 이것의 2배임
	public static int unitPassed = 0; //통과한 점수 단위 수(basic노트는 하나당 2, 다른 노트들은 하나당 1)
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		unitScore = (double) 1000000 / (2*objSys.noteCounts[1]+objSys.noteCounts[2]+objSys.noteCounts[3]);
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		objSys.score = (int)Math.Truncate(unitScore * unitPassed);
		SetText($"{objSys.score}\n{objSys.combo}");
	}
}
