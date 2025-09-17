
using UnityEngine;

public static class RemedyRandom
{
    /// <summary>
    /// Determines the rarity score of a card using a probability-based progressive method.
    /// </summary>
    /// <param name="baseChance">The initial probability (0 to 1) of increasing the rarity score.</param>
    /// <param name="rarityCurve">A modifier that affects how quickly the probability decreases as rarity increases.</param>
    /// <param name="maxIterations">The maximum rarity score that can be reached.</param>
    /// <returns>An integer representing the calculated rarity score.</returns>
    public static int Progressive(float baseChance, float rarityCurve, int maxIterations)
    {
        int count = 0;

        while (count<maxIterations && Random.Range(0.0f, 1.0f) < Mathf.Pow(baseChance, count * rarityCurve))
        {
            count++;
        }

        return count;
    }

}