# AusDevs Bot

This is my entry to the AusDevs discord bot hackathon/competition

This bot is designed around the need of saving code snippets for later use.

The bot is developed with C# and the Discord.NET library and makes use of Sqlite for a database


## Running the bot
- first create the .env file: `cp .env.example .env`
- create a discord bot and generate the token ((see this article for how to do that)[https://www.writebots.com/discord-bot-token/])
  - **Important**: make sure you check the `bot` and `application.commands` scopes when creating the oauth url for the bot
  - **Important**: make sure to check the `Read Messages`, `Send Messages`, `Manage Messages` and `Use Slash Commands` permissions




## Functionality
- save snippet
- fetch snippets
- 