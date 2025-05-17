using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Class
{
    using System;

    namespace Project.Class
    {
        public class ProjUser
        {
            public int Id { get; set; } 
            public int Project { get; set; } 
            public int User { get; set; } 
            public string Role { get; set; } 

            public ProjUser(int id, int project, int user, string role)
            {
                this.Id = id;
                this.Project = project;
                this.User = user;
                this.Role = role;
            }
        }
    }
}
