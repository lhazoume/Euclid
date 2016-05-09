# Euclid
C# library for maths

This is a work in progress, the classes are not in the definitive form but the structure should not evolve that much.

Classes :
- Matrix : all matrix operators, inversion, det, LU decomposition
- Complex : basic operations
- Polynomial : evaluation for real and complex values, real and complex roots
- DataFrame, Series : generic content, legends and label, allows extraction, modification, lines and columns removal, XML and CSV serialization
- Histogram : basic histograms

Methods :
- 1D Solvers : NewtonRaphson and RootBracketing (Dichotomy and False Position)
- Numerical integration and differentiation : Left / Right / Middle / Trapeze / Simpson integration and Forward / Backward / Central multiple degree differentiation
- Regressions : OLS Linear Regression
- Linear Algebra : eigen-decomposition, 
- coming : Gradient descents, other regressions (Percentage Least Squares, RIDGE, LASSO, Elastic NET, etc), QGPI

Distributions :
- Continuous : Normal, LogNormal, Exponential, ChiSquared, Pareto
- coming : other continuous distributions (Weibull, Cauchy, Student, uniform, gamma, Laplace), discrete distributions, random number generators

Computations :
- Pascal's triangle coefficients
- coming : Combinations and arrangements
