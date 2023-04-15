using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Project_B_V2._0
{
    internal class Program
    {
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        private static IntPtr ThisCon = GetConsoleWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // State of the application once loaded
        private const int HIDE = 0;
        private const int MAXIMIZE = 3;
        private const int MINIMIZE = 6;
        private const int RESTORE = 9;


        private static int currentScreen;
        private static int lastscreen;
        private static List<Screen> screens = new List<Screen>();

        static void Main(string[] args)
        {
            ShowWindow(ThisCon, MAXIMIZE);
            screens.Add(new HomeScreen()); // 0
            screens.Add(new TestDataGeneratorScreen()); // 1
            screens.Add(new AfdelingshoofdScherm()); //2
            
            currentScreen = 0;
            do
            {
                Display();
                Refresh();
            } while (currentScreen != -1);
        }

        static internal void Display()
        {
            lastscreen = currentScreen;
            currentScreen = screens[currentScreen].DoWork();
            screens = screens[lastscreen].Update(screens);
        }

        static internal void Refresh()
        {
            Console.Clear();
        }
    }

    internal abstract partial class Screen
    {
        protected const string ESCAPE_KEY = "ESCAPE";
        protected const string ENTER_KEY = "ENTER";
        protected const string BACKSPACE_KEY = "BACKSPACE";
        protected const string UP_ARROW = "UPARROW";
        protected const string DOWN_ARROW = "DOWNARROW";
        protected const string LEFT_ARROW = "LEFTARROW";
        protected const string RIGHT_ARROW = "RIGHTARROW";
        public string previousScreen = "one";

        /// <summary>
        /// This is the main function of the current screen. Here is all the logic of that current screen
        /// </summary>
        /// <returns>This function returns the ID of the next screen to display</returns>
        internal abstract int DoWork();

        /// <summary>
        /// This function updates all screens with data from one screen to an other
        /// </summary>
        /// <param name="screens">This is the list of screens to update</param>
        /// <returns>This returns the same list you just gave as param but now it has been updated with information</returns>
        internal abstract List<Screen> Update(List<Screen> screens);

        /// <summary>
        /// Returns true if the key with the specified keycode is pressed.
        /// </summary>
        /// <param name="key">This is the name of the key, use one of the key constants described in the BaseScreen.</param>
        /// <returns>True if the right key is pressed, false is not</returns>
        protected static bool IsKeyPressed(ConsoleKeyInfo cki, string key) => cki.Key.ToString().ToUpper() == key.ToUpper();


        protected (string, int) AskForInput(int screenIndex)
        {
            bool AskRepeat = true;
            List<char> output = new();

            while (AskRepeat)
            {
                ConsoleKeyInfo CKInfo = Console.ReadKey(true);

                if (CKInfo.KeyChar == '\0') continue;

                if (IsKeyPressed(CKInfo, ENTER_KEY)) break;

                if (IsKeyPressed(CKInfo, ESCAPE_KEY)) return (null, screenIndex);

                if (IsKeyPressed(CKInfo, BACKSPACE_KEY))
                {
                    (int, int) curserPos = Console.GetCursorPosition();
                    if (curserPos.Item1 > 0)
                    {
                        Console.SetCursorPosition(curserPos.Item1 - 1, curserPos.Item2);
                        Console.Write(" ");
                        output.RemoveAt(output.Count - 1);
                    }
                }
                else
                {
                    output.Add(CKInfo.KeyChar);
                }

                if (CKInfo.KeyChar != '\0')
                {
                    Console.Write(CKInfo.KeyChar);
                }
            }

            // -1 means no interruptions has been found while asking for input
            return (string.Join(null, output), -1);
        }

        /// <summary>
        /// You use this function if you want to make 1 box
        /// </summary>
        /// <param name="input">This is the list of string wich are the lines where the box needs to made around</param>
        /// <param name="sym">This is the symbole the box will be made from</param>
        /// <param name="spacingside">This is the amount of spaces between the sides of the box and the lines</param>
        /// <param name="spacingtop">This is the amount of lines that will be added before and after the lines</param>
        /// <param name="maxlength">This is the max length of a line</param>
        /// <param name="openbottom">If this is true the box will not have a bottom made from sym</param>
        /// <returns>This function returns a string with is a box of sym around it</returns>
        protected static string BoxAroundText(List<string> input, string sym, int spacingside, int spacingtop, int maxlength, bool openbottom)
        {
            string output = new string(Convert.ToChar(sym), maxlength + 2 + spacingside * 2) + "\n";
            for (int a = 0; a < spacingtop; a++)
            {
                output += sym + new string(' ', maxlength + spacingside * 2) + sym + "\n";
            }

            foreach (var line in input)
            {
                output += sym + new string(' ', spacingside) + line + new string(' ', spacingside) + sym + "\n";
            }

            for (int a = 0; a < spacingtop; a++)
            {
                output += sym + new string(' ', maxlength + spacingside * 2) + sym + "\n";
            }

            if (openbottom)
            {
                return output;
            }
            return output += new string(Convert.ToChar(sym), maxlength + 2 + spacingside * 2) + "\n";
        }

        /// <summary>
        /// you use this function if you want to make a list of boxes
        /// </summary>
        /// <param name="blocks">This is a list of list string where this is a list of list of lines</param>
        /// <param name="sym">This is the symbole the box will be made from</param>
        /// <param name="spacingside">This is the amount of spaces between the sides of the box and the lines</param>
        /// <param name="spacingtop">This is the amount of lines that will be added before and after the lines</param>
        /// <param name="maxlength">his is the max length of a line</param>
        /// <param name="openbottom">If this is true the box will not have a bottom made from sym</param>
        /// <returns>This function returns a list of boxes</returns>
        protected static List<string> BoxAroundText(List<List<string>> blocks, string sym, int spacingside, int spacingtop, int maxlength, bool openbottom)
        {
            List<string> output = new List<string>();

            foreach (var input in blocks)
            {
                string block = new string(Convert.ToChar(sym), maxlength + 2 + spacingside * 2) + "\n";
                for (int a = 0; a < spacingtop; a++)
                {
                    block += sym + new string(' ', maxlength + spacingside * 2) + sym + "\n";
                }

                foreach (var line in input)
                {
                    block += sym + new string(' ', spacingside) + line + new string(' ', spacingside) + sym + "\n";
                }

                for (int a = 0; a < spacingtop; a++)
                {
                    block += sym + new string(' ', maxlength + spacingside * 2) + sym + "\n";
                }

                if (!openbottom)
                {
                    block += new string(Convert.ToChar(sym), maxlength + 2 + spacingside * 2) + "\n";
                }

                output.Add(block);
            }
            return output;
        }

        /// <summary>
        /// You use this function if you want to make a  box with custom bottom text
        /// </summary>
        /// <param name="input">This is the list of string wich are the lines where the box needs to made around</param>
        /// <param name="sym">This is the symbole the box will be made from</param>
        /// <param name="spacingside">This is the amount of spaces between the sides of the box and the lines</param>
        /// <param name="spacingtop">This is the amount of lines that will be added before and after the lines</param>
        /// <param name="maxlength">his is the max length of a line</param>
        /// <param name="openbottom">If this is true the box will not have a bottom made from sym</param>
        /// <param name="bottomtext">This is a list of strings where this is an extra list of lines</param>
        /// <returns>This function returns a list of strings with consists of a list of boxes with sym around it and has custom bottom text</returns>
        protected static string BoxAroundText(List<string> input, string sym, int spacingside, int spacingtop, int maxlength, bool openbottom, List<string> bottomtext)
        {
            string output = new string(Convert.ToChar(sym), maxlength + 2 + spacingside * 2) + "\n";
            for (int a = 0; a < spacingtop; a++)
            {
                output += sym + new string(' ', maxlength + spacingside * 2) + sym + "\n";
            }

            foreach (var line in input)
            {
                output += sym + new string(' ', spacingside) + line + new string(' ', spacingside) + sym + "\n";
            }

            for (int b = 0; b < bottomtext.Count; b++)
            {
                output += sym + new string(' ', spacingside) + bottomtext[b] + new string(' ', spacingside) + sym + "\n";
            }

            for (int a = 0; a < spacingtop; a++)
            {
                output += sym + new string(' ', maxlength + spacingside * 2) + sym + "\n";
            }

            if (openbottom)
            {
                return output;
            }
            return output += new string(Convert.ToChar(sym), maxlength + 2 + spacingside * 2) + "\n";
        }

        protected static List<string> MakePages(List<string> alldata, int maxblocks)
        {
            string[] output = new string[alldata.Count / maxblocks + 1];

            for (int a = 0, b = 1; a < alldata.Count; a++)
            {
                if (a < maxblocks * b)
                {
                    output[b - 1] += alldata[a];
                }
                else
                {
                    b++;
                    output[b - 1] += alldata[a];
                }
            }

            List<string> done = output.ToList();
            done.RemoveAll(x => x == null);
            return done;
        }

        protected (int, int, double) Nextpage(int page, double pos, double maxpos, int screenIndex, List<Tuple<(int, int, double), string>> choices, List<string> text)
        {
            foreach (var item in text)
            {
                Console.WriteLine(item);
            }
            ConsoleKeyInfo key = new ConsoleKeyInfo();
            do
            {
                key = new ConsoleKeyInfo();
                key = Console.ReadKey();
            } while (IsKeyPressed(key, ENTER_KEY));
            if (IsKeyPressed(key, ESCAPE_KEY))
            {
                return (page, screenIndex, pos);
            }
            if (IsKeyPressed(key, UP_ARROW))
            {
                if (pos % 2 != 0)
                {
                    if ((pos - 1 > 6 * page && page != 0) || (pos > 2 && page == 0))
                    {
                        pos -= 2;
                    }
                }
                else
                {
                    if ((pos > 6 * page && page != 0) || (pos > 1 && page == 0))
                    {
                        pos -= 2;
                    }
                }
                return (page, -1, pos);
            }
            else if (IsKeyPressed(key, DOWN_ARROW))
            {
                if (pos % 2 != 0)
                {
                    if (((pos + 1 < 6 * (page + 1) && page != 0) || pos < 4) && pos < maxpos - 1)
                    {
                        pos += 2;
                    }
                }
                else
                {
                    if (((pos + 2 < 6 * (page + 1) && page != 0) || pos < 4) && pos < maxpos - 1)
                    {
                        pos += 2;
                    }
                }
                return (page, -1, pos);
            }
            else if (IsKeyPressed(key, LEFT_ARROW))
            {
                if (pos % 2 != 0 && pos > 0)
                {
                    pos -= 1;
                }
                return (page, -1, pos);
            }
            else if (IsKeyPressed(key, RIGHT_ARROW))
            {
                if ((pos % 2 == 0 || pos == 0) && pos < maxpos)
                {
                    pos += 1;
                }
                return (page, -1, pos);
            }

            Console.ReadKey();
/*            if (IsKeyPressed(key, "D0") || IsKeyPressed(key, "NumPad0"))
            {
                logoutUpdate = true;
                Logout();
                return (page, 0, pos);
            }*/
            foreach (var choice in choices)
            {
                if (IsKeyPressed(key, choice.Item2))
                {
                    return choice.Item1;
                }
            }

            Console.WriteLine("U moet wel een juiste keuze maken...");
            Console.WriteLine("Druk op en knop om verder te gaan.");
            Console.ReadKey();
            return (page, -1, pos);
        }

        protected (int, int) Nextpage(int page, int maxpage, int screenIndex)
        {
            if (page < maxpage)
            {
                Console.WriteLine("[1] Volgende pagina");
                Console.WriteLine("[2] Terug");
                ConsoleKeyInfo key = Console.ReadKey();
                if (IsKeyPressed(key, ESCAPE_KEY))
                {
                    return (page, screenIndex);
                }
                Console.ReadKey();
                if (IsKeyPressed(key, "D1"))
                {
                    return (page + 1, -1);
                }
                else if (IsKeyPressed(key, "D2"))
                {
                    return (page, screenIndex);
                }
/*                else if (IsKeyPressed(key, "D0"))
                {
                    logoutUpdate = true;
                    Logout();
                    return (page, 0);
                }*/
                else
                {
                    Console.WriteLine("U moet wel een juiste keuze maken...");
                    Console.WriteLine("Druk op en knop om verder te gaan.");
                    Console.ReadKey();
                    return (page, -1);
                }
            }
            else
            {
                Console.WriteLine("[1] Terug");
                ConsoleKeyInfo key = Console.ReadKey();
                if (IsKeyPressed(key, ESCAPE_KEY))
                {
                    return (page, screenIndex);
                }
                Console.ReadKey();
                if (IsKeyPressed(key, "D1"))
                {
                    return (page, screenIndex);
                }
/*                else if (IsKeyPressed(key, "D0"))
                {
                    logoutUpdate = true;
                    Logout();
                    return (page, 0);
                }*/
                else
                {
                    Console.WriteLine("U moet wel een juiste keuze maken...");
                    Console.WriteLine("Druk op en knop om verder te gaan.");
                    Console.ReadKey();
                    return (page, -1);
                }
            }
        }
        

        /// <summary>
        /// you use this function if you want to put 2 lists of lines together to make one big box
        /// </summary>
        /// <param name="input">This is the list of list string where this is a list of list lines</param>
        /// <param name="sym">This is the symbole you want to use as a saperator for the 2 boxes. It is smart to use the same symbol here you also use for the boxaroundtext function</param>
        /// <returns>This returns a list of list lines</returns>
        protected List<List<string>> MakeDubbelBoxes(List<List<string>> input, string sym)
        {
            List<List<string>> output = new List<List<string>>();
            for (int a = 0; a < input.Count; a += 2)
            {
                //When you have a uneven list you need to check if you are at the last item and then break.
                //This is because we take steps of 2 in this forloop (a+=2)
                if (a == input.Count - 1)           
                {
                    output.Add(input[a]);
                    break;
                }
                //blocknew is new list of blocks where block n and block n + 1 are concatenated togerther.
                //This means that all the lines from blockold1 aka block n and all the lines from blockold2 aka block n + 1 are added together.
                //This will make one big block with sym + sym as a seperator thus making 2 boxes next to each other
                List<string> blocknew = new List<string>();
                List<string> blockold1 = input[a];
                List<string> blockold2 = input[a + 1];

                for (int b = 0; b < blockold1.Count; b++)
                {
                    blocknew.Add(blockold1[b] + $"{sym + sym}  " + blockold2[b]);
                }
                output.Add(blocknew);
            }

            return output;
        }
    }

    internal class StartScreen : Screen
    {
        /// <summary>
        /// This is the main entrypoint for the current screen. In here you can do whatever you want your screen to do.
        /// </summary>
        /// <returns>here you return the index of the next screen. This index is based on the Screens field in the program class</returns>
        internal override int DoWork()
        {
            Console.WriteLine("SHoofdscherm");
            Console.WriteLine($"Druk op [1] + [ENTER] om naar het scherm te gaan om test data aan te maken.");
            Console.WriteLine("Druk op [2] + [ENTER] om naar homescherm te gaan");
            Console.WriteLine("Druk op [ESC] om af te sluiten");
            
            (string, int) answer = AskForInput(-1);
            return Convert.ToInt32(answer.Item1);
        }

        /// <summary>
        /// This function is needed when you need to update information on all the screens. For example when you are logged in or logged uit.
        /// It is done this way to avoid shared state.
        /// </summary>
        /// <param name="screens"></param>
        /// <returns></returns>
        internal override List<Screen> Update(List<Screen> screens)
        {
            return screens;
        }
    }

    internal class TestDataGeneratorScreen : Screen
    {
        /// <summary>
        /// This is the main entrypoint for the current screen. In here you can do whatever you want your screen to do.
        /// </summary>
        /// <returns>here you return the index of the next screen. This index is based on the Screens field in the program class</returns>
        internal override int DoWork()
        {
            Console.WriteLine("TestDataGeneratorScreen");
            Console.WriteLine("Druk op [1] + [ENTER] om unieke codes aan te maken.");
            Console.WriteLine("Druk op [2] + [ENTER] om gebruikers aan te maken.");
            Console.WriteLine("Druk op [3] + [ENTER] om rondleidingen aan te maken.");
            (string, int) answer = AskForInput(0);
            
            if (answer.Item1 == "1")
            {
                (List<int>, Exception) result = TestDataGenerator.MaakUniekeCodes(10);
                if (result.Item2.Message != "Exception of type 'System.Exception' was thrown.")
                {
                    Console.WriteLine($"Er is een error opgetreden: {result.Item1}");
                    Thread.Sleep(4000);
                    return 1;
                }
                Console.WriteLine();
                Console.WriteLine("De unieke codes zijn aangemaakt. Druk op [ESC] om terug te gaan of druk op [1] + [ENTER] om de aangemaakte unieke codes te zien");
                answer = AskForInput(0);
                Console.WriteLine();
                if (answer.Item1 == "1")
                {
                    List<string> uniekeCodes = new List<string>();
                    foreach (var code in result.Item1)
                    {
                        uniekeCodes.Add("".PadRight(21));
                        uniekeCodes.Add($"Unieke code: {code}".PadRight(20));
                        uniekeCodes.Add("".PadRight(21));
                    }

                    string box = BoxAroundText(uniekeCodes, "#", 2, 0, 21, false);
                    Console.WriteLine(box);
                    Console.WriteLine("Druk op een toets om terug te gaan.");
                    Console.ReadKey(false);
                }
                return 0;
            }
            else if (answer.Item1 == "2")
            {
                bool isNum = false;
                Console.WriteLine();
                do
                {                  
                    Console.WriteLine("Hoeveel gebruikers wilt u aanmaken: ");
                    answer = AskForInput(0);
                    isNum = int.TryParse(answer.Item1, out _);
                    if (!isNum)
                    {
                        Console.WriteLine("Dit was geen getal...");
                        Thread.Sleep(2000);
                        Console.Clear();
                    }
                } while (!isNum);
                
                (List<User>, Exception) result = TestDataGenerator.MaakGebruikers(Convert.ToInt32(answer.Item1));
                if (result.Item2.Message != "Exception of type 'System.Exception' was thrown.")
                {
                    Console.WriteLine($"Er is een error opgetreden: {result.Item2}");
                    Thread.Sleep(4000);
                    return 1;
                }
                Console.WriteLine();
                JsonManager.SerializeGebruikers(result.Item1);
                Console.WriteLine("De gebruikers zijn aangemaakt. Druk op een toets om terug te gaan of druk op [1] + [ENTER] om de aangemaakte users te zien.");
                answer = AskForInput(0);
                Console.WriteLine();
                if (answer.Item1 == "1")
                {
                    List<List<string>> gebruikers = new List<List<string>>();
                    foreach (var gebruiker in result.Item1)
                    {
                        List<string> gebruikerInfo = new List<string>
                        {
                            "".PadRight(40),
                            "".PadRight(40),
                            $"Unieke code: {gebruiker.UniekeCode}".PadRight(40),
                            $"Reservering datum: {gebruiker.Reservering}".PadRight(40),
                            "".PadRight(40),
                            "".PadRight(40),
                        };
                        gebruikers.Add(gebruikerInfo);
                    }

                    List<string> boxes = BoxAroundText(MakeDubbelBoxes(gebruikers, "#"), "#", 2, 0, 84, false);
;
                    for (int a = 0; a < boxes.Count; a++)
                    {
                        Console.WriteLine(boxes[a]);
                    }
                    Console.WriteLine("Druk op een toets om terug te gaan.");
                    Console.ReadKey(false);
                }
                return 0;
            }
            else if (answer.Item1 == "3")
            {
                Console.WriteLine("Geef de start datum op vanaf wanneer je rondleidngen wilt maken. Format: dd-MM-YYYY");
                DateTime start = Convert.ToDateTime(Console.ReadLine());
                Console.WriteLine("Geef de eind datum op vanaf wanneer je rondleidngen wilt maken. Format: dd-MM-YYYY");
                DateTime end = Convert.ToDateTime(Console.ReadLine());
                (List<Rondleiding>, Exception) result = TestDataGenerator.MaakRondleidingen(start, end);
                if (result.Item2.Message != "Exception of type 'System.Exception' was thrown.")
                {
                    Console.WriteLine($"Er is een error opgetreden: {result.Item2}");
                    Thread.Sleep(4000);
                    return 1;
                }

                JsonManager.SerializeRondleidingen(result.Item1);

                Console.WriteLine("Rondleidingen zijn opgeslagen!");
                Thread.Sleep(4000);
            }
            return Convert.ToInt32(answer.Item1);
        }

        /// <summary>
        /// This function is needed when you need to update information on all the screens. For example when you are logged in or logged uit.
        /// It is done this way to avoid shared state.
        /// </summary>
        /// <param name="screens"></param>
        /// <returns></returns>
        internal override List<Screen> Update(List<Screen> screens)
        {
            return screens;
        }
    }

    internal class HomeScreen : Screen {
        internal override int DoWork()
        {
            int pos = 0;
            bool cont = true;
            List<List<string>> rondleidingInformatie = new List<List<string>>();
            List<DateTime> tijden = new List<DateTime>();
            DateTime time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 11, 0, 0);
            for (int i = 0; i < 18; i++)
            {
                tijden.Add(time);
                rondleidingInformatie.Add(new List<string>
                {
                    (time.ToShortTimeString() + "-" + time.AddMinutes(40).ToShortTimeString()).PadRight(19),
                    "".PadRight(19),

                });
                time = time.AddMinutes(20);
            }

            do
            {
                List<string> boxes = new List<string>();
                if (pos == 0)
                {
                    boxes.Add(BoxAroundText(MakeDubbelBoxes(rondleidingInformatie.GetRange(0, 2), "#")[0], "#", 2, 0, 42, true,
                        new List<string> { "[1] Reserveren     ##".PadRight(42), "##".PadLeft(21) + "".PadRight(21) }));

                    boxes.AddRange(BoxAroundText(MakeDubbelBoxes(rondleidingInformatie.GetRange(2, rondleidingInformatie.Count - 2), "#")
                        , "#", 2, 0, 42, true));
                }
                else if (pos == rondleidingInformatie.Count)
                {
                    boxes.AddRange(BoxAroundText(MakeDubbelBoxes(rondleidingInformatie.GetRange(0, rondleidingInformatie.Count - 2), "#"), "#", 2, 0, 42, true));

                    boxes.Add(BoxAroundText(MakeDubbelBoxes(rondleidingInformatie.GetRange(rondleidingInformatie.Count - 2, 2), "#")[0], "#", 2, 0, 42, true,
                        new List<string> { "[1] Reserveren     ##".PadRight(42), "##".PadLeft(21) + "".PadRight(21) }));
                }
                else if (pos % 2 == 0)
                {
                    boxes.AddRange(BoxAroundText(MakeDubbelBoxes(rondleidingInformatie.GetRange(0, pos), "#"), "#", 2, 0, 42, true));

                    boxes.Add(BoxAroundText(MakeDubbelBoxes(rondleidingInformatie.GetRange(pos, 2), "#")[0], "#", 2, 0, 42, true,
                        new List<string> { "[1] Reserveren     ##".PadRight(42), "##".PadLeft(21) + "".PadRight(21) }));

                    boxes.AddRange(BoxAroundText(MakeDubbelBoxes(rondleidingInformatie.GetRange(pos + 2, rondleidingInformatie.Count - (pos + 2)), "#"), "#", 2, 0, 42, true));
                }
                else if (pos % 2 == 1)
                {
                    boxes.AddRange(BoxAroundText(MakeDubbelBoxes(rondleidingInformatie.GetRange(0, pos - 1), "#"), "#", 2, 0, 42, true));

                    boxes.Add(BoxAroundText(MakeDubbelBoxes(rondleidingInformatie.GetRange(pos - 1, 2), "#")[0], "#", 2, 0, 42, true,
                        new List<string> { "".PadRight(19) + "##  [1] Reserveren     ", "##".PadLeft(21) + "".PadRight(21) }));

                    boxes.AddRange(BoxAroundText(MakeDubbelBoxes(rondleidingInformatie.GetRange(pos + 1, rondleidingInformatie.Count - (pos + 1)), "#"), "#", 2, 0, 42, true));
                }
                Console.Clear();
                for (int i = 0; i < boxes.Count; i++)
                {
                    Console.Write(boxes[i]);
                
                }

                Console.WriteLine(new string('#', 48));
                Console.WriteLine("Gebruik de pijltoesten om te navigeren.");
                Console.WriteLine("Druk op [2] om je reservering te annuleren.");
                Console.WriteLine("Druk op [4] om naar het afdelingshoofdscherm te gaan.");
                Console.WriteLine("Druk op [9] voor developper scherm.");
                ConsoleKeyInfo key = Console.ReadKey(false);

                if (IsKeyPressed(key, UP_ARROW))
                {
                    if (pos > 1)
                    {
                        pos -= 2;
                    }
                }

                else if (IsKeyPressed(key, DOWN_ARROW))
                {
                    if (pos < rondleidingInformatie.Count - 2)
                    {
                        pos += 2;
                    }
                }
                else if (IsKeyPressed(key, LEFT_ARROW))
                {
                    if (pos % 2 == 1)
                    {
                        pos -= 1;
                    }

                }
                else if (IsKeyPressed(key, RIGHT_ARROW))
                {
                    if (pos % 2 == 0)
                    {
                        pos += 1;
                    }
                }
                else if (IsKeyPressed(key, ESCAPE_KEY))
                {
                    cont = false;
                }
                else if (IsKeyPressed(key, "D9") || IsKeyPressed(key, "NUMPAD9"))
                {
                    return 1;
                }
                else if (IsKeyPressed(key, "D2") || IsKeyPressed(key, "NUMPAD2"))
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine(new string('_', 48));
                    Console.WriteLine("Voer hier uw unieke code in: ");
                    (string, int) answer = AskForInput(0);
                    if (answer.Item2 != -1)
                    {
                        return answer.Item2;
                    }
                    List<User> gebruikers = JsonManager.DeserializeGebruikers();

                    //Als de gebruiker is gevonden, returned hij het naar een nieuwe variabel
                    User targetedUser = gebruikers.FirstOrDefault(geb => geb.UniekeCode.Equals(answer.Item1));

                    if (targetedUser != null)
                    {
                        //Als de gebruiker geen reservering heeft, geef de volgende melding
                        if(targetedUser.Reservering == default)
                        {
                            Console.WriteLine();
                            Console.WriteLine("U heeft nog geen reservering geplaatst");
                            Thread.Sleep(2000);
                            return 0;
                        }

                        //Zet de resveringsdatum naar default om te legen / resetten
                        targetedUser.Reservering = default;

                        //Zoek naar de gebruiker in de gebruikers lijst
                        int index = gebruikers.FindIndex(geb => geb.UniekeCode == targetedUser.UniekeCode);

                        //Overschrijf de gebruiker in de lijst
                        gebruikers[index] = targetedUser;

                        JsonManager.SerializeGebruikers(gebruikers);

                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine("Uw reservering is succesvol geannuleerd");
                        Thread.Sleep(2000);
                        return 0;
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("Deze user is niet bekend bij ons.");
                        Thread.Sleep(2000);
                        return 0;
                    }

                }
                else if (IsKeyPressed(key, "D1") || IsKeyPressed(key, "NUMPAD1"))
                {
                    Console.WriteLine();
                    Console.WriteLine("Vul hier uw unieke code in: ");
                    (string, int) answer = AskForInput(0);
                    if (answer.Item2 != -1)
                    {
                        return answer.Item2;
                    }
                    List<User> gebruikers = JsonManager.DeserializeGebruikers();
                    List<string> uniekeCodes = gebruikers.Select(geb => geb.UniekeCode).ToList(); //?

                    for (int a = 0; a < gebruikers.Count; a++)
                    {
                        if (gebruikers[a].UniekeCode == answer.Item1 && gebruikers[a].Reservering != new DateTime(1, 1, 1))
                        {
                            Console.WriteLine();
                            Console.WriteLine("U heeft al een reservering geplaatst");
                            Thread.Sleep(2000);
                            return 0;
                        }
                        else if (gebruikers[a].UniekeCode == answer.Item1)
                        {
                            gebruikers[a].Reservering = tijden[pos];

                            JsonManager.SerializeGebruikers(gebruikers);

                            Thread.Sleep(2000);
                            return 0;
                        }
                    }

                    Console.WriteLine();
                    Console.WriteLine("Deze unieke code is bij ons niet bekent");
                    Thread.Sleep(2000);
                }
                else if (IsKeyPressed(key, "D4") || IsKeyPressed(key, "NUMPAD4"))
                { 
                    return 2;
                }

            } while (cont);
            return 0;

        }

        internal override List<Screen> Update(List<Screen> screens) {

            return screens;
        }
    }
    internal class AfdelingshoofdScherm : Screen {

        internal override int DoWork()
        {
            Console.WriteLine("AfdelingshoofdScherm");
            List<Rondleiding> rondleidinen = new List<Rondleiding>();
            List<List<string>> rondleidingInformatie = new List<List<string>>();
            DateTime time = new DateTime(2023, 1, 1, 11, 0, 0);
            for (int i = 0; i < 18; i++)
            {
                Rondleiding rondleiding = new()
                {
                    Datum = time,
                    Bezettingsgraad = 0
                };

                rondleidinen.Add(rondleiding);
                
                time = time.AddMinutes(20);
            }

            List<User> gebruikers = JsonManager.DeserializeGebruikers();

            foreach(Rondleiding rondleiding in rondleidinen)
            {
                rondleiding.CalculateBezettingsgraad(gebruikers);

                Console.WriteLine($"{rondleiding.Datum}, Bezettingsgraad: {rondleiding.Bezettingsgraad}%");
            }

            ConsoleKeyInfo key = Console.ReadKey(false);

            if (IsKeyPressed(key, "D9") || IsKeyPressed(key, "NUMPAD9"))
            {
                return 1;
            }
            else
            {
                return 1;
            }
        }

        internal override List<Screen> Update(List<Screen> screens)
        {

            return screens;
        }
    }

}


