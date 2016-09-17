using AnotherTriviaSlackBot.Entities;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotherTriviaSlackBot.DAL
{
    public static class TriviaDB
    {
        private static readonly Logger log = LogManager.GetLogger("TriviaDB");

        private static object dbQuestionLock = new object();
        private static object dbDataLock = new object();

        public static List<Question> GetRandomQuestions(int amount)
        {
            try
            {
                lock (dbQuestionLock)
                {
                    using (var context = new QuestionDBContext())
                    {
                        return context.Database.SqlQuery<Question>($"SELECT * FROM questions ORDER BY RANDOM() LIMIT {amount}").ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Failed to get random questions.");
                return new List<Question>();
            }
        }

        public static void UpdatePlayerStats(Dictionary<string, int> latestAddition)
        {
            if (latestAddition.Count == 0)
                return;

            try
            {
                lock (dbDataLock)
                {
                    using (var context = new DataDBContext())
                    {
                        var users = context.UserStats.ToList();
                        foreach (var scoredUser in latestAddition)
                        {
                            var user = users.FirstOrDefault(u => u.UserID == scoredUser.Key);
                            if (user == null)
                            {
                                user = new UserStats { UserID = scoredUser.Key, Score = 0 };
                                context.UserStats.Add(user);
                            }
                            user.Score += latestAddition[user.UserID];
                        }

                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Failed to get update user stats.");
            }
        }

        public static List<UserStats> GetTopFiveUsers()
        {
            try
            {
                lock (dbDataLock)
                {
                    using (var context = new DataDBContext())
                    {
                        return context
                            .UserStats
                            .OrderByDescending(u => u.Score)
                            .Take(5)
                            .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Failed to get top five user stats.");
                return new List<UserStats>();
            }
        }
    }
}
