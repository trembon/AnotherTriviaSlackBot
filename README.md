# AnotherTriviaSlackBot
Just another trivia slack bot, inspired the classic trivia bot from IRC

Can be installed as a Windows service with the [InstallUtil](https://msdn.microsoft.com/en-us/library/sd8zc8ha(v=vs.110).aspx) command or just start the application and select the install choice.

###Example configuration (settings.json)
```
{
  "SlackAuthToken": "xoxb-your-bot-key", - the slack authentication key
  "ChannelName": "ChannelName", - the channel name to listen for @triviabot <command> messages
  "QuestionsPerRound": 10, - questions per round when a trivia is started
  "ShowHintAfterSeconds": 10, - after the number of seconds to show a hint to the players
  "ShowAnswerAfterSeconds": 20, - maxiumum time to let the users guess on the answer,
  "DefaultCategory": "general" - the default category to start if the command @botname start is used
}
```
