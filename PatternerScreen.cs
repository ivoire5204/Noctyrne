using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace NoctyurnPatterner
{
    public struct PatternInfo //Bar Information
    {
        public float BPM;
        public bool[] lineActive;
        public int beat;

        public PatternInfo(float BPM, bool[] lineActive, int beat)
        {
            this.BPM = BPM;
            this.lineActive = lineActive;
            this.beat = beat;
        }
    }

    public struct InitRoom //Pattern Information
    {
        public string title;
        public string composer;
        public string difficulty;
        public int lines;
        public int mainBeat;
        public int musicDelay;
        public float offset;
        public string delaystr;
        public string offsetstr;

        public InitRoom(string title, string composer, string difficulty, int lines, int mainBeat, int musicDelay, float offset)
        {
            this.title = title;
            this.composer = composer;
            this.difficulty = difficulty;
            this.lines = lines;
            this.mainBeat = mainBeat;
            this.musicDelay = musicDelay;
            this.offset = offset;
            delaystr = musicDelay.ToString();
            offsetstr = offset.ToString();
        }
    }

    public struct Notes //Note Type Select
    {
        public int select;
        public string[] noteList;

        public Notes(int select)
        {
            this.select = select;
            noteList = new string[4] { "Basic", "On", "Off", "Delete" };
        }

        public string noteName() { return noteList[select]; }

        public void changeNote() { select = (select + 1) % 4; }
    }

    public struct Grid
    {
        public bool finer; //12->24 16->32
        public Vector2 beatMode; //beatMode (1,0) 16,32beat //beatMode (0,1) 12,24beat
        public string[] snapList;

        public Grid(bool finer, Vector2 beatMode)
        {
            this.finer = finer;
            this.beatMode = beatMode;
            snapList = new string[4] { "12", "16", "24", "32" };
        }

        public void ChangeBeatMode()
        {
            beatMode = new Vector2(beatMode.Y, beatMode.X);
        }

        public int Beat16() { return (int)beatMode.X; }

        public int Beat12() { return (int)beatMode.Y; }

        public string GridName() { return snapList[Beat16() + 2 * (finer ? 1 : 0)]; }

        public int SnapQ() { return 96 / Convert.ToInt32(GridName()); }

        public void ChangeGrid()
        {
            finer = (Beat12() == 1) ? !finer : finer;
            ChangeBeatMode();
        }
    }

    public class Patterner //Pattern Notes Information
    {
        public int mainBeat;
        public int lines;
        public int totalBars; //total bar
        public int bar; //current bar
        public int[] position;
        public Dictionary<int, PatternInfo> barInfo = new Dictionary<int, PatternInfo>();
        public Dictionary<int, SortedDictionary<int,int[]>> Pattern = new Dictionary<int, SortedDictionary<int, int[]>>();
        public Notes notes;
        public Grid grid;
        public int configSel = 0;
        public string BPMstr;

        public Patterner(InitRoom initRoom)
        {
            lines = initRoom.lines;
            mainBeat = initRoom.mainBeat;
            totalBars = 1;
            bar = 0;
            notes = new Notes(0);
            grid = new Grid(false, new Vector2(1, 0));
            position = new int[2] { 0, 0 };
            ChangeBarInfo(bar, new PatternInfo(120, createLineSwitch(lines), initRoom.mainBeat));
            Pattern.Add(0, new SortedDictionary<int, int[]>());
            BPMstr = "120";
        }

        public PatternInfo CurInfo()
        {
            return barInfo[bar];
        }

        public Vector2 posVector(int[] position)
        {
            Vector2 posv = new Vector2();
            posv.X = 1334 / 2 + (2 * position[0] - lines + 1) * ((lines == 7) ? 144 : 168) / 2;
            posv.Y = 39 + (96 / 4 * barInfo[bar].beat - position[1]) * 7 + 7 * 12 * (4 - barInfo[bar].beat);
            return posv - new Vector2(15, 15);
        }

        public void ChangeBarInfo(int barNum, PatternInfo info)
        {
            barInfo.Remove(barNum);
            barInfo.Add(barNum, info);
        }

        public void AddBar()
        {
            Pattern.Add(totalBars, new SortedDictionary<int, int[]>());
            ChangeBarInfo(totalBars, new PatternInfo(barInfo[totalBars - 1].BPM, (bool[])barInfo[totalBars - 1].lineActive.Clone(), mainBeat));
            totalBars++;
        }

        public void NextBar() { if (bar < totalBars - 1) { bar++; BPMstr = CurInfo().BPM.ToString();} }
        public void PrevBar() { if (bar > 0) { bar--; BPMstr = CurInfo().BPM.ToString();} }

        public bool[] createLineSwitch(int n)
        {
            bool[] sw = new bool[n];
            for (int i = 0; i < sw.Length; i++) sw[i] = true;
            return sw;
        }

        public void LineSwitch(int n)
        {
            bool[] sw = (bool[])CurInfo().lineActive.Clone();
            sw[n - 1] = !sw[n - 1];
            ChangeBarInfo(bar, new PatternInfo(CurInfo().BPM, sw, CurInfo().beat));
        }

        public void AddNote()
        {
            int[] noteCache;
            SortedDictionary<int, int[]> barCache = Pattern[bar];
            if (notes.select != 3)
            {
                if (barCache.ContainsKey(position[1]))
                {
                    noteCache = barCache[position[1]];
                    barCache.Remove(position[1]);
                }
                else { noteCache = new int[lines]; }
                noteCache[position[0]] = (notes.select + 1) % 4;
                barCache.Add(position[1], noteCache);
                Pattern.Remove(bar);
                Pattern.Add(bar, barCache);
            }
            else
            {
                if (barCache.ContainsKey(position[1]))
                {
                    noteCache = barCache[position[1]];
                    barCache.Remove(position[1]);
                    noteCache[position[0]] = (notes.select + 1) % 4;
                    barCache.Add(position[1], noteCache);
                    Pattern.Remove(bar);
                    Pattern.Add(bar, barCache);
                }
            }
        } 
    }

    public static class PatternerScreen
    {
        public static void DrawInitScreen(this SpriteBatch _spriteBatch, SpriteFont font, int select, InitRoom initRoom, bool selected) //Init Room Draw
        {
            int[] centre = new int[2] { 1334 / 2, 750 / 2 };
            Color selection = selected ? Color.Coral : Color.Chartreuse;

            _spriteBatch.DrawString(font, "Title", new Vector2(centre[0] - 360, -20+centre[1] - 64 * 4), select == 0 ? selection : Color.White);
            _spriteBatch.DrawString(font, "Composer", new Vector2(centre[0] - 360, -20 + centre[1] - 64 * 3), select == 1 ? selection : Color.White);
            _spriteBatch.DrawString(font, "Difficulty", new Vector2(centre[0] - 360, -20 + centre[1] - 64 * 2), select == 2 ? selection : Color.White);
            _spriteBatch.DrawString(font, "Lines", new Vector2(centre[0] - 360, -20 + centre[1] - 64), select == 3 ? selection : Color.White);
            _spriteBatch.DrawString(font, "Main Beat", new Vector2(centre[0] - 360, -20 + centre[1] + 64 * 1), select == 4 ? selection : Color.White);
            _spriteBatch.DrawString(font, "Music Delay", new Vector2(centre[0] - 360, -20 + centre[1] + 64 * 2), select == 5 ? selection : Color.White);
            _spriteBatch.DrawString(font, "Offset", new Vector2(centre[0] - 360, -20 + centre[1] + 64 * 3), select == 6 ? selection : Color.White);
            _spriteBatch.DrawString(font, "Confirm", new Vector2(centre[0]-360, -20 + centre[1] + 64 * 4), select == 7 && !selected ? selection : Color.White);

            _spriteBatch.DrawString(font, initRoom.title, new Vector2(centre[0]+240, -20 + centre[1] - 64 * 4), select == 0 ? selection : Color.White);
            _spriteBatch.DrawString(font, initRoom.composer, new Vector2(centre[0] + 240, -20 + centre[1] - 64 * 3), select == 1 ? selection : Color.White);
            _spriteBatch.DrawString(font, initRoom.difficulty, new Vector2(centre[0] + 240, -20 + centre[1] - 64 * 2), select == 2 ? selection : Color.White);
            _spriteBatch.DrawString(font, initRoom.lines.ToString(), new Vector2(centre[0] + 240, -20 + centre[1] - 64), select == 3 ? selection : Color.White);
            _spriteBatch.DrawString(font, initRoom.mainBeat.ToString(), new Vector2(centre[0] + 240, -20 + centre[1] + 64  * 1), select == 4 ? selection : Color.White);
            _spriteBatch.DrawString(font, initRoom.delaystr, new Vector2(centre[0] + 240, -20 + centre[1] + 64 * 2), select == 5 ? selection : Color.White);
            _spriteBatch.DrawString(font, initRoom.offsetstr, new Vector2(centre[0] + 240, -20 + centre[1] + 64 * 3), select == 6 ? selection : Color.White);
            _spriteBatch.DrawString(font, "Load", new Vector2(centre[0] + 240, -20 + centre[1] + 64 * 4), select == 7 && selected ? selection : Color.White);

        }

        public static void DrawFrame(this SpriteBatch _spriteBatch, SpriteFont font, Patterner patterner) //Patterner Room Draw
        {
            int centre = 1334 / 2;
            int lowInterval = (patterner.lines == 7) ? 144 : 168;
            for (int i = 0; i < patterner.lines * 2; i += 2) //vertical
            {
                float activity = patterner.CurInfo().lineActive[i / 2] ? 1f : 0.1f;
                _spriteBatch.DrawLineSegment(new Vector2(centre + (i - patterner.lines + 1) * lowInterval / 2, 750), new Vector2(centre + (i - patterner.lines + 1) * lowInterval / 2, 0), new Color(Color.White, activity), 2);
            }
            for (int i = 0; i <= 96 / 4 * patterner.barInfo[patterner.bar].beat; i += 1) //horizon
            {
                float a = 0f;
                if (i % 24 == 0) //4beat
                {
                    a = 1f;
                }
                else if (i % 12 == 0) //8beat
                {
                    a = 0.7f * patterner.grid.Beat16() + 0.2f * patterner.grid.Beat12() * (patterner.grid.finer ? 1 : 0);
                }
                else if (i % 8 == 0) //12beat
                {
                    a = 0.5f * patterner.grid.Beat12();
                }
                else if (i % 6 == 0) //16beat
                {
                    a = 0.4f * patterner.grid.Beat16();
                }
                else if (i % 4 == 0) //24beat
                {
                    a = 0.2f * patterner.grid.Beat12() * (patterner.grid.finer ? 1 : 0);
                }
                else if (i % 3 == 0) //32beat
                {
                    a = 0.1f * patterner.grid.Beat16() * (patterner.grid.finer ? 1 : 0);
                }
                _spriteBatch.DrawLineSegment(new Vector2(centre + (1 - patterner.lines) * lowInterval / 2 - 32, 39 + i * 7 + 7 * 12 * (4 - patterner.barInfo[patterner.bar].beat)),
                    new Vector2(centre + (patterner.lines - 1) * lowInterval / 2 + 32, 39 + i * 7 + 7 * 12 * (4 - patterner.barInfo[patterner.bar].beat)), new Color(Color.White, a), (i % 24 == 0) ? 3 : 2);
            }

            _spriteBatch.DrawString(font, "Page : " + patterner.bar.ToString() + "/" + (patterner.totalBars - 1).ToString(), new Vector2(24, 24), Color.White);
            _spriteBatch.DrawString(font, "(B)PM : " + patterner.BPMstr, new Vector2(24, 24 + 48), (patterner.configSel == 1) ? Color.Coral : Color.White);
            _spriteBatch.DrawString(font, "Bea(t) : " + patterner.CurInfo().beat.ToString(), new Vector2(24, 24 + 48 * 3), Color.White);
            _spriteBatch.DrawString(font, "(N)ote : " + patterner.notes.noteName(), new Vector2(1136, 24 + 48), Color.White);
            _spriteBatch.DrawString(font, "(G)rid : " + patterner.grid.GridName(), new Vector2(1136, 24 + 48 * 2), Color.White);
        }
    }
}
