# tcslackbot

[![Build Status](https://timecockpit.visualstudio.com/tcslackbot/_apis/build/status/software-architects.tcslackbot?branchName=master)](https://timecockpit.visualstudio.com/tcslackbot/_build/latest?definitionId=39&branchName=master)

Time Cockpit Slackbot (Work in Progress)

# Task
Users of the time management application time cockpit spend a lot of time in the communication software Slack and do not want to change the tool to record their time. The goal of this diploma thesis is to create a Slack Bot that solves this problem for time cockpit customers. This will save valuable time and money.

# Implementation
The bot was implemented as an open source ASP.NET core application, which was published under the MIT license. The bot is hosted in the cloud.  Time cockpit customers can use commands to make their time bookings directly in Slack. The commands are sent from Slack to our server, which then processes them.

# Result
Our bot is designed to help customers of time cockpit to automate tasks and thus gain valuable time. At the time of printing it was not yet decided if the results of this diploma thesis will be used in practice at customers of the client. A final presentation was still pending. In the preliminary presentation the client was satisfied with the results of our work.

## How to add TCSlackbot to your Server?
To install the Bot on your server click [here](https://slack.com/oauth/v2/authorize?client_id=645682850067.645685522130&scope=app_mentions:read,calls:read,calls:write,channels:history,channels:read,chat:write,commands,dnd:read,emails:write,files:read,groups:history,groups:read,im:history,im:read,im:write,pins:write,reactions:read,reactions:write,team:read,users:read&user_scope=channels:read,groups:read,identify,im:read,im:write,users.profile:read,users:read).

When adding choose a channel of your choice, where the bot can read and react to commands
