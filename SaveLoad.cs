using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NoctyurnPatterner
{
    //Json Construction
    public class UniversalInfo
    {
        public Head PatternInformation { get; set; }
        public Dictionary<string,Body> Pattern { get; set; }
    }

    public class Head
    {
        public string Title { get; set; }
        public string Composer { get; set; }
        public string Difficulty { get; set; }
        public int Lines { get; set; }
        public int MainBeat { get; set; }
        public int MusicDelay { get; set; }
        public float Offset { get; set; }
    }

    public class Body
    {
        public PageSet PageSet { get; set; }
        public Dictionary<string, int[]> Notes { get; set; }
    }

    public class PageSet
    {
        public float BPM { get; set; }
        public int beat { get; set; }
        public bool[] lineActive { get; set; }
    }

    //Save
    public static class SaveLoad
    {
        public static void Save(InitRoom init, Patterner patterner)
        {
            Head head = new Head()
            {
                Title = init.title,
                Composer = init.composer,
                Difficulty = init.difficulty,
                Lines = init.lines,
                MainBeat = init.mainBeat,
                MusicDelay = init.musicDelay,
                Offset = init.offset
            };
            Dictionary<string, Body> Pattern = new Dictionary<string, Body>();
            for (int i = 0; i < patterner.totalBars; i++)
            {
                Dictionary<string, int[]> notestr = new Dictionary<string, int[]>();
                foreach (KeyValuePair<int, int[]> items in patterner.Pattern[i])
                {
                    notestr.Add(items.Key.ToString(), items.Value);
                }
                Body body = new Body()
                {
                    PageSet = new PageSet()
                    {
                        BPM = patterner.barInfo[i].BPM,
                        lineActive = patterner.barInfo[i].lineActive,
                        beat = patterner.barInfo[i].beat
                    },
                    Notes = notestr
                };
                Pattern.Add(i.ToString(), body);
            }
            UniversalInfo data = new UniversalInfo()
            {
                PatternInformation = head,
                Pattern = Pattern
            };
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            string jsonString = JsonSerializer.Serialize(data, options);
            using (StreamWriter outputFile = new StreamWriter(@"Pattern.json"))
            {
                outputFile.Write(jsonString);
            }
        }

        public static (InitRoom, Dictionary<int, PatternInfo>, Dictionary<int, SortedDictionary<int, int[]>>) Load(string Addr = @"Pattern.json")
        {
            string jsonString = File.ReadAllText(Addr);
            UniversalInfo data = JsonSerializer.Deserialize<UniversalInfo>(jsonString);
            Dictionary<int, PatternInfo> barInfo = new Dictionary<int, PatternInfo>();
            Dictionary<int, SortedDictionary<int, int[]>> pattern = new Dictionary<int, SortedDictionary<int, int[]>>();

            InitRoom initRoom = new InitRoom(data.PatternInformation.Title, data.PatternInformation.Composer, data.PatternInformation.Difficulty, data.PatternInformation.Lines,
                data.PatternInformation.MainBeat, data.PatternInformation.MusicDelay, data.PatternInformation.Offset);
            foreach (KeyValuePair<string,Body> item in data.Pattern)
            {
                int bar = Convert.ToInt32(item.Key);
                barInfo.Add(bar, new PatternInfo(item.Value.PageSet.BPM, item.Value.PageSet.lineActive, item.Value.PageSet.beat));
                SortedDictionary<int, int[]> noteCache = new SortedDictionary<int, int[]>();
                foreach (KeyValuePair<string,int[]> item2 in item.Value.Notes)
                {
                    noteCache.Add(Convert.ToInt32(item2.Key), item2.Value);
                }
                pattern.Add(bar, noteCache);
            }

            return (initRoom, barInfo, pattern);
        }
    }

    public static class SavedScreen
    {
        public static int[] CountNote(Patterner patterner)
        {
            int[] count = new int[3];

            for (int i = 0; i < patterner.totalBars; i++)
            {
                foreach (KeyValuePair<int, int[]> items in patterner.Pattern[i])
                {
                    foreach (int note in items.Value)
                    {
                        if (note != 0) { count[note - 1]++; }
                    }
                }
            }

            return count;
        }

        public static void DrawSavedScreen(this SpriteBatch _spriteBatch, SpriteFont font, string count)
        {
            _spriteBatch.DrawString(font, " Saved. ", new Vector2(667-64, 750 / 2 - 96*2), Color.White);
            _spriteBatch.DrawString(font, count, new Vector2(667 - 64, 750 / 2), Color.White);
            _spriteBatch.DrawString(font, "OK (Enter)", new Vector2(667 - 64, 750 / 2 + 96 * 2), Color.White);
        }

        public static void DrawConvertedScreen(this SpriteBatch _spriteBatch, SpriteFont font, string count)
        {
            _spriteBatch.DrawString(font, " Save % Converted. ", new Vector2(667 - 64, 750 / 2 - 96 * 2), Color.White);
            _spriteBatch.DrawString(font, count, new Vector2(667 - 64, 750 / 2), Color.White);
            _spriteBatch.DrawString(font, "OK (Enter)", new Vector2(667 - 64, 750 / 2 + 96 * 2), Color.White);
        }
    }
}
