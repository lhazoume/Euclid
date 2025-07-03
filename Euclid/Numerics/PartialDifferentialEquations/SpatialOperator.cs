using System;

namespace Euclid.Numerics.PartialDifferentialEquations
{
    /// <summary> Represents a spatial operator used in solving partial differential equations (PDEs).</summary>

    public class SpatialOperator
    {
        #region Properties
        /// <summary>The full operator matrix resulting from boundary conditions (L).</summary>
        public Matrix OperatorMatrix { get; }
        /// <summary>The full constant vector part resulting from boundary conditions (d).</summary>
        public Vector BoundaryVector { get; }

        /// <summary>The matrix for the first spatial dimension .</summary>
        public Matrix OperatorMatrixX { get; }
        /// <summary>The matrix for the second spatial dimension .</summary>
        public Matrix OperatorMatrixY { get; }
        /// <summary>The matrix for the cross-derivative term.</summary>
        public Matrix OperatorMatrixXY { get; }
        /// <summary>The boundary vector corresponding to the X operator.</summary>
        public Vector BoundaryVectorX { get; }
        /// <summary>The boundary vector corresponding to the Y operator.</summary>
        public Vector BoundaryVectorY { get; }
        /// <summary>The boundary vector corresponding to the XY operator.</summary>
        public Vector BoundaryVectorXY { get; }
        /// <summary>Lower bandwidth for the operator matrix, used in banded matrix solvers.</summary>
        public int LowerBandwidth { get; }
        /// <summary>Upper bandwidth for the operator matrix, used in banded matrix solvers.</summary>
        public int UpperBandwidth { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Builds a combined spatial operator
        /// </summary>
        /// <param name="operatorMatrix">The combined operator matrix L.</param>
        /// <param name="boundaryVector">The combined boundary vector d.</param>
        /// <param name="lowerBandwidth">Lower bandwidth for the operator matrix, used in banded matrix solvers.</param>
        /// <param name="upperBandwidth">Upper bandwidth for the operator matrix, used in banded matrix solvers.</param>
        public SpatialOperator(Matrix operatorMatrix, Vector boundaryVector, int lowerBandwidth = 0, int upperBandwidth = 0)
        {
            OperatorMatrix = operatorMatrix ?? throw new ArgumentNullException(nameof(operatorMatrix));
            BoundaryVector = boundaryVector ?? throw new ArgumentNullException(nameof(boundaryVector));
            LowerBandwidth = lowerBandwidth;
            UpperBandwidth = upperBandwidth;
        }
        /// <summary>
        /// Builds a fully split spatial operator. L = Lx + Ly + Lxy </summary>
        /// <param name="operatorMatrixX">The matrix for the first spatial dimension (Lx).</param>
        /// <param name="operatorMatrixY">The matrix for the second spatial dimension (Ly).</param>
        /// <param name="operatorMatrixXY">The matrix for the cross-derivative term (Lxy).</param>
        /// <param name="boundaryVectorX">The boundary vector corresponding to the X operator.</param>
        /// <param name="boundaryVectorY">The boundary vector corresponding to the Y operator.</param>
        /// <param name="boundaryVectorXY">The boundary vector corresponding to the XY operator.</param>
        public SpatialOperator(Matrix operatorMatrixX, Matrix operatorMatrixY, Matrix operatorMatrixXY,Vector boundaryVectorX, Vector boundaryVectorY, Vector boundaryVectorXY)
        {
            OperatorMatrixX = operatorMatrixX ?? throw new ArgumentNullException(nameof(operatorMatrixX));
            OperatorMatrixY = operatorMatrixY ?? throw new ArgumentNullException(nameof(operatorMatrixY));
            OperatorMatrixXY = operatorMatrixXY ?? throw new ArgumentNullException(nameof(operatorMatrixXY));
            BoundaryVectorX = boundaryVectorX ?? throw new ArgumentNullException(nameof(boundaryVectorX));
            BoundaryVectorY = boundaryVectorY ?? throw new ArgumentNullException(nameof(boundaryVectorY));
            BoundaryVectorXY = boundaryVectorXY ?? throw new ArgumentNullException(nameof(boundaryVectorXY));
        }
        #endregion
    }
}
