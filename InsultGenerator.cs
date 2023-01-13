using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DiscordLeagueBot;

public class InsultGenerator
{
    private string adjectivesFilePath = @"WordListFiles\adjectives.txt";
    private string nounsFilePath = @"WordListFiles\nouns.txt";
    
    List<string> adjectives = new();
    private List<string> nouns = new();

    private Random rand = new();

    public InsultGenerator()
    {
        Console.WriteLine("Insult Generator created");
    }

    // Generates a random insult
    public async Task<string> GenerateRandomInsult()
    {
        return $"You are a {adjectives[rand.Next(0, adjectives.Count)]} {nouns[rand.Next(0, nouns.Count)]}";
    }

    // Add all words in the file path to the adjectives list
    public async Task UpdateAdjectives()//string filePath)
    {
        //var filePath = adjectivesFilePath;
        //filePath = @"WordListFiles\adjectives.txt";

        if (File.Exists(adjectivesFilePath))
        {
            var text = await File.ReadAllLinesAsync(adjectivesFilePath);
            adjectives = text.ToList();
            Console.WriteLine("Updated list of adjectives.");
        }
        else
        {
            Console.WriteLine($"File not found: {adjectivesFilePath}");
            throw new Exception($"File not found at: {adjectivesFilePath}");
        }
    }

    public async Task UpdateNouns()
    {
        if (File.Exists(nounsFilePath))
        {
            var text = await File.ReadAllLinesAsync(nounsFilePath);
            nouns = text.ToList();
            Console.WriteLine("Updated list of nouns.");
        }
        else
        {
            Console.WriteLine($"File not found: {nounsFilePath}");
            throw new Exception($"File not found at: {nounsFilePath}");
        }
    }
}