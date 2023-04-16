using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using Newtonsoft.Json;

namespace HyperDBDotNet {
    public class HyperDBDotNet {
        private List<string> Documents { get; set; }
        private Matrix<double> Vectors { get; set; }
        private Func<IEnumerable<string>, Matrix<double>> EmbeddingFunction { get; set; }
        private Func<Matrix<double>, Vector<double>, Vector<double>> SimilarityMetric { get; set; }

        public HyperDBDotNet(IEnumerable<string> documents, string key, Func<IEnumerable<string>, Matrix<double>> embeddingFunction = null, string similarityMetric = "cosine") {
            if (embeddingFunction == null) {
                embeddingFunction = docs => GetEmbedding(docs, key);
            }

            Documents = new List<string>();
            EmbeddingFunction = embeddingFunction;
            Vectors = null;

            AddDocuments(documents);

            switch (similarityMetric.ToLower()) {
                case "cosine":
                    SimilarityMetric = VectorOperations.CosineSimilarity;
                    break;
                case "euclidean":
                    SimilarityMetric = VectorOperations.EuclideanMetric;
                    break;
                case "derrida":
                    SimilarityMetric = VectorOperations.DerridaeanSimilarity;
                    break;
                default:
                    throw new ArgumentException("Similarity metric not supported. Please use either 'cosine', 'euclidean' or 'derrida'.");
            }
        }

        public void AddDocument(string document, Vector<double> vector = null) {
            if (vector == null) {
                vector = EmbeddingFunction(new[] { document }).Row(0);
            }

            if (Vectors == null) {
                Vectors = Matrix<double>.Build.Dense(0, vector.Count);
            } else if (vector.Count != Vectors.ColumnCount) {
                throw new ArgumentException("All vectors must have the same length.");
            }

            Vectors = Vectors.InsertRow(Vectors.RowCount, vector);
            Documents.Add(document);
        }

        public void AddDocuments(IEnumerable<string> documents) {
            foreach (var document in documents) {
                AddDocument(document);
            }
        }

        public void Load(string storageFile) {
            using (var reader = new StreamReader(storageFile)) {
                var json = reader.ReadToEnd();
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                Vectors = Matrix<double>.Build.DenseOfRowArrays(((List<List<double>>)data["vectors"]).Select(row => row.ToArray()));
                Documents = ((List<string>)data["documents"]);
            }
        }

        public List<string> Query(string queryText, int topK = 5) {
            var queryVector = EmbeddingFunction(new[] { queryText }).Row(0);
            var rankedResults = VectorOperations.HyperSvmRankingAlgorithmSort(Vectors, queryVector, topK, SimilarityMetric);

            return rankedResults.Select(index => Documents[index]).ToList();
        }

        // The GetEmbedding function would go here, but it's not possible to provide a direct C# equivalent
        // since it depends on the OpenAI API that isn't available for C#.
        // You can create your own implementation that uses a different text embedding library or API.
        private static Matrix<double> GetEmbedding(IEnumerable<string> documents, string key) {
            throw new NotImplementedException("Please implement the GetEmbedding method.");
        }
    }
}
