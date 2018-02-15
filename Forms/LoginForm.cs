using Common;
using Common.Contracts;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static LuisBot.Constants;

namespace LuisBot.Forms
{
    [Serializable]
    public class LoginForm
    {

       

        [Prompt("May I know your user name?")]
            public string Name; // type: String

            //[Optional]
            //[Template(TemplateUsage.EnumSelectOne, "Please select gender, if you want else you can skip. {||}", ChoiceStyle = ChoiceStyleOptions.Buttons)]
            //public GenderOpts? Gender; // type: Enumeration

            //[Prompt("What will be your arrival time?")]
            //public DateTime ArrivalTime; // type: DateTime

            //[Numeric(1, 6)]
            //[Prompt("How many guests are accompanying you?<br>If more than 3, you will get complementory drink ! :)")]
            //public Int16 TotalAttendees; // type: Integral

            //[Prompt("What is Your Passowrd?")]
            //public string Password; // type: List of enumerations

            //[Template(TemplateUsage.EnumSelectOne, "Which complementory drink you would like to have? {||}", ChoiceStyle = ChoiceStyleOptions.Carousel)]
            //public ComplementoryDrinkOpts? ComplementoryDrink; // type: Enumeration

            public static IForm<LoginForm> BuildForm()
            {
            //OnCompletionAsyncDelegate<LoginForm> processOrder = async (context, state) =>
            //{
            //    await context.PostAsync("We are currently processing your sandwich. We will message you the status.");
            //};

            
            return new FormBuilder<LoginForm>()
                        .Message("Hey, Welcome to the Chat Pot!")

                        .Field(nameof(Name))

                          //  .Field(nameof(Gender))
                          //  .Field(nameof(ArrivalTime))
                          //   .Field(nameof(TotalAttendees))
                          //     .Field(nameof(Password))
                          //  .Field(new FieldReflector<LoginForm>(nameof(ComplementoryDrink))
                          //.SetType(null)
                          //.SetActive(state => state.TotalAttendees > 3)
                         
                          
                        .Confirm(async (state) =>
                        {

                            return new PromptAttribute("Hi {Name}, Do you want to continue? {||}");

                           

                        })
                        .Build();
            }
        

    }
}