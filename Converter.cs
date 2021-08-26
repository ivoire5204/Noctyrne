using System;
using System.Collections.Generic;
using System.IO;

namespace NoctyurnPatterner
{
    public class Converter
    {
        public Dictionary<int, PatternInfo>  barInfo;
        public Dictionary<int, SortedDictionary<int, int[]>> Pattern;
        //public SortedDictionary<int, float> hispeedDict; //일단 변속없이 구현
        public SortedDictionary<float, List<int[]>> times;
        SortedDictionary<float, Int16> lineActiveTime;
        int[] count;
        public int musicDelay;
        public float offset;
        public int lines;

        public Converter(Patterner patterner, InitRoom init)
        {
            barInfo = patterner.barInfo;
            Pattern = patterner.Pattern;
            //hispeedDict = makeSpeedInfo();
            musicDelay = init.musicDelay;
            offset = init.offset;
            lines = init.lines;
            (times, lineActiveTime) = makeTiming();
            count = SavedScreen.CountNote(patterner);
            Console.WriteLine(Pattern);
        }

        /*
        public SortedDictionary<int, float> makeSpeedInfo()
        {
            SortedDictionary<int, float> hispeedDict = new SortedDictionary<int, float>();
            int i = 0;
            foreach (KeyValuePair<int,PatternInfo>items in barInfo)
            {
                hispeedDict.Add(i, items.Value.speed);
                i += items.Value.beat;
            }
            return hispeedDict;
        }
        */

        public (SortedDictionary<float, List<int[]>>, SortedDictionary<float, short>) makeTiming()
        {
            SortedDictionary<float, List<int[]>> times = new SortedDictionary<float, List<int[]>>();
            SortedDictionary<float, short> lineActiveTime = new SortedDictionary<float, short>();
            int[] note = new int[2];
            int i = 0;
            float interval = 60f/barInfo[0].BPM; //time per one beat
            float initOffset = interval * musicDelay + offset;

            foreach (KeyValuePair<int, SortedDictionary<int, int[]>> item1 in Pattern)
            {
                short activity = 0;
                for (short j = 0; j < lines; j++)
                {
                    activity += (short)((barInfo[item1.Key].lineActive[j] ? 1 : 0) * (short)Math.Pow(2, j));
                }

                foreach (KeyValuePair<int, int[]> item2 in item1.Value)
                {
                    float time = initOffset + i * interval + item2.Key * interval / 24;
                    times.Add(time, new List<int[]>());
                    lineActiveTime.Add(time, activity);
                    for (int j = 0; j < lines; j++)
                    {
                        if (item2.Value[j] != 0)
                        {
                            times[time].Add(new int[] { j, item2.Value[j] });
                        }
                    }
                }

                i += barInfo[item1.Key].beat;
            }

            return (times, lineActiveTime);
        }


        public void Convert()
        {
            FileStream fs = File.Open("Pattern.nctr", FileMode.Create);
            byte[] int2byte = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 };
            using (BinaryWriter wr = new BinaryWriter(fs))
            {
                wr.Write(count[0]);
                wr.Write(count[1]);
                wr.Write(count[2]);
                wr.Write(lines);
                foreach (KeyValuePair<float,List<int[]>> item in times)
                {
                    foreach (int[] item2 in item.Value)
                    {
                        wr.Write(item.Key);
                        wr.Write(item.Key*0);
                        wr.Write(int2byte[item2[0]]);
                        wr.Write(int2byte[item2[1]]);
                        wr.Write(1f);
                        wr.Write(lineActiveTime[item.Key]);
                    }
                }
            }
        }
    }
}
