# Euclid
C# library for maths

This is a work in progress, the classes are not in the definitive form but the structure should not evolve that much. The library was built from the needs I met while crunching data for finance or other reasons, so there are parts that I did not code myself and I tried to keep all the licensing where it is. Feel free to raise any issues

Coming :
========
 serious comments, examples for all classes, etc...

Classes :
=========
- *Matrix*, *Vector* : complete matrix and vector classes
- *Complex* : basic operations
- *Polynomial* : evaluation for real and complex values, real and complex roots (Weierstrasss algorithm, coming soon : **RPOLY**)

Data / Statistics :
===================
- *DataFrame*, *Series*, *Slice* : generic content, legends and label, allows extraction, modification, lines and columns removal, XML and CSV serialization
- **Regressions** : OLS, LASSO, RIDGE
- **coming** :  Partial LS, Percentage LS, Elastic NET

Probabilities / Statistics :
============================
- **Distributions** : *Cauchy*, *ChiSquared*, *Exponential*, *ExponentialPower*, *Gamma*, *Laplace*, *Logistic*, *LogNormal*, *Normal*, *Pareto*, *TruncatedNormal*, *Uniform*, *Weibull* (average, variance, support, skewness, mode, entropy, samples, CDF, Inverse CDF, PDF, Log PDF, etc...)
- *Histogram* : basic histograms
- **coming** : other continuous distributions (F, Student), discrete distributions, random number generators

Methods :
=========
- **Root finding** : *RootBracketing*, *NewtonRaphson* univariate solvers (comming soon **Ridder**)
- *GradientDescent* : with or without momentum, line search (Naïve, Armijo, Goldstein, Wolfe), and BFGS
- **Derivation & Integration** : simple and multiple order numerical differentiation(Forward / Backward / Central), numerical integration (Left / Right / Middle / Trapeze / Simpson)
- **coming** : QGPI, neural networks

Useful :
========
- *Ellipsoid*, *Coordinates* : geodesic distances on Earth
- Linear Algebra : symmetric matrix eigen-decomposition 
- Binomial coefficients
- **coming** : Combinations and arrangements
