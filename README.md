# FleckyBot - a bot for testing out stuff

![my dog Flecky enjoying the snow](./flecky.png)

*This is my personal bot that I am currently using to test out thing on the Discord platform. Written in C# on .Net 6.0. (originally on .Net 5.0) Running on my personal discord server.*

---

Basically I am testing out many things that the Discord platform allows, including:

- Audio Player, provided by [LavaLink](https://github.com/freyacodes/Lavalink)/[Victoria](https://github.com/Yucked/Victoria/)
- Posting images that are avaiable in the database
- Posting inspirational quotes from the database, provided by [DerEingerostete](https://github.com/DerEingerostete)
- Acceptance of rules and role select per Guild, set up in the database
- Ticketing and votes system
- 8ball
- Web Interface that allows to view settings per Discord Server/Guild, Tickets and Votes
- And some other neat things

Currently I am planning to add a proper web interface where you are able to change the desired settings for a specific Guild (like Tickets Channel, Votes Channel, etc.) and to clean up the current website because every Front End developer would start to *cringe* if they see that. *Sorry not sorry; I live in the backend, not the frontend ;)*

Anyway, if you have anything to comment on my code (critique, additional comments, etc.), feel free to raise an issue or send me an email.

---

## Database files

I am currently using a MongoDB Database, the database template files are in the folder "DatabaseFiles". Please do keep in mind that those are only examples and are not real data files! Those are only here to visualise how it might look like. *Have fun trying to import those, it will fail!*

I am planning to implement a MSSQL Database to this program to try out MSSQL and EF for database migrations as this is not possible with MongoDB.

---

## .Net 7.0 version

There is currently a .Net 7.0 branch but this will not get updated frequently like the main branch because of breaking changes in the specifications. Also, some packages used are still not updated for 7.0 so that might take a while to make it runnable or until I find a substitute.