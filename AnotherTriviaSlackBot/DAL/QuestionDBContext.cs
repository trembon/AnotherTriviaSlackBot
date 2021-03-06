﻿using AnotherTriviaSlackBot.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotherTriviaSlackBot.DAL
{
    public class QuestionDBContext : DbContext
    {
        /// <summary>
        /// Gets the questions database set.
        /// </summary>
        /// <value>
        /// The questions.
        /// </value>
        public DbSet<Question> Questions { get; set; }
    }
}
