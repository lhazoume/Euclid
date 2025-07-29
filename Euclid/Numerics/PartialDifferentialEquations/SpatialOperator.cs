using Euclid;

namespace Euclid.Numerics.PartialDifferentialEquations
{
    /// <summary>
    /// Represents the spatial discretization of a PDE, in the form dV/dt = L*V + d.
    /// This class can hold either a dense (Matrix) or a sparse (SparseMatrix) representation of the operator L.
    /// </summary>
    public class SpatialOperator
    {
        #region Properties

        #region Dense Representations (for legacy or small problems)
        /// <summary>The full operator matrix (L) as a dense matrix.</summary>
        public Matrix OperatorMatrix { get; }
        /// <summary>The matrix for the first spatial dimension (Lx) as a dense matrix.</summary>
        public Matrix OperatorMatrixX { get; }
        /// <summary>The matrix for the second spatial dimension (Ly) as a dense matrix.</summary>
        public Matrix OperatorMatrixY { get; }
        /// <summary>The matrix for the cross-derivative term (Lxy) as a dense matrix.</summary>
        public Matrix OperatorMatrixXY { get; }
        #endregion

        #region Sparse Representations (for performance on large grids)
        /// <summary>The full operator matrix (L) as a sparse matrix.</summary>
        public SparseMatrix SparseOperatorMatrix { get; }
        /// <summary>The matrix for the first spatial dimension (Lx) as a sparse matrix.</summary>
        public SparseMatrix SparseOperatorMatrixX { get; }
        /// <summary>The matrix for the second spatial dimension (Ly) as a sparse matrix.</summary>
        public SparseMatrix SparseOperatorMatrixY { get; }
        /// <summary>The matrix for the cross-derivative term (Lxy) as a sparse matrix.</summary>
        public SparseMatrix SparseOperatorMatrixXY { get; }
        #endregion

        #region Boundary Vectors
        /// <summary>The full constant vector part resulting from boundary conditions (d).</summary>
        public Vector BoundaryVector { get; }
        /// <summary>The boundary vector corresponding to the X operator.</summary>
        public Vector BoundaryVectorX { get; }
        /// <summary>The boundary vector corresponding to the Y operator.</summary>
        public Vector BoundaryVectorY { get; }
        /// <summary>The boundary vector corresponding to the XY operator.</summary>
        public Vector BoundaryVectorXY { get; }
        #endregion

        /// <summary>Lower bandwidth for the dense operator matrix, used in banded matrix solvers.</summary>
        public int LowerBandwidth { get; }
        /// <summary>Upper bandwidth for the dense operator matrix, used in banded matrix solvers.</summary>
        public int UpperBandwidth { get; }
        #endregion

        #region Constructors

        /// <summary>Builds a spatial operator using a dense matrix representation.</summary>
        public SpatialOperator(Matrix operatorMatrix, Vector boundaryVector, int lowerBandwidth = 0, int upperBandwidth = 0)
        {
            OperatorMatrix = operatorMatrix;
            BoundaryVector = boundaryVector;
            LowerBandwidth = lowerBandwidth;
            UpperBandwidth = upperBandwidth;
        }

        /// <summary>Builds a spatial operator using a sparse matrix representation.</summary>
        public SpatialOperator(SparseMatrix sparseOperatorMatrix, Vector boundaryVector, int lowerBandwidth = 0, int upperBandwidth = 0)
        {
            SparseOperatorMatrix = sparseOperatorMatrix;
            BoundaryVector = boundaryVector;
            LowerBandwidth = lowerBandwidth;
            UpperBandwidth = upperBandwidth;
        }
        // Note: Additional constructors for split sparse operators could be added here if needed.
        #endregion
    }
}