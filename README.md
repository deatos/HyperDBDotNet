#This project has been rewritten from the ground up at https://github.com/deatos/HyperVectorDB <--- Please use this instead.


Questions?  [Follow me on Twitter](https://twitter.com/deatos2k) or [Add me on Telegram](https://t.me/deatos)

# HyperDBDotNet

Welcome to the flashy world of HyperDBDotNet(Now with GPU Support... kinda)(Now with caching... kinda), a hyper-fast local vector database for c#/dotnet. We're standing on the shoulders of giants here, being based on https://github.com/jdagdelen/hyperDB.

We're all about LLM Agents, Automated General Intelligence (AGI), and other buzzwords that just scream "we know what we're doing". And don't worry, we're well-funded. Over a 100 trillion in VC capital kind of well-funded. Heck, we could buy a large country with that kind of money, but we chose to make a database instead. Aren't we responsible? Also, we're card-carrying members of the Silicon Valley Hyper Vector Database Association (SVHVDA). Yes, that's a real thing. Yes, we did make it up.  you're diving into the deep end of high-dimensional data (and who isn't these days?), remember it's the vector databases, financed by those deep-pocketed venture capitalists, that are working behind the scenes to make sense of it all.

## What's this vector database you speak of?
Buckle up, future tech moguls! You're about to dive into the latest rave of Silicon Valley: Vector Databases. Fancy name, right? It's a database, but not just any old database. It's for vectors. The more dimensions, the merrier.

So, you're wondering where these shiny vector databases are being put to use? The answer is... everywhere! From those eerily accurate "we thought you might like" suggestions when you're shopping online (because, obviously, you need another cat-themed mug), to those search engines that somehow know what you're looking for before you do.

And that's not all. Ever wondered how your phone knows it's you just by looking at your face, or how scientists match up DNA sequences as a fun weekend activity? You guessed it, vector databases. They're also the unsung heroes in monitoring systems and even in plotting your quickest escape to the nearest coffee shop.

So, next time you're swimming with the high-dimensional data sharks (and let's face it, who isn't?), remember the vector databases. Backed by those generous venture capitalists, they're the silent knights making sense of the chaos.

## Things we swear we're going to do at some point:
 - [ ] Add GPU support. Because we're all about speed.
 - [ ] Make indexes instead of multiple instances of the main class
 - [ ] Major code cleanup. It's not a mess, it's just... uniquely organized.
 - [ ] Add more documentation. Because who doesn't love a good manual?
 - [ ] Add actual tests, not just a demo. Because "it works on my machine" isn't a testing strategy.
 - [ ] Look into ways to reduce the search space. It's not that we're lost, we just enjoy exploring.
 - [X] Add a caching layer so we don't have to store the entire database in memory. RAM is expensive, you know.
 - [ ] Maybe some kind of compression? Because we're all about efficiency.
 - [X] Add a way to save the database to disk. Because we're not all about efficiency.



VectorOperations.cs is a port of galaxy_brain_math_shit from https://github.com/jdagdelen/hyperDB,  check this project out if you need a local vector database for python.

## How to use this thing(HDEmbed uses OpenAI,  but you can supply your own embedder,  just impliment IEmbed):

```
            var embedder = new HyperDBDotNet.HDEmbed(apikey);
            var DB = new HyperDBDotNet.HyperDBDotNet(embedder);
            DB.AddDocument("This is instructions for a program");
            DB.AddDocument("This is a test document");
            var res = DB.Query("find me a test document", 5);
            var res2 = DB.Query("find me instructions on a program", 5);
```


