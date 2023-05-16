# HyperDBDotNet
This is a dotnet port of https://github.com/jdagdelen/hyperDB, check it out if you need this for python.
Please note that while this was initially ported from this repo,  it may or may not stay in sync with it.  I do plan to extend this with new functionality as needed and in doing so this project may diverge from the original.

## TODO:

 - [X] Create a function to use openai api to get vectors from documents
 - [ ] Create a function to use a local model to get vectors from documents
 - [ ] create interface so additional models can be added
 - [ ] create import interface so that document importation is pluggable


This was a quick port, so it's not very well tested(not at all as of now). I'm using it for a project, so I'll be adding to it as I need to. Feel free to contribute.
I will be adding more documentation as I go.
Initial port done by GPT,  code cleanup etc by deatos

## Usage:

    ```            
            var embedder = new HyperDBDotNet.HDEmbed(apikey);
            var DB = new HyperDBDotNet.HyperDBDotNet(embedder);
            DB.AddDocument("This is instructions for a program");
            DB.AddDocument("This is a test document");
            var res = DB.Query("find me a test document", 5);
            var res2 = DB.Query("find me instructions on a program", 5);
```


