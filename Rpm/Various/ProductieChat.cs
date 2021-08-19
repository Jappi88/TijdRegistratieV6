﻿using Rpm.Misc;
using Rpm.Productie;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using ProductieManager.Rpm.Misc;

namespace ProductieManager.Rpm.Various
{
    public class ProductieChat
    {
        public static string ChatPath;
        public static string ProfielPath;
        public static string BerichtenPath;
        public static string GroupChatImagePath { get; set; }

        public static string GebruikerPath { get; set; }

        private FileSystemWatcher _berichtenWatcher;
        private FileSystemWatcher _gebruikerWatcher;
        public static bool LoggedIn { get; private set; }
        public static UserChat Chat { get; private set; }

        public static bool RaiseNewMessageEvent { get; set; } = true;
        public static bool RaiseUserUpdateEvent { get; set; } = true;

        public static List<UserChat> Gebruikers { get; private set; }

        public ProductieChat()
        {

        }

        public bool Login()
        {
            try
            {
                if (Manager.LogedInGebruiker == null) return false;
                if (LoggedIn) return true;
                ChatPath = Manager.DbPath + "\\Chat";
                if (!Directory.Exists(ChatPath))
                    Directory.CreateDirectory(ChatPath);
                ProfielPath = Manager.DbPath + $"\\Chat\\{Manager.LogedInGebruiker.Username}";
                BerichtenPath = ChatPath + $"\\{Manager.LogedInGebruiker.Username}\\Berichten";
                GroupChatImagePath = ChatPath + "\\GroupChatImage.png";
                if (!File.Exists(GroupChatImagePath) || !GroupChatImagePath.IsImageFile())
                {
                    try
                    {
                        var data = Properties.Resources.users_12820.ToByteArray();
                        File.WriteAllBytes(GroupChatImagePath, data);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                  
                }
                if (!Directory.Exists(ProfielPath))
                    Directory.CreateDirectory(ProfielPath);
                if (!Directory.Exists(BerichtenPath))
                    Directory.CreateDirectory(BerichtenPath);
                GebruikerPath = ChatPath + $"\\{Manager.LogedInGebruiker.Username}.rpm";

                try
                {
                    if (File.Exists(GebruikerPath))
                        Chat = File.ReadAllBytes(GebruikerPath).DeSerialize<UserChat>() ?? new UserChat();
                    else Chat = new UserChat();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
               
                Chat.UserName = Manager.LogedInGebruiker.Username;
                Chat.IsOnline = true;
                Chat.LastOnline = DateTime.Now;
                if (string.IsNullOrEmpty(Chat.ProfielImage) || !File.Exists(Chat.ProfielImage))
                {
                    Chat.ProfielImage = $"{ProfielPath}\\ProfielFoto.png";
                    Properties.Resources.avatardefault_92824.Save(Chat.ProfielImage, ImageFormat.Png);
                }
                Chat.Save();
                UpdateGebruikers();
                _berichtenWatcher = new FileSystemWatcher(BerichtenPath);
                _berichtenWatcher.EnableRaisingEvents = true;
                _berichtenWatcher.Changed += _berichtenWatcher_Changed;

                _gebruikerWatcher = new FileSystemWatcher(ChatPath);
                _gebruikerWatcher.EnableRaisingEvents = true;
                _gebruikerWatcher.NotifyFilter = NotifyFilters.LastWrite;
                _gebruikerWatcher.Changed += _gebruikerWatcher_Changed;
                LoggedIn = true;
                OnGebruikerUpdate(Chat);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public bool LogOut()
        {
            try
            {
                if (!LoggedIn) return true;

                if (Chat != null)
                {
                    Chat.IsOnline = false;
                    Chat.LastOnline = DateTime.Now;
                    Chat.Save();
                    Chat = null;
                }

                Gebruikers?.Clear();
                _berichtenWatcher?.Dispose();
                _berichtenWatcher = null;
                _gebruikerWatcher?.Dispose();
                _gebruikerWatcher = null;
                LoggedIn = false;
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static bool SendMessage(string message, string destination)
        {
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(destination) || Chat == null) return false;
            string[] users = destination.Split(';');
            bool xreturn = false;
            foreach (var user in users)
            {
                var ent = new ProductieChatEntry()
                {
                    Afzender = Chat,
                    Bericht = message,
                    Ontvanger = destination
                };
                string path = ChatPath + $"\\{user}\\Berichten\\{ent.ID}.rpm";
                xreturn |= ent.Serialize(path);
            }

            return xreturn;
        }

        public static bool ChangeProfielImage(string img)
        {
            if (Chat == null || string.IsNullOrEmpty(img)) return false;
            try
            {
                
                Chat.ProfielImage = ProfielPath + "\\ProfielFoto.png";
                File.Copy(img, Chat.ProfielImage, true);
                return Chat.Save();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        private bool UpdateGebruikers()
        {
            try
            {
                Gebruikers = new List<UserChat>();
                if (Chat == null || !Directory.Exists(ChatPath)) return false;
                string[] files = Directory.GetFiles(ChatPath, "*.rpm", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    var ent = file.DeSerialize<UserChat>();
                    if (ent == null || string.Equals(Chat.UserName, ent.UserName,
                        StringComparison.CurrentCultureIgnoreCase)) continue;
                    Gebruikers.Add(ent);
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        private static readonly object _locker = new object();
        private void _berichtenWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            lock(_locker)
            {
                try
                {
                    if (Chat == null || !LoggedIn || !RaiseNewMessageEvent) return;
                    var ent = e.FullPath.DeSerialize<ProductieChatEntry>();
                    if (ent == null) return;
                    OnMessageRecieved(ent);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }

        private void _gebruikerWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            lock (_locker)
            {
                try
                {
                    if (Chat == null || !LoggedIn || !RaiseUserUpdateEvent) return;
                    var ent = e.FullPath.DeSerialize<UserChat>();
                    if (ent == null) return;
                    UpdateGebruikers();
                    OnGebruikerUpdate(ent);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
          
        }

        public static List<ProductieChatEntry> GetConversation(UserChat user, bool privatemessages)
        {
            var xreturn = new List<ProductieChatEntry>();
            try
            {
                if (user == null) return xreturn;
                var usermessages = user.GetMessagesFromAfzender(privatemessages ? Chat.UserName: null);
                var mymessages = Chat.GetMessagesFromAfzender(privatemessages ? user.UserName : null);
                if(mymessages.Count > 0)
                    usermessages.AddRange(mymessages);
                xreturn = usermessages.OrderBy(x => x.Tijd).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return xreturn;
        }

        public delegate void NewMessageHandler(ProductieChatEntry message);

        public static event NewMessageHandler MessageRecieved;

        protected virtual void OnMessageRecieved(ProductieChatEntry message)
        {
            MessageRecieved?.Invoke(message);
        }

        public delegate void GebruikerUpdateHandler(UserChat user);

        public static event GebruikerUpdateHandler GebruikerUpdate;

        protected virtual void OnGebruikerUpdate(UserChat user)
        {
            GebruikerUpdate?.Invoke(user);
        }
    }
}
