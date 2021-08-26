using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;

namespace NoctyurnPatterner
{
    public static class PlayScreen
    {
        public static void DrawFrame(this SpriteBatch _spriteBatch, int lines, bool[] lineAcitivity)
        {
            int centre = 1334 / 2;
            int lowInterval = (lines == 7) ? 144 : 168;
            int upInterval = (lines == 7) ? 108 : 126;
            for (int i = 0; i < lines * 2; i += 2)
            {
                _spriteBatch.DrawLineSegment(new Vector2(centre + (i - lines + 1) * lowInterval / 2, 750), new Vector2(centre + (i - lines + 1) * upInterval / 2, 0), Color.White, 2);
            }
            _spriteBatch.DrawLineSegment(new Vector2(0, 600), new Vector2(1334, 600), Color.White, 2);
        }
    }

    public struct NoteQueues
    {
        public Queue<float> notetime;
        public Queue<int[]> note;
        public Queue<float> bartime;
        public Queue<bool[]> activity;
        public Queue<float> speed;
    }

    public struct PlayerInfo
    {
        public float hispeed;
        public float timer;
        public int[] totalNote;
        public float score;
        public int combo;
        public float dScore;
        public int lines;
        public float curspeed;
        public bool[] lineActivity;
        public NoteQueues noteQueues;

        public PlayerInfo(float hispeed, bool[] lineActivity, float curspeed = 1, float score = 0, int combo = 0, float timer = 0)
        {
            this.hispeed = hispeed;
            this.timer = timer;
            this.score = score;
            this.combo = combo;
            this.curspeed = curspeed;
            this.lineActivity = lineActivity;
            totalNote = new int[3];
            lines = 0;
            dScore = 0;
            noteQueues = new NoteQueues()
            {
                notetime = new Queue<float>(),
                note = new Queue<int[]>(),
                bartime = new Queue<float>(),
                activity = new Queue<bool[]>(),
                speed = new Queue<float>()
            };
            LoadNctr();
            this.lineActivity = noteQueues.activity.Peek();
        }

        public void AddScore(int noteType)
        {
            combo++;
            score += (noteType == 1) ? 2 * dScore : dScore;
        }

        public int CurScore() { return (int)score; }

        public void LoadNctr()
        {
            using (BinaryReader rdr = new BinaryReader(File.Open("Pattern.nctr", FileMode.Open)))
            {
                totalNote = new int[] { rdr.ReadInt32(), rdr.ReadInt32(), rdr.ReadInt32() };
                lines = rdr.ReadInt32();
                if (totalNote[0] + totalNote[1] + totalNote[2] != 0) { dScore = 1000000 / (2 * totalNote[0] + totalNote[1] + totalNote[2]); }
                else { dScore = 1000000; }

                while (true)
                {
                    try
                    {
                        float arriveTime = rdr.ReadSingle();
                        float createTime = rdr.ReadSingle();
                        int[] note = new int[] { rdr.ReadByte(), rdr.ReadByte() };
                        float speed = rdr.ReadSingle();
                        short activity = rdr.ReadInt16();
                        bool[] activityBool = new bool[lines];
                        for (int i = 0; i < lines; i++)
                        {
                            activityBool[i] = (activity % (short)Math.Pow(2, i + 1) / (short)Math.Pow(2, i) == 1) ? true : false;
                        }

                        noteQueues.notetime.Enqueue(arriveTime - (arriveTime - createTime) / (hispeed / 3f));
                        noteQueues.bartime.Enqueue(arriveTime);
                        noteQueues.note.Enqueue(note);
                        noteQueues.speed.Enqueue(speed);
                        noteQueues.activity.Enqueue(activityBool);
                    }
                    catch (EndOfStreamException)
                    {
                        break;
                    }
                }
            }
        }
    }

    public class NoteObj
    {
        public Vector2 position;
        Vector2 velocity;
        int lowInterval;
        int upInterval;
        public Texture2D type;
        int intType;

        public NoteObj(int pos, float hispeed, int lines, int intType, Texture2D type)
        {
            lowInterval = (lines == 7) ? 144 : 168;
            upInterval = (lines == 7) ? 108 : 126;
            position = new Vector2(1334 / 2 + (pos * 2 - lines + 1) * upInterval / 2 - 16, -16);
            velocity = new Vector2((pos * 2 - lines + 1) * (lowInterval - upInterval) * 0.4f, 600) * (hispeed / 3);
            this.type = type;
            this.intType = intType;
        }

        public void DrawNote(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(type, position, Color.White);
        }

        public void Move(TimeSpan dt)
        {
            position.X += velocity.X * (float)dt.TotalSeconds;
            position.Y += velocity.Y * (float)dt.TotalSeconds;
        }

        public int Remove()
        {
            return intType;
        }
    }
}
