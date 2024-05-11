using System;

namespace Desc
{
    public class Project
    {
            public int IdProject { get; set; }
            public string FullTitle { get; set; }
            public string ShortTitle { get; set; }
            public object Icon { get; set; }
            public string CreatedTime { get; set; }
            public object DeletedTime { get; set; }
            public DateTime StartSheduleDate { get; set; }
            public object FinishSheduleDate { get; set; }
            public string Description { get; set; }
            public int IdEmployee { get; set; }
            public int IdResponsible { get; set; }
    }
}