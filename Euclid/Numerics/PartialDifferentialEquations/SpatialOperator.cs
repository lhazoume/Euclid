using Euclid;

namespace Euclid.Numerics.PartialDifferentialEquations
{
    public sealed class SpatialOperator
    {
        // FULL
        public Matrix OperatorMatrixFull { get; }
        public Vector BoundaryVectorFull { get; }
        public SparseMatrix SparseOperatorMatrix { get; }
        public Vector BoundaryVector { get; }

        //Split (dense)
        public Matrix OperatorMatrixX { get; }
        public Matrix OperatorMatrixY { get; }
        public Matrix OperatorMatrixXY { get; }

        //  Split (sparse)
        public SparseMatrix SparseOperatorMatrixX { get; }
        public SparseMatrix SparseOperatorMatrixY { get; }
        public SparseMatrix SparseOperatorMatrixXY { get; }

        public double DiscountRate { get; }
        public int LowerBandwidth { get; }
        public int UpperBandwidth { get; }

        public SpatialOperator(SparseMatrix Lx_s, SparseMatrix Ly_s, SparseMatrix Lxy_s,Vector dx, Vector dy, Vector dxy,double rate, int lowerBandwidth, int upperBandwidth)
        {
            SparseOperatorMatrixX = Lx_s;
            SparseOperatorMatrixY = Ly_s;
            SparseOperatorMatrixXY = Lxy_s;

            DiscountRate = rate;
            LowerBandwidth = lowerBandwidth;
            UpperBandwidth = upperBandwidth;

            int n = Lx_s.Rows;
            SparseMatrix I_s = SparseMatrix.CreateIdentityMatrix(n);

            SparseOperatorMatrix = (Lx_s + Ly_s) + Lxy_s - (rate * I_s);
            BoundaryVector = dx + dy + dxy;

            OperatorMatrixFull = null;
            BoundaryVectorFull = null;
        }

        // Constructeur "FULL sparse"
        public SpatialOperator(SparseMatrix Lfull_s, Vector d_full, int lowerBandwidth = 0, int upperBandwidth = 0)
        {
            SparseOperatorMatrix = Lfull_s;
            BoundaryVector = d_full;
            LowerBandwidth = lowerBandwidth;
            UpperBandwidth = upperBandwidth;

            DiscountRate = 0.0;
            OperatorMatrixFull = null;
            BoundaryVectorFull = null;
        }

        // Constructeur "FULL dense" 
        public SpatialOperator(Matrix Lfull, Vector d_full, int lowerBandwidth, int upperBandwidth, double rate = 0.0)
        {
            OperatorMatrixFull = Lfull;
            BoundaryVectorFull = d_full;
            LowerBandwidth = lowerBandwidth;
            UpperBandwidth = upperBandwidth;
            DiscountRate = rate;

            SparseOperatorMatrix = null;
            BoundaryVector = null;
        }
    }
}
