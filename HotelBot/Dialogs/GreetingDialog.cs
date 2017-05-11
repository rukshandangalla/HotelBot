using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Threading.Tasks;

namespace HotelBot.Dialogs
{
    [Serializable]
    public class GreetingDialog : IDialog
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Hello there, Rukshan's BOT here");
            await Respond(context);

            context.Wait(MessageRecievedAsync);
        }

        private static async Task Respond(IDialogContext context)
        {
            var userName = string.Empty;
            context.UserData.TryGetValue<string>("Name", out userName);

            if (string.IsNullOrEmpty(userName))
            {
                await context.PostAsync("What is your name?");
                context.UserData.SetValue<bool>("GetName", true);
            }
            else
            {
                await context.PostAsync(string.Format("Hi {0}, How can I help you today?", userName));
            }
        }

        private async Task MessageRecievedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            var userName = string.Empty;
            var getName = false;

            context.UserData.TryGetValue<string>("Name", out userName);
            context.UserData.TryGetValue<bool>("GetName", out getName);

            if (getName)
            {
                userName = message.Text;

                context.UserData.SetValue<string>("Name", userName);
                context.UserData.SetValue<bool>("GetName", getName);
            }

            await Respond(context);
            context.Done(message);
        }
    }
}