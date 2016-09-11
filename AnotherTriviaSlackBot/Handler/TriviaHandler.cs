using AnotherTriviaSlackBot.Configuration;
using AnotherTriviaSlackBot.DAL;
using AnotherTriviaSlackBot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotherTriviaSlackBot.Handler
{
    public class TriviaHandler
    {
        private MainConfiguration configuration;

        public bool IsActive { get; set; }

        private Action<string> sendMessage;

        public TriviaHandler(MainConfiguration configuration, Action<string> sendMessage)
        {
            this.configuration = configuration;
            this.sendMessage = sendMessage;
        }

        public void Start()
        {
            if (IsActive)
                return;
            
            sendMessage($"A new trivia will start in a moment, just gonna find {configuration.QuestionsPerRound} questions!");

            currentQuestion = -1;
            questions = TriviaDB.GetRandomQuestions(configuration.QuestionsPerRound).Select(q => new CurrentTriviaQuestion(q)).ToList();

            Task.Delay(1000).ContinueWith(x => ShowQuestion());

            IsActive = true;
        }

        public void Cancel()
        {
            if (!IsActive)
            {
                sendMessage("There isn't an active trivia to stop, stupid");
                return;
            }
            
            questions.ForEach(q => q.IsAnswered = true);

            sendMessage("Someone is boring... stopping current trivia");
            IsActive = false;
        }

        private List<CurrentTriviaQuestion> questions;
        private int currentQuestion;

        private object questionsLock = new object();

        private void ShowQuestion()
        {
            currentQuestion++;

            if (currentQuestion < questions.Count)
            {
                if (!IsActive)
                    return;

                var question = questions[currentQuestion];
                sendMessage($"*Question {currentQuestion + 1}*: {question.Question.Text}");

                Task.Delay(configuration.ShowHintAfterSeconds * 1000).ContinueWith(x => ShowHelp(question));
                Task.Delay(configuration.ShowAnswerAfterSeconds * 1000).ContinueWith(x => ShowAnswer(question));
            }
            else
            {
                Dictionary<string, int> statsAfterRound = questions.Where(x => x.AnswererUserID != null).GroupBy(x => x.AnswererUserID).OrderByDescending(x => x.Count()).ToDictionary(k => k.Key, v => v.Count());

                TriviaDB.UpdatePlayerStats(statsAfterRound);
                var top5 = TriviaDB.GetTopFiveUsers();

                StringBuilder resultMsg = new StringBuilder();
                resultMsg.Append($"This trivia round is now done, the results are the following:\n");
                if (statsAfterRound.Count > 0)
                {
                    resultMsg.Append(String.Join(", ", statsAfterRound.Select(x => $"<@{x.Key}>: {x.Value}")));
                }
                else
                {
                    resultMsg.Append("_No users scored this round, better luck next time!_");
                }

                resultMsg.Append("\n\n");

                resultMsg.Append($"Top 5 total:\n");
                if (top5.Count > 0)
                {
                    resultMsg.Append(String.Join(", ", top5.Select(x => $"<@{x.UserID}>: {x.Score}")));
                }
                else
                {
                    resultMsg.Append("_No users have scored yet._");
                }

                sendMessage(resultMsg.ToString());
                IsActive = false;
            }
        }

        public void ReceiveAnswer(string answer, string userId)
        {
            if (!IsActive)
                return;

            lock (questionsLock)
            {
                var question = questions[currentQuestion];
                if (!question.IsAnswered && question.Question.Answer.Equals(answer, StringComparison.InvariantCultureIgnoreCase))
                {
                    sendMessage($"Correct <@{userId}>! The answer is '{question.Question.Answer}'");

                    question.IsAnswered = true;
                    question.AnswererUserID = userId;

                    Task.Delay(1000).ContinueWith(x => ShowQuestion());
                }
            }
        }

        private void ShowAnswer(CurrentTriviaQuestion currentQuestion)
        {
            if (!IsActive)
                return;
            
            lock (questionsLock)
            {
                if (!currentQuestion.IsAnswered)
                {
                    sendMessage($"No correct answer, what I was looking for was '{currentQuestion.Question.Answer}'");

                    currentQuestion.IsAnswered = true;

                    Task.Delay(1000).ContinueWith(x => ShowQuestion());
                }
            }
        }

        private void ShowHelp(CurrentTriviaQuestion currentQuestion)
        {
            if (!IsActive)
                return;
            
            lock (questionsLock)
            {
                if (!currentQuestion.IsAnswered)
                {
                    sendMessage($"*Hint*: {currentQuestion.Question.Hint}");
                }
            }
        }
    }
}
