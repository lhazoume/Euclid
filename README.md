# Euclid
C# library for maths

This is a work in progress, the classes are not in the definitive form but the structure should not evolve that much. The library was built from the needs I met while crunching data for finance or other reasons, so there are parts that I did not code myself and I tried to keep all the licensing where it is. Feel free to ask me for better credits where needed.

Coming :
========
 serious comments, examples for all classes, etc...

Classes :
=========
- *Matrix* : all matrix operators, inversion, det, LU decomposition
- *Complex* : basic operations
- *Polynomial* : evaluation for real and complex values, real and complex roots
- *DataFrame*, *Series*, *Slice* : generic content, legends and label, allows extraction, modification, lines and columns removal, XML and CSV serialization
- *Histogram* : basic histograms

Methods :
=========
- 1D Solvers : NewtonRaphson and RootBracketing (Dichotomy and False Position)
- Numerical integration and differentiation : Left / Right / Middle / Trapeze / Simpson integration and Forward / Backward / Central high degree differentiation
- Regressions : OLS Linear Regression
- Linear Algebra : symmetric matrix eigen-decomposition 
- **coming** : Gradient descents, other regressions (Percentage Least Squares, RIDGE, LASSO, Elastic NET, etc), QGPI

Distributions :
===============
- Continuous : Cauchy, ChiSquared, Exponential, Gamma, Laplace, LogNormal, Normal, Pareto, TruncatedNormal, Uniform, Weibull
- **coming** : other continuous distributions (F, Student), discrete distributions, random number generators

Computations :
==============
- Binomial coefficients
- **coming** : Combinations and arrangements
