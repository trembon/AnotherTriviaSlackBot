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

        public static List<Question> GetRandomQuestions(string category, int amount)
        {
            try
            {
                List<string> badQuestionIds = new List<string>(0);
                lock (dbDataLock)
                {
                    using(var context = new DataDBContext())
                    {
                        badQuestionIds = context.BadQuestions.Select(bq => "'" + bq.QuestionID + "'").ToList();
                    }
                }

                lock (dbQuestionLock)
                {
                    using (var context = new QuestionDBContext())
                    {
                        return context.Database.SqlQuery<Question>($"SELECT * FROM questions WHERE id NOT IN ({String.Join(",", badQuestionIds)}) AND category = '{category}' ORDER BY RANDOM() LIMIT {amount}").ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Failed to get random questions.");
                return new List<Question>();
            }
        }

        private static int? cacheGetQuestionCount = null;
        public static int GetQuestionCount()
        {
            try
            {
                lock (dbQuestionLock)
                {
                    if (cacheGetQuestionCount != null)
                        return cacheGetQuestionCount.Value;

                    using (var context = new QuestionDBContext())
                    {
                        cacheGetQuestionCount = context.Questions.Count();
                        return cacheGetQuestionCount.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Failed to get question count.");
                return -1;
            }
        }

        private static Dictionary<string, int> cacheGetCategories = null;
        public static Dictionary<string, int> GetCategories()
        {
            try
            {
                lock (dbQuestionLock)
                {
                    if (cacheGetCategories != null)
                        return cacheGetCategories;

                    using (var context = new QuestionDBContext())
                    {
                        cacheGetCategories = context.Questions.GroupBy(q => q.Category).ToDictionary(k => k.Key, v => v.Count());
                        return cacheGetCategories;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Failed to get categories.");
                return new Dictionary<string, int>();
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

        public static void SetQuestionAsBad(string questionId, string userId)
        {
            try
            {
                lock (dbDataLock)
                {
                    using (var context = new DataDBContext())
                    {
                        var badQuestion = new BadQuestion { QuestionID = questionId, UserID = userId };
                        context.BadQuestions.Add(badQuestion);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, $"Failed to set question '{questionId}' as bad.");
            }
        }
    }
}
