using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_B_V2._0
{
    internal class Program
    {
        private static int currentScreen;
        private static int lastscreen;
        private static List<Screen> screens = new List<Screen>();

        static void Main(string[] args)
        {
            screens.Add(new StartScreen()); // 0
            screens.Add(new TestDataGeneratorScreen()); // 1
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
            Console.WriteLine($"Druk op 1 om naar het scherm te gaan om test data aan te maken.");
            Console.WriteLine("Druk op escape om af te sluiten");
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
            Console.WriteLine("Druk op 1 om unieke codes aan te maken.");
            Console.WriteLine("Druk op 2 om gebruikers aan te maken.");
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
                Console.WriteLine("De unieke codes zijn aangemaakt. Druk op escape om terug te gaan of druk op 2 om de gebruikers aan te maken");
            }
            else if (answer.Item1 == "2")
            {
                (List<User>, Exception) result = TestDataGenerator.MaakGebruikers(10);
                if (result.Item2.Message != "Exception of type 'System.Exception' was thrown.")
                {
                    Console.WriteLine($"Er is een error opgetreden: {result.Item2}");
                    Thread.Sleep(4000);
                    return 1;
                }
                Console.WriteLine("De gebruikers zijn aangemaakt. Druk op escape om terug te gaan of druk op 1 om de aangemaakte users te zien.");
                answer = AskForInput(0);
                if (answer.Item1 == "1")
                {
                    List<List<string>> gebruikers = new List<List<string>>();
                    foreach (var gebruiker in result.Item1)
                    {
                        List<string> gebruikerInfo = new List<string>
                        {
                            $"Unieke code: {gebruiker.UniekeCode}",
                            $"Reservering datum: {gebruiker.Reservering}"
                        };
                        gebruikers.Add(gebruikerInfo);
                    }

                    var boxes = BoxAroundText(gebruikers, "#", 2, 2, 80, false);
                    for (int a = 0; a < boxes.Count; a++)
                    {
                        Console.WriteLine(boxes[a]);
                    }
                    Console.ReadLine();
                }
                return 0;

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
}
