namespace AgentForge.Agents.Learning.Domain.Models;

/// <summary>Immutable sufficient statistics for one action in a disjoint linear contextual bandit.</summary>
public sealed class ContextualActionStatistics
{
    private readonly double[,] _designMatrix;
    private readonly double[] _rewardVector;

    /// <summary>Creates statistics from a symmetric positive-definite design matrix and reward vector.</summary>
    public ContextualActionStatistics(double[,] designMatrix, IEnumerable<double> rewardVector, long observationCount = 0)
    {
        ArgumentNullException.ThrowIfNull(designMatrix);
        ArgumentNullException.ThrowIfNull(rewardVector);
        if (observationCount < 0) throw new ArgumentOutOfRangeException(nameof(observationCount));
        ObservationCount = observationCount;
        var dimension = designMatrix.GetLength(0);
        if (dimension == 0 || designMatrix.GetLength(1) != dimension)
            throw new ArgumentException("The design matrix must be non-empty and square.", nameof(designMatrix));

        var vector = rewardVector.ToArray();
        if (vector.Length != dimension || vector.Any(value => !double.IsFinite(value)))
            throw new ArgumentException("The reward vector must contain one finite value per feature.", nameof(rewardVector));

        _designMatrix = (double[,])designMatrix.Clone();
        _rewardVector = vector;
        for (var row = 0; row < dimension; row++)
        {
            for (var column = 0; column < dimension; column++)
            {
                var value = _designMatrix[row, column];
                if (!double.IsFinite(value))
                    throw new ArgumentException("The design matrix must contain only finite values.", nameof(designMatrix));
                if (Math.Abs(value - _designMatrix[column, row]) > 1e-10)
                    throw new ArgumentException("The design matrix must be symmetric.", nameof(designMatrix));
            }
        }

        _ = CholeskyFactorization.Factor(_designMatrix);
    }

    /// <summary>Gets the number of context features.</summary>
    public int FeatureCount => _rewardVector.Length;

    /// <summary>Gets the number of reward observations represented by these statistics.</summary>
    public long ObservationCount { get; }

    /// <summary>Gets a defensive copy of the design matrix.</summary>
    public double[,] DesignMatrix => (double[,])_designMatrix.Clone();

    /// <summary>Gets a defensive copy of the reward vector.</summary>
    public IReadOnlyList<double> RewardVector => Array.AsReadOnly((double[])_rewardVector.Clone());

    internal double[,] CopyDesignMatrix() => (double[,])_designMatrix.Clone();
    internal double[] CopyRewardVector() => (double[])_rewardVector.Clone();
}

/// <summary>Small, dependency-free Cholesky factorization for symmetric positive-definite matrices.</summary>
internal static class CholeskyFactorization
{
    public static double[,] Factor(double[,] matrix)
    {
        var size = matrix.GetLength(0);
        var lower = new double[size, size];
        for (var row = 0; row < size; row++)
        {
            for (var column = 0; column <= row; column++)
            {
                var sum = matrix[row, column];
                for (var k = 0; k < column; k++)
                    sum -= lower[row, k] * lower[column, k];

                if (row == column)
                {
                    if (!double.IsFinite(sum) || sum <= 1e-12)
                        throw new ArgumentException("The design matrix must be positive definite.");
                    lower[row, column] = Math.Sqrt(sum);
                }
                else
                {
                    lower[row, column] = sum / lower[column, column];
                    if (!double.IsFinite(lower[row, column]))
                        throw new ArgumentException("The design matrix is numerically invalid.");
                }
            }
        }
        return lower;
    }

    public static double[] Solve(double[,] lower, IReadOnlyList<double> rightHandSide)
    {
        var size = rightHandSide.Count;
        var intermediate = new double[size];
        for (var row = 0; row < size; row++)
        {
            var value = rightHandSide[row];
            for (var column = 0; column < row; column++)
                value -= lower[row, column] * intermediate[column];
            intermediate[row] = value / lower[row, row];
        }

        var solution = new double[size];
        for (var row = size - 1; row >= 0; row--)
        {
            var value = intermediate[row];
            for (var column = row + 1; column < size; column++)
                value -= lower[column, row] * solution[column];
            solution[row] = value / lower[row, row];
            if (!double.IsFinite(solution[row]))
                throw new ArithmeticException("The linear system produced a non-finite solution.");
        }
        return solution;
    }
}
