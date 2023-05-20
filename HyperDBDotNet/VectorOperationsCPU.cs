using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace HyperDBDotNet {

    public class VectorOperationsCPU {
        private Matrix<double> _vectors;
        private List<string> _documents;
        private Matrix<double> _normvectors;
        private Dictionary<Vector<double>, int[]> _cache;
        private bool _isDirty = true;
        public VectorOperationsCPU(Matrix<double> HDVectors, List<string> HDDocuments) {
            this._vectors = HDVectors;
            this._documents = HDDocuments;
            this._cache = new Dictionary<Vector<double>, int[]>();
        }
       private Vector<double> GetNormVector(Vector<double> vector) {
           return vector / vector.L2Norm();
       }

       private Matrix<double> GetNormVector(Matrix<double> matrix) {
           var normMatrix = Matrix<double>.Build.Dense(matrix.RowCount, matrix.ColumnCount);
           Parallel.For(0, matrix.RowCount, i => {
               var norm = matrix.Row(i) / matrix.Row(i).L2Norm();
               normMatrix.SetRow(i, norm);
           });
           return normMatrix;
       }
       
       private Vector<double> CosineSimilarity(Vector<double> queryVector) {
            if (_isDirty) {
                _normvectors = GetNormVector(_vectors);
                _isDirty = false;
            }
            var normQueryVector = GetNormVector(queryVector);
            return _normvectors * normQueryVector;
        }

        public int[] HyperSvmRankingAlgorithmSort(Vector<double> queryVector, int topK = 5, Func<Matrix<double>, Vector<double>, Vector<double>> metric = null) {
          //  if (metric == null) {
            //    metric = CosineSimilarity;
           // }
           if (_cache.ContainsKey(queryVector)) {
                return _cache[queryVector];
            }

            var similarities = CosineSimilarity(queryVector);
            var sortedIndices = Enumerable.Range(0, similarities.Count).ToArray();
            Array.Sort(similarities.ToArray(), sortedIndices, Comparer<double>.Create((x, y) => y.CompareTo(x)));
            var results = sortedIndices.Take(topK).ToArray();
            _cache.Add(queryVector, results);
            return results;
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
                _isDirty = true;
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
            _isDirty = true;
        }
        public List<string> GetResults(int[] RankedResults) {
            return RankedResults.Select(index => _documents[index]).ToList();
        }
    }
}