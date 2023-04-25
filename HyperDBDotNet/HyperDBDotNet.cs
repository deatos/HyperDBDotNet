using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using Newtonsoft.Json;

namespace HyperDBDotNet {
    public class HyperDBDotNet {
        private List<string> HDDocuments { get; set; }
        private Matrix<double> HDVectors { get; set; }
        
        //private Func<IEnumerable<string>, Matrix<double>> EmbeddingFunction { get; set; }
        //private Func<Matrix<double>, Vector<double>, Vector<double>> SimilarityMetric { get; set; }
        private IEmbed Embedder { get; set; }
        private SimilarityMetric SimilarityMetric;

        public HyperDBDotNet(SimilarityMetric metric) {
            //this.SimilarityMetric = VectorOperations.CosineSimilarity;
            this.HDDocuments = new List<string>();
            this.HDVectors = null;
            this.SimilarityMetric = metric;
            this.Embedder = new HDEmbed();
        }

        /*
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
        */
        public void AddDocument(string document, Vector<double> vector = null) {
            if (vector == null) {
               // vector = EmbeddingFunction(new[] { document }).Row(0);
                vector = this.Embedder.GetVector(document);
            }

            if (HDVectors == null) {
                HDVectors = Matrix<double>.Build.Dense(0, vector.Count);
            } else if (vector.Count != HDVectors.ColumnCount) {
                throw new ArgumentException("All vectors must have the same length.");
            }

            HDVectors = HDVectors.InsertRow(HDVectors.RowCount, vector);
            HDDocuments.Add(document);
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
                HDVectors = Matrix<double>.Build.DenseOfRowArrays(((List<List<double>>)data["vectors"]).Select(row => row.ToArray()));
                HDDocuments = ((List<string>)data["documents"]);
            }
        }

        public List<string> Query(string queryText, int topK = 5) {
            //var queryVector = EmbeddingFunction(new[] { queryText }).Row(0);
            var queryVector = this.Embedder.GetVector(queryText);
            //TODO: FIX THIS
            var rankedResults = VectorOperations.HyperSvmRankingAlgorithmSort(HDVectors, queryVector, topK, SimilarityMetric);

            return rankedResults.Select(index => HDDocuments[index]).ToList();
        }

        // The GetEmbedding function would go here, but it's not possible to provide a direct C# equivalent
        // since it depends on the OpenAI API that isn't available for C#.
        // You can create your own implementation that uses a different text embedding library or API.
        private static Matrix<double> GetEmbedding(IEnumerable<string> documents, string key) {
            throw new NotImplementedException("Please implement the GetEmbedding method.");
        }
    }
    public enum SimilarityMetric {
        CosineSimilarity,
        EuclideanMetric,
        DerridaeanSimilarity
    }
}
