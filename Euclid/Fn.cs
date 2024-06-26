﻿using System;

namespace Euclid
{
    /*
**************************************************************************
**
**    Class  SpecialFunction (C#)
**
**************************************************************************
**    Copyright (C) 1984 Stephen L. Moshier (original C version - Cephes Math Library)
**    Copyright (C) 1996 Leigh Brookshaw	(Java version)
**    Copyright (C) 2005 Miroslav Stampar	(C# version [->this<-])
**
**    This program is free software; you can redistribute it and/or modify
**    it under the terms of the GNU General Public License as published by
**    the Free Software Foundation; either version 2 of the License, or
**    (at your option) any later version.
**
**    This program is distributed in the hope that it will be useful,
**    but WITHOUT ANY WARRANTY; without even the implied warranty of
**    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
**    GNU General Public License for more details.
**
**    You should have received a copy of the GNU General Public License
**    along with this program; if not, write to the Free Software
**    Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
**************************************************************************
**
**    This class is an extension of System.Math. It includes a number
**    of special functions not found in the Math class.
**
*************************************************************************/


    /**
	 * This class contains physical constants and special functions not found
	 * in the System.Math class.
	 * Like the System.Math class this class is final and cannot be
	 * subclassed.
	 * All physical constants are in cgs units.
	 * NOTE: These special functions do not necessarily use the fastest
	 * or most accurate algorithms.
	 *
	 * @version $Revision: 1.8 $, $Date: 2005/09/12 09:52:34 $
	 */
    public static class Fn
    {
        // Machine constants
        private const double MACHEP = 1.11022302462515654042E-16;
        private const double MAXLOG = 7.09782712893383996732E2;
        private const double MINLOG = -7.451332191019412076235E2;
        private const double MAXGAM = 171.624376956302725;
        private const double SQTPI = 2.50662827463100050242E0;
        private const double LOGPI = 1.14472988584940017414;

        /// <summary>The Euler-Mascheroni constant</summary>
        /// <remarks>lim(n -> inf){ Sum(k=1 -> n) { 1/k - log(n) } }</remarks>
        public const double EulerGamma = 0.5772156649015328606065120900824024310421593359399235988057672348849d;

        #region Physical Constants in cgs Units

        /// <summary>Boltzman Constant. Units erg/deg(K)</summary>
        public const double BOLTZMAN = 1.3807e-16;

        /// <summary>Elementary Charge. Units statcoulomb</summary>
        public const double ECHARGE = 4.8032e-10;

        /// <summary>Electron Mass. Units g</summary>
        public const double EMASS = 9.1095e-28;

        /// <summary>
        /// Proton Mass. Units g 
        /// </summary>
        public const double PMASS = 1.6726e-24;

        /// <summary>Gravitational Constant. Units dyne-cm^2/g^2</summary>
        public const double GRAV = 6.6720e-08;

        /// <summary>
        /// Planck constant. Units erg-sec 
        /// </summary>
        public const double PLANCK = 6.6262e-27;

        /// <summary> Speed of Light in a Vacuum. Units cm/sec </summary>
        public const double LIGHTSPEED = 2.9979e10;

        /// <summary>
        /// Stefan-Boltzman Constant. Units erg/cm^2-sec-deg^4 
        /// </summary>
        public const double STEFANBOLTZ = 5.6703e-5;

        /// <summary>
        /// Avogadro Number. Units  1/mol 
        /// </summary>
        public const double AVOGADRO = 6.0220e23;

        /// <summary>
        /// Gas Constant. Units erg/deg-mol 
        /// </summary>
        public const double GASCONSTANT = 8.3144e07;

        /// <summary>
        /// Gravitational Acceleration at the Earths surface. Units cm/sec^2 
        /// </summary>
        public const double GRAVACC = 980.67;

        /// <summary>
        /// Solar Mass. Units g 
        /// </summary>
        public const double SOLARMASS = 1.99e33;

        /// <summary>
        /// Solar Radius. Units cm
        /// </summary>
        public const double SOLARRADIUS = 6.96e10;

        /// <summary>
        /// Solar Luminosity. Units erg/sec
        /// </summary>
        public const double SOLARLUM = 3.90e33;

        /// <summary>
        /// Solar Flux. Units erg/cm^2-sec
        /// </summary>
        public const double SOLARFLUX = 6.41e10;

        /// <summary>
        /// Astronomical Unit (radius of the Earth's orbit). Units cm
        /// </summary>
        public const double AU = 1.50e13;
        #endregion

        private const double ACC = 40.0;
        private const double BIGNO = 1.0e10;
        private const double BIGNI = 1.0e-10;

        // Function Methods

        /// <summary>
        /// More precise way of calculating the norm
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>the norm</returns>
        public static double Norm(double a, double b)
        {
            double r;
            if (Math.Abs(a) > Math.Abs(b))
            {
                r = b / a;
                return Math.Abs(a) * Math.Sqrt(1 + r * r);
            }
            r = a / b;
            return Math.Abs(b) * Math.Sqrt(1 + r * r);
        }

        #region Hyperbolic arc functions

        /// <summary>
        /// Returns the hyperbolic arc cosine of the specified number.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Acosh(double x)
        {
            if (x < 1.0) throw new ArithmeticException("range exception");
            return Math.Log(x + Math.Sqrt(x * x - 1));
        }

        /// <summary>
        /// Returns the hyperbolic arc sine of the specified number.
        /// </summary>
        /// <param name="xx"></param>
        /// <returns></returns>
        public static double Asinh(double xx)
        {
            double x;
            int sign;
            if (xx == 0.0) return xx;
            if (xx < 0.0)
            {
                sign = -1;
                x = -xx;
            }
            else
            {
                sign = 1;
                x = xx;
            }
            return sign * Math.Log(x + Math.Sqrt(x * x + 1));
        }

        /// <summary>
        /// Returns the hyperbolic arc tangent of the specified number.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Atanh(double x)
        {
            if (x > 1.0 || x < -1.0)
                throw
                    new ArithmeticException("range exception");
            return 0.5 * Math.Log((1.0 + x) / (1.0 - x));
        }

        #endregion

        #region Bessel functions

        #region Bessel J Functions
        /// <summary>Returns the Bessel function of order 0 of the specified number.</summary>
        /// <param name="x"></param>
        /// <returns>a double</returns>
        public static double j0(double x)
        {
            double ax;

            if ((ax = Math.Abs(x)) < 8.0)
            {
                double y = x * x;
                double ans1 = 57568490574.0 + y * (-13362590354.0 + y * (651619640.7
                    + y * (-11214424.18 + y * (77392.33017 + y * (-184.9052456)))));
                double ans2 = 57568490411.0 + y * (1029532985.0 + y * (9494680.718
                    + y * (59272.64853 + y * (267.8532712 + y * 1.0))));

                return ans1 / ans2;

            }
            else
            {
                double z = 8.0 / ax;
                double y = z * z;
                double xx = ax - 0.785398164;
                double ans1 = 1.0 + y * (-0.1098628627e-2 + y * (0.2734510407e-4
                    + y * (-0.2073370639e-5 + y * 0.2093887211e-6)));
                double ans2 = -0.1562499995e-1 + y * (0.1430488765e-3
                    + y * (-0.6911147651e-5 + y * (0.7621095161e-6
                    - y * 0.934935152e-7)));

                return Math.Sqrt(0.636619772 / ax) *
                    (Math.Cos(xx) * ans1 - z * Math.Sin(xx) * ans2);
            }
        }

        /// <summary>
        /// Returns the Bessel function of order 1 of the specified number.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double j1(double x)
        {
            double ax;
            double y;
            double ans1, ans2;

            if ((ax = Math.Abs(x)) < 8.0)
            {
                y = x * x;
                ans1 = x * (72362614232.0 + y * (-7895059235.0 + y * (242396853.1
                    + y * (-2972611.439 + y * (15704.48260 + y * (-30.16036606))))));
                ans2 = 144725228442.0 + y * (2300535178.0 + y * (18583304.74
                    + y * (99447.43394 + y * (376.9991397 + y * 1.0))));
                return ans1 / ans2;
            }
            else
            {
                double z = 8.0 / ax;
                double xx = ax - 2.356194491;
                y = z * z;

                ans1 = 1.0 + y * (0.183105e-2 + y * (-0.3516396496e-4
                    + y * (0.2457520174e-5 + y * (-0.240337019e-6))));
                ans2 = 0.04687499995 + y * (-0.2002690873e-3
                    + y * (0.8449199096e-5 + y * (-0.88228987e-6
                    + y * 0.105787412e-6)));
                double ans = Math.Sqrt(0.636619772 / ax) *
                    (Math.Cos(xx) * ans1 - z * Math.Sin(xx) * ans2);
                if (x < 0.0) ans = -ans;
                return ans;
            }
        }

        /// <summary> Returns the Bessel function of order n of the specified number.</summary>
        /// <param name="n"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double jn(int n, double x)
        {
            int j, m;
            double ax, bj, bjm, bjp, sum, tox, ans;
            bool jsum;

            double ACC = 40.0;
            double BIGNO = 1.0e+10;
            double BIGNI = 1.0e-10;

            if (n == 0) return j0(x);
            if (n == 1) return j1(x);

            ax = Math.Abs(x);
            if (ax == 0.0) return 0.0;
            else if (ax > (double)n)
            {
                tox = 2.0 / ax;
                bjm = j0(ax);
                bj = j1(ax);
                for (j = 1; j < n; j++)
                {
                    bjp = j * tox * bj - bjm;
                    bjm = bj;
                    bj = bjp;
                }
                ans = bj;
            }
            else
            {
                tox = 2.0 / ax;
                m = 2 * ((n + (int)Math.Sqrt(ACC * n)) / 2);
                jsum = false;
                bjp = ans = sum = 0.0;
                bj = 1.0;
                for (j = m; j > 0; j--)
                {
                    bjm = j * tox * bj - bjp;
                    bjp = bj;
                    bj = bjm;
                    if (Math.Abs(bj) > BIGNO)
                    {
                        bj *= BIGNI;
                        bjp *= BIGNI;
                        ans *= BIGNI;
                        sum *= BIGNI;
                    }
                    if (jsum) sum += bj;
                    jsum = !jsum;
                    if (j == n) ans = bjp;
                }
                sum = 2.0 * sum - bj;
                ans /= sum;
            }
            return x < 0.0 && n % 2 == 1 ? -ans : ans;
        }
        #endregion

        #region Bessel I Functions

        /// <summary>Returns the modified Bessel function of first type and order 0 of the specified number.</summary>
        /// <param name="x"></param>
        /// <returns>a double</returns>
        public static double i0(double x)
        {
            double ax, ans;
            double y;


            if ((ax = Math.Abs(x)) < 3.75)
            {
                y = x / 3.75;
                y *= y;
                ans = 1.0 + y * (3.5156229 + y * (3.0899424 + y * (1.2067492
                   + y * (0.2659732 + y * (0.360768e-1 + y * 0.45813e-2)))));
            }
            else
            {
                y = 3.75 / ax;
                ans = (Math.Exp(ax) / Math.Sqrt(ax)) * (0.39894228 + y * (0.1328592e-1
                   + y * (0.225319e-2 + y * (-0.157565e-2 + y * (0.916281e-2
                   + y * (-0.2057706e-1 + y * (0.2635537e-1 + y * (-0.1647633e-1
                   + y * 0.392377e-2))))))));
            }
            return ans;
        }

        /// <summary>Returns the modified Bessel function of first type and order 1 of the specified number.</summary>
        /// <param name="x">the argument</param>
        /// <returns>a double</returns>
        public static double i1(double x)
        {
            double ax, ans;
            double y;


            if ((ax = Math.Abs(x)) < 3.75)
            {
                y = x / 3.75;
                y *= y;
                ans = ax * (0.5 + y * (0.87890594 + y * (0.51498869 + y * (0.15084934
                   + y * (0.2658733e-1 + y * (0.301532e-2 + y * 0.32411e-3))))));
            }
            else
            {
                y = 3.75 / ax;
                ans = 0.2282967e-1 + y * (-0.2895312e-1 + y * (0.1787654e-1
                   - y * 0.420059e-2));
                ans = 0.39894228 + y * (-0.3988024e-1 + y * (-0.362018e-2
                   + y * (0.163801e-2 + y * (-0.1031555e-1 + y * ans))));
                ans *= (Math.Exp(ax) / Math.Sqrt(ax));
            }
            return x < 0.0 ? -ans : ans;
        }

        /// <summary>Returns the modified Bessel function of first type and any order of the specified number.</summary>
        /// <param name="x">the argument</param>
        /// <param name="n">the order</param>
        /// <returns>a double</returns>
        public static double ik(int n, double x)
        {

            if (n == 0)
                return i0(x);
            if (n == 1)
                return i1(x);

            int j;
            double bi, bim, bip, tox, ans;

            if (x == 0.0)
                return 0.0;
            else
            {
                tox = 2.0 / Math.Abs(x);
                bip = ans = 0.0;
                bi = 1.0;
                for (j = 2 * (n + (int)Math.Sqrt(ACC * n)); j > 0; j--)
                {
                    bim = bip + j * tox * bi;
                    bip = bi;
                    bi = bim;
                    if (Math.Abs(bi) > BIGNO)
                    {
                        ans *= BIGNI;
                        bi *= BIGNI;
                        bip *= BIGNI;
                    }
                    if (j == n) ans = bip;
                }
                ans *= i0(x) / bi;
                return x < 0.0 && n % 2 == 1 ? -ans : ans;
            }
        }

        #endregion

        #region Bessel Y Functions

        /// <summary>Returns the Bessel function of the second kind, of order 0 of the specified number.</summary>
        /// <param name="x">the argument</param>
        /// <returns>a double</returns>
        public static double y0(double x)
        {
            if (x < 8.0)
            {
                double y = x * x;

                double ans1 = -2957821389.0 + y * (7062834065.0 + y * (-512359803.6
                    + y * (10879881.29 + y * (-86327.92757 + y * 228.4622733))));
                double ans2 = 40076544269.0 + y * (745249964.8 + y * (7189466.438
                    + y * (47447.26470 + y * (226.1030244 + y * 1.0))));

                return (ans1 / ans2) + 0.636619772 * j0(x) * Math.Log(x);
            }
            else
            {
                double z = 8.0 / x;
                double y = z * z;
                double xx = x - 0.785398164;

                double ans1 = 1.0 + y * (-0.1098628627e-2 + y * (0.2734510407e-4
                    + y * (-0.2073370639e-5 + y * 0.2093887211e-6)));
                double ans2 = -0.1562499995e-1 + y * (0.1430488765e-3
                    + y * (-0.6911147651e-5 + y * (0.7621095161e-6
                    + y * (-0.934945152e-7))));
                return Math.Sqrt(0.636619772 / x) *
                    (Math.Sin(xx) * ans1 + z * Math.Cos(xx) * ans2);
            }
        }

        /// <summary>Returns the Bessel function of the second kind, of order 1 of the specified number.</summary>
        /// <param name="x">the argument</param>
        /// <returns>a double</returns>
        public static double y1(double x)
        {
            if (x < 8.0)
            {
                double y = x * x;
                double ans1 = x * (-0.4900604943e13 + y * (0.1275274390e13
                    + y * (-0.5153438139e11 + y * (0.7349264551e9
                    + y * (-0.4237922726e7 + y * 0.8511937935e4)))));
                double ans2 = 0.2499580570e14 + y * (0.4244419664e12
                    + y * (0.3733650367e10 + y * (0.2245904002e8
                    + y * (0.1020426050e6 + y * (0.3549632885e3 + y)))));
                return (ans1 / ans2) + 0.636619772 * (j1(x) * Math.Log(x) - 1.0 / x);
            }
            else
            {
                double z = 8.0 / x;
                double y = z * z;
                double xx = x - 2.356194491;
                double ans1 = 1.0 + y * (0.183105e-2 + y * (-0.3516396496e-4
                    + y * (0.2457520174e-5 + y * (-0.240337019e-6))));
                double ans2 = 0.04687499995 + y * (-0.2002690873e-3
                    + y * (0.8449199096e-5 + y * (-0.88228987e-6
                    + y * 0.105787412e-6)));
                return Math.Sqrt(0.636619772 / x) *
                    (Math.Sin(xx) * ans1 + z * Math.Cos(xx) * ans2);
            }
        }

        /// <summary>Returns the Bessel function of the second kind, of order n of the specified number.</summary>
        /// <param name="n">the order</param>
        /// <param name="x">the argument</param>
        /// <returns>a double</returns>
        public static double yn(int n, double x)
        {
            double by, bym, byp, tox;

            if (n == 0) return y0(x);
            if (n == 1) return y1(x);

            tox = 2.0 / x;
            by = y1(x);
            bym = y0(x);
            for (int j = 1; j < n; j++)
            {
                byp = j * tox * by - bym;
                bym = by;
                by = byp;
            }
            return by;
        }

        #endregion

        #endregion

        #region Gamma functions

        /// <summary>
        /// Returns the gamma function of the specified number.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Gamma(double x)
        {
            double[] P = {
                         1.60119522476751861407E-4,
                         1.19135147006586384913E-3,
                         1.04213797561761569935E-2,
                         4.76367800457137231464E-2,
                         2.07448227648435975150E-1,
                         4.94214826801497100753E-1,
                         9.99999999999999996796E-1
                     };
            double[] Q = {
                         -2.31581873324120129819E-5,
                         5.39605580493303397842E-4,
                         -4.45641913851797240494E-3,
                         1.18139785222060435552E-2,
                         3.58236398605498653373E-2,
                         -2.34591795718243348568E-1,
                         7.14304917030273074085E-2,
                         1.00000000000000000320E0
                     };

            double p, z;

            double q = Math.Abs(x);

            if (q > 33.0)
            {
                if (x < 0.0)
                {
                    p = Math.Floor(q);
                    if (p == q) throw new ArithmeticException("gamma: overflow");
                    //int i = (int)p;
                    z = q - p;
                    if (z > 0.5)
                    {
                        p += 1.0;
                        z = q - p;
                    }
                    z = q * Math.Sin(Math.PI * z);
                    if (z == 0.0) throw new ArithmeticException("gamma: overflow");
                    z = Math.Abs(z);
                    z = Math.PI / (z * stirf(q));

                    return -z;
                }
                else
                {
                    return stirf(x);
                }
            }

            z = 1.0;
            while (x >= 3.0)
            {
                x -= 1.0;
                z *= x;
            }

            while (x < 0.0)
            {
                if (x == 0.0)
                {
                    throw new ArithmeticException("gamma: singular");
                }
                else if (x > -1.0E-9)
                {
                    return (z / ((1.0 + 0.5772156649015329 * x) * x));
                }
                z /= x;
                x += 1.0;
            }

            while (x < 2.0)
            {
                if (x == 0.0)
                {
                    throw new ArithmeticException("gamma: singular");
                }
                else if (x < 1.0E-9)
                {
                    return (z / ((1.0 + 0.5772156649015329 * x) * x));
                }
                z /= x;
                x += 1.0;
            }

            if ((x == 2.0) || (x == 3.0)) return z;

            x -= 2.0;
            p = EvaluatePolynomial(x, P, 6);
            q = EvaluatePolynomial(x, Q, 7);
            return z * p / q;

        }

        /// <summary>
        /// Returns the complemented incomplete gamma function.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double IncompleteUpperGamma(double a, double x)
        {
            double big = 4.503599627370496e15;
            double biginv = 2.22044604925031308085e-16;
            double ans, ax, c, yc, r, t, y, z;
            double pk, pkm1, pkm2, qk, qkm1, qkm2;

            if (x <= 0 || a <= 0) return 1.0;

            if (x < 1.0 || x < a) return 1.0 - IncompleteLowerGamma(a, x);

            ax = a * Math.Log(x) - x - lgamma(a);
            if (ax < -MAXLOG) return 0.0;

            ax = Math.Exp(ax);

            /* continued fraction */
            y = 1.0 - a;
            z = x + y + 1.0;
            c = 0.0;
            pkm2 = 1.0;
            qkm2 = x;
            pkm1 = x + 1.0;
            qkm1 = z * x;
            ans = pkm1 / qkm1;

            do
            {
                c += 1.0;
                y += 1.0;
                z += 2.0;
                yc = y * c;
                pk = pkm1 * z - pkm2 * yc;
                qk = qkm1 * z - qkm2 * yc;
                if (qk != 0)
                {
                    r = pk / qk;
                    t = Math.Abs((ans - r) / r);
                    ans = r;
                }
                else
                    t = 1.0;

                pkm2 = pkm1;
                pkm1 = pk;
                qkm2 = qkm1;
                qkm1 = qk;
                if (Math.Abs(pk) > big)
                {
                    pkm2 *= biginv;
                    pkm1 *= biginv;
                    qkm2 *= biginv;
                    qkm1 *= biginv;
                }
            } while (t > MACHEP);

            return ans * ax;
        }

        /// <summary>Returns the incomplete gamma function.</summary>
        /// <param name="a"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double IncompleteLowerGamma(double a, double x)
        {
            double ans, ax, c, r;

            if (x <= 0 || a <= 0) return 0.0;

            if (x > 1.0 && x > a) return 1.0 - IncompleteUpperGamma(a, x);

            /* Compute  x**a * exp(-x) / gamma(a)  */
            ax = a * Math.Log(x) - x - lgamma(a);
            if (ax < -MAXLOG) return (0.0);

            ax = Math.Exp(ax);

            /* power series */
            r = a;
            c = 1.0;
            ans = 1.0;

            do
            {
                r += 1.0;
                c *= x / r;
                ans += c;
            } while (c / ans > MACHEP);

            return (ans * ax / a);

        }

        /// <summary>
        /// Returns the digamma (psi) function of real values (except at 0, -1, -2, ...).
        /// Digamma is the logarithmic derivative of the <see cref="Gamma"/> function.
        /// </summary>
        public static double DiGamma(double x)
        {
            double y;
            double nz = 0.0;
            bool negative = (x <= 0);

            if (negative)
            {
                double q = x;
                double p = Math.Floor(q);
                negative = true;

                if (Math.Abs(p - q) < 1E-9)
                    return double.NaN; // singularity, undefined

                nz = q - p;

                if (nz != 0.5)
                {
                    if (nz > 0.5)
                    {
                        p += 1.0;
                        nz = q - p;
                    }

                    nz = Math.PI / Math.Tan(Math.PI * nz);
                }
                else
                    nz = 0.0;

                x = 1.0 - x;
            }

            if ((x <= 10.0) && (x == Math.Floor(x)))
            {
                y = 0.0;
                int n = (int)Math.Floor(x);
                for (int i = 1; i <= n - 1; i++)
                    y += 1.0 / i;

                y -= EulerGamma;
            }
            else
            {
                double s = x;
                double w = 0.0;

                while (s < 10.0)
                {
                    w += 1.0 / s;
                    s += 1.0;
                }

                if (s < 1.0e17)
                {
                    double z = 1.0 / (s * s);
                    double polv = 8.33333333333333333333e-2;
                    polv = polv * z - 2.10927960927960927961e-2;
                    polv = polv * z + 7.57575757575757575758e-3;
                    polv = polv * z - 4.16666666666666666667e-3;
                    polv = polv * z + 3.96825396825396825397e-3;
                    polv = polv * z - 8.33333333333333333333e-3;
                    polv = polv * z + 8.33333333333333333333e-2;
                    y = z * polv;
                }
                else
                    y = 0.0;

                y = Math.Log(s) - 0.5 / s - y - w;
            }

            if (negative) return y - nz;

            return y;
        }

        #endregion

        #region Beta functions

        /// <summary>
        /// Returns the beta function
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double Beta(double x, double y)
        {
            return Gamma(x) * Gamma(y) / Gamma(x + y);
        }

        /// <summary>
        /// Return the incomplete regularized beta function
        /// </summary>
        /// <param name="t">the integral's upper bound</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double IncompleteRegularizedBeta(double t, double x, double y)
        {
            return IncompleteBeta(x, y, t) / Beta(x, y);
        }

        /// <summary>
        /// Returns the incomplete beta function evaluated from zero to T.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static double IncompleteBeta(double x, double y, double t)
        {
            double a_, b_, t_, x_, xc, w, y_;
            bool flag;

            if (x <= 0.0 || y <= 0.0)
                throw new
                    ArithmeticException("ibeta: Domain error!");

            if ((t <= 0.0) || (t >= 1.0))
            {
                if (t == 0.0) return 0.0;
                if (t == 1.0) return 1.0;
                throw new ArithmeticException("ibeta: Domain error!");
            }

            flag = false;
            if ((y * t) <= 1.0 && t <= 0.95)
            {
                t_ = PowerSeries(x, y, t);
                return t_;
            }

            w = 1.0 - t;

            /* Reverse a and b if x is greater than the mean. */
            if (t > (x / (x + y)))
            {
                flag = true;
                a_ = y;
                b_ = x;
                xc = t;
                x_ = w;
            }
            else
            {
                a_ = x;
                b_ = y;
                xc = w;
                x_ = t;
            }

            if (flag && (b_ * x_) <= 1.0 && x_ <= 0.95)
            {
                t_ = PowerSeries(a_, b_, x_);
                if (t_ <= MACHEP) t_ = 1.0 - MACHEP;
                else t_ = 1.0 - t_;
                return t_;
            }

            /* Choose expansion for better convergence. */
            y_ = x_ * (a_ + b_ - 2.0) - (a_ - 1.0);
            if (y_ < 0.0)
                w = incbcf(a_, b_, x_);
            else
                w = incbd(a_, b_, x_) / xc;

            /* Multiply w by the factor
                   a      b   _             _     _
                  x  (1-x)   | (a+b) / ( a | (a) | (b) ) .   */

            y_ = a_ * Math.Log(x_);
            t_ = b_ * Math.Log(xc);
            if ((a_ + b_) < MAXGAM && Math.Abs(y_) < MAXLOG && Math.Abs(t_) < MAXLOG)
            {
                t_ = Math.Pow(xc, b_);
                t_ *= Math.Pow(x_, a_);
                t_ /= a_;
                t_ *= w;
                t_ *= Gamma(a_ + b_) / (Gamma(a_) * Gamma(b_));
                if (flag)
                {
                    if (t_ <= MACHEP) t_ = 1.0 - MACHEP;
                    else t_ = 1.0 - t_;
                }
                return t_;
            }
            /* Resort to logarithms.  */
            y_ += t_ + lgamma(a_ + b_) - lgamma(a_) - lgamma(b_);
            y_ += Math.Log(w / a_);
            if (y_ < MINLOG)
                t_ = 0.0;
            else
                t_ = Math.Exp(y_);

            if (flag)
            {
                if (t_ <= MACHEP) t_ = 1.0 - MACHEP;
                else t_ = 1.0 - t_;
            }
            return t_;
        }

        #endregion

        /// <summary>Evaluates the logistic function (sigmoïd) </summary>
        /// <param name="x">the evaluation point</param>
        /// <returns>a double</returns>
        public static double LogisticFunction(double x)
        {
            return Math.Min(Math.Max(1e-15, 1 / (1 + Math.Exp(-x))), 1 - 1e-15);
        }

        /// <summary>Evaluates the logistic function (sigmoïd)</summary>
        /// <param name="theta">the scaling vector</param>
        /// <param name="x">the input vector</param>
        /// <returns></returns>
        public static double LogisticFunction(Vector theta, Vector x)
        {
            return LogisticFunction(Vector.Scalar(theta, x));
        }

        /// <summary>Returns the factorial of the specified number</summary>
        /// <param name="j">the target number</param>
        /// <returns>the factorial</returns>
        public static int Factorial(int j)
        {
            int i = j,
                d = 1;
            if (j < 0) i = Math.Abs(j);
            while (i > 1)
                d *= i--;
            return j < 0 ? -d : d;
        }

        /// <summary>Returns the factorial of the specified number</summary>
        /// <param name="j">the target number</param>
        /// <returns>the factorial</returns>
        public static long Factorial(long j)
        {
            long i = j,
                d = 1;
            if (j < 0) i = Math.Abs(j);
            while (i > 1)
                d *= i--;
            return j < 0 ? -d : d;
        }

        /// <summary> Returns the gamma function computed by Stirling's formula</summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private static double stirf(double x)
        {
            double[] STIR = {
                            7.87311395793093628397E-4,
                            -2.29549961613378126380E-4,
                            -2.68132617805781232825E-3,
                            3.47222221605458667310E-3,
                            8.33333333333482257126E-2,
            };
            double MAXSTIR = 143.01608,
                w = 1.0 / x,
                y = Math.Exp(x);

            w = 1.0 + w * EvaluatePolynomial(w, STIR, 4);

            if (x > MAXSTIR)
            {
                /* Avoid overflow in Math.Pow() */
                double v = Math.Pow(x, 0.5 * x - 0.25);
                y = v * (v / y);
            }
            else
                y = Math.Pow(x, x - 0.5) / y;

            y = SQTPI * y * w;
            return y;
        }

        #region Chi Square

        /// <summary> Returns the chi-square function (left hand tail)</summary>
        /// <param name="df">degrees of freedom</param>
        /// <param name="x">double value</param>
        /// <returns></returns>
        public static double chisq(double df, double x)
        {
            if (x < 0.0 || df < 1.0) return 0.0;

            return IncompleteLowerGamma(df / 2.0, x / 2.0);

        }

        /// <summary>Returns the chi-square function (right hand tail)</summary>
        /// <param name="df">degrees of freedom</param>
        /// <param name="x">double value</param>
        /// <returns></returns>
        public static double chisqc(double df, double x)
        {
            if (x < 0.0 || df < 1.0) return 0.0;

            return IncompleteUpperGamma(df / 2.0, x / 2.0);
        }
        #endregion

        /// <summary>Computes the cumulative distribution(CDF) of the sup brownian bridge distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">the location at which to compute the function</param>
        /// <returns>a double</returns>
        public static double SupBrownianBridgeCDF(double x)
        {
            if (x <= 0) return 0; // the probability that the sup of an absolute value of a brownian bridge is zero is null. 
            double sum = 0;
            double numberOfStep = Math.Min(1000, (3 / x));
            double u = -1, v = 1, c1 = Math.Exp(-2 * x * x), c2 = Math.Exp(-4 * x * x);

            for (int i = 0; i < numberOfStep; i++)
            {
                u = (-1) * u * c1 * v;
                sum += u;
                v *= c2;
            }

            return 2 * sum;
        }

        /// <summary>Returns the sum of the first k terms of the Poisson distribution</summary>
        /// <param name="k">number of terms</param>
        /// <param name="x">double value</param>
        /// <returns></returns>
        public static double Poisson(int k, double x)
        {
            if (k < 0 || x < 0) return 0.0;

            return IncompleteUpperGamma((double)(k + 1), x);
        }

        /// <summary>Returns the sum of the terms k+1 to infinity of the Poisson distribution</summary>
        /// <param name="k">start</param>
        /// <param name="x">double value</param>
        /// <returns></returns>
        public static double Poissonc(int k, double x)
        {
            if (k < 0 || x < 0) return 0.0;

            return IncompleteLowerGamma((double)(k + 1), x);
        }

        #region Gauss Bell functions

        /// <summary>Computes the Phi function which is the cumulative distribution for the standard normal distribution</summary>
        /// <param name="x">The location at which to compute the Phi</param>
        /// <returns>a double</returns>
        public static double Phi(double x)
        {
            // This algorithm is ported from dcdflib:
            // Cody, W.D. (1993). "ALGORITHM 715: SPECFUN - A Portabel FORTRAN
            // Package of Special Function Routines and Test Drivers"
            // acm Transactions on Mathematical Software. 19, 22-32.
            int i;
            double del, xden, xnum, xsq;
            double result, ccum;
            const double sixten = 1.60e0;
            const double sqrpi = 3.9894228040143267794e-1;
            const double thrsh = 0.66291e0;
            const double root32 = 5.656854248e0;
            const double zero = 0.0e0;
            const double min = Double.Epsilon;
            double y = Math.Abs(x);
            const double half = 0.5e0;
            const double one = 1.0e0;

            double[] a = { 2.2352520354606839287e00, 1.6102823106855587881e02, 1.0676894854603709582e03, 1.8154981253343561249e04, 6.5682337918207449113e-2 };

            double[] b = { 4.7202581904688241870e01, 9.7609855173777669322e02, 1.0260932208618978205e04, 4.5507789335026729956e04 };

            double[] c =
            {
                3.9894151208813466764e-1, 8.8831497943883759412e00, 9.3506656132177855979e01,
                5.9727027639480026226e02, 2.4945375852903726711e03, 6.8481904505362823326e03,
                1.1602651437647350124e04, 9.8427148383839780218e03, 1.0765576773720192317e-8
            };

            double[] d =
            {
                2.2266688044328115691e01, 2.3538790178262499861e02, 1.5193775994075548050e03,
                6.4855582982667607550e03, 1.8615571640885098091e04, 3.4900952721145977266e04,
                3.8912003286093271411e04, 1.9685429676859990727e04
            };
            double[] p =
            {
                2.1589853405795699e-1, 1.274011611602473639e-1, 2.2235277870649807e-2,
                1.421619193227893466e-3, 2.9112874951168792e-5, 2.307344176494017303e-2
            };


            double[] q = { 1.28426009614491121e00, 4.68238212480865118e-1, 6.59881378689285515e-2, 3.78239633202758244e-3, 7.29751555083966205e-5 };
            if (y <= thrsh) // Evaluate  anorm  for  |X| <= 0.66291
            {
                xsq = zero;
                if (y > double.Epsilon) xsq = x * x;
                xnum = a[4] * xsq;
                xden = xsq;
                for (i = 0; i < 3; i++)
                {
                    xnum = (xnum + a[i]) * xsq;
                    xden = (xden + b[i]) * xsq;
                }
                result = x * (xnum + a[3]) / (xden + b[3]);
                double temp = result;
                result = half + temp;
            }
            else if (y <= root32)   // Evaluate  anorm  for 0.66291 <= |X| <= sqrt(32)
            {
                xnum = c[8] * y;
                xden = y;
                for (i = 0; i < 7; i++)
                {
                    xnum = (xnum + c[i]) * y;
                    xden = (xden + d[i]) * y;
                }
                result = (xnum + c[7]) / (xden + d[7]);
                xsq = Math.Floor(y * sixten) / sixten;
                del = (y - xsq) * (y + xsq);
                result = Math.Exp(-(xsq * xsq * half)) * Math.Exp(-(del * half)) * result;
                ccum = one - result;
                if (x > zero)
                {
                    result = ccum;
                }
            }
            else if (y < 28) // Evaluate  anorm  for sqrt(32) < |X| <28
            {
                xsq = one / (x * x);
                xnum = p[5] * xsq;
                xden = xsq;
                for (i = 0; i < 4; i++)
                {
                    xnum = (xnum + p[i]) * xsq;
                    xden = (xden + q[i]) * xsq;
                }
                result = xsq * (xnum + p[4]) / (xden + q[4]);
                result = (sqrpi - result) / y;
                xsq = Math.Floor(x * sixten) / sixten;
                del = (x - xsq) * (x + xsq);
                result = Math.Exp(-(xsq * xsq * half)) * Math.Exp(-(del * half)) * result;
                ccum = one - result;
                if (x > zero)
                    result = ccum;
            }
            else //Evaluate anorm for |X| >= 28
            {
                double tiny = 8.12387e-173;
                if (x < 0)
                    return tiny;
                else
                    return 1 - tiny;
            }


            if (result < min)
                result = 0.0e0;
            return result;

        }

        /// <summary>Computes the inverse of the Phi function</summary>
        /// <param name="p">The location at which to compute the inverse Phi function</param>
        /// <returns> a <c>double</c></returns>
        public static double InvPhi(double p)
        {
            if (p < 0 || p > 1) throw new ArgumentOutOfRangeException(nameof(p), "The probability must be comprised in [0, 1].");
            if (p == 0) return double.MinValue;
            if (p == 1) return double.MaxValue;

            #region Const values
            double[] a = { -3.969683028665376e+01, 2.209460984245205e+02, -2.759285104469687e+02, 1.383577518672690e+02, -3.066479806614716e+01, 2.506628277459239e+00 },
                b = { -5.447609879822406e+01, 1.615858368580409e+02, -1.556989798598866e+02, 6.680131188771972e+01, -1.328068155288572e+01 },
                c = { -7.784894002430293e-03, -3.223964580411365e-01, -2.400758277161838e+00, -2.549732539343734e+00, 4.374664141464968e+00, 2.938163982698783e+00 },
                d = { 7.784695709041462e-03, 3.224671290700398e-01, 2.445134137142996e+00, 3.754408661907416e+00 };
            #endregion

            double pLow = 0.02425,
                pHigh = 1 - pLow,
                result,
                q;

            if (p < pLow)
            {
                // Rational approximation for lower region:
                q = Math.Sqrt(-2 * Math.Log(p));
                result = (((((c[0] * q + c[1]) * q + c[2]) * q + c[3]) * q + c[4]) * q + c[5]) /
                    ((((d[0] * q + d[1]) * q + d[2]) * q + d[3]) * q + 1);
            }
            else if (pHigh < p)
            {
                // Rational approximation for upper region:
                q = Math.Sqrt(-2 * Math.Log(1 - p));
                result = -(((((c[0] * q + c[1]) * q + c[2]) * q + c[3]) * q + c[4]) * q + c[5]) /
                    ((((d[0] * q + d[1]) * q + d[2]) * q + d[3]) * q + 1);
            }
            else
            {
                // Rational approximation for central region:
                q = p - 0.5;
                double r = q * q;
                result = (((((a[0] * r + a[1]) * r + a[2]) * r + a[3]) * r + a[4]) * r + a[5]) * q /
                    (((((b[0] * r + b[1]) * r + b[2]) * r + b[3]) * r + b[4]) * r + 1);
            }

            return result;
        }

        /// <summary>Computes the Gauss-bell function</summary>
        /// <param name="x">The location at which to compute the function</param>
        /// <returns>a <c>double</c></returns>
        public static double GaussBell(double x)
        {
            if (x == double.MinValue || x == double.MaxValue) return 0;
            return Math.Exp((-0.5 * x * x)) / Math.Sqrt(2.0 * Math.PI);
        }

        #endregion

        /// <summary>Evaluates polynomial of degree N</summary>
        /// <param name="x"></param>
        /// <param name="coef"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        private static double EvaluatePolynomial(double x, double[] coef, int N)
        {
            double ans = coef[0];

            for (int i = 1; i <= N; i++)
                ans = ans * x + coef[i];

            return ans;
        }

        /// <summary>Evaluates polynomial of degree N with assumtion that coef[N] = 1.0</summary>
        /// <param name="x"></param>
        /// <param name="coef"></param>
        /// <param name="N"></param>
        /// <returns></returns>		
        private static double p1evl(double x, double[] coef, int N)
        {
            double ans = x + coef[0];

            for (int i = 1; i < N; i++)
                ans = ans * x + coef[i];

            return ans;
        }

        /// <summary>Returns the natural logarithm of gamma function.</summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double lgamma(double x)
        {
            double p, q, w, z;

            double[] A = {
                         8.11614167470508450300E-4,
                         -5.95061904284301438324E-4,
                         7.93650340457716943945E-4,
                         -2.77777777730099687205E-3,
                         8.33333333333331927722E-2
                     };
            double[] B = {
                         -1.37825152569120859100E3,
                         -3.88016315134637840924E4,
                         -3.31612992738871184744E5,
                         -1.16237097492762307383E6,
                         -1.72173700820839662146E6,
                         -8.53555664245765465627E5
                     };
            double[] C = {
						 /* 1.00000000000000000000E0, */
						 -3.51815701436523470549E2,
                         -1.70642106651881159223E4,
                         -2.20528590553854454839E5,
                         -1.13933444367982507207E6,
                         -2.53252307177582951285E6,
                         -2.01889141433532773231E6
                     };

            if (x < -34.0)
            {
                q = -x;
                w = lgamma(q);
                p = Math.Floor(q);
                if (p == q) throw new ArithmeticException("lgam: Overflow");
                z = q - p;
                if (z > 0.5)
                {
                    p += 1.0;
                    z = p - q;
                }
                z = q * Math.Sin(Math.PI * z);
                if (z == 0.0)
                    throw new
                        ArithmeticException("lgamma: Overflow");
                z = LOGPI - Math.Log(z) - w;
                return z;
            }

            if (x < 13.0)
            {
                z = 1.0;
                while (x >= 3.0)
                {
                    x -= 1.0;
                    z *= x;
                }
                while (x < 2.0)
                {
                    if (x == 0.0)
                        throw new
                            ArithmeticException("lgamma: Overflow");
                    z /= x;
                    x += 1.0;
                }
                if (z < 0.0) z = -z;
                if (x == 2.0) return Math.Log(z);
                x -= 2.0;
                p = x * EvaluatePolynomial(x, B, 5) / p1evl(x, C, 6);
                return (Math.Log(z) + p);
            }

            if (x > 2.556348e305)
                throw new
                    ArithmeticException("lgamma: Overflow");

            q = (x - 0.5) * Math.Log(x) - x + 0.91893853320467274178;
            if (x > 1.0e8) return (q);

            p = 1.0 / (x * x);
            if (x >= 1000.0)
                q += ((7.9365079365079365079365e-4 * p
                    - 2.7777777777777777777778e-3) * p
                    + 0.0833333333333333333333) / x;
            else
                q += EvaluatePolynomial(p, A, 4) / x;
            return q;
        }

        /// <summary>Returns the continued fraction expansion #1 for incomplete beta integral</summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        private static double incbcf(double a, double b, double x)
        {
            double xk, pk, pkm1, pkm2, qk, qkm1, qkm2;
            double k1, k2, k3, k4, k5, k6, k7, k8;
            double r, t, ans, thresh;
            int n;
            double big = 4.503599627370496e15;
            double biginv = 2.22044604925031308085e-16;

            k1 = a;
            k2 = a + b;
            k3 = a;
            k4 = a + 1.0;
            k5 = 1.0;
            k6 = b - 1.0;
            k7 = k4;
            k8 = a + 2.0;

            pkm2 = 0.0;
            qkm2 = 1.0;
            pkm1 = 1.0;
            qkm1 = 1.0;
            ans = 1.0;
            r = 1.0;
            n = 0;
            thresh = 3.0 * MACHEP;
            do
            {
                xk = -(x * k1 * k2) / (k3 * k4);
                pk = pkm1 + pkm2 * xk;
                qk = qkm1 + qkm2 * xk;
                pkm2 = pkm1;
                pkm1 = pk;
                qkm2 = qkm1;
                qkm1 = qk;

                xk = (x * k5 * k6) / (k7 * k8);
                pk = pkm1 + pkm2 * xk;
                qk = qkm1 + qkm2 * xk;
                pkm2 = pkm1;
                pkm1 = pk;
                qkm2 = qkm1;
                qkm1 = qk;

                if (qk != 0) r = pk / qk;
                if (r != 0)
                {
                    t = Math.Abs((ans - r) / r);
                    ans = r;
                }
                else
                    t = 1.0;

                if (t < thresh) return ans;

                k1 += 1.0;
                k2 += 1.0;
                k3 += 2.0;
                k4 += 2.0;
                k5 += 1.0;
                k6 -= 1.0;
                k7 += 2.0;
                k8 += 2.0;

                if ((Math.Abs(qk) + Math.Abs(pk)) > big)
                {
                    pkm2 *= biginv;
                    pkm1 *= biginv;
                    qkm2 *= biginv;
                    qkm1 *= biginv;
                }
                if ((Math.Abs(qk) < biginv) || (Math.Abs(pk) < biginv))
                {
                    pkm2 *= big;
                    pkm1 *= big;
                    qkm2 *= big;
                    qkm1 *= big;
                }
            } while (++n < 300);

            return ans;
        }

        /// <summary>Returns the continued fraction expansion #2 for incomplete beta integral.</summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        private static double incbd(double a, double b, double x)
        {
            double xk, pk, pkm1, pkm2, qk, qkm1, qkm2;
            double k1, k2, k3, k4, k5, k6, k7, k8;
            double r, t, ans, z, thresh;
            int n;
            double big = 4.503599627370496e15;
            double biginv = 2.22044604925031308085e-16;

            k1 = a;
            k2 = b - 1.0;
            k3 = a;
            k4 = a + 1.0;
            k5 = 1.0;
            k6 = a + b;
            k7 = a + 1.0;
            ;
            k8 = a + 2.0;

            pkm2 = 0.0;
            qkm2 = 1.0;
            pkm1 = 1.0;
            qkm1 = 1.0;
            z = x / (1.0 - x);
            ans = 1.0;
            r = 1.0;
            n = 0;
            thresh = 3.0 * MACHEP;
            do
            {
                xk = -(z * k1 * k2) / (k3 * k4);
                pk = pkm1 + pkm2 * xk;
                qk = qkm1 + qkm2 * xk;
                pkm2 = pkm1;
                pkm1 = pk;
                qkm2 = qkm1;
                qkm1 = qk;

                xk = (z * k5 * k6) / (k7 * k8);
                pk = pkm1 + pkm2 * xk;
                qk = qkm1 + qkm2 * xk;
                pkm2 = pkm1;
                pkm1 = pk;
                qkm2 = qkm1;
                qkm1 = qk;

                if (qk != 0) r = pk / qk;
                if (r != 0)
                {
                    t = Math.Abs((ans - r) / r);
                    ans = r;
                }
                else
                    t = 1.0;

                if (t < thresh) return ans;

                k1 += 1.0;
                k2 -= 1.0;
                k3 += 2.0;
                k4 += 2.0;
                k5 += 1.0;
                k6 += 1.0;
                k7 += 2.0;
                k8 += 2.0;

                if ((Math.Abs(qk) + Math.Abs(pk)) > big)
                {
                    pkm2 *= biginv;
                    pkm1 *= biginv;
                    qkm2 *= biginv;
                    qkm1 *= biginv;
                }
                if ((Math.Abs(qk) < biginv) || (Math.Abs(pk) < biginv))
                {
                    pkm2 *= big;
                    pkm1 *= big;
                    qkm2 *= big;
                    qkm1 *= big;
                }
            } while (++n < 300);

            return ans;
        }

        /// <summary>Returns the power series for incomplete beta integral. Use when b*x is small and x not too close to 1.</summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        private static double PowerSeries(double a, double b, double x)
        {
            double s, t, u, v, n, t1, z, ai;

            ai = 1.0 / a;
            u = (1.0 - b) * x;
            v = u / (a + 1.0);
            t1 = v;
            t = u;
            n = 2.0;
            s = 0.0;
            z = MACHEP * ai;
            while (Math.Abs(v) > z)
            {
                u = (n - b) * x / n;
                t *= u;
                v = t / (a + n);
                s += v;
                n += 1.0;
            }
            s += t1;
            s += ai;

            u = a * Math.Log(x);
            if ((a + b) < MAXGAM && Math.Abs(u) < MAXLOG)
            {
                t = Gamma(a + b) / (Gamma(a) * Gamma(b));
                s = s * t * Math.Pow(x, a);
            }
            else
            {
                t = lgamma(a + b) - lgamma(a) - lgamma(b) + u + Math.Log(s);
                if (t < MINLOG) s = 0.0;
                else s = Math.Exp(t);
            }
            return s;
        }
    }
}
