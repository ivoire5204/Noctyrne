using Godot;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class objSys : Node2D
{
	struct Notes
	{ //각 시간의 노트, 라인 정보
		public double time;
		public byte linePos;
		public byte noteType;
		public float noteSpeed;
		public bool[] lineactive;

		public Notes(double time, byte linePos, byte noteType, float noteSpeed, int lines)
		{
			this.time = time;
			this.linePos = linePos;
			this.noteType = noteType;
			this.noteSpeed = noteSpeed;
			this.lineactive = new bool[lines];
		}
	}
	
	static private Queue<Notes> noteQueue = new Queue<Notes>(); //노트 시간 및 노트 정보가 담긴 큐
	static public int[] noteCounts = new int[4]; //노트 갯수들, 순서대로 전체 개수, Basic, On, Off
	static public int lines; //라인 개수
	static public int score = 0; //현재 점수
	static public int combo = 0; //현재 콤보수
	static public float hispeed; //노트 스피드
	static public double musicDelay; //Music Delay 시간(초)
	
	public string nctrPath;
	
	private Image image = new Image();
	static public ImageTexture basicSpr = new ImageTexture(); //노트 스프라이트 텍스쳐 
	static public ImageTexture onSpr = new ImageTexture();
	static public ImageTexture offSpr = new ImageTexture();
	
	static private float sec = -(float)musicDelay; //현재 시간
	static private bool playing = false;
	static public Node playSceneNode;

	private void LoadSetting() //세팅 json 로딩
	{
		string jsonStr = System.IO.File.ReadAllText(@"Setting.txt");
		string[] data = jsonStr.Split('\n');
		hispeed = Convert.ToSingle(data[1]);
		nctrPath = data[0];
	}

	private void LoadPattern() //패턴 파일 로딩
	{
		using (BinaryReader rdr = new BinaryReader(System.IO.File.Open(nctrPath, FileMode.Open)))
		{
			noteCounts[0] = 0;
			for (int i = 1; i <= 3; i++) //노트 갯수 로드
			{
				noteCounts[i] = rdr.ReadInt32();
				noteCounts[0] += noteCounts[i];
			}
			lines = rdr.ReadInt32(); //라인 갯수 로드
			musicDelay = rdr.ReadDouble();
			rdr.ReadDouble();

			for (int i = 0; i < noteCounts[0]; i++) //노트 정보 로드
			{
				Notes NoteCache = new Notes(rdr.ReadDouble(), rdr.ReadByte(), rdr.ReadByte(), rdr.ReadSingle(), lines);
				short lineactiveCache = rdr.ReadInt16(); //Line Activity 복호화
				for (int j = 0; j < lines; j++)
				{
					NoteCache.lineactive[j] = Convert.ToBoolean(lineactiveCache % (short)(Math.Pow(2,j+1)) / (short)(Math.Pow(2,j)));
				}
				noteQueue.Enqueue(NoteCache);
			}
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (GetTree().CurrentScene.Filename == "res://LoadScreen.tscn") 
		{
			LoadSetting();
			LoadPattern();
			foreach (int item in noteCounts)
			{
				GD.Print(item);
			}
			GD.Print(noteQueue.Count);
			GD.Print(noteQueue.Peek().time);
			GD.Print(musicDelay);
		}
		
		image.Load("res://BasicNote48.png");
		basicSpr.CreateFromImage(image);
		image.Load("res://OnNote48.png");
		onSpr.CreateFromImage(image);
		image.Load("res://OffNote48.png");
		offSpr.CreateFromImage(image);
		
		playing = true;
		
		GetTree().ChangeScene("res://PlayScreen.tscn");
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		if (playing && (noteQueue.Count > 0)) 
		{
			while (noteQueue.Peek().time < sec)
			{
				var newNote = new NoteSpr((int)(noteQueue.Peek().linePos),(int)(noteQueue.Peek().noteType)-1);
				//newNote.setLines((int)(noteQueue.Peek().linePos));
				//newNote.setSpr((int)(noteQueue.Peek().noteType));
				GetTree().CurrentScene.AddChild(newNote);
				noteQueue.Dequeue();
				if (noteQueue.Count == 0) {playing = false; break;}
			}
			sec += delta;
		}
	}
}
