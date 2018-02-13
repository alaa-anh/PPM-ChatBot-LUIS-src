using System;
using System.Configuration;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Web.ModelBinding;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using LuisBot.Model;
using Chronic;
using System.Collections.Generic;


using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections;

namespace Microsoft.Bot.Sample.LuisBot
{
    [Serializable]
    public class PPMDialog : LuisDialog<object>
    {
        private string userName;
        private DateTime msgReceivedDate;
        private string PPMServerURL;


        public PPMDialog(Activity activity) : base(new LuisService(new LuisModelAttribute(


            ConfigurationManager.AppSettings["LuisAppId"],
            ConfigurationManager.AppSettings["LuisAPIKey"],
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {

            userName = activity.From.Name;
            msgReceivedDate = DateTime.Now;// activity.Timestamp ? ? DateTime.Now;
            PPMServerURL = ConfigurationManager.AppSettings["PPMServerURL"];
        }




        [LuisIntent("")]
        [LuisIntent("none")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult luisResult)
        {
            string response = string.Empty;
            await context.PostAsync(response);
            context.Wait(this.MessageReceived);
        }


        [LuisIntent("Greet.Welcome")]
        public async Task GreetWelcome(IDialogContext context, LuisResult luisResult)
        {
            StringBuilder response = new StringBuilder();

            if (this.msgReceivedDate.ToString("tt") == "AM")
            {
                response.Append($"Good morning, {userName}.. :)");
            }
            else
            {
                response.Append($"Hey {userName}.. :)");
            }



            await context.PostAsync(response.ToString());
            context.Wait(this.MessageReceived);
        }


        [LuisIntent("Greet.Farewell")]
        public async Task GreetFarewell(IDialogContext context, LuisResult luisResult)
        {
            string response = string.Empty;
            if (this.msgReceivedDate.ToString("tt") == "AM")
            {
                response = $"Good bye, {userName}.. Have a nice day. :)";
            }
            else
            {
                response = $"b'bye {userName}, Take care.";
            }
            await context.PostAsync(response);
            context.Wait(this.MessageReceived);
        }


        [LuisIntent("Search.Projects")]
        public async Task SearchProjects(IDialogContext context, LuisResult luisResult)
        {
            EntityRecommendation projectname;

            string searchTerm_ProjectName = string.Empty;

            if (luisResult.TryFindEntity("Project.name", out projectname))
            {
                searchTerm_ProjectName = projectname.Entity;
            }

            if (string.IsNullOrWhiteSpace(searchTerm_ProjectName))
            {
                await context.PostAsync($"Unable to get search term.");
            }
            else
            {
                await context.PostAsync(new Common.ProjectServer(this.userName, PPMServerURL).FindProjectByName(searchTerm_ProjectName));
            }

            context.Wait(this.MessageReceived);
        }


        [LuisIntent("GetAllProjects")]
        public async Task GetAllProjects(IDialogContext context, LuisResult luisResult)
        {
            EntityRecommendation projects;

            string searchTerm_ProjectName = string.Empty;

            if (luisResult.TryFindEntity("Projects", out projects))
            {
                searchTerm_ProjectName = projects.Entity;
            }

            if (string.IsNullOrWhiteSpace(searchTerm_ProjectName))
            {
                await context.PostAsync($"Unable to get search term.");
            }
            else
            {
                await context.PostAsync(new Common.ProjectServer(this.userName, PPMServerURL).GetAllProjects());
            }

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("GetProjectIssues")]
        public async Task GetProjectIssues(IDialogContext context, LuisResult luisResult)
        {
            EntityRecommendation projectname;
            EntityRecommendation projectIssues;


            string searchTerm_ProjectName = string.Empty;


            if (luisResult.TryFindEntity("Project.name", out projectname) && luisResult.TryFindEntity("Project.Issues", out projectIssues))
            {
                searchTerm_ProjectName = projectname.Entity;
            }

            if (string.IsNullOrWhiteSpace(searchTerm_ProjectName))
            {
                await context.PostAsync($"Unable to get search term.");
            }
            else
            {
                await context.PostAsync(new Common.ProjectServer(this.userName, PPMServerURL).GetProjectIssues(searchTerm_ProjectName));
            }

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("GetProjectTasks")]
        public async Task GetProjectTasks(IDialogContext context, LuisResult luisResult)
        {
            EntityRecommendation projectname;
            EntityRecommendation projectTasks;


            string searchTerm_ProjectName = string.Empty;


            if (luisResult.TryFindEntity("Project.name", out projectname) && luisResult.TryFindEntity("Project.Tasks", out projectTasks))
            {
                searchTerm_ProjectName = projectname.Entity;
            }

            if (string.IsNullOrWhiteSpace(searchTerm_ProjectName))
            {
                await context.PostAsync($"Unable to get search term.");
            }
            else
            {
                await context.PostAsync(new Common.ProjectServer(this.userName, PPMServerURL).GetProjectTasks(searchTerm_ProjectName));
            }

            context.Wait(this.MessageReceived);
        }


        [LuisIntent("GetProjectDates")]
        public async Task GetProjectDates(IDialogContext context, LuisResult luisResult)
        {
            EntityRecommendation projectname;
            EntityRecommendation projectDate;


            string searchTerm_ProjectName = string.Empty;


            if (luisResult.TryFindEntity("Project.name", out projectname) && luisResult.TryFindEntity("Project.Date", out projectDate))
            {
                searchTerm_ProjectName = projectname.Entity;
            }

            if (string.IsNullOrWhiteSpace(searchTerm_ProjectName))
            {
                await context.PostAsync($"Unable to get search term.");
            }
            else
            {
                await context.PostAsync(new Common.ProjectServer(this.userName, PPMServerURL).GetProjectDates(searchTerm_ProjectName));
            }

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("GetProjectRiskResources")]
        public async Task GetProjectRiskResources(IDialogContext context, LuisResult luisResult)
        {
            EntityRecommendation projectname;


            string searchTerm_ProjectName = string.Empty;


            if (luisResult.TryFindEntity("Project.name", out projectname))
            {
                searchTerm_ProjectName = projectname.Entity;
            }

            if (string.IsNullOrWhiteSpace(searchTerm_ProjectName))
            {
                await context.PostAsync($"Unable to get search term.");
            }
            else
            {
                await context.PostAsync(new Common.ProjectServer(this.userName, PPMServerURL).GetProjectRiskResources(searchTerm_ProjectName));
            }

            context.Wait(this.MessageReceived);
        }


        [LuisIntent("GetProjectRiskStatus")]
        public async Task GetProjectRiskStatus(IDialogContext context, LuisResult luisResult)
        {
            EntityRecommendation projectname;


            string searchTerm_ProjectName = string.Empty;


            if (luisResult.TryFindEntity("Project.name", out projectname))
            {
                searchTerm_ProjectName = projectname.Entity;
            }

            if (string.IsNullOrWhiteSpace(searchTerm_ProjectName))
            {
                await context.PostAsync($"Unable to get search term.");
            }
            else
            {
                await context.PostAsync(new Common.ProjectServer(this.userName, PPMServerURL).GetProjectRiskStatus(searchTerm_ProjectName));
            }

            context.Wait(this.MessageReceived);
        }


        [LuisIntent("FilterProjectsByDate")]
        public async Task FilterProjectsByDate(IDialogContext context, LuisResult luisResult)
        {
            string FilterType = string.Empty;
            string ProjectSEdateFlag = "START";
            string ProjectED = string.Empty;


            string ProjectSDate = string.Empty;
            string ProjectEDate= string.Empty;

             var filterDate = (object)null;

            EntityRecommendation dateTimeEntity, dateRangeEntity , ProjectS , ProjectE;

            
            if (luisResult.TryFindEntity("builtin.datetimeV2.daterange", out dateRangeEntity))
            {
                filterDate = dateRangeEntity.Resolution.Values.Select(x => x).OfType<List<object>>().SelectMany(i => i).FirstOrDefault();
                if (Datevalues(filterDate, "Mod") != "")
                {
                    FilterType = Datevalues(filterDate, "Mod");
                    ProjectSDate = Datevalues(filterDate, "timex");

                }
                else
                {
                    FilterType = "Between";

                    ProjectSDate = Datevalues(filterDate, "start");
                }
                ProjectEDate = Datevalues(filterDate, "end");               

            }
            else if (luisResult.TryFindEntity("Project.Start", out ProjectS))
            {
                ProjectSEdateFlag = "START";
            }
            else if (luisResult.TryFindEntity("Project.Finish", out ProjectE))
            {
                ProjectSEdateFlag = "Finish";
            }


           if(filterDate !=null)
                await context.PostAsync(new Common.ProjectServer(this.userName, PPMServerURL).FilterProjectsByDate(FilterType , ProjectSDate , ProjectEDate , ProjectSEdateFlag));




            context.Wait(this.MessageReceived);
        }



        public string Datevalues(object obj , string keyNeed)
        {
            string keyval = string.Empty;
            if (typeof(IDictionary).IsAssignableFrom(obj.GetType()))
            {
                IDictionary idict = (IDictionary)obj;

                Dictionary<string, string> newDict = new Dictionary<string, string>();
                foreach (object key in idict.Keys)
                {
                    if (keyNeed == key.ToString())
                    {
                        keyval = idict[key].ToString();
                        //newDict.Add(key.ToString(), idict[key].ToString());
                        break;
                    }
                }
            }
            return keyval;
            
        }

        private static DateTime? ParseLuisDateString(string value)
        {
            int year;
            int month;
            int weekNumber;
            int day;

            string[] dateParts = value.Split('-');

            if (dateParts[0] != "XXXX")
            {
                year = Convert.ToInt16(dateParts[0]);
            }
            else
            {
                year = DateTime.Now.Year;
            }

            if (dateParts[1].Contains("W"))
            {
                if (!dateParts[1].Contains("XX"))
                {
                    weekNumber = Convert.ToInt16(dateParts[1].Substring(1, dateParts[1].Length - 1));
                    return FirstDateOfWeekIso8601(year, weekNumber);
                }
                else
                {
                    month = DateTime.Now.Month;
                }
            }
            else
            {
                month = Convert.ToInt16(dateParts[1]);
            }

            if (dateParts[2] != null && dateParts[2] != "XX")
            {
                day = Convert.ToInt16(dateParts[2]);
            }
            else
            {
                day = 1;
            }

            var dateString = string.Format("{0}-{1}-{2}", year, month, day);
            return DateTime.Parse(dateString);
        }

        public static DateTime FirstDateOfWeekIso8601(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }
            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3);
        }

        private static DateTime? ResolveDateTime(IEnumerable<EntityRecommendation> entities)
        {
            DateTime? date = null;
            DateTime? time = null;

            //foreach (var entity in entities)
            //{
            //    if (entity.Type.Contains("builtin.datetime") && entity.Resolution.Any())
            //    {
            //        switch (entity.Resolution.First().Key)
            //        {
            //            case "date":
            //                var dateTimeParts = entity.Resolution.First().Value.Split('T');
            //                date = ParseLuisDateString(dateTimeParts[0]);

            //                if (date.HasValue)
            //                {
            //                    if (!string.IsNullOrEmpty(dateTimeParts[1]))
            //                        time = ParseLuisTimeString(dateTimeParts[1]);
            //                }
            //                break;
            //            case "time":
            //                time = ParseLuisTimeString(entity.Resolution.First().Key);
            //                break;
            //        }
            //    }

            //    if (date.HasValue)
            //    {
            //        if (time.HasValue)
            //        {
            //            return date.Value.Date + time.Value.TimeOfDay;
            //        }

            //        return date.Value;
            //    }

            //    if (time.HasValue)
            //    {
            //        return DateTime.Now + time.Value.TimeOfDay;
            //    }
            //}

            return null;
        }
    }


}


