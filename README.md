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

## Installation

### Step 1 - Generating the SlackAuthToken

1. Open up your slack client.


2. Go to the "Apps & Integration" for the slack team.


3. Search for the app named "Bots".  
    The description of the app should be "Run code that listens and posts to your Slack team just as a user would.".
    
    
4. Click on "Add Configuration".


5. Choose what the bot should be called, then click "Add bot integration". Example: triviabot  
    Note: This name will be used for commands, like @triviabot start.


6. You can now change how the bot will appear in slack to the users. When done, click on "Save Integration".


7. Copy the value in the field "API Token" and place it in your configuration as the "SlackAuthToken".


### Step 2 - Setting up Slack

1. Create a channel for trivia, like "Trivia".


2. Enter the channel you just created.


3. Select the option to invite a team member to the channel and select the trivia bot.  
    The trivia bot will appear with the name what you choose to call it when creating the bot integration (like triviabot) in the list of team member.