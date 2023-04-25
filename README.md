# HyperDBDotNet
This is a dotnet port of https://github.com/jdagdelen/hyperDB, check it out if you need this for python.
Please note that while this was initially ported from this repo,  it may or may not stay in sync with it.  I do plan to extend this with new functionality as needed and in doing so this project may diverge from the original.

## TODO:

 - [ ] Create a function to use openai api to get vectors from documents
 - [ ] Create a function to use a local model to get vectors from documents
 - [ ] create interface so additional models can be added
 - [ ] create import interface so that document importation is pluggable


This was a quick port, so it's not very well tested(not at all as of now). I'm using it for a project, so I'll be adding to it as I need to. Feel free to contribute.
I will be adding more documentation as I go.
This port was done by asking gpt4 to port the project

## Usage(will updated soon):

    ```csharp
        var documents = new List<string>();
        using (var reader = new StreamReader("demo/pokemon.jsonl"))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                documents.Add(line);
            }
        }

        // Instantiate HyperDB with the list of documents
        var db = new HyperDB(documents, "info.description");

        // Save the HyperDB instance to a file
        db.Save("demo/pokemon_hyperdb.json");

        // Load the HyperDB instance from the save file
        db.Load("demo/pokemon_hyperdb.json");

        // Query the HyperDB instance with a text input
        var results = db.Query("Likes to sleep.", topK: 5);

        // Print the results
        foreach (var result in results)
        {
            Console.WriteLine(result);
        }
```


