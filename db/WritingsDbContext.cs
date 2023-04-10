using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLitedb
{
    public class WritingsDbContext : DbContext
    {
        public DbSet<ChAssessment> ChAssessments { get; set; }
        public DbSet<EngAssessment> EngAssessments { get; set; }
        public DbSet<StrokeData> StrokeDatas { get; set; }

        public WritingsDbContext() : base("Default")
        {
            Database.SetInitializer<WritingsDbContext>(null);
        }


        private static string LoadConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
        }
    }
}
