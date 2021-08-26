using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace NoctyurnPatterner
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D basicNote;
        private Texture2D onNote;
        private Texture2D offNote;
        private Texture2D judgeBox0;
        private SpriteFont _BarDscrp;

        private readonly string[] difficultyList = new string[4] { "Morning", "Day", "Night", "Dawn" };
        private readonly int[] lineList = new int[3] { 4, 6, 7 };
        private readonly int[] beatList = new int[4] { 1, 2, 3, 4 };

        private int roomNumber = 0;
        private int initSelect = 0;
        private bool selected = false;

        private KeyboardState oldState; //caches
        private Stack<char> stringCache = new Stack<char>();
        private int selectCache = 0;
        private float num;
        private string noteCount = "";
        private List<NoteObj> noteObjs = new List<NoteObj>();

        Patterner patterner;
        InitRoom initRoom = new InitRoom("", "", "Morning", 4, 4, 0, 0);
        PlayerInfo playerInfo;
        Converter converter;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;

            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            _graphics.PreparingDeviceSettings += Graphics_PreparingDeviceSettings;
            _graphics.ApplyChanges();

        }

        // Auxilary Functions
        private void Graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            _graphics.PreferMultiSampling = true;
            e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 8;
        }

        private bool KeyPressedOnce(Keys key)
        {
            return oldState.IsKeyUp(key) && Keyboard.GetState().IsKeyDown(key);
        }

        //Main Functions
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            DrawShapes.LoadContent(GraphicsDevice); //Drawshapes Load

            _graphics.PreferredBackBufferWidth = 1334; //Screen size
            _graphics.PreferredBackBufferHeight = 750;
            GraphicsDevice.PresentationParameters.MultiSampleCount = 8;
            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            basicNote = Content.Load<Texture2D>("BasicNote48"); //Note Sprites Load
            onNote = Content.Load<Texture2D>("OnNote48");
            offNote = Content.Load<Texture2D>("OffNote48");
            _BarDscrp = Content.Load<SpriteFont>("BarDscrp");
            judgeBox0 = Content.Load<Texture2D>("judge_0");

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            KeyboardState newState = Keyboard.GetState();

            switch (roomNumber)
            {
                case 0: //Init Room Control
                    if (!selected && KeyPressedOnce(Keys.Down)) //Init Screen Selection
                    {
                        initSelect = (initSelect + 1) % 8;
                    }
                    if (!selected && KeyPressedOnce(Keys.Up))
                    {
                        initSelect = (7 + initSelect) % 8;
                    }

                    switch (initSelect) //Selection Control
                    {
                        case 0: //Title Change
                            if (KeyPressedOnce(Keys.Enter))
                            {
                                if (!selected)
                                {
                                    stringCache.Clear();
                                    foreach (char c in initRoom.title.ToCharArray()) stringCache.Push(c);
                                }
                                else
                                {
                                    char[] chrCat = stringCache.ToArray();
                                    Array.Reverse(chrCat);
                                    initRoom.title = new string(chrCat);
                                }
                                selected = !selected;
                            }
                            else if (selected)
                            {
                                if (stringCache.Count != 0 && KeyPressedOnce(Keys.Back)) stringCache.Pop();
                                else if (stringCache.Count == 0 && KeyPressedOnce(Keys.Back)) { }
                                else if (Keyboard.GetState().GetPressedKeyCount() != 0)
                                {
                                    if (KeyPressedOnce(Keyboard.GetState().GetPressedKeys()[0]))
                                    {
                                        if (Key2Char.KeyToChar(Keyboard.GetState().GetPressedKeys()[0]) != '\0')
                                        {
                                            stringCache.Push(Key2Char.KeyToChar(Keyboard.GetState().GetPressedKeys()[0], Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift)));
                                        }
                                    }
                                }
                                char[] chrCat = stringCache.ToArray();
                                Array.Reverse(chrCat);
                                initRoom.title = new string(chrCat);
                            }
                            break;

                        case 1: //Composer Change
                            if (KeyPressedOnce(Keys.Enter))
                            {
                                if (!selected)
                                {
                                    stringCache.Clear();
                                    foreach (char c in initRoom.composer.ToCharArray()) stringCache.Push(c);
                                }
                                else
                                {
                                    char[] chrCat = stringCache.ToArray();
                                    Array.Reverse(chrCat);
                                    initRoom.composer = new string(chrCat);
                                }
                                selected = !selected;
                            }
                            else if (selected)
                            {
                                if (stringCache.Count != 0 && KeyPressedOnce(Keys.Back)) stringCache.Pop();
                                else if (stringCache.Count == 0 && KeyPressedOnce(Keys.Back)) { }
                                else if (Keyboard.GetState().GetPressedKeyCount() != 0)
                                {
                                    if (KeyPressedOnce(Keyboard.GetState().GetPressedKeys()[0]))
                                    {
                                        if (Key2Char.KeyToChar(Keyboard.GetState().GetPressedKeys()[0]) != '\0')
                                        {
                                            stringCache.Push(Key2Char.KeyToChar(Keyboard.GetState().GetPressedKeys()[0], Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift)));
                                        }
                                    }
                                }
                                char[] chrCat = stringCache.ToArray();
                                Array.Reverse(chrCat);
                                initRoom.composer = new string(chrCat);
                            }
                            break;

                        case 2: //Difficulty Change
                            selectCache = Array.IndexOf(difficultyList, initRoom.difficulty);
                            if (KeyPressedOnce(Keys.Right)) selectCache = (selectCache + 1) % 4;
                            else if (KeyPressedOnce(Keys.Left)) selectCache = (3 + selectCache) % 4;
                            initRoom.difficulty = difficultyList[selectCache];
                            break;

                        case 3: //Lines Change
                            selectCache = Array.IndexOf(lineList, initRoom.lines);
                            if (KeyPressedOnce(Keys.Right)) selectCache = (selectCache + 1) % 3;
                            else if (KeyPressedOnce(Keys.Left)) selectCache = (2 + selectCache) % 3;
                            initRoom.lines = lineList[selectCache];
                            break;

                        case 4: //Beat Change
                            selectCache = Array.IndexOf(beatList, initRoom.mainBeat);
                            if (KeyPressedOnce(Keys.Right)) selectCache = (selectCache + 1) % 4;
                            else if (KeyPressedOnce(Keys.Left)) selectCache = (3 + selectCache) % 4;
                            initRoom.mainBeat = beatList[selectCache];
                            break;

                        case 5: //Music Delay Change
                            if (KeyPressedOnce(Keys.Enter))
                            {
                                if (!selected)
                                {
                                    stringCache.Clear();
                                    foreach (char c in initRoom.delaystr.ToCharArray()) stringCache.Push(c);
                                }
                                else
                                {
                                    if (!int.TryParse(initRoom.delaystr, out initRoom.musicDelay)) initRoom.delaystr = initRoom.musicDelay.ToString();
                                }
                                selected = !selected;
                            }
                            else if (selected)
                            {
                                if (stringCache.Count != 0 && KeyPressedOnce(Keys.Back)) stringCache.Pop();
                                else if (stringCache.Count == 0 && KeyPressedOnce(Keys.Back)) { }
                                else if (Keyboard.GetState().GetPressedKeyCount() != 0)
                                {
                                    if (KeyPressedOnce(Keyboard.GetState().GetPressedKeys()[0]))
                                    {
                                        if (Key2Char.KeyToChar(Keyboard.GetState().GetPressedKeys()[0]) != '\0')
                                        {
                                            stringCache.Push(Key2Char.KeyToChar(Keyboard.GetState().GetPressedKeys()[0], Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift)));
                                        }
                                    }
                                }
                                char[] chrCat = stringCache.ToArray();
                                Array.Reverse(chrCat);
                                initRoom.delaystr = new string(chrCat);
                            }
                            break;

                        case 6: //Offset Change
                            if (KeyPressedOnce(Keys.Enter))
                            {
                                if (!selected)
                                {
                                    stringCache.Clear();
                                    foreach (char c in initRoom.offsetstr.ToCharArray()) stringCache.Push(c);
                                }
                                else
                                {
                                    if (!float.TryParse(initRoom.offsetstr, out initRoom.offset)) initRoom.offsetstr = initRoom.offset.ToString();
                                }
                                selected = !selected;
                            }
                            else if (selected)
                            {
                                if (stringCache.Count != 0 && KeyPressedOnce(Keys.Back)) stringCache.Pop();
                                else if (stringCache.Count == 0 && KeyPressedOnce(Keys.Back)) { }
                                else if (Keyboard.GetState().GetPressedKeyCount() != 0)
                                {
                                    if (KeyPressedOnce(Keyboard.GetState().GetPressedKeys()[0]))
                                    {
                                        if (Key2Char.KeyToChar(Keyboard.GetState().GetPressedKeys()[0]) != '\0')
                                        {
                                            stringCache.Push(Key2Char.KeyToChar(Keyboard.GetState().GetPressedKeys()[0], Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift)));
                                        }
                                    }
                                }
                                char[] chrCat = stringCache.ToArray();
                                Array.Reverse(chrCat);
                                initRoom.offsetstr = new string(chrCat);
                            }
                            break;

                        case 7: //Patterner Execute
                            if (KeyPressedOnce(Keys.Right)) selected = !selected;
                            else if (KeyPressedOnce(Keys.Left)) selected = !selected;
                            if (KeyPressedOnce(Keys.Enter))
                            {
                                if (!selected) //Confirm
                                {
                                    patterner = new Patterner(initRoom);
                                    this.Window.Title = "[Noctyurn Patterner] " + initRoom.title + " - " + initRoom.composer;

                                    roomNumber = 1;
                                }
                                else //Load
                                {
                                    var loadResult = SaveLoad.Load();

                                    initRoom = loadResult.Item1;
                                    patterner = new Patterner(initRoom);
                                    this.Window.Title = "[Noctyurn Patterner] " + initRoom.title + " - " + initRoom.composer;

                                    patterner.barInfo = loadResult.Item2;
                                    patterner.totalBars = patterner.barInfo.Count;
                                    patterner.ChangeBarInfo(0, patterner.barInfo[0]);
                                    patterner.Pattern = loadResult.Item3;

                                    selected = false;
                                    roomNumber = 1;
                                }
                            }
                            break;
                    }
                    break;

                case 1: //Patterner Room Control
                    if (!selected)
                    {
                        if (KeyPressedOnce(Keys.Tab)) { patterner.AddBar(); } //page config
                        if (KeyPressedOnce(Keys.OemPlus)) { patterner.NextBar(); }
                        if (KeyPressedOnce(Keys.OemMinus)) { patterner.PrevBar(); }

                        if (KeyPressedOnce(Keys.B)) //bpm
                        {
                            stringCache.Clear();
                            selected = true;
                            patterner.configSel = 1;
                        }

                        if (KeyPressedOnce(Keys.T)) //beat
                        {
                            selectCache = (Array.IndexOf(beatList, patterner.CurInfo().beat) + 3) % 4;
                            patterner.ChangeBarInfo(patterner.bar, new PatternInfo(patterner.barInfo[patterner.bar].BPM, patterner.barInfo[patterner.bar].lineActive, beatList[selectCache]));
                            if (patterner.position[1] >= 24 * patterner.CurInfo().beat) { patterner.position[1] = patterner.position[1] % 24 + 24 * (patterner.CurInfo().beat - 1); }
                        }

                        if (KeyPressedOnce(Keys.N)) { patterner.notes.changeNote(); } //note type
                        if (KeyPressedOnce(Keys.G)) //grid
                        {
                            patterner.grid.ChangeGrid();
                            patterner.position[1] = patterner.position[1] / patterner.grid.SnapQ() * patterner.grid.SnapQ();
                        }

                        if (KeyPressedOnce(Keys.Right) && patterner.position[0] < patterner.lines - 1) { patterner.position[0]++; }//Note Movement
                        if (KeyPressedOnce(Keys.Left) && patterner.position[0] > 0) { patterner.position[0]--; }
                        if (KeyPressedOnce(Keys.Up) && patterner.position[1] < 96 / 4 * patterner.barInfo[patterner.bar].beat - 1) { patterner.position[1] += +patterner.grid.SnapQ(); }
                        if (KeyPressedOnce(Keys.Down) && patterner.position[1] > 0) { patterner.position[1] -= patterner.grid.SnapQ(); }

                        if ((KeyPressedOnce(Keys.Space) || KeyPressedOnce(Keys.Enter)) && patterner.position[1] != 96) { patterner.AddNote();  }

                        if (KeyPressedOnce(Keys.S)) //save
                        {
                            SaveLoad.Save(initRoom, patterner);
                            int[] count = SavedScreen.CountNote(patterner);
                            noteCount = $"Basic : {count[0]}\nOn : {count[1]}\nOff : {count[2]}\nTotal : {count[0]+count[1]+count[2]}";
                            roomNumber = 3;
                        }

                        if (KeyPressedOnce(Keys.C)) //Convert
                        {
                            SaveLoad.Save(initRoom, patterner);
                            converter = new Converter(patterner, initRoom);
                            converter.Convert();
                            int[] count = SavedScreen.CountNote(patterner);
                            noteCount = $"Basic : {count[0]}\nOn : {count[1]}\nOff : {count[2]}\nTotal : {count[0] + count[1] + count[2]}";
                            roomNumber = 4;
                        }

                        if (patterner.lines == 7)
                        {
                            if (KeyPressedOnce(Keys.D1)) { patterner.LineSwitch(1); }
                            if (KeyPressedOnce(Keys.D2)) { patterner.LineSwitch(2); }
                            if (KeyPressedOnce(Keys.D3)) { patterner.LineSwitch(3); }
                            if (KeyPressedOnce(Keys.D4)) { patterner.LineSwitch(4); }
                            if (KeyPressedOnce(Keys.D5)) { patterner.LineSwitch(5); }
                            if (KeyPressedOnce(Keys.D6)) { patterner.LineSwitch(6); }
                            if (KeyPressedOnce(Keys.D7)) { patterner.LineSwitch(7); }
                        }
                    }
                    else if (patterner.configSel == 1) //BPM Change
                    {
                        if (stringCache.Count != 0 && KeyPressedOnce(Keys.Back)) stringCache.Pop();
                        else if (KeyPressedOnce(Keys.Enter))
                        {
                            patterner.configSel = 0;
                            selected = false;
                            if (float.TryParse(patterner.BPMstr, out num))
                            {
                                patterner.ChangeBarInfo(patterner.bar, new PatternInfo(num, patterner.barInfo[patterner.bar].lineActive, patterner.barInfo[patterner.bar].beat));
                            }
                            else { patterner.BPMstr = patterner.CurInfo().BPM.ToString(); }
                        }
                        else if (Keyboard.GetState().GetPressedKeyCount() != 0)
                        {
                            if (KeyPressedOnce(Keyboard.GetState().GetPressedKeys()[0]))
                            {
                                if (Key2Char.KeyToChar(Keyboard.GetState().GetPressedKeys()[0]) != '\0')
                                {
                                    stringCache.Push(Key2Char.KeyToChar(Keyboard.GetState().GetPressedKeys()[0], Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift)));
                                }
                            }
                        }
                        char[] chrCat = stringCache.ToArray();
                        Array.Reverse(chrCat);
                        patterner.BPMstr = new string(chrCat);
                    }
                    break;

                case 3: //Saved Room Control
                    if (KeyPressedOnce(Keys.Enter)) { roomNumber = 1; }
                    break;

                case 4: //Converted Room Control
                    if (KeyPressedOnce(Keys.Enter)) { roomNumber = 1; }
                    break;
            }

            oldState = newState;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            _spriteBatch.Begin(blendState: BlendState.NonPremultiplied);
            switch (roomNumber)
            {
                case 0: //Init Room Draw
                    PatternerScreen.DrawInitScreen(_spriteBatch, _BarDscrp, initSelect, initRoom, selected);
                    break;
                case 1: //Patterner Draw
                    PatternerScreen.DrawFrame(_spriteBatch, _BarDscrp, patterner);
                    _spriteBatch.Draw(judgeBox0, patterner.posVector(patterner.position), null, Color.White, 0f, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0f);
                    switch (patterner.notes.select)
                    {
                        case 0:
                            _spriteBatch.Draw(basicNote, patterner.posVector(patterner.position), Color.White);
                            break;
                        case 1:
                            _spriteBatch.Draw(onNote, patterner.posVector(patterner.position), Color.White);
                            break;
                        case 2:
                            _spriteBatch.Draw(offNote, patterner.posVector(patterner.position), Color.White);
                            break;
                    }
                    foreach (KeyValuePair<int, int[]> items in patterner.Pattern[patterner.bar])
                    {
                        for (int i = 0; i < items.Value.Length; i++)
                        {
                            if (items.Value[i] == 0) { continue; }
                            switch (items.Value[i])
                            {
                                case 1:
                                    _spriteBatch.Draw(basicNote, patterner.posVector(new int[2] { i, items.Key }), Color.White);
                                    break;
                                case 2:
                                    _spriteBatch.Draw(onNote, patterner.posVector(new int[2] { i, items.Key }), Color.White);
                                    break;
                                case 3:
                                    _spriteBatch.Draw(offNote, patterner.posVector(new int[2] { i, items.Key }), Color.White);
                                    break;
                            }
                        }
                    }
                    break;

                case 3: //Saved Draw
                    SavedScreen.DrawSavedScreen(_spriteBatch, _BarDscrp, noteCount);
                    break;

                case 4: //Converted Draw
                    SavedScreen.DrawConvertedScreen(_spriteBatch, _BarDscrp, noteCount);
                    break;
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
