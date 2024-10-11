using UnityEngine;

public class Tank : TankBase
{
    float fitness = 0;
    float fitnessMod = 1;
    float minimumFitness = 0.0f;
    float maximumFitness = 2.0f;
    float badMines = 0f;

    protected override void OnReset()
    {
        fitness = 1;
        fitnessMod = 1;
        badMines = 0f;
    }

    protected override void OnThink(float dt)
    {
        Vector3 dirToMine = GetDirToMine(nearMine);

        inputs[0] = dirToMine.x;
        inputs[1] = dirToMine.z;
        inputs[2] = transform.forward.x;
        inputs[3] = transform.forward.z;
        inputs[4] = isGoodMine;

        float[] output = brain.Synapsis(inputs);

        SetForces(output[0], output[1], dt);
    }

    protected override void OnTakeMine(GameObject mine)
    {
        if (isGoodMine == 1)
        {
            fitnessMod *= 1.1f;

            if (fitnessMod < maximumFitness)
                fitnessMod = maximumFitness;

            fitness += 10 * fitnessMod;

            badMines = 0;
        }
        else
        {
            fitnessMod *= 0.9f;

            fitness *= 0.85f + 0.4f * fitnessMod;

            badMines++;
        }
        genome.fitness = fitness;
    }
}
