﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeneticAlgorithmRobot
{
    class GeneticAlgorithm
    {
        const int MAX_GENERATION = 1000;
        const int POPULATION_SIZE = 10;

        const int ELITISM = 1;
        const double MUTATION_CHANCE = 0.05;

        private int generation = 0;
        private List<Individual> population = new List<Individual>();
        private ManualResetEvent[] doneEvents = new ManualResetEvent[POPULATION_SIZE];
        private RobotManager robotManager;

        private Random random = new Random();
        private int randomRange;

        PlotDisplay plotDisplay = new PlotDisplay();

        public GeneticAlgorithm(RobotManager robotManager)
        {
            this.robotManager = robotManager;
            for (int i = 0; i < POPULATION_SIZE; i++)
                doneEvents[i] = new ManualResetEvent(false);
        }

        public void Execute()
        {
            Init();
            while (generation < MAX_GENERATION)
            {
                EvaluateGeneration();
                LogResults();
                NextGeneration();
                ++generation;
            }
        }

        private void NextGeneration()
        {
            List<Individual> newPopulation = new List<Individual>();

            for (int i = 0; i < ELITISM; i++)
                newPopulation.Add(population[i]);

            for (int i = 0; i < POPULATION_SIZE - ELITISM; i++)
            {
                Individual newIndividual = Individual.GenerateFromParents(population[getRandomWithFalloff()], population[getRandomWithFalloff()]);
                if (random.NextDouble() < MUTATION_CHANCE)
                    newIndividual = Individual.GenerateFromMutation(newIndividual);
                newPopulation.Add(newIndividual);
            }
            population = newPopulation;
        }

        private void LogResults()
        {
            double maxDistanceMoved = -1;
            double averageDistanceMoved = 0;
            foreach (Individual individual in population)
            {
                averageDistanceMoved += individual.DistanceMoved;
                if (individual.DistanceMoved > maxDistanceMoved)
                    maxDistanceMoved = individual.DistanceMoved;
            }
            averageDistanceMoved /= population.Count;
            Console.WriteLine("Generation " + generation + "| Max: " + maxDistanceMoved + ", Average: " + averageDistanceMoved + ".");
            plotDisplay.LogData(generation, population);
            plotDisplay.Refresh();
        }

        private void EvaluateGeneration()
        {
            int i = 0;
            foreach (Individual individual in population)
            {
                doneEvents[i].Reset();
                individual.Evaluate(doneEvents[i]);
                //ThreadPool.QueueUserWorkItem(individual.Evaluate, doneEvents[i]);
                i++;
            }
            //WaitHandle.WaitAll(doneEvents);
            population.Sort();
        }

        private void Init()
        {
            plotDisplay.Init();

            randomRange = 0;
            for (int i = 0; i < POPULATION_SIZE; i++)
            {
                population.Add(Individual.GenerateRandom(robotManager));
                randomRange += POPULATION_SIZE - i;
            }
        }


        private int getRandomWithFalloff()
        {
            int value = random.Next(randomRange);
            for (int i = 0; i < POPULATION_SIZE; i++)
            {
                value -= POPULATION_SIZE - i;
                if (value <= 0)
                    return i;
            }
            throw new Exception("randomRange error");
        }
    }
}
