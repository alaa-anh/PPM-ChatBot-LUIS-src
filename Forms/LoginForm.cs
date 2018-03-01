using Common;
using Common.Contracts;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace LuisBot.Forms
{
    [Serializable]
    public class LoginForm
    {

       

        //[Prompt("May I know your user name?")]
        //    public string Name; // type: String

        [Pattern(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$")]
        [Prompt("May I know your user name?")]
        public string Name;

        
        //[Template(TemplateUsage.EnumSelectOne, "Please select gender, if you want else you can skip. {||}", ChoiceStyle = ChoiceStyleOptions.Buttons)]
        //public GenderOpts? Gender; // type: Enumeration

        //[Prompt("What will be your arrival time?")]
        //public DateTime ArrivalTime; // type: DateTime

        //[Numeric(1, 6)]
        //[Prompt("How many guests are accompanying you?<br>If more than 3, you will get complementory drink ! :)")]
        //public Int16 TotalAttendees; // type: Integral

        [Prompt("May I know your Passowrd?")]
        public string Password; // type: List of enumerations

        //[Template(TemplateUsage.EnumSelectOne, "Which complementory drink you would like to have? {||}", ChoiceStyle = ChoiceStyleOptions.Carousel)]
        //public ComplementoryDrinkOpts? ComplementoryDrink; // type: Enumeration

        public static IForm<LoginForm> BuildForm()
            {
           


            return new FormBuilder<LoginForm>()
                       // .Message("Hey, Welcome to the Chat Pot!")

                        .Field(nameof(Name))
                             .Field(nameof(Password))


            //.Confirm(async (state) =>
            //{

            //    return new PromptAttribute("Hi {Name}, Do you want to continue? {||}");



            //})
            //.OnCompletion(async (context, state) =>
            //             {
            //                 //await context.PostAsync("You are currently Logged In. Please Enjoy Using our App.");
            //                 //context.UserData.SetValue("UserName", state.Name);
            //                 //context.UserData.SetValue("Password", state.Password);

            //                 //new Mongo().Insert("ContextTokens", new Token((string)response));
            //             })
                        .Build();
            }



        //private static ValidateAsyncDelegate<LoginForm> ValidateUserPermissionOnPPMSite = async (state, response) =>
        //{
        //    var result = new ValidateResult { IsValid = true, Value = response };
        //  //  var zip = (response as string).Trim();
        //    if (TokenHelper.checkUserPermission(state.Name, (string)response) == true)
        //    {
        //        result.IsValid = true;
        //        //context.UserData.SetValue("UserName", (string)response);
        //        //new Mongo().Insert("ContextTokens", new Token((string)response));
        //        //  await context.PostAsync("You are registerd. Have a happy time with us.");
        //        // result.Feedback = "You did not enter valid email address.";
        //    }
        //    else
        //    {
        //        result.IsValid = false;
        //           result.Feedback = $"Sorry, Your User Name / Password Is wrong or you don't have permission. Please try again.";

        //        //    response.Append($"Hey {userName}.. you Don't have permission anymore to the current site:)");
        //        //    var form = new FormDialog<LoginForm>(new LoginForm(), LoginForm.BuildForm);
        //        //    context.Call(form, SignUpComplete);

                
        //        //PromptDialog.Confirm(context, UseAnotherCridentials,
        //        //$"You Don't Have permission to the Site , do you want to use another cridentials ?");

        //    }

        //    return await Task.FromResult(result);
        //};

        //private static Task<ValidateResult> ValidateUserPermissionOnPPMSite(LoginForm state, object response)
        //{
        //    var result = new ValidateResult { IsValid = true, Value = response };



        //    if (TokenHelper.checkAuthorizedUser(state.Name, (string)response) == true)
        //    {
        //        result.IsValid = true;
        //        //context.UserData.SetValue("UserName", (string)response);
        //        //new Mongo().Insert("ContextTokens", new Token((string)response));
        //        //  await context.PostAsync("You are registerd. Have a happy time with us.");
        //        // result.Feedback = "You did not enter valid email address.";
        //    }
        //    else
        //    {
        //        result.IsValid = false;
        //        result.Feedback = $"Sorry, Your User Name / Password Is wrong or you don't have permission. Please try again.";
        //    }


        //    return Task.FromResult(result);
        //}
        

        }
}