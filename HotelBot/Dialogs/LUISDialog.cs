using HotelBot.Models;
using HotelBot.Models.FacebookModels;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HotelBot.Dialogs
{
    [LuisModel("8e344133-57a4-40c2-ba32-4fb7bdce6f2a", "6c5075ae11ac40ddbfbc98fd4671bb7a")]
    [Serializable]
    public class LUISDialog : LuisDialog<RoomReservation>
    {
        private readonly BuildFormDelegate<RoomReservation> ReserveRoom;

        [field: NonSerialized()]
        protected Activity _message;

        public LUISDialog(BuildFormDelegate<RoomReservation> reserveRoom)
        {
            ReserveRoom = reserveRoom;
        }

        protected override async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            _message = (Activity)await item;
            await base.MessageReceived(context, item);
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I'm sorry I don't know what you mean.");
            context.Wait(MessageReceived);
        }

        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            context.Call(new GreetingDialog(), CallBack);
        }

        [LuisIntent("Reservation")]
        public async Task Reservation(IDialogContext context, LuisResult result)
        {
            var enrollmentForm = new FormDialog<RoomReservation>(new RoomReservation(), ReserveRoom, FormOptions.PromptInStart);
            context.Call<RoomReservation>(enrollmentForm, CallBack);
        }

        [LuisIntent("QueryAmeniti")]
        public async Task QueryAmeniti(IDialogContext context, LuisResult result)
        {
            foreach (var entity in result.Entities.Where(Entity => Entity.Type == "Amenity"))
            {
                var value = entity.Entity.ToLower();

                if (value == "pool" || value == "gym" || value == "wifi" || value == "towels")
                {
                    //await context.PostAsync("Yes we have that!");
                    Activity replyMessage = _message.CreateReply();

                    var facebookMessage = new FacebookMessage();
                    facebookMessage.attachment = new FacebookAttachment();
                    facebookMessage.attachment.type = "template";
                    facebookMessage.attachment.payload = new FacebookPayload();
                    facebookMessage.attachment.payload.template_type = "generic";

                    var amenity = new FacebookElement();
                    amenity.subtitle = "Yes, we have that!";
                    amenity.title = value;

                    switch (value)
                    {
                        case "pool":
                            amenity.image_url = "http://www.girltweetsworld.com/wp-content/uploads/2012/02/P1000180.jpg";
                            break;
                        case "gym":
                            amenity.image_url = "https://s-media-cache-ak0.pinimg.com/originals/cb/c9/4a/cbc94af79da9e334a8555e850da136f4.jpg";
                            break;
                        case "wifi":
                            amenity.image_url = "http://media.idownloadblog.com/wp-content/uploads/2016/02/wifi-icon.png";
                            break;
                        case "towels":
                            amenity.image_url = "http://www.prabhutextile.com/images/bath_towel_1.jpg";
                            break;
                        default:
                            break;
                    }

                    facebookMessage.attachment.payload.elements = new FacebookElement[] { amenity };
                    replyMessage.ChannelData = facebookMessage;

                    await context.PostAsync(replyMessage);
                    context.Wait(MessageReceived);
                    return;
                }
                else
                {
                    await context.PostAsync("I'm sorry, we don't have that.");
                    context.Wait(MessageReceived);

                    return;
                }
            }

            await context.PostAsync("I'm sorry, we don't have that.");
            context.Wait(MessageReceived);

            return;
        }

        private async Task CallBack(IDialogContext context, IAwaitable<object> result)
        {
            context.Wait(MessageReceived);
        }
    }
}