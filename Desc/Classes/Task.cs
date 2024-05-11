using System;

namespace Desc.Classes
{
    public class Task
    {
            public int IdTask { get; set; }
            public int ProjectId { get; set; }
            public string FullTitle { get; set; }
            public string ShortTitle { get; set; }
            public string Description { get; set; }
            public int ExecutiveEmployeeId { get; set; }
            public int StatusId { get; set; }
            public string CreatedTime { get; set; }
            public TimeSpan? UpdatedTime { get; set; }
            public TimeSpan? DeletedTime { get; set; }
            public DateTime Deadline { get; set; }
            public TimeSpan? StartActualTime { get; set; }
            public TimeSpan? FinishActualTime { get; set; }
            public int? PreviousTaskId { get; set; }
    }
}
