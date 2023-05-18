using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace HyperDBDotNet {

    public class VectorOperationsCPU {
        private Matrix<double> _vectors;
        private List<string> _documents;

        public VectorOperationsCPU(Matrix<double> HDVectors, List<string> HDDocuments) {
            this._vectors = HDVectors;
            this._documents = HDDocuments;
        }
        /*
       private Vector<double> GetNormVector(Vector<double> vector) {
           return vector / vector.L2Norm();
       }

       private Matrix<double> GetNormVector(Matrix<double> matrix) {
           var normMatrix = Matrix<double>.Build.Dense(matrix.RowCount, matrix.ColumnCount);
           Parallel.For(0, matrix.RowCount, i => {
               normMatrix.SetRow(i, GetNormVector(matrix.Row(i)));
           });
           return normMatrix;
       }
       */
        private void NormalizeInPlace(Vector<double> vector) {
            vector /= vector.L2Norm();
        }

        private void NormalizeInPlace(Matrix<double> matrix) {
            Parallel.For(0, matrix.RowCount, i =>
            {
                var row = matrix.Row(i);
                NormalizeInPlace(row);
                matrix.SetRow(i, row);
            });
        }
      /* public static Vector<double> CosineSimilarity(Matrix<double> vectors, Vector<double> queryVector) {
            var normVectors = GetNormVector(vectors);
            var normQueryVector = GetNormVector(queryVector);
            return normVectors * normQueryVector;
        }*/

        public Vector<double> CosineSimilarity(Vector<double> queryVector) {
            NormalizeInPlace(_vectors);
            NormalizeInPlace(queryVector);
            return _vectors * queryVector;
        }


        public int[] HyperSvmRankingAlgorithmSort(Vector<double> queryVector, int topK = 5, Func<Matrix<double>, Vector<double>, Vector<double>> metric = null) {
          //  if (metric == null) {
            //    metric = CosineSimilarity;
           // }

            var similarities = CosineSimilarity(queryVector);
            var sortedIndices = Enumerable.Range(0, similarities.Count).ToArray();
            Array.Sort(similarities.ToArray(), sortedIndices, Comparer<double>.Create((x, y) => y.CompareTo(x)));
            return sortedIndices.Take(topK).ToArray();
        }

        public void Save(string storageFile) {
            var data = new Dictionary<string, object> {
                ["vectors"] = _vectors.ToRowArrays().ToList(),
                ["documents"] = _documents
            };
            var json = JsonConvert.SerializeObject(data);
            using (var writer = new StreamWriter(storageFile)) {
                writer.Write(json);
            }
        }
        public int Load(string storageFile) {
            using (var reader = new StreamReader(storageFile)) {
                var json = reader.ReadToEnd();
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                _documents = ((JArray)data["documents"]).ToObject<List<string>>();

                var vectorsList = ((JArray)data["vectors"]).ToObject<List<List<double>>>();
                var rows = vectorsList.Count;
                var cols = vectorsList[0].Count;
                _vectors = Matrix<double>.Build.DenseOfRowArrays(vectorsList.Select(v => v.ToArray()).ToArray());
                return _vectors.RowCount;
            }
        }
        public void AddDocument(string document, Vector<double> vector) {
          

            if (_vectors == null) {
                _vectors = Matrix<double>.Build.Dense(0, vector.Count);
            } else if (vector.Count != _vectors.ColumnCount) {
                throw new ArgumentException("All vectors must have the same length.");
            }
            _vectors = _vectors.InsertRow(_vectors.RowCount, vector);
            _documents.Add(document);
        }
        public List<string> GetResults(int[] RankedResults) {
            return RankedResults.Select(index => _documents[index]).ToList();
        }
    }
}