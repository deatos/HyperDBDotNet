using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace HyperDBDotNet {
    public static class VectorOperations {
        public static Vector<double> GetNormVector(Vector<double> vector) {
            return vector / vector.L2Norm();
        }

        public static Matrix<double> GetNormVector(Matrix<double> matrix) {
            var normMatrix = matrix.Clone();
            for (int i = 0; i < matrix.RowCount; i++) {
                normMatrix.SetRow(i, GetNormVector(matrix.Row(i)));
            }
            return normMatrix;
        }

        public static Vector<double> CosineSimilarity(Matrix<double> vectors, Vector<double> queryVector) {
            var normVectors = GetNormVector(vectors);
            var normQueryVector = GetNormVector(queryVector);
            return normVectors * normQueryVector;
        }

        public static Vector<double> EuclideanMetric(Matrix<double> vectors, Vector<double> queryVector) {
            return EuclideanMetric(vectors, queryVector, true);
        }
        public static Vector<double> EuclideanMetric(Matrix<double> vectors, Vector<double> queryVector, bool getSimilarityScore = true) {
            var rowDiffNorms = Vector<double>.Build.Dense(vectors.RowCount);

            for (int i = 0; i < vectors.RowCount; i++) {
                var rowDiff = vectors.Row(i) - queryVector;
                rowDiffNorms[i] = rowDiff.L2Norm();
            }

            if (getSimilarityScore) {
                rowDiffNorms = 1 / (1 + rowDiffNorms);
            }

            return rowDiffNorms;
        }

        public static Vector<double> DerridaeanSimilarity(Matrix<double> vectors, Vector<double> queryVector) {
            var random = new Random();
            Func<double, double> randomChange = value => value + random.NextDouble() * 0.4 - 0.2;

            var similarities = CosineSimilarity(vectors, queryVector);
            return similarities.Map(randomChange);
        }

        public static int[] HyperSvmRankingAlgorithmSort(Matrix<double> vectors, Vector<double> queryVector, int topK = 5, Func<Matrix<double>, Vector<double>, Vector<double>> metric = null) {
            if (metric == null) {
                metric = CosineSimilarity;
            }

            var similarities = metric(vectors, queryVector);
            var sortedIndices = Enumerable.Range(0, similarities.Count).ToArray();
            Array.Sort(similarities.ToArray(), sortedIndices, Comparer<double>.Create((x, y) => y.CompareTo(x)));
            return sortedIndices.Take(topK).ToArray();
        }
    }
}