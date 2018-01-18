using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MiniBlogEngine.Models
{
    public class DashboardModel
    {
        public User User { get; set; }

        public List<Post> Posts { get; set; }
    }
}