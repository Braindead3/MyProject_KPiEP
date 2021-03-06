using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using System.Timers;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;

namespace MyProject_KPiEP
{
    class Program
    {
        static string BotToken { get; set; } = "1713856634:AAEtV-XyEnqhmJAcNCOfLedlvxSjj5cftz8";
        static TelegramBotClient client;
        static bool addNoteName = false, addNoteText = false, viewNote = false, delNote = false,editNote=false,editNoteName=false,editNoteText=false,addNoteTime=false;
        static NotesEntity note= new NotesEntity();
        static List<NotesEntity> notes = new List<NotesEntity>();
        static string noteText="", noteName="";
        static int noteId=0;
        static System.Timers.Timer timer = new System.Timers.Timer(60000);

        static void Main(string[] args)
        {
            client = new TelegramBotClient(BotToken);

            notes = GetAllNotes();

            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Enabled = true;

            client.StartReceiving();
            Console.WriteLine("Пуск");
            client.OnMessage += Client_OnMessage;
            client.OnCallbackQuery += Client_OnCallbackQuery;

            Console.ReadKey();

            client.StopReceiving();
        }


        private static async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var delayedNote in notes.Where(delayedNote => delayedNote.Delay == 1))
            {
                DateTime date;
                date = DateTime.Parse(delayedNote.Time);
                if (date.Date >= DateTime.Now.Date)
                {
                    string noteStr = "Время оповещения:" + delayedNote.Time + "\nИмя заметки: " + delayedNote.NoteName + "\nТекст заметки:\n\n" + delayedNote.Text;
                    await client.SendTextMessageAsync(delayedNote.IdChat, noteStr);
                    using (var context = new DataContext())
                    {
                        var searchedNote = context.Notes.Find(delayedNote.Id);
                        searchedNote.Delay = 0;
                        context.SaveChanges();
                    }
                }
            }

            notes = GetAllNotes();
        }

        private static async void Client_OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            switch (e.CallbackQuery.Data)
            {
                case "editN":
                    await client.SendTextMessageAsync(chatId: e.CallbackQuery.Message.Chat.Id,"Введите новое имя заметки");
                    editNoteName = true;
                    break;

                case "editT":
                    await client.SendTextMessageAsync(chatId: e.CallbackQuery.Message.Chat.Id, "Введите новое содержание");
                    editNoteText = true;
                    break;

                case "Save":
                        using (var context = new DataContext())
                        {
                            var editedNote = context.Notes.Select(x => x).Where(x => x.Id == noteId).FirstOrDefault();
                            editedNote.Text = noteText;
                            editedNote.NoteName = noteName;
                            context.SaveChanges();
                            editNote = false;
                            notes = context.Notes.ToList();
                        }
                        await client.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Изменение прошло успешно!");
                    break;

                default:
                    break;
            }
            
        }

        private static async void Client_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var messege = e.Message;
            if (messege.Text!=null)
            {
                switch (messege.Text)
                {
                    case "/menu":
                        await client.SendTextMessageAsync(
                            messege.Chat.Id,
                            "Вот клавиатура.",
                            replyMarkup: GetButtons()
                            );
                        break;

                    case "Добавить заметку":
                        await client.SendTextMessageAsync(
                            messege.Chat.Id,
                            "Введите имя заметки"
                            );
                        addNoteName = true;
                        break;

                    case "Добавить заметку с напоминанием":
                        await client.SendTextMessageAsync(
                            messege.Chat.Id,
                            "Введите дату и время(dd.mm.yyyy hh:mm)"
                            );
                        addNoteTime = true;
                        break;

                    case "Список заметок":
                        string mes = "Список заметок:\n";
                        var selectedNotes = notes.Select(x => x).Where(x => x.IdChat == Convert.ToString(messege.Chat.Id));
                        foreach (var note in selectedNotes)
                        {
                            mes += note.NoteName + "\n";
                        }
                        await client.SendTextMessageAsync(messege.Chat.Id, mes, replyMarkup: GetButtons());
                        break;

                    case "Посмотреть заметку":
                        await client.SendTextMessageAsync(messege.Chat.Id, "Введите имя заметки:");
                        viewNote = true;
                        break;

                    case "Удалить заметку":
                        await client.SendTextMessageAsync(messege.Chat.Id, "Введите имя заметки:");
                        delNote = true;
                        break;

                    case "Редактировать заметку":
                        await client.SendTextMessageAsync(messege.Chat.Id, "Введите имя заметки:");
                        editNote = true;
                        break;

                    case "/start":
                        await client.SendTextMessageAsync(
                            messege.Chat.Id,
                            $"Привет, это твой верный помощник для запоминания всякой фигни, для начала работы введи /menu.\n" +
                            $"Список команд:\n" +
                            $"/menu - вывести клавиатуру для взаимодействия с ботом.\n" +
                            $"/help - список всех команд",
                            replyToMessageId: messege.MessageId
                            );
                        break;

                    case "/help":
                        await client.SendTextMessageAsync(
                            messege.Chat.Id,
                            $"Список команд:\n" +
                            $"/menu - вывести клавиатуру для взаимодействия с ботом.\n" +
                            $"/help - список всех команд",
                            replyToMessageId: messege.MessageId
                            );
                        break;

                    default:
                        if (addNoteName)
                        {
                            if (notes.Select(x=>x).Where(x=>x.NoteName==messege.Text).FirstOrDefault()!=null)
                            {
                                await client.SendTextMessageAsync(messege.Chat.Id, "Такая заметка уже существует, введи другое имя");
                            }
                            else
                            {
                                note.NoteName = messege.Text;
                                note.IdChat = Convert.ToString(messege.Chat.Id);

                                addNoteName = false;
                                await client.SendTextMessageAsync(messege.Chat.Id, "Введите текст заметки:");
                                addNoteText = true;
                            }
                        }
                        else if (addNoteText)
                        {
                            note.Text = messege.Text;
                            AddNoteToDB();
                            addNoteText = false;

                            await client.SendTextMessageAsync(messege.Chat.Id, "Заметка добавлена успешно!", replyMarkup: GetButtons());
                        }
                        else if (viewNote)
                        {
                            if (notes.Select(x => x).Where(x => x.IdChat == Convert.ToString(messege.Chat.Id)).Where(x => x.NoteName == messege.Text).FirstOrDefault() == null)
                            {
                                await client.SendTextMessageAsync(messege.Chat.Id, "Такой заметки нету", replyMarkup: GetButtons());
                            }
                            else
                            {
                                var searchedNote = notes.Select(x => x).Where(x => x.IdChat == Convert.ToString(messege.Chat.Id)).Where(x => x.NoteName == messege.Text).FirstOrDefault();
                                string noteStr = "Имя заметки: " + searchedNote.NoteName + "\nТекст заметки:\n\n" + searchedNote.Text;
                                await client.SendTextMessageAsync(messege.Chat.Id, noteStr, replyMarkup: GetButtons());
                                viewNote = false;
                            }
                        }
                        else if (delNote)
                        {
                            using (var context = new DataContext())
                            {
                                context.Notes.Remove(notes.Select(x => x).Where(x => x.IdChat == Convert.ToString(messege.Chat.Id)).Where(x => x.NoteName == messege.Text).FirstOrDefault());
                                context.SaveChanges();
                                notes = context.Notes.ToList();
                            }
                            string otv = "Удаление прошло успешно";
                            await client.SendTextMessageAsync(messege.Chat.Id, otv, replyMarkup: GetButtons());
                            delNote = false;
                        }
                        else if (editNote)
                        {
                            await client.SendTextMessageAsync(messege.Chat.Id, "Выберите, что изменить.", replyMarkup: GetInLineButtons());
                            using (var context = new DataContext())
                            {
                                var editedNote = context.Notes.Select(x => x).Where(x => x.NoteName == messege.Text).FirstOrDefault();
                                noteName = editedNote.NoteName;
                                noteText = editedNote.Text;
                                noteId = editedNote.Id;
                                editNote = false;
                            }
                        }
                        else if (editNoteName)
                        {
                            noteName = messege.Text;
                            editNoteName = false;
                            await client.SendTextMessageAsync(messege.Chat.Id, "Выберите опцию", replyMarkup: GetInLineButtons());
                        }
                        else if (editNoteText)
                        {
                            noteText = messege.Text;
                            editNoteText = false;
                            await client.SendTextMessageAsync(messege.Chat.Id, "Выберите опцию", replyMarkup: GetInLineButtons());
                        }
                        else if (addNoteTime)
                        {
                            DateTime date = new DateTime();
                            if (DateTime.TryParse(messege.Text, out date))
                            {
                                if (date <= DateTime.Now)
                                {
                                    await client.SendTextMessageAsync(messege.Chat.Id, "Введите другое время.(которое позже настоящего)");
                                }
                                else
                                {
                                    note.Delay = 1;
                                    note.Time = messege.Text;
                                    addNoteTime = false;
                                    await client.SendTextMessageAsync(messege.Chat.Id, "Введите имя заметки:");
                                    addNoteName = true;
                                }
                            }
                            else
                            {
                                await client.SendTextMessageAsync(messege.Chat.Id, "Это не дата, введите дату.");
                            }
                        }
                        else
                        {
                            await client.SendTextMessageAsync(
                                messege.Chat.Id,
                                $"И че мне с этим делать",
                                replyToMessageId: messege.MessageId
                                );
                        }
                        break;
                }
            }
        }

        private static List<NotesEntity> GetAllNotes()
        {
            List<NotesEntity> notes = new List<NotesEntity>();
            using (var context = new DataContext())
            {
                notes = context.Notes.ToList();
            }

            return notes;
        }

        private static void AddNoteToDB()
        {
            using (var dataSource = new DataContext())
            {
                dataSource.Notes.Add(note);
                dataSource.SaveChanges();
                notes = dataSource.Notes.ToList();
                note = new NotesEntity();
            }
        }

        private static IReplyMarkup GetInLineButtons()
        {
            List<List<InlineKeyboardButton>> buttons = new List<List<InlineKeyboardButton>>
            {
                new List<InlineKeyboardButton>{new InlineKeyboardButton { Text = "Имя заметки", CallbackData = "editN" },new InlineKeyboardButton { Text = "Содержание заметки", CallbackData = "editT" } },
                new List<InlineKeyboardButton>{new InlineKeyboardButton { Text = "Сохранить", CallbackData = "Save" } }
            };
            return new InlineKeyboardMarkup(buttons.ToArray());
        }

        private static IReplyMarkup GetButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton> { new KeyboardButton { Text = "Добавить заметку" }, new KeyboardButton { Text = "Удалить заметку" }, new KeyboardButton { Text = "Редактировать заметку" }},
                    new List<KeyboardButton> { new KeyboardButton { Text = "Список заметок" }, new KeyboardButton { Text = "Посмотреть заметку" }},
                    new List<KeyboardButton> { new KeyboardButton { Text = "Добавить заметку с напоминанием" } }
                },
                ResizeKeyboard = true
            };
        }
    }
}
