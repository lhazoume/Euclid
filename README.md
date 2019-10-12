# Euclid

Euclid is an opensource toolbox library for .Net. It aims at providing all necessary objects and methods to tackle very fast most problems.

Objects :
=========
The objects implemented include :
- linear algebra tools : *Vector*, *Matrix*, *Complex*, *Polynomial*
- continuous and Discrete **Distributions** (*Cauchy*, *ChiSquared*, *Exponential*, *ExponentialPower*, *Gamma*, *Laplace*, *Logistic*, *LogNormal*, *Normal*, *Pareto*, *TruncatedNormal*, *Uniform*, *Weibull* with average, variance, support, skewness, mode, entropy, samples, CDF, Inverse CDF, PDF, Log PDF, etc...)
- *Interval* and *Histogram*
- data handling tools : *DataFrame*, *Series*, *Slice*

For more details, please go to [Euclid's wiki](https://github.com/lhazoume/Euclid/wiki)

Methods :
=========
Some tools come with it :
- Optimization : *ParticleSwarmOptimization*, *GradientDescent*, *DifferentialEvolution*, *NelderMead*
- Univariate solvers : *RootBracketing*, *NewtonRaphson*
- Numerics : *Derivation & Integration* i.e. simple and multiple order numerical differentiation(Forward / Backward / Central) and integration (Left / Right / Middle / Trapeze / Simpson)
- Regressions : *OrdinaryLeastSquares*, *Logistic*, *LASSO*, *RIDGE*, *LazyPartialLeastSquares*

Disclaimer :
============
This is a work in progress, the classes are not in the definitive form but the structure should not evolve that much. The library was built from the needs I met while crunching data for finance or other reasons, so there are parts that I did not code myself and I tried to keep all the licensing where it is. Feel free to raise any issues
