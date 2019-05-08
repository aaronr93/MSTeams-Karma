# MSTeams-Karma

## About
A karma bot for Microsoft Teams inspired by the late [HipChat's karma bot.](https://bitbucket.org/atlassianlabs/ac-koa-hipchat-karma/src/master/) [![Build status](https://dev.azure.com/arosenberger/MSTeams-Karma/_apis/build/status/arosenberger-MSTeams-Karma%20-%20CI)](https://dev.azure.com/arosenberger/MSTeams-Karma/_build/latest?definitionId=1)

[Contributing](MSTeams-Karma/Public/CONTRIBUTING.md) | [Privacy Policy](MSTeams-Karma/Public/Privacy.md) | [Terms](MSTeams-Karma/Public/Terms.md)

## Install
1. [Create a bot](https://dev.botframework.com/) or use my test bot (contact me).
2. Add the bot's App Id to the [manifest.json](MSTeams-Karma/Manifest/manifest.json) file, save, and zip together with both .png images.
3. Sideload the zip to Microsoft Teams, or use other Microsoft tools to test.
4. Follow instructions [here](https://docs.microsoft.com/en-us/microsoftteams/platform/resources/general/debug) for debugging the bot locally using ngrok. You will need a connection to an Azure Cosmos resource, or a mocked-out connection (working on making this easier).

## Usage
```
help                   show the help message
get top things         show the top 5 things
get bottom things      show the bottom 5 things
get top users          show the top 5 users
get bottom users       show the bottom 5 users
get thing              lookup thing's current karma
get @MentionName       lookup a user's current karma by @MentionName
thing++                add 1 karma to thing
thing++++              add 3 karma to thing (count(+) - 1, max 5)
thing--                remove 1 karma from thing
thing----              remove 3 karma from thing (count(-) - 1, max 5)
"subject phrase"++     add 1 karma to a subject phrase
@MentionName++         add 1 karma to a user by @MentionName
```

## License
Open Source [MIT License](LICENSE.txt)
