﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using NeuralNetworkDirectory.NeuralNet;
using StateMachine.Agents.Simulation;

namespace NeuralNetworkDirectory.DataManagement
{
    public static class NeuronDataSystem
    {
        public static void SaveNeurons(List<AgentNeuronData> agentsData, string directoryPath, int generation)
        {
            /*
            var groupedData = agentsData
                .GroupBy(agent => new { agent.AgentType, agent.BrainType })
                .ToDictionary(group => group.Key, group => group.ToList());

            foreach (var group in groupedData)
            {
                string agentTypeDirectory = Path.Combine(directoryPath, group.Key.AgentType.ToString());
                Directory.CreateDirectory(agentTypeDirectory);
            
                string fileName = $"gen{generation}{group.Key.BrainType}.json";
                string filePath = Path.Combine(agentTypeDirectory, fileName);
                string json = "";// = JsonSerializer.Serialize(group.Value);
                File.WriteAllText(filePath, json);
            }*/
        }
        
        public static Dictionary<SimAgentTypes, Dictionary<BrainType, List<AgentNeuronData>>> LoadLatestNeurons(string directoryPath)
        {
            Dictionary<SimAgentTypes, Dictionary<BrainType, List<AgentNeuronData>>> agentsData = new Dictionary<SimAgentTypes, Dictionary<BrainType, List<AgentNeuronData>>>();
            string[] directories = Directory.GetDirectories(directoryPath);

            foreach (string agentTypeDirectory in directories)
            {
                SimAgentTypes agentType = Enum.Parse<SimAgentTypes>(Path.GetFileName(agentTypeDirectory));
                agentsData[agentType] = new Dictionary<BrainType, List<AgentNeuronData>>();

                string[] files = Directory.GetFiles(agentTypeDirectory, "gen*.json");
                if (files.Length == 0)
                    continue;

                string latestFile = files.OrderByDescending(f =>
                {
                    string fileName = Path.GetFileName(f);
                    Match match = Regex.Match(fileName, @"gen(\d+)");
                    if (match.Success)
                    {
                        return int.Parse(match.Groups[1].Value);
                    }
                    return 0;
                }).First();

                string json = File.ReadAllText(latestFile);
                AgentNeuronData[] agentDataList = new AgentNeuronData[0];//  JsonSerializer.Deserialize<List<AgentNeuronData>>(json);

                foreach (AgentNeuronData agentData in agentDataList)
                {
                    if (!agentsData[agentType].ContainsKey(agentData.BrainType))
                    {
                        agentsData[agentType][agentData.BrainType] = new List<AgentNeuronData>();
                    }
                    agentsData[agentType][agentData.BrainType].Add(agentData);
                }
            }

            return agentsData;
        }        
    }
}