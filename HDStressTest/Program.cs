using System.Diagnostics;

namespace HDStressTest {
    public class Program {

        public static HyperDBDotNet.HyperDBDotNet DB;
        public static int TotalCount = 0;
        static void Main(string[] args) {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var apikey = "";
            if (File.Exists(Path.Combine(path, "oak.txt"))) {
                apikey = File.ReadAllText(Path.Combine(path, "oak.txt")).Trim();
            } else {
                Console.WriteLine("Cant load api key from file. Please enter it manually.");
                apikey = Console.ReadLine();
            }

            var embedder = new StressEmbed(apikey);
            DB = new HyperDBDotNet.HyperDBDotNet(embedder);
            Console.WriteLine("Enter path to database:");
            path = Console.ReadLine();
            if (File.Exists(path)) {
                var loadedcount = DB.Load(path);
                Console.WriteLine($"Loaded: {loadedcount} records");
            } else {
                Console.WriteLine("File not found.  Press any key to exist.");
                Console.ReadKey();
                return;
            }
            var list1 = new[] { "pig", "sheep", "goat", "chicken", "duck", "turkey", "deer", "bear", "lion", "dog", "cat", "bird", "fish", "horse", "cow" };
            var list2 = new[] { "jump", "swim", "fly", "climb", "crawl", "hop", "skip", "dance", "sing", "play", "fight", "eat", "sleep", "run", "walk" };
            var list3 = new[] { "big", "small", "tall", "short", "fat", "skinny", "fast", "slow", "strong", "weak", "smart", "dumb", "happy", "sad", "angry" };
            var list4 = new[] { "quickly", "slowly", "happily", "sadly", "angrily", "loudly", "quietly", "softly", "roughly", "smoothly", "easily", "hardly", "gently", "carefully", "carelessly" };
            var list5 = new[] { "on", "in", "under", "over", "above", "below", "beside", "behind", "in front of", "inside", "outside", "between", "among", "through", "around" };
            var r = new Random();
            Console.WriteLine("Generate how many search records for loop?");
            var count = int.Parse(Console.ReadLine());
            for (int i = 0; i < count; i++) {
                var query = $"{list1[r.Next(0, list1.Count() - 1)]} {list2[r.Next(0, list2.Count() - 1)]} {list3[r.Next(0, list3.Count() - 1)]} {list4[r.Next(0, list4.Count() - 1)]} {list5[r.Next(0, list5.Count() - 1)]}";
                embedder.AddVector(query);
            }
            var stopwatch = new Stopwatch();
            
            stopwatch.Start();
            for(int i = 0; i < 1; i++) {
                var thread = new Thread(() => WorkThread(stopwatch));
                thread.Start();
            }
            while (stopwatch.Elapsed.TotalSeconds < 65) {
                Thread.Sleep(250);
            }
            Console.WriteLine($"Complete, QPS: {TotalCount/60}");


        }
        
        private static void WorkThread(Stopwatch stopwatch) {
            var loopcount = 0;
            while (stopwatch.Elapsed.TotalSeconds < 60) {
                loopcount++;
                TotalCount++;
                var query = DB.Query("Stresstest"); //Data dont matter as embedder returns random vectors from generated list
                if (loopcount % 100 == 0) {
                    Console.WriteLine($"Loop: {loopcount} - Time: {stopwatch.Elapsed.TotalSeconds} QPS:{loopcount / stopwatch.Elapsed.TotalSeconds}");
                }
            }
        }
    }
}