using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AnotherTriviaSlackBot.Entities
{
    public class CurrentTriviaQuestion
    {
        public Question Question { get; }

        public bool IsAnswered { get; set; }

        public string AnswererUserID { get; set; }
        
        public CurrentTriviaQuestion(Question question)
        {
            this.Question = question;
            this.IsAnswered = false;
            this.AnswererUserID = null;
        }
    }
}
