using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MyProject_KPiEP
{
    class Program
    {
        static string BotToken { get; set; } = "1713856634:AAEtV-XyEnqhmJAcNCOfLedlvxSjj5cftz8";
        static TelegramBotClient client;
        static bool addNote = false;
        static NotesEntity note= new NotesEntity();
        static void Main(string[] args)
        {
            client = new TelegramBotClient(BotToken);

            client.StartReceiving();

            client.OnMessage += Client_OnMessage;
            client.OnCallbackQuery += Client_OnCallbackQuery;

            Console.ReadKey();

            client.StopReceiving();


            //using (var dataSource = new DataContext())
            //{
            //    dataSource.Notes.Add(new NotesEntity { Text="vlad1" });
            //    dataSource.Notes.Add(new NotesEntity { Text="vlad2" });
            //    dataSource.SaveChanges();

            //    var notes = dataSource.Notes.ToList();

            //    foreach (var note in notes)
            //    {
            //        Console.WriteLine(note.Text);
            //    }
            //}
        }

        private static async void Client_OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            switch (e.CallbackQuery.Data)
            {
                case "1":
                    await client.SendTextMessageAsync(chatId: e.CallbackQuery.Message.Chat.Id,"otvet",replyMarkup: GetButtons());
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
                    //case "sticker":
                    //    await client.SendStickerAsync(
                    //        chatId: messege.Chat.Id,
                    //        sticker: "https://tlgrm.ru/_/stickers/ff6/4b6/ff64b611-aa7c-3603-b73c-7cd86d4b71dc/10.webp",
                    //        replyToMessageId: messege.MessageId,
                    //        replyMarkup: GetButtons() 
                    //        );// отправка стикера на сообщение пользователя, ссылка это стикер(скопируй ссылку на стикер с сайта)

                    //    break;
                    //case "image":
                    //    await client.SendPhotoAsync
                    //        (
                    //        chatId: messege.Chat.Id,
                    //        photo: "https://www.google.com/imgres?imgurl=https%3A%2F%2Fcdn.eso.org%2Fimages%2Fthumb300y%2Feso1907a.jpg&imgrefurl=https%3A%2F%2Fwww.eso.org%2Fpublic%2Fimages%2Farchive%2Fzoomable%2F&tbnid=nqk2hAE034Z3nM&vet=12ahUKEwiW3tODvo3wAhUZhaQKHencAtoQMyggegUIARD5AQ..i&docid=KeUcu23CqZzwAM&w=515&h=300&q=image&ved=2ahUKEwiW3tODvo3wAhUZhaQKHencAtoQMyggegUIARD5AQ",
                    //        replyMarkup: GetButtons()
                    //        );
                    //    break;
                    case "menu":
                        await client.SendTextMessageAsync(
                            messege.Chat.Id,
                            messege.Text,
                            replyMarkup : GetButtons()
                            );
                        break;
                    case "Добавить заметку":
                        {
                            await client.SendTextMessageAsync(
                                messege.Chat.Id,
                                "Введите имя заметки"
                                );
                            addNote = true;
                            break;
                        }
                    case "test":
                        await client.SendTextMessageAsync(
                            messege.Chat.Id,
                            $"InLineButton",replyMarkup: GetInLineButton(1)
                            );
                        break;
                    default:
                        if (addNote != false)
                        {
                            note.NoteName = messege.Text;
                            note.IdChat =Convert.ToInt32(messege.Chat.Id);
                        }
                        else
                        {
                            await client.SendTextMessageAsync(
                                messege.Chat.Id,
                                $"И че мне с этим делать",
                                replyToMessageId: messege.MessageId
                                );//отвечает на сообщение пользователя test
                        }
                        break;
                }
            }
        }

        private static IReplyMarkup GetInLineButton(int id)
        {
            return new InlineKeyboardMarkup(new InlineKeyboardButton { Text = "vlad", CallbackData=id.ToString() }); ;
        }

        private static IReplyMarkup GetButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton> { new KeyboardButton { Text = "Добавить заметку" }, new KeyboardButton { Text = "Удалить заметку" }, new KeyboardButton {Text = "Редактировать заметку" } }
                }
            };
        }
    }
}
