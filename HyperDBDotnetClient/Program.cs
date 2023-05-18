namespace HyperDBDotnetClient {
    internal class Program {

        public static HyperDBDotNet.HyperDBDotNet DB;
        private static OpenAI.OpenAIClient Client;

        static void Main(string[] args) {
            HyperDBDotNet.HyperDBDotNet.DEBUGMODE = true;
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var apikey = "";
            if (File.Exists(Path.Combine(path, "oak.txt"))) {
                apikey = File.ReadAllText(Path.Combine(path, "oak.txt")).Trim();
            } else {
                Console.WriteLine("Cant load api key from file. Please enter it manually.");
                apikey = Console.ReadLine();
            }

            var embedder = new HyperDBDotNet.HDEmbed(apikey);
            Client = new OpenAI.OpenAIClient(new OpenAI.OpenAIAuthentication(apikey));
            DB = new HyperDBDotNet.HyperDBDotNet(embedder);
            if (File.Exists("database.json")) {
                var loadedcount = DB.Load("database.json");
                Console.WriteLine($"Loaded: {loadedcount} records");
            }

            var hasdocs = true;
            while (hasdocs) {
                Console.WriteLine("Enter the path to a document to add to the database, type testdata to generate test data, or enter 'done' to continue.");
                var docpath = Console.ReadLine();
                if (docpath == "done") {
                    hasdocs = false;
                } else if (docpath == "testdata") {
                    Console.WriteLine("How many documents do you want to generate?");
                    var count = int.Parse(Console.ReadLine());
                    GenerateTestData(count);
                } else {
                    LoadDocument(docpath);
                }
            }
            DB.Save("database.json");
            while (true) {
                Console.WriteLine();
                Console.WriteLine("Enter query:");
               var res = QueryDatabase(Console.ReadLine(), 10);
                Console.WriteLine(res);
            }
        }

        static void LoadDocument(string path) {
            var docdata = File.ReadAllText(path);
            var docs = docdata.Split("!@!");
            foreach (var doc in docs) {
                DB.AddDocument(doc.Trim());
            }
        }   
        static string QueryDatabase(string query, int topK, string append = "") {
            var res = DB.Query(query, topK);
            query = query + append;
            if (res.Count > 0) {
                var results = "";
                foreach (var doc in res) {
                    results += doc + "\n";
                }
                var messages = new List<OpenAI.Chat.Message>();
                messages.Add(new OpenAI.Chat.Message(OpenAI.Chat.Role.System,query));
                messages.Add(new OpenAI.Chat.Message(OpenAI.Chat.Role.User,results));
                var chatreq = new OpenAI.Chat.ChatRequest(messages, OpenAI.Models.Model.GPT3_5_Turbo);
                var response = Client.ChatEndpoint.GetCompletionAsync(chatreq).GetAwaiter().GetResult();
                var ret = response.FirstChoice.Message;
                return ret;
            } else {
                return "No results found.";
            }
        }
        static void GenerateTestData(int count = 1000) {
            var list1 = new[] { "pig", "sheep", "goat", "chicken", "duck", "turkey", "deer", "bear", "lion","dog","cat", "bird", "fish", "horse", "cow"};
            var list2 = new[] { "jump", "swim", "fly", "climb", "crawl", "hop", "skip", "dance", "sing", "play", "fight", "eat", "sleep", "run", "walk" };
            var list3 = new[] { "big", "small", "tall", "short", "fat", "skinny", "fast", "slow", "strong", "weak", "smart", "dumb", "happy", "sad", "angry" };
            var list4 = new[] { "quickly", "slowly", "happily", "sadly", "angrily", "loudly", "quietly", "softly", "roughly", "smoothly", "easily", "hardly", "gently", "carefully", "carelessly" };
            var list5 = new[] { "on", "in", "under", "over", "above", "below", "beside", "behind", "in front of", "inside", "outside", "between", "among", "through", "around" };
            foreach (string i1 in list1) {
                foreach (string i2 in list2) {
                    foreach (string i3 in list3) {
                        foreach (string i4 in list4) {
                            foreach (string i5 in list5) {
                                DB.AddDocument($"{i1} {i2} {i3} {i4} {i5}");
                                Console.WriteLine($"{count} left");
                                if (count % 50 == 0) {
                                    Console.WriteLine("Saving database...");
                                    DB.Save("database.json");
                                }
                                if (count-- <= 0) return;
                            }
                        }
                    }
                }
            }
        }

    }
}