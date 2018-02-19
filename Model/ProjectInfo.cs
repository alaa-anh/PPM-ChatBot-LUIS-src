using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot.Model
{

    [Serializable]
    public class ProjectInfo
    {
        public string ProjectName { get; set; }

        public string ProjectStartDate { get; set; }

        public string ProjectEndDate { get; set; }

        public int ProjectCost { get; set; }
    }
}