using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Classes
{
    public class TaskUser
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TaskId { get; set; }

        public TaskUser(int Id, int UserId, int TaskId)
        {
            this.Id = Id;
            this.UserId = UserId;
            this.TaskId = TaskId;
        }
    }
}
