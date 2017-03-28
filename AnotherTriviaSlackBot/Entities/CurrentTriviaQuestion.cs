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
        /// <summary>
        /// Gets the question.
        /// </summary>
        /// <value>
        /// The question.
        /// </value>
        public Question Question { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this question is answered.
        /// </summary>
        /// <value>
        /// <c>true</c> if this question is answered; otherwise, <c>false</c>.
        /// </value>
        public bool IsAnswered { get; set; }

        /// <summary>
        /// Gets or sets the answerer user identifier.
        /// </summary>
        /// <value>
        /// The answerer user identifier.
        /// </value>
        public string AnswererUserID { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentTriviaQuestion"/> class.
        /// </summary>
        /// <param name="question">The question.</param>
        public CurrentTriviaQuestion(Question question)
        {
            this.Question = question;
            this.IsAnswered = false;
            this.AnswererUserID = null;
        }
    }
}
