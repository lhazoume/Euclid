using System;

namespace Euclid.Numerics.PartialDifferentialEquations
{
    /// <summary> 
    /// Represents a spatial operator used in solving partial differential equations (PDEs).
    /// Can represent a combined operator or a split operator for methods like ADI.
    /// </summary>
    public class SpatialOperator
    {
        #region Properties
        // --- Dimensions ---
        /// <summary>The number of columns in the internal grid (width).</summary>
        public int InnerGridM { get; }
        /// <summary>The number of rows in the internal grid (height).</summary>
        public int InnerGridN { get; }

        // --- Combined Operator ---
        /// <summary>The full operator matrix resulting from boundary conditions (L).</summary>
        public Matrix OperatorMatrix { get; }
        /// <summary>The full constant vector part resulting from boundary conditions (d).</summary>
        public Vector BoundaryVector { get; }

        // --- Split Operator Components ---
        /// <summary>The matrix for the first spatial dimension (Lx).</summary>
        public Matrix OperatorMatrixX { get; }
        /// <summary>The matrix for the second spatial dimension (Ly).</summary>
        public Matrix OperatorMatrixY { get; }
        /// <summary>The matrix for the cross-derivative term (Lxy).</summary>
        public Matrix OperatorMatrixXY { get; }
        /// <summary>The boundary vector corresponding to the X operator.</summary>
        public Vector BoundaryVectorX { get; }
        /// <summary>The boundary vector corresponding to the Y operator.</summary>
        public Vector BoundaryVectorY { get; }
        /// <summary>The boundary vector corresponding to the XY operator.</summary>
        public Vector BoundaryVectorXY { get; }

        // --- Solver Hints ---
        /// <summary>Lower bandwidth for the combined operator matrix, used in banded matrix solvers.</summary>
        public int LowerBandwidth { get; }
        /// <summary>Upper bandwidth for the combined operator matrix, used in banded matrix solvers.</summary>
        public int UpperBandwidth { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Builds a combined spatial operator.
        /// </summary>
        /// <param name="operatorMatrix">The combined operator matrix L.</param>
        /// <param name="boundaryVector">The combined boundary vector d.</param>
        /// <param name="lowerBandwidth">Lower bandwidth for the operator matrix.</param>
        /// <param name="upperBandwidth">Upper bandwidth for the operator matrix.</param>
        public SpatialOperator(Matrix operatorMatrix, Vector boundaryVector, int lowerBandwidth, int upperBandwidth)
        {
            OperatorMatrix = operatorMatrix ?? throw new ArgumentNullException(nameof(operatorMatrix));
            BoundaryVector = boundaryVector ?? throw new ArgumentNullException(nameof(boundaryVector));
            LowerBandwidth = lowerBandwidth;
            UpperBandwidth = upperBandwidth;

            // Pour un opérateur combiné, les dimensions de la grille interne ne sont pas toujours évidentes.
            // On peut les laisser à 0 ou essayer de les déduire si nécessaire.
            InnerGridM = 0;
            InnerGridN = 0;
        }

        /// <summary>
        /// Builds a fully split spatial operator. L = Lx + Ly + Lxy 
        /// </summary>
        /// <param name="operatorMatrixX">The matrix for the first spatial dimension (Lx).</param>
        /// <param name="operatorMatrixY">The matrix for the second spatial dimension (Ly).</param>
        /// <param name="operatorMatrixXY">The matrix for the cross-derivative term (Lxy).</param>
        /// <param name="boundaryVectorX">The boundary vector corresponding to the X operator.</param>
        /// <param name="boundaryVectorY">The boundary vector corresponding to the Y operator.</param>
        /// <param name="boundaryVectorXY">The boundary vector corresponding to the XY operator.</param>
        /// <param name="innerM">The number of columns in the internal grid.</param>
        /// <param name="innerN">The number of rows in the internal grid.</param>
        public SpatialOperator(Matrix operatorMatrixX, Matrix operatorMatrixY, Matrix operatorMatrixXY,
                               Vector boundaryVectorX, Vector boundaryVectorY, Vector boundaryVectorXY,
                               int innerM, int innerN)
        {
            OperatorMatrixX = operatorMatrixX ?? throw new ArgumentNullException(nameof(operatorMatrixX));
            OperatorMatrixY = operatorMatrixY ?? throw new ArgumentNullException(nameof(operatorMatrixY));
            OperatorMatrixXY = operatorMatrixXY ?? throw new ArgumentNullException(nameof(operatorMatrixXY));
            BoundaryVectorX = boundaryVectorX ?? throw new ArgumentNullException(nameof(boundaryVectorX));
            BoundaryVectorY = boundaryVectorY ?? throw new ArgumentNullException(nameof(boundaryVectorY));
            BoundaryVectorXY = boundaryVectorXY ?? throw new ArgumentNullException(nameof(boundaryVectorXY));

            InnerGridM = innerM > 0 ? innerM : throw new ArgumentOutOfRangeException(nameof(innerM));
            InnerGridN = innerN > 0 ? innerN : throw new ArgumentOutOfRangeException(nameof(innerN));

            if (operatorMatrixX.Rows != innerM * innerN)
                throw new ArgumentException("Matrix dimensions do not match inner grid dimensions.");

            // Le constructeur combine les parties pour la compatibilité avec les anciens schémas
            OperatorMatrix = OperatorMatrixX + OperatorMatrixY + OperatorMatrixXY;
            BoundaryVector = BoundaryVectorX + BoundaryVectorY + BoundaryVectorXY;

            // Pour le combined operator, la bande est large
            LowerBandwidth = innerM;
            UpperBandwidth = innerM;
        }
        #endregion
    }
}