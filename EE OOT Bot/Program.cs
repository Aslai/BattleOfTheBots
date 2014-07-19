using System;
using System.Collections.Generic;
using System.Text;
using PlayerIOClient;
using System.IO;
using System.Threading;
using Skylight;

namespace KasBot
{
    class Program
    {
        static string read(String fname)
        {
            String line;
            try
            {
                using (StreamReader sr = new StreamReader(fname))
                {
                    line = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                line = "";
            }
            return line;
        }
        private class PlayerStorage
        {
            public int BlockXPrev;
            public int BlockYPrev;
            public Dictionary<int, int> HotspotTrack = new Dictionary<int, int>();
            public int Coins = 0;
            public bool HasSword = false;
            public bool HasShield = false;
            public bool SeenMido = false;
            public bool MidoApproved = false;
            public bool ShopOpen = false;
            public bool MetLissan = false;
            public bool LissanRidiculed = false;
            public int EarningMoney = 0;
            public bool FastFall = false;
            public PlayerStorage()
            {
                BlockXPrev = BlockYPrev = 0;
            }
        }
        class Hotspot
        {
            public int x1, y1, x2, y2;
            public String trigger;
            public delegate void action(Player p);
            public action callback;
            public Hotspot(int X1, int Y1, int X2, int Y2, String Trigger, action Callback)
            {
                x1 = X1; x2 = X2;
                y1 = Y1; y2 = Y2;
                trigger = Trigger.ToLower();
                callback = Callback;
            }
        }
        static Room room;
        static Bot bot;
        static Bot ShopOwner;
        static Bot Lissan;
        static Bot MOM;
        static Bot Karisi;
        static Bot Midokun;

        static Dictionary<int, PlayerStorage> PlayerStores = new Dictionary<int, PlayerStorage>();
        static List<Hotspot> Hotspots = new List<Hotspot>();
        static bool Loaded = false;
        static public Bot AddBot(String email, String password)
        {
            Bot b = new Bot(room, email, password);
            b.ShouldTick = false;
            b.LogIn();
            b.Join();
            while (!b.IsConnected)
            {
                System.Threading.Thread.Sleep(100);
            }
            b.BlockDelay = 0;

            return b;
        }
        static PlayerStorage GetStore(int ID)
        {
            try
            {
                return PlayerStores[ID];
            }
            catch
            {
                PlayerStores[ID] = new PlayerStorage();
                try
                {
                    return PlayerStores[ID];
                }
                catch
                {
                    return null;
                }
            }
        }

        static void Init()
        {
            ShopOwner = AddBot("a7497994@drdrb.net", "asddsa");
            Lissan = AddBot("lissan@invalid.com", "asddsa");
            MOM = AddBot("mom@invalid.com", "asddsa");
            Karisi = AddBot("karisi@invalid.com", "asddsa");
            Midokun = AddBot("mido@invalid.com", "asddsa");

            Thread.Sleep(5000);

            ShopOwner.Push.Jump(252 * 16, 158 * 16);
            Lissan.Push.Jump(193 * 16, 151 * 16);
            MOM.Push.Jump(361 * 16, 132 * 16);
            Midokun.Push.Jump(126 * 16, 170 * 16);
            Karisi.Push.Jump(154 * 16, 168 * 16);

            Hotspots.Add(new Hotspot(0, 167, 10000, 10000, "Once", (Player player) =>
            {
                if (player.Name != "karisi" && player.Name != "midokun")
                {
                    Karisi.Push.Jump(154 * 16, 168 * 16);
                    Karisi.Push.Say("The grand tree needs your help!");
                    Karisi.Push.Say("Go left and find out what's wrong with him.");
                }
            }));

            Hotspots.Add(new Hotspot(125, 160, 135, 175, "OnEnter", (Player player) =>
            {
                PlayerStorage store = GetStore(player.Id);
                if (store == null)
                    return;
                if (player.Name != "midokun")
                {
                    if (store.MidoApproved == false)
                    {
                        if (store.SeenMido == false)
                        {
                            Midokun.Push.Say("Who goes there?");
                            Midokun.Push.Say("Ah, it's " + player.Name + "!");
                            Midokun.Push.Say("There's no way I'm going to let a twerp like you through.");
                            Midokun.Push.Say("You don't even have a shield!");
                            store.SeenMido = true;
                        }
                        else
                        {
                            if (store.HasShield == false)
                            {
                                Midokun.Push.Say("I said to get lost!");
                            }
                            else
                            {
                                Midokun.Push.Say("Get los- Wait...");
                                Midokun.Push.Say("Is that a shield you have?");
                                if (store.HasSword == false)
                                {
                                    Midokun.Push.Say("It doesn't even matter though.");
                                    Midokun.Push.Say("You don't even have a sword!");
                                }
                                else
                                {
                                    Midokun.Push.Say("And is that a sword you have, too?");
                                    Midokun.Push.Say("Fine, if you really want to kill yourself, go ahead.");
                                    store.MidoApproved = true;
                                }
                            }
                        }
                    }
                }
            }));

            Hotspots.Add(new Hotspot(125, 167, 127, 177, "OnEnter", (Player player) =>
            {
                PlayerStorage store = GetStore(player.Id);
                if (store == null)
                    return;
                if (store.MidoApproved)
                {
                    bot.Push.Teleport(120, 170, player);
                }
            }));

            Hotspots.Add(new Hotspot(121, 167, 124, 177, "OnEnter", (Player player) =>
            {
                PlayerStorage store = GetStore(player.Id);
                if (store == null)
                    return;
                if (store.MidoApproved)
                {
                    bot.Push.Teleport(129, 170, player);
                }
            }));

            Hotspots.Add(new Hotspot(245, 155, 247, 159, "OnEnter", (Player player) =>
            {
                PlayerStorage store = GetStore(player.Id);
                if (store == null)
                    return;
                if (store.ShopOpen == false)
                {
                    store.ShopOpen = true;
                    ShopOwner.Push.Say("Welcome to my shop!");
                    ShopOwner.Push.Say("Would you like to buy a shield for 50 coins?");
                }
            }));

            Hotspots.Add(new Hotspot(0, 160, 4000, 1000, "OnEnter", (Player player) =>
            {
                PlayerStorage store = GetStore(player.Id);
                if (store == null)
                    return;
                if (store.ShopOpen == true)
                {
                    store.ShopOpen = false;
                    ShopOwner.Push.Say("Do come again.");
                }
            }));

            Hotspots.Add(new Hotspot(192, 140, 199, 152, "OnEnter", (Player player) =>
            {
                PlayerStorage store = GetStore(player.Id);
                if (store == null)
                    return;
                if (store.MetLissan == false)
                {
                    store.MetLissan = true;
                    Lissan.Push.Say("It's a fine day, isn't it?");
                    store.Coins += 25;
                    Lissan.Push.Say("Here's 25 coins for climbing up here! (" + store.Coins + " total)");
                }
                else
                {
                    Lissan.Push.Say("You're always welcome up here.");
                }
            }));

            Hotspots.Add(new Hotspot(199, 169, 233, 300, "OnEnter", (Player player) =>
            {
                PlayerStorage store = GetStore(player.Id);

                Lissan.Push.Say("Tee-hee! Try to avoid getting wet!");
                if (store == null)
                    return;
                store.EarningMoney = 0;
            }));

            Hotspots.Add(new Hotspot(234, 100, 300, 300, "OnEnter", (Player player) =>
            {
                PlayerStorage store = GetStore(player.Id);
                if (store == null)
                    return;
                if (store.EarningMoney == 1)
                {
                    Lissan.Push.Say("What an excellent display of agility!");
                    store.Coins += 5;
                    Lissan.Push.Say("Have 5 coins for your effort! (" + store.Coins + " total)");
                }
                store.EarningMoney = 2;
            }));

            Hotspots.Add(new Hotspot(190, 100, 198, 300, "OnEnter", (Player player) =>
            {
                PlayerStorage store = GetStore(player.Id);
                if (store == null)
                    return;
                if (store.EarningMoney == 2)
                {
                    Lissan.Push.Say("What an excellent display of agility!");
                    store.Coins += 5;
                    Lissan.Push.Say("Have 5 coins for your effort! (" + store.Coins + " total)");
                }
                store.EarningMoney = 1;
            }));

            Hotspots.Add(new Hotspot(251, 130, 400, 136, "OnEnter", (Player player) =>
            {
                PlayerStorage store = GetStore(player.Id);
                if (store == null)
                    return;
                if (store.HasSword == false)
                {
                    MOM.Push.Say("You there.");
                    MOM.Push.Say("Yes... You!");
                    MOM.Push.Say("Take my sword... Make a dying man proud!");
                    store.HasSword = true;
                }
                else
                {
                    MOM.Push.Say("I'm dead!");
                    MOM.Push.Say("I swear it on my own grave!");
                }
            }));

            Hotspots.Add(new Hotspot(61, 161, 66, 168, "OnEnter", (Player player) =>
            {
                PlayerStorage store = GetStore(player.Id);
                if (store == null)
                    return;
                store.FastFall = false;
            }));

            Hotspots.Add(new Hotspot(0, 0, 78, 151, "OnEnter", (Player player) =>
            {
                PlayerStorage store = GetStore(player.Id);
                if (store == null)
                    return;
                store.FastFall = true;
            }));

            Hotspots.Add(new Hotspot(42, 164, 49, 167, "OnEnter", (Player player) =>
            {
                PlayerStorage store = GetStore(player.Id);
                if (store == null)
                    return;
                if (store.FastFall == true)
                {
                    store.FastFall = false;
                    bot.Push.Teleport(player.BlockX + 1, 169, player);
                }
            }));

            Hotspots.Add(new Hotspot(42, 168, 49, 169, "OnEnter", (Player player) =>
            {
                PlayerStorage store = GetStore(player.Id);
                if (store == null)
                    return;
                store.FastFall = false;
                bot.Push.Teleport(player.BlockX + 1, 166, player);
            }));

            Hotspots.Add(new Hotspot(30, 150, 42, 166, "OnEnter", (Player player) =>
            {
                PlayerStorage store = GetStore(player.Id);
                if (store == null)
                    return;
                store.FastFall = false;
            }));

            Hotspots.Add(new Hotspot(49, 150, 42, 166, "OnEnter", (Player player) =>
            {
                PlayerStorage store = GetStore(player.Id);
                if (store == null)
                    return;
                store.FastFall = false;
            }));

        }

        static void Main(string[] args)
        {
            String login = read("login.txt");


            String[] a = new String[2];
            a[0] = "\r\n";
            a[1] = "\n";

            String[] logininfo = login.Split(a, StringSplitOptions.None);

            room = new Room(logininfo[3]);
            if (logininfo[0].ToLower()[0] == 't')
            {
                bot = new Bot(room, logininfo[1], logininfo[2]);
            }
            else
            {
                bot = new Bot(room, logininfo[1], "", Bot.AccountType.Facebook);
            }

            Tools.ProgramMessage += HandleProgram;
            room.Pull.AddEvent += OnJoin;
            room.Pull.LeaveEvent += OnLeave;
            room.Pull.TickEvent += OnTick;
            room.Pull.NormalChatEvent += OnChat;
            room.Pull.WootEvent += OnWoot;

            bot.LogIn();
            bot.Join();
            Init();
            Loaded = true;

            Console.Read();
        }
        static void HandleProgram(string s)
        {
            Console.WriteLine(s);
        }

        static void OnJoin(PlayerEventArgs args)
        {
            PlayerStorage store = new PlayerStorage();
            store.BlockXPrev = args.Subject.BlockX;
            store.BlockYPrev = args.Subject.BlockY;
            PlayerStores[args.Subject.Id] = store;

            if (!Loaded)
                return;
            {
                Thread.Sleep(1000);

                Karisi.Push.Jump(154 * 16, 168 * 16);

                Karisi.Push.Say(args.Subject.Name + "! " + args.Subject.Name + "!");
                Thread.Sleep(200);
                Karisi.Push.Jump(154 * 16, 168 * 16);

                Karisi.Push.Say("Wake up, " + args.Subject.Name + "!");
            }
        }
        static void OnLeave(PlayerEventArgs args)
        {
            if (!Loaded)
                return;
            PlayerStores[args.Subject.Id] = null;
        }
        static void OnMessage(Player speaker, String message)
        {
            message += "      ";
            PlayerStorage store = GetStore(speaker.Id);
            if (store == null)
                return;
            if (store.ShopOpen == true)
            {
                if (message.Substring(0, 3).ToLower() == "yes")
                {
                    if (store.Coins < 50)
                    {
                        ShopOwner.Push.Say("Oh dear, you haven't enough coins.");
                        ShopOwner.Push.Say("You only have " + store.Coins + " right now.");
                    }
                    else
                    {
                        ShopOwner.Push.Say("Well here you go, then!");
                        store.HasShield = true;
                        store.Coins -= 50;
                        ShopOwner.Push.Say("Would you like to buy another?");
                    }
                }
                else if (message.Substring(0, 2).ToLower() == "no")
                {
                    ShopOwner.Push.Say("Okay then.");
                }
                else
                {
                    ShopOwner.Push.Say("It's a simple yes or no question, mate.");
                }
            }
        }
        static void OnCommand(Player speaker, String command, String message)
        {
            switch (command.ToLower())
            {
                case "getposition":
                    bot.Push.Say("X: " + speaker.BlockX + "   Y:" + speaker.BlockY);
                    break;
                default: break;
            }
        }

        static void OnChat(ChatEventArgs args)
        {
            OnMessage(args.Speaker, args.Message);

            if (args.Message[0] == '!')
            {
                String[] messagearray = args.Message.Split(' ');
                String command = messagearray[0].Substring(1).ToLower();
                String argument = "";
                if (command.Length + 2 < args.Message.Length)
                    argument = args.Message.Substring(command.Length + 2);
                OnCommand(args.Speaker, command, argument);
            }

        }
        static void OnWoot(PlayerEventArgs args)
        {
            bot.Push.Say("Thanks, mate!");
        }
        static void OnTick(PlayerEventArgs args)
        {
            PlayerStorage store = GetStore(args.Subject.Id);
            if (store == null)
                return;
            if (Karisi == null || Midokun == null || MOM == null || Lissan == null || ShopOwner == null)
                return;
            if (args.Subject.Id == Karisi.Id || args.Subject.Id == Midokun.Id || args.Subject.Id == MOM.Id ||
                args.Subject.Id == Lissan.Id || args.Subject.Id == ShopOwner.Id)
                return;

            for (int i = 0; i < Hotspots.Count; ++i)
            {
                Hotspot hotspot = Hotspots[i];

                if (args.Subject.BlockX >= hotspot.x1 && args.Subject.BlockX < hotspot.x2 &&
                        args.Subject.BlockY < hotspot.y2 && args.Subject.BlockY >= hotspot.y1)
                {
                    int v = 0;
                    try { v = store.HotspotTrack[i]; }
                    catch (Exception) { }

                    if ((hotspot.trigger == "once" && v == 0) ||
                        (hotspot.trigger == "onenter" && v == 0) ||
                         hotspot.trigger == "continuous")
                    {
                        if (hotspot.trigger == "onenter")
                            store.HotspotTrack[i] = 1;
                        if (hotspot.trigger == "once")
                            store.HotspotTrack[i] = 2;
                        hotspot.callback(args.Subject);

                    }

                }
                else
                {
                    int v = 0;
                    try
                    {
                        v = store.HotspotTrack[i];

                        if (v == 1)
                            store.HotspotTrack[i] = 0;
                    }
                    catch (Exception) { }
                }
            }
            store.BlockXPrev = args.Subject.BlockX;
            store.BlockYPrev = args.Subject.BlockY;

        }
    }
}
