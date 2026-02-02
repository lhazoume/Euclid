using System;
using System.Linq;
using Euclid.Distributions;
using Euclid.Distributions.Continuous;
using Euclid.Distributions.Discrete;


namespace EuclidExamples
{
    public class DistributionExample
    {
        public static void Binomial()
        {
            // Paramètres de la distribution
            int n = 10;
            double p = 0.3;

            // Création de la distribution binomiale
            BinomialDistribution binomial = new BinomialDistribution(n, p);

            Console.WriteLine("=== BINOMIAL DISTRIBUTION EXAMPLE ===");
            Console.WriteLine($"Parameters: n = {n}, p = {p}");
            Console.WriteLine();

            // Statistiques descriptives
            Console.WriteLine(">> Descriptive Statistics");
            Console.WriteLine($"Mean               : {binomial.Mean:N2}");
            Console.WriteLine($"Median             : {binomial.Median:N2}");
            Console.WriteLine($"Mode               : {binomial.Mode:N2}");
            Console.WriteLine($"Standard Deviation : {binomial.StandardDeviation:N2}");
            Console.WriteLine($"Variance           : {binomial.Variance:N2}");
            Console.WriteLine($"Skewness           : {binomial.Skewness:N2}");
            Console.WriteLine($"Support            : {{x in \u2115  | {binomial.Support.First()} <= x <= {binomial.Support.Last()}}}");
            Console.WriteLine();

            // Fonctions de distribution
            Console.WriteLine(">> Distribution Functions");
            Console.WriteLine($"PDF at x = 2       : P(X = 2) = {binomial.ProbabilityDensity(2):P2}");
            Console.WriteLine($"CDF at x = 4       : P(X <= 4) = {binomial.CumulativeDistribution(4):P2}");
            Console.WriteLine($"P(3 < X ≤ 5)       : {binomial.CumulativeDistribution(5) - binomial.CumulativeDistribution(3):P2}");
            Console.WriteLine($"1st Quartile (Q1)  : {binomial.InverseCumulativeDistribution(0.25):N2}");
            Console.WriteLine($"3rd Quartile (Q3)  : {binomial.InverseCumulativeDistribution(0.75):N2}");
            Console.WriteLine();

            // Simulation et estimation
            Console.WriteLine(">> Simulation & Fitting");
            double[] sample = binomial.Sample(5000);
            BinomialDistribution estimatedBinomial = BinomialDistribution.Fit(FittingMethod.Moments, sample);
            Console.WriteLine($"Estimated from sample: {estimatedBinomial}");
            Console.WriteLine();

            Console.WriteLine("=== END OF EXAMPLE ===\n");
        }

        public static void Poisson()
        {
            // Distribution parameter
            double lambda = 4.5;

            // Create a Poisson distribution
            PoissonDistribution poisson = new PoissonDistribution(lambda);

            Console.WriteLine("=== POISSON DISTRIBUTION EXAMPLE ===");
            Console.WriteLine($"Parameter: λ  = {lambda}");
            Console.WriteLine();

            // Descriptive statistics
            Console.WriteLine(">> Descriptive Statistics");
            Console.WriteLine($"Mean               : {poisson.Mean:N2}");
            Console.WriteLine($"Median             : {poisson.Median:N2}");
            Console.WriteLine($"Mode               : {poisson.Mode:N2}");
            Console.WriteLine($"Standard Deviation : {poisson.StandardDeviation:N2}");
            Console.WriteLine($"Variance           : {poisson.Variance:N2}");
            Console.WriteLine($"Skewness           : {poisson.Skewness:N2}");
            Console.WriteLine($"Support            : {{x in \u2115  | {poisson.Support.First()} <= x <= {poisson.Support.Last()}}}");
            Console.WriteLine();

            // Distribution functions
            Console.WriteLine(">> Distribution Functions");
            Console.WriteLine($"PDF at x = 3       : P(X = 3) = {poisson.ProbabilityDensity(3):P4}");
            Console.WriteLine($"CDF at x = 6       : P(X ≤ 6) = {poisson.CumulativeDistribution(6):P4}");
            Console.WriteLine($"P(2 < X ≤ 5)       : {poisson.CumulativeDistribution(5) - poisson.CumulativeDistribution(2):P4}");
            Console.WriteLine($"1st Quartile (Q1)  : {poisson.InverseCumulativeDistribution(0.25):N2}");
            Console.WriteLine($"3rd Quartile (Q3)  : {poisson.InverseCumulativeDistribution(0.75):N2}");
            Console.WriteLine();

            // Simulation and parameter estimation
            Console.WriteLine(">> Simulation & Fitting");
            double[] sample = poisson.Sample(5000);
            PoissonDistribution estimatedPoisson = PoissonDistribution.Fit(sample);
            Console.WriteLine($"Estimated from sample: {estimatedPoisson}");
            Console.WriteLine();

            Console.WriteLine("=== END OF EXAMPLE ===\n");
        }

        public static void Skellam()
        {
            // Distribution parameters
            double mu1 = 7.0;
            double mu2 = 4.0;

            // Create a Skellam distribution
            SkellamDistribution skellam = new SkellamDistribution(mu1, mu2);

            Console.WriteLine("=== SKELLAM DISTRIBUTION EXAMPLE ===");
            Console.WriteLine($"Parameters: μ1 = {mu1}, μ2 = {mu2}");
            Console.WriteLine();

            // Descriptive statistics
            Console.WriteLine(">> Descriptive Statistics");
            Console.WriteLine($"Mean               : {skellam.Mean:N2}");
            Console.WriteLine($"Median             : {skellam.Median:N2}");
            Console.WriteLine($"Mode               : {skellam.Mode:N2}");
            Console.WriteLine($"Standard Deviation : {skellam.StandardDeviation:N2}");
            Console.WriteLine($"Variance           : {skellam.Variance:N2}");
            Console.WriteLine($"Skewness           : {skellam.Skewness:N2}");
            Console.WriteLine($"Support            : {{x in \u2124  | {skellam.Support.First()} <= x <= {skellam.Support.Last()}}}");
            Console.WriteLine();

            // Distribution functions
            Console.WriteLine(">> Distribution Functions");
            Console.WriteLine($"PDF at x = 1       : P(X = 1) = {skellam.ProbabilityDensity(1):P4}");
            Console.WriteLine($"CDF at x = 3       : P(X ≤ 3) = {skellam.CumulativeDistribution(3):P4}");
            Console.WriteLine($"P(0 < X ≤ 2)       : {skellam.CumulativeDistribution(2) - skellam.CumulativeDistribution(0):P4}");
            Console.WriteLine($"1st Quartile (Q1)  : {skellam.InverseCumulativeDistribution(0.25):N2}");
            Console.WriteLine($"3rd Quartile (Q3)  : {skellam.InverseCumulativeDistribution(0.75):N2}");
            Console.WriteLine();

            // Simulation and parameter estimation
            Console.WriteLine(">> Simulation & Fitting");
            double[] sample = skellam.Sample(5000);
            SkellamDistribution estimatedSkellam = SkellamDistribution.Fit(FittingMethod.Moments, sample);
            Console.WriteLine($"Estimated from sample: {estimatedSkellam}");
            Console.WriteLine();

            Console.WriteLine("=== END OF EXAMPLE ===\n");
        }

        public static void Exponential()
        {
            // Distribution parameter
            double lambda = 0.4;

            // Create an exponential distribution
            ExponentialDistribution exponential = new ExponentialDistribution(lambda);

            Console.WriteLine("=== EXPONENTIAL DISTRIBUTION EXAMPLE ===");
            Console.WriteLine($"Parameter: λ = {lambda}");
            Console.WriteLine();

            // Descriptive statistics
            Console.WriteLine(">> Descriptive Statistics");
            Console.WriteLine($"Mean               : {exponential.Mean:N2}");
            Console.WriteLine($"Median             : {exponential.Median:N2}");
            Console.WriteLine($"Mode               : {exponential.Mode:N2}");
            Console.WriteLine($"Standard Deviation : {exponential.StandardDeviation:N2}");
            Console.WriteLine($"Variance           : {exponential.Variance:N2}");
            Console.WriteLine($"Skewness           : {exponential.Skewness:N2}");
            Console.WriteLine($"Support            : {exponential.Support}");

            Console.WriteLine();

            // Distribution functions
            Console.WriteLine(">> Distribution Functions");
            Console.WriteLine($"Density at x = 2   : f(2) = {exponential.ProbabilityDensity(2):N4}");
            Console.WriteLine($"CDF at x = 3       : P(X ≤ 3) = {exponential.CumulativeDistribution(3):P4}");
            Console.WriteLine($"P(1 < X < 4)        : {exponential.CumulativeDistribution(4) - exponential.CumulativeDistribution(1):P4}");

            Console.WriteLine($"1st Quartile (Q1)  : {exponential.InverseCumulativeDistribution(0.25):N2}");
            Console.WriteLine($"3rd Quartile (Q3)  : {exponential.InverseCumulativeDistribution(0.75):N2}");
            Console.WriteLine();

            // Simulation and parameter estimation
            Console.WriteLine(">> Simulation & Fitting");
            double[] sample = exponential.Sample(5000, seed: 42);
            ExponentialDistribution estimatedExp = ExponentialDistribution.Fit(sample);
            Console.WriteLine($"Estimated from sample: {estimatedExp}");
            Console.WriteLine();

            Console.WriteLine("=== END OF EXAMPLE ===\n");
        }

        public static void Cauchy()
            {
                // Distribution parameters
                double x0 = 4.0;
                double gamma = 6.0;

                // Create a Cauchy distribution
                CauchyDistribution cauchy = new CauchyDistribution(x0, gamma);

                Console.WriteLine("=== CAUCHY DISTRIBUTION EXAMPLE ===");
                Console.WriteLine($"Parameters: x0 = {x0}, γ = {gamma}");
                Console.WriteLine();

                // Descriptive statistics
                Console.WriteLine(">> Descriptive Statistics");
                Console.WriteLine($"Median             : {cauchy.Median:N2}");
                Console.WriteLine($"Mode               : {cauchy.Mode:N2}");
                Console.WriteLine($"Mean               : undefined");
                Console.WriteLine($"Variance           : undefined");
                Console.WriteLine($"Standard Deviation : undefined");
                Console.WriteLine($"Skewness           : undefined");
                Console.WriteLine($"Entropy            : {cauchy.Entropy:N4}");
                Console.WriteLine($"Support            : {cauchy.Support}");
                Console.WriteLine();

                // Distribution functions
                Console.WriteLine(">> Distribution Functions");
                Console.WriteLine($"Density at x = 1   : f(1) = {cauchy.ProbabilityDensity(1):N4}");
                Console.WriteLine($"CDF at x = 2       : P(X ≤ 2) = {cauchy.CumulativeDistribution(2):P4}");
                Console.WriteLine($"P(-1 < X < 3)      : {cauchy.CumulativeDistribution(3) - cauchy.CumulativeDistribution(-1):P4}");
                Console.WriteLine($"1st Quartile (Q1)  : {cauchy.InverseCumulativeDistribution(0.25):N2}");
                Console.WriteLine($"3rd Quartile (Q3)  : {cauchy.InverseCumulativeDistribution(0.75):N2}");
                Console.WriteLine();

                // Simulation & Fitting
                Console.WriteLine(">> Simulation & Fitting");
                double[] sample = cauchy.Sample(5000, seed: 42);
                CauchyDistribution estimatedCauchy = CauchyDistribution.Fit(sample);
                Console.WriteLine($"Estimated from sample: {estimatedCauchy}");
                Console.WriteLine();
                Console.WriteLine("=== END OF EXAMPLE ===\n");
            }

        public static void Uniform()
        {
            // Distribution parameters
            double a = 2.0;
            double b = 5.0;

            // Create a uniform distribution
            UniformDistribution uniform = new UniformDistribution(a, b);

            Console.WriteLine("=== UNIFORM DISTRIBUTION EXAMPLE ===");
            Console.WriteLine($"Parameters: a = {a}, b = {b}");
            Console.WriteLine();

            // Descriptive statistics
            Console.WriteLine(">> Descriptive Statistics");
            Console.WriteLine($"Mean               : {uniform.Mean:N2}");
            Console.WriteLine($"Median             : {uniform.Median:N2}");
            Console.WriteLine($"Mode               : {uniform.Mode:N2}");
            Console.WriteLine($"Standard Deviation : {uniform.StandardDeviation:N2}");
            Console.WriteLine($"Variance           : {uniform.Variance:N2}");
            Console.WriteLine($"Skewness           : {uniform.Skewness:N2}");
            Console.WriteLine($"Entropy            : {uniform.Entropy:N4}");
            Console.WriteLine($"Support            : {uniform.Support}");
            Console.WriteLine();

            // Distribution functions
            Console.WriteLine(">> Distribution Functions");
            Console.WriteLine($"Density at x = 3   : f(3) = {uniform.ProbabilityDensity(3):N4}");
            Console.WriteLine($"CDF at x = 4       : P(X ≤ 4) = {uniform.CumulativeDistribution(4):P4}");
            Console.WriteLine($"P(2.5 < X < 4.5)   : {uniform.CumulativeDistribution(4.5) - uniform.CumulativeDistribution(2.5):P4}");
            Console.WriteLine($"1st Quartile (Q1)  : {uniform.InverseCumulativeDistribution(0.25):N2}");
            Console.WriteLine($"3rd Quartile (Q3)  : {uniform.InverseCumulativeDistribution(0.75):N2}");
            Console.WriteLine();

            // Simulation & Fitting
            Console.WriteLine(">> Simulation & Fitting");
            Random rand = new Random(42);
            double[] sample = new double[5000];
            for (int i = 0; i < sample.Length; i++)
                sample[i] = a + (b - a) * rand.NextDouble();

            UniformDistribution estimated = UniformDistribution.Fit(sample);
            Console.WriteLine($"Estimated from sample: {estimated}");
            Console.WriteLine();

            Console.WriteLine("=== END OF EXAMPLE ===\n");
        }

        public static void Normal()
            {
                // Distribution parameters
                double mu = 2.0;
                double sigma = 1.5;

                // Create a normal distribution
                NormalDistribution normal = new NormalDistribution(mu, sigma);

                Console.WriteLine("=== NORMAL DISTRIBUTION EXAMPLE ===");
                Console.WriteLine($"Parameters: μ = {mu}, σ = {sigma}");
                Console.WriteLine();

                // Descriptive statistics
                Console.WriteLine(">> Descriptive Statistics");
                Console.WriteLine($"Mean               : {normal.Mean:N2}");
                Console.WriteLine($"Median             : {normal.Median:N2}");
                Console.WriteLine($"Mode               : {normal.Mode:N2}");
                Console.WriteLine($"Standard Deviation : {normal.StandardDeviation:N2}");
                Console.WriteLine($"Variance           : {normal.Variance:N2}");
                Console.WriteLine($"Skewness           : {normal.Skewness:N2}");
                Console.WriteLine($"Support            : {normal.Support}");

                Console.WriteLine();

                // Distribution functions
                Console.WriteLine(">> Distribution Functions");
                Console.WriteLine($"Density at x = 2   : f(2) = {normal.ProbabilityDensity(2):N4}");
                Console.WriteLine($"CDF at x = 3       : P(X ≤ 3) = {normal.CumulativeDistribution(3):P4}");
                Console.WriteLine($"P(1 < X < 4)       : {normal.CumulativeDistribution(4) - normal.CumulativeDistribution(1):P4}");
                Console.WriteLine($"1st Quartile (Q1)  : {normal.InverseCumulativeDistribution(0.25):N2}");
                Console.WriteLine($"3rd Quartile (Q3)  : {normal.InverseCumulativeDistribution(0.75):N2}");

                Console.WriteLine();

                // Simulation and parameter estimation
                Console.WriteLine(">> Simulation & Fitting");
                double[] sample = normal.Sample(5000, seed: 42);
                NormalDistribution estimated = NormalDistribution.Fit(sample);
                Console.WriteLine($"Estimated from sample: {estimated}");
                Console.WriteLine();

                Console.WriteLine("=== END OF EXAMPLE ===\n");
            }


    }   
}
